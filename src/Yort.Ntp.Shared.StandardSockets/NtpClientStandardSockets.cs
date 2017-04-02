using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Yort.Ntp
{
	public partial class NtpClient
	{
		partial void SendTimeRequest()
		{
			try
			{
				byte[] buffer = new byte[48];
				buffer[0] = 0x1B;

#if REQUIRES_IPENDPOINT
				// Xamarin Android throws a NotSupported or NotImplemented exception when calling
				// Socket.Connect using a DnsEndPoint instance, but is fine with IPAddress. So
				// specifically for Android we'll do the DNS lookup on the name ourselves
				// and then use the address we find into an IPEndpoint. If we were given an 
				// IP anyway, ServerAddressToIPEndpoint, should detect that and just use it.
				EndPoint _endPoint = ServerAddressToIPEndpoint(_ServerAddress, 123);
#else
				EndPoint _endPoint = new DnsEndPoint(_ServerAddress, 123, AddressFamily.InterNetwork);
#endif

				Socket socket = null;
				var socketArgs = new SocketAsyncEventArgs() { RemoteEndPoint = _endPoint };
				try
				{
					socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

					try
					{
						socketArgs.Completed += Socket_Completed_SendAgain;
						//TFW - For some reason 'ConnectAsync' reports an error
						//desktop .Net 4, but 'Connect' works fine. On WP
						//only ConnectAsync is available, and it appears to work.
#if USE_CONNECTASYNC
					socketArgs.SetBuffer(buffer, 0, buffer.Length);
					if (!socket.ConnectAsync(socketArgs))
						Socket_Completed_SendAgain(socket, socketArgs);
#else
						socket.Connect(socketArgs.RemoteEndPoint);
						socketArgs.SetBuffer(buffer, 0, buffer.Length);
						if (!socket.SendAsync(socketArgs))
							Socket_Completed_SendAgain(socket, socketArgs);
#endif
					}
					catch
					{
						socketArgs.Completed -= this.Socket_Completed_SendAgain;
						throw;
					}
				}
				catch
				{
					socket?.Dispose();
					throw;
				}
			}
			catch (SocketException se)
			{
				OnErrorOccurred(new NtpNetworkException(se.Message, (int)se.SocketErrorCode, se));
			}
			catch (Exception ex)
			{
				OnErrorOccurred(new NtpNetworkException(ex.Message, -1, ex));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		private void Socket_Completed_SendAgain(object sender, SocketAsyncEventArgs e)
		{
			Socket socket = null; 

			try
			{
				e.Completed -= this.Socket_Completed_SendAgain;
				socket = ((System.Net.Sockets.Socket)sender);

				if (e.SocketError == SocketError.Success)
				{
					var socketArgs = new SocketAsyncEventArgs() { RemoteEndPoint = e.RemoteEndPoint };
					try
					{
						socketArgs.Completed += new EventHandler<SocketAsyncEventArgs>(Socket_Completed_Receive);
						socketArgs.SetBuffer(e.Buffer, 0, e.Buffer.Length);
						if (!socket.SendAsync(socketArgs))
							Socket_Completed_Receive(socket, socketArgs);
					}
					catch
					{
						socketArgs.Completed -= this.Socket_Completed_Receive;

						throw;
					}
				}
				else
					throw NtpNetworkExceptionFromSocketArgs(e);
			}
			catch (Exception ex)
			{
				OnErrorOccurredAndDisposeSocket(ex, socket);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		private void Socket_Completed_Receive(object sender, SocketAsyncEventArgs e)
		{
			System.Net.Sockets.Socket socket = null; 
			try
			{
				e.Completed -= this.Socket_Completed_Receive;
				socket = ((System.Net.Sockets.Socket)sender); 
				if (e.SocketError == SocketError.Success)
				{
					var buffer = (byte[])e.Buffer;
					var socketArgs = new SocketAsyncEventArgs();
					socketArgs.RemoteEndPoint = e.RemoteEndPoint;

					socketArgs.SetBuffer(buffer, 0, buffer.Length);
					try
					{
						socketArgs.Completed += Socket_Completed_ProcessResult;

						//Sometimes ReceiveAsync doesn't receive any data (UDP packet loss?)
						//which can leave us in a hung state. Setup a 1 second timer
						//here to close the socket, which will cancel the request
						//and raise the completed event with an OperationAbandoned
						//error code if we haven't already completed.
#if SUPPORTS_TASKDELAY
						var waitTask = System.Threading.Tasks.Task.Delay(1000); 
#else
						var waitTask = TaskEx.Delay(1000);
#endif
						waitTask.ContinueWith(
							(pt) =>
							{
								var receiveComplete = (bool)(socketArgs.UserToken ?? false);
								if (!receiveComplete)
									socket?.Dispose();
							}
						);
						if (!socket.ReceiveAsync(socketArgs))
							Socket_Completed_ProcessResult(socket, socketArgs);
					}
					catch
					{
						socketArgs.Completed -= this.Socket_Completed_ProcessResult;
						throw;
					}
				}
				else
					throw NtpNetworkExceptionFromSocketArgs(e);
			}
			catch (Exception ex)
			{
				OnErrorOccurredAndDisposeSocket(ex, socket);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		private void Socket_Completed_ProcessResult(object sender, SocketAsyncEventArgs e)
		{
			Socket socket = null;
			try
			{
				e.Completed -= this.Socket_Completed_ProcessResult;
				socket = ((System.Net.Sockets.Socket)sender);

				if (e.SocketError == SocketError.Success)
					ConvertBufferToCurrentTime(e.Buffer);
				else
				{
					OnErrorOccurred(NtpNetworkExceptionFromSocketArgs(e));
				}
			}
			catch (Exception ex)
			{
				OnErrorOccurred(ex);
			}
			finally
			{
				socket?.Dispose();
			}
		}

#if REQUIRES_IPENDPOINT
		private EndPoint ServerAddressToIPEndpoint(string serverAddress, int portNumber)
		{
			IPAddress ipAddress = null;
			if (IPAddress.TryParse(serverAddress, out ipAddress))
				return new IPEndPoint(ipAddress, portNumber);

			var addresses = Dns.GetHostAddresses(serverAddress);

			if (!(addresses?.Any() ?? false)) throw new NtpNetworkException("DNS failure for " + serverAddress);

			return new IPEndPoint(addresses.First(), portNumber);
		}
#endif

		private static NtpNetworkException NtpNetworkExceptionFromSocketArgs(SocketAsyncEventArgs e)
		{
			return new NtpNetworkException(e.SocketError.ToString(), (int)e.SocketError);
		}

		private void OnErrorOccurredAndDisposeSocket(Exception exception, Socket socket)
		{
			try
			{
				OnErrorOccurred(exception);
			}
			finally
			{
				socket?.Dispose();
			}
		}

	}
}
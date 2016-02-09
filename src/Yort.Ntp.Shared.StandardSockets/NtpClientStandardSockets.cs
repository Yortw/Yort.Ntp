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
			byte[] buffer = new byte[48];
			buffer[0] = 0x1B;

			DnsEndPoint _endPoint = new DnsEndPoint(_ServerAddress, 123, AddressFamily.InterNetwork);
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
									socket?.Close();
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
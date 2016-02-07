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
			var socketArgs = new SocketAsyncEventArgs() { RemoteEndPoint = _endPoint };
			try
			{
				var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
				try
				{
					socketArgs.Completed += Socket_Completed_SendAgain;
					//TFW - For some reason 'ConnectAsync' reports an error
					//desktop .Net 4, but 'Connect' works fine. On WP
					//only ConnectAsync is available, and it appears to work.
#if USE_CONNECTASYNC
					socketArgs.SetBuffer(buffer, 0, buffer.Length);
					socket.ConnectAsync(socketArgs);
#else
					socket.Connect(socketArgs.RemoteEndPoint);
					socketArgs.SetBuffer(buffer, 0, buffer.Length);
					socket.SendAsync(socketArgs);
#endif
				}
				catch
				{
					socket?.Dispose();
					throw;
				}
			}
			catch
			{
				socketArgs.Completed -= this.Socket_Completed_SendAgain;
				throw;
			}
		}

		private void Socket_Completed_SendAgain(object sender, SocketAsyncEventArgs e)
		{
			e.Completed -= this.Socket_Completed_SendAgain;

			var socket = ((System.Net.Sockets.Socket)sender);
			try
			{
				if (e.SocketError == SocketError.Success)
				{
					var socketArgs = new SocketAsyncEventArgs() { RemoteEndPoint = e.RemoteEndPoint };
					try
					{
						socketArgs.Completed += new EventHandler<SocketAsyncEventArgs>(Socket_Completed_Receive);
						socketArgs.SetBuffer(e.Buffer, 0, e.Buffer.Length);
						socket.SendAsync(socketArgs);
					}
					catch
					{
						socketArgs.Completed -= this.Socket_Completed_Receive;

						throw;
					}
				}
				else
				{
					OnErrorOccurred(NtpNetworkExceptionFromSocketArgs(e));
					socket?.Dispose();
				}
			}
			catch
			{
				socket?.Dispose();
				throw;
			}
		}

		private void Socket_Completed_Receive(object sender, SocketAsyncEventArgs e)
		{
			e.Completed -= this.Socket_Completed_Receive;

			var socket = ((System.Net.Sockets.Socket)sender);
			try
			{
				if (e.SocketError == SocketError.Success)
				{
					var buffer = (byte[])e.Buffer;
					var socketArgs = new SocketAsyncEventArgs();
					socketArgs.RemoteEndPoint = e.RemoteEndPoint;

					socketArgs.SetBuffer(buffer, 0, buffer.Length);
					try
					{
						socketArgs.Completed += Socket_Completed_ProcessResult;
						socket.ReceiveAsync(socketArgs);
					}
					catch
					{
						socketArgs.Completed -= this.Socket_Completed_ProcessResult;
						throw;
					}
				}
				else
				{
					OnErrorOccurred(NtpNetworkExceptionFromSocketArgs(e));
					socket?.Dispose();
				}
			}
			catch
			{
				socket?.Dispose();
				throw;
			}
		}

		private void Socket_Completed_ProcessResult(object sender, SocketAsyncEventArgs e)
		{
			e.Completed -= this.Socket_Completed_ProcessResult;

			var socket = ((System.Net.Sockets.Socket)sender);
			try
			{
				if (e.SocketError == SocketError.Success)
					ConvertBufferToCurrentTime(e.Buffer);
				else
					OnErrorOccurred(NtpNetworkExceptionFromSocketArgs(e));
			}
			catch
			{
				socket?.Dispose();
				throw;
			}
		}

		private static NtpNetworkException NtpNetworkExceptionFromSocketArgs(SocketAsyncEventArgs e)
		{
			return new NtpNetworkException(e.SocketError.ToString(), (int)e.SocketError);
		}
	}
}
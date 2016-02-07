using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.IO;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Networking.Sockets;

namespace Yort.Ntp
{
	public partial class NtpClient
	{
		private static readonly TimeSpan OneSecond = TimeSpan.FromSeconds(1);

		async partial void SendTimeRequest()
		{
			var socket = new Windows.Networking.Sockets.DatagramSocket();
			try
			{
				var buffer = new byte[48];
				buffer[0] = 0x1B;

				socket.MessageReceived += Socket_Completed_Receive;
				var asyncResult = new AsyncUdpResult(socket);
				await socket.BindEndpointAsync(new Windows.Networking.HostName(_ServerAddress), "123").AsTask().ContinueWith(
					async (pt) =>
					{
						await socket.ConnectAsync(new Windows.Networking.HostName(_ServerAddress), "123").AsTask().ConfigureAwait(false);
						using (var udpWriter = new DataWriter(socket.OutputStream))
						{
							udpWriter.WriteBytes(buffer);
							await udpWriter.StoreAsync().AsTask().ConfigureAwait(false);

							udpWriter.WriteBytes(buffer);
							await udpWriter.StoreAsync().AsTask().ConfigureAwait(false);

							asyncResult.Wait(OneSecond);
						}
					}
				).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				socket.MessageReceived -= this.Socket_Completed_Receive;
				socket?.Dispose();

				OnErrorOccurred(ExceptionToNtpNetworkException(ex));
			}
		}

		private void Socket_Completed_Receive(Windows.Networking.Sockets.DatagramSocket sender, Windows.Networking.Sockets.DatagramSocketMessageReceivedEventArgs args)
		{
			try
			{
				sender.MessageReceived -= this.Socket_Completed_Receive;

				byte[] buffer = null;
				using (var reader = args.GetDataReader())
				{
					buffer = new byte[reader.UnconsumedBufferLength];
					reader.ReadBytes(buffer);
				}

				ConvertBufferToCurrentTime(buffer);
			}
			catch (Exception ex)
			{
				OnErrorOccurred(ExceptionToNtpNetworkException(ex));
			}
		}

		private static NtpNetworkException ExceptionToNtpNetworkException(Exception ex)
		{
			return new NtpNetworkException(ex.Message, (int)SocketError.GetStatus(ex.HResult), ex);
		}

		private class AsyncUdpResult
		{
			private DatagramSocket _Socket;
			private System.Threading.AutoResetEvent _DataArrivedSignal;

			public AsyncUdpResult(DatagramSocket socket)
			{	
				_Socket = socket;
				_Socket.MessageReceived += _Socket_MessageReceived;
				_DataArrivedSignal = new System.Threading.AutoResetEvent(false);
			}

			private void _Socket_MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
			{
				_DataArrivedSignal.Set();
			}

			public void Wait(TimeSpan timeout)
			{
				_DataArrivedSignal.WaitOne(timeout);
			}
		}
	}
}
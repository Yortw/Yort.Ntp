using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yort.Ntp
{
	public class NtpClient
	{

		#region Fields

		private string _ServerAddress;
		private Socket _Socket;

		#endregion

		#region Events

		/// <summary>
		/// Raised when a new time is received from an NTP server.
		/// </summary>
		/// <seealso cref="NtpTimeReceivedEventArgs"/>
		/// <seealso cref="OnTimeReceived(DateTime)"/>
		public event EventHandler<NtpTimeReceivedEventArgs> TimeReceived;

		#endregion

		#region Constructors

		/// <summary>
		/// Default constructor. Uses the <see cref="KnownNtpServers.TimeANist"/> server as a default.
		/// </summary>
		public NtpClient() : this(KnownNtpServers.TimeANist)
		{
		}

		/// <summary>
		/// Full constructor.
		/// </summary>
		/// <param name="serverAddress">the name or address the NTP server to be used.</param>
		public NtpClient(string serverAddress)
		{
			if (serverAddress == null) throw new ArgumentNullException(nameof(serverAddress));
			if (String.IsNullOrWhiteSpace(serverAddress)) throw new ArgumentException(nameof(serverAddress) + " cannot be empty or whitespace.", nameof(serverAddress));

			_ServerAddress = serverAddress;
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Connects to the server and begins listening for time updates.
		/// </summary>
		public void BeginRequestTime()
		{
			byte[] buffer = new byte[48];
			buffer[0] = 0x1B;
			for (var i = 1; i < buffer.Length; ++i)
			{
				buffer[i] = 0;
			}

			var endPoint = new DnsEndPoint(_ServerAddress, 123);

			_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			var socketArgs = new SocketAsyncEventArgs() { RemoteEndPoint = endPoint };
			socketArgs.Completed += (o, e) =>
			{
				if (e.SocketError == SocketError.Success)
				{
					var sArgs = new SocketAsyncEventArgs()
					{
						RemoteEndPoint = endPoint
					};

					sArgs.Completed += new EventHandler<SocketAsyncEventArgs>(sArgs_Completed);
					sArgs.SetBuffer(buffer, 0, buffer.Length);
					sArgs.UserToken = buffer;
					_Socket.SendAsync(sArgs);
				}
			};
			_Socket.ConnectAsync(socketArgs);
		}

		/// <summary>
		/// Raisese the <seealso cref="TimeReceived"/> event.
		/// </summary>
		/// <param name="time">The date and time received from the NTP server.</param>
		protected void OnTimeReceived(DateTime time)
		{
			TimeReceived?.Invoke(this, new NtpTimeReceivedEventArgs(time));
		}

		#endregion

		#region Event Handlers

		private void sArgs_Completed(object sender, SocketAsyncEventArgs e)
		{
			if (e.SocketError == SocketError.Success)
			{
				var buffer = (byte[])e.Buffer;
				var sArgs = new SocketAsyncEventArgs();
				sArgs.RemoteEndPoint = e.RemoteEndPoint;

				sArgs.SetBuffer(buffer, 0, buffer.Length);
				sArgs.Completed += (o, a) =>
				{
					if (a.SocketError == SocketError.Success)
					{
						var timeData = a.Buffer;

						ConvertBufferToCurrentTime(buffer);
					}
				};
				_Socket.ReceiveAsync(sArgs);
			}
		}

		private void ConvertBufferToCurrentTime(byte[] buffer)
		{
			ulong hTime = 0, lTime = 0;

			for (var i = 40; i <= 43; ++i)
			{
				hTime = hTime << 8 | buffer[i];
			}
			for (var i = 44; i <= 47; ++i)
			{
				lTime = lTime << 8 | buffer[i];
			}

			ulong milliseconds = (hTime * 1000 + (lTime * 1000) / 0x100000000L);

			var timeSpan = TimeSpan.FromTicks((long)milliseconds * TimeSpan.TicksPerMillisecond);
			var currentTime = new DateTime(1900, 1, 1) + timeSpan;

			OnTimeReceived(currentTime);
		}

		#endregion

	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yort.Ntp
{
	/// <summary>
	/// Use instances of this class to request an up to date, accurate time from an NTP server.
	/// </summary>
	public partial class NtpClient
	{

		#region Fields

		private readonly string _ServerAddress;

		private static readonly DateTime NtpEpoch = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		#endregion

		#region Events

		/// <summary>
		/// Raised when a new time is received from an NTP server.
		/// </summary>
		/// <seealso cref="NtpTimeReceivedEventArgs"/>
		/// <seealso cref="OnTimeReceived(DateTime, DateTime)"/>
		public event EventHandler<NtpTimeReceivedEventArgs> TimeReceived;

		/// <summary>
		/// Raised when an error occurs trying to request an updated time from an NTP server.
		/// </summary>
		/// <remarks>
		/// <para>The <see cref="NtpNetworkErrorEventArgs.Exception"/> property will usually contain a <see cref="NtpNetworkException"/>, indicating the library is working properly but an error (probably network related) occurred. Other exceptions types are possible, and *may* indicate a bug or poor exception handling within the library.</para>
		/// </remarks>
		/// <seealso cref="NtpNetworkErrorEventArgs"/>
		/// <seealso cref="NtpNetworkException"/>
		/// <seealso cref="OnErrorOccurred(Exception)"/>
		public event EventHandler<NtpNetworkErrorEventArgs> ErrorOccurred;

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

		#region Public Properties

		/// <summary>
		/// Returns the address of the NTP server this client obtains times from.
		/// </summary>
		/// <remarks>
		/// <para>The server address used is provided via the constructor.</para>
		/// </remarks>
		/// <seealso cref="NtpClient()"/>
		/// <seealso cref="NtpClient(string)"/>
		public string ServerAddress
		{
			get
			{
				return _ServerAddress;
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Asynchronously requests a time from the NTP server specified in the constructor. When a time is received the <seealso cref="TimeReceived" /> event is raised with the result, otherwise the <seealso cref="ErrorOccurred"/> event should be raised containing details of the failure.
		/// </summary>
		/// <remarks>
		/// <para>Note, events raised by this class may not (and probably will not) occur on the same thread that called this method. If the event handlers call UI components, dispatched invoke may be required.</para>
		/// <para>This method may throw exceptions (most likely a <seealso cref="NtpNetworkException"/> if an error occurs trying to connect/bind to the network endpoint. Exception handling in client code is recommended.</para>
		/// </remarks>
		/// <seealso cref="NtpNetworkException"/>
		/// <seealso cref="OnTimeReceived(DateTime, DateTime)"/>
		/// <seealso cref="OnErrorOccurred(Exception)"/>
		public void BeginRequestTime()
		{
			SendTimeRequest();
		}

#if SUPPORTS_TASKASYNC
		/// <summary>
		/// Returns an awaitable task whose result is the current time from the NTP server specified in the constructor.
		/// </summary>
		/// <remarks>
		/// <para>This method may throw exceptions (most likely a <seealso cref="NtpNetworkException"/> if an error occurs trying to connect/bind to the network endpoint. Exception handling in client code is recommended.</para>
		/// </remarks>
		/// <seealso cref="NtpNetworkException"/>
		public System.Threading.Tasks.Task<RequestTimeResult> RequestTimeAsync()
		{
			var tcs = new System.Threading.Tasks.TaskCompletionSource<RequestTimeResult>();
			var client = new NtpClient(_ServerAddress);

			var timeReceivedHandler = new EventHandler<NtpTimeReceivedEventArgs>(
				(sender, args) => 
				{
					tcs.SetResult(new RequestTimeResult(args.CurrentTime, args.SysTime));
				}
			);
			var errorOccurredHandler = new EventHandler<NtpNetworkErrorEventArgs>(
				(sender, args) => 
				{
					if (!tcs.Task.IsCanceled && !tcs.Task.IsCompleted)
						tcs.SetException(args.Exception);
				}
			);

			client.TimeReceived += timeReceivedHandler;
			client.ErrorOccurred += errorOccurredHandler;

			var retVal = tcs.Task;
			tcs.Task.ContinueWith(
				(pt) =>
				{
					client.TimeReceived -= timeReceivedHandler;
					client.ErrorOccurred -= errorOccurredHandler;
				}
			);

			client.BeginRequestTime();

			return retVal;
		}

#endif

		/// <summary>
		/// Raises the <seealso cref="TimeReceived"/> event.
		/// </summary>
		/// <remarks>
		/// <para>This event may be raised on a different thread than called the <see cref="BeginRequestTime"/> method. If the event handler refers to UI, COM or other components that require thread affinity then dispatched invoke may be required.</para>
		/// <para>The time returned is a UTC time.</para>
		/// </remarks>
		/// <param name="ntpTime">The date and time received from the NTP server.</param>
		/// <param name="sysTime">The (UTC) date and time of the system upon reception.</param>
		/// <seealso cref="TimeReceived"/>
		protected void OnTimeReceived(DateTime ntpTime, DateTime sysTime)
		{
			ExecuteWithSuppressedExceptions(() =>
			{
				TimeReceived?.Invoke(this, new NtpTimeReceivedEventArgs(ntpTime, sysTime));
			});
		}

		/// <summary>
		/// Raises the <see cref="ErrorOccurred"/> event.
		/// </summary>
		/// <remarks>
		/// <para>This event may be raised on a different thread than called the <see cref="BeginRequestTime"/> method. If the event handler refers to UI, COM or other components that require thread affinity then dispatched invoke may be required.</para>
		/// </remarks>
		/// <param name="exception">A <see cref="System.Exception"/> derived instance describing the error.</param>
		protected virtual void OnErrorOccurred(Exception exception)
		{
			ExecuteWithSuppressedExceptions(() =>
			{
				ErrorOccurred?.Invoke(this, new NtpNetworkErrorEventArgs(exception));
			});
		}

		#endregion

		#region Private/Partial Methods

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		partial void SendTimeRequest();

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
			if (milliseconds == 0)
			{
				OnErrorOccurred(new NtpNetworkException("Incomplete or invalid data received."));
				return;
			}

			var timeSpan = TimeSpan.FromMilliseconds(milliseconds);
			var currentTime = NtpEpoch + timeSpan;

			OnTimeReceived(currentTime, DateTime.UtcNow);
		}

		/// <summary>
		/// Executes a delegate and suppresses any non-fatal exceptions thrown.
		/// </summary>
		/// <param name="work"></param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		protected static void ExecuteWithSuppressedExceptions(Action work)
		{
			if (work == null) throw new ArgumentNullException(nameof(work));

			try
			{
				work();
			}
			catch (OutOfMemoryException) { throw; }
			catch { }
		}

		#endregion

	}
}
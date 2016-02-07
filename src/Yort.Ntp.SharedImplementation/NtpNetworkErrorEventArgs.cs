using System;
using System.Collections.Generic;
using System.Text;

namespace Yort.Ntp
{
	/// <summary>
	/// Event arguments for the <see cref="NtpClient.ErrorOccurred"/> event, containing details of the error that occurred.
	/// </summary>
	public class NtpNetworkErrorEventArgs : EventArgs
	{
		private readonly NtpNetworkException _Exception;

		/// <summary>
		/// Full constructor.
		/// </summary>
		/// <param name="exception">A <see cref="Exception"/> containing details of the network or socket error that occurred.</param>
		public NtpNetworkErrorEventArgs(NtpNetworkException exception)
		{
			_Exception = exception;
		}

		/// <summary>
		/// Returns a <see cref="Exception"/> containing details of the network or socket error that occurred.
		/// </summary>
		public NtpNetworkException Exception
		{
			get
			{
				return _Exception;
			}
		}
	}
}
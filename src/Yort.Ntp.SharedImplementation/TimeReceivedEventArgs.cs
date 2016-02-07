using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yort.Ntp
{
	/// <summary>
	/// Event arguments for the <see cref="NtpClient.TimeReceived"/> event, providing the updated time.
	/// </summary>
	public class NtpTimeReceivedEventArgs : EventArgs
	{
		private readonly DateTime _CurrentTime;

		/// <summary>
		/// Full constructor.
		/// </summary>
		/// <param name="currentTime">The date and time just received from the NTP server.</param>
		public NtpTimeReceivedEventArgs(DateTime currentTime)
		{
			_CurrentTime = currentTime;
		}

		/// <summary>
		/// Returns the (UTC) time returned by the NTP server.
		/// </summary>
		public DateTime CurrentTime
		{
			get
			{
				return _CurrentTime;
			}
		}
	}
}
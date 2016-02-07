using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Yort.Ntp.Net40.Tests
{
	//TODO: Really need more and better tests, but not sure how or what.

	[TestClass]
	public class NtpClientTests
	{
		private System.Threading.AutoResetEvent _GotResultSignal;
		private DateTime? _Result;

		[TestMethod]
		public void NtpClient_DefaultServer_GetsNonNullResponse()
		{
			_GotResultSignal = new System.Threading.AutoResetEvent(false);
			var client = new Yort.Ntp.NtpClient();
			try
			{
				client.TimeReceived += Client_TimeReceived;
				client.ErrorOccurred += Client_ErrorOccurred;
				client.BeginRequestTime();
				_GotResultSignal.WaitOne(1000);
				Assert.AreNotEqual(null, _Result);
			}
			finally
			{
				client.TimeReceived -= this.Client_TimeReceived;
				client.ErrorOccurred -= this.Client_ErrorOccurred;
			}
		}

		private void Client_ErrorOccurred(object sender, NtpNetworkErrorEventArgs e)
		{
			_GotResultSignal.Set();
		}

		private void Client_TimeReceived(object sender, NtpTimeReceivedEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine(e.CurrentTime);
			_Result = e.CurrentTime;
			_GotResultSignal.Set();
		}
	}
}
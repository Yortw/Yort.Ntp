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
		[TestCategory("NetworkRequiredTests")]
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
				Assert.IsNotNull(_Result);
			}
			finally
			{
				client.TimeReceived -= this.Client_TimeReceived;
				client.ErrorOccurred -= this.Client_ErrorOccurred;
			}
		}

		[TestMethod]
		[TestCategory("NetworkRequiredTests")]
		public void NtpClient_DefaultServer_GetsValidResponsesOverMultipleRequests()
		{
			var ntpEpoch = new DateTime(1900, 01, 01, 0, 0, 0, DateTimeKind.Utc);
			_GotResultSignal = new System.Threading.AutoResetEvent(false);
			var client = new Yort.Ntp.NtpClient();
			try
			{
				client.TimeReceived += Client_TimeReceived;
				client.ErrorOccurred += Client_ErrorOccurred;
				for (int cnt = 0; cnt < 60; cnt++)
				{
					client.BeginRequestTime();
					_GotResultSignal.WaitOne(2000);
					Assert.IsNotNull(_Result);
					Assert.AreNotEqual(ntpEpoch, _Result);
				}
			}
			finally
			{
				client.TimeReceived -= this.Client_TimeReceived;
				client.ErrorOccurred -= this.Client_ErrorOccurred;
			}
		}

		private void Client_ErrorOccurred(object sender, NtpNetworkErrorEventArgs e)
		{
			Assert.Fail(e.Exception.Message);
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
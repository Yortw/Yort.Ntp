using System;
using NUnit.Framework;


namespace Yort.Ntp.Android.Tests
{
	[TestFixture]
	public class AndroidTests
	{

		private System.Threading.AutoResetEvent _GotResultSignal;
		private DateTime? _Result;

		[SetUp]
		public void Setup() { }


		[TearDown]
		public void Tear() { }

		[Test]
		public void Android_NtpClient_DefaultServer_GetsNonNullResponse()
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System.Threading.Tasks;

namespace Yort.Ntp.WinRT.Tests
{
	//TODO: Really need more and better tests, but not sure how or what.

	[TestClass]
	public class WinRT_NtpTests
	{
		private System.Threading.Tasks.TaskCompletionSource<DateTime?> _GotTimeTaskCompletionSource;

		[TestMethod]
		[TestCategory("NetworkRequiredTests")]
		public async Task WinRT_NtpClient_DefaultServer_GetsNonNullResponse()
		{
			var ntpClient = new Yort.Ntp.NtpClient();
			try
			{
				_GotTimeTaskCompletionSource = new TaskCompletionSource<DateTime?>();

				ntpClient.TimeReceived += ntpClient_TimeReceived;
				ntpClient.ErrorOccurred += NtpClient_ErrorOccurred;
				ntpClient.BeginRequestTime();

				var result = await _GotTimeTaskCompletionSource.Task;
				Assert.AreNotEqual(null, result);
			}
			finally
			{
				ntpClient.TimeReceived -= ntpClient_TimeReceived;
			}
		}

		[TestMethod]
		[TestCategory("NetworkRequiredTests")]
		public async Task WinRT_NtpClient_DefaultServer_GetAsyncReturnsResponse()
		{
			var client = new Yort.Ntp.NtpClient();
			var result = await client.RequestTimeAsync();

			Assert.AreNotEqual(DateTime.Now, result.NtpTime);
			Assert.AreEqual(DateTimeKind.Utc, result.ReceivedAt.Kind);
		}

		[TestMethod]
		public async Task WinRT_NtpClient_DefaultServer_ClientCanBeReused()
		{
			int successCount = 0;
			var client = new Yort.Ntp.NtpClient();
			for (int cnt = 0; cnt < 100; cnt++)
			{
				try
				{
					var result = await client.RequestTimeAsync();
					successCount++;
					Assert.AreNotEqual(DateTime.Now, result.NtpTime);
					await Task.Delay(500);
				}
				catch (NtpNetworkException)
				{
				}
			}
			Assert.AreNotEqual(0, successCount);
		}

		private void NtpClient_ErrorOccurred(object sender, NtpNetworkErrorEventArgs e)
		{
			_GotTimeTaskCompletionSource.SetException(e.Exception);
		}

		private void ntpClient_TimeReceived(object sender, NtpTimeReceivedEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine(e.CurrentTime);
			System.Diagnostics.Debug.WriteLine("NTP Local: " + e.CurrentTime.ToLocalTime().ToString());
			System.Diagnostics.Debug.WriteLine("Local: " + DateTime.Now.ToString());
			_GotTimeTaskCompletionSource.SetResult(e.CurrentTime);
		}
	}

}

namespace TestApp.Net80
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
			var client = new Yort.Ntp.NtpClient();
			var response = await client.RequestTimeAsync();
			Console.WriteLine("NTP Time: " + response.NtpTime.ToString());
			Console.WriteLine("Press enter to exit");
			Console.ReadLine();
		}
	}
}

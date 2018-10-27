# Yort.Ntp.Portable
A cross platform NTP client library for .Net platforms. Allows you to easily retrieve an accurate, current date & time from internet NTP servers.

[![GitHub license](https://img.shields.io/github/license/mashape/apistatus.svg)](https://github.com/Yortw/Yort.Ntp/blob/master/LICENSE.md) 

## Supported Platforms
Currently;

* .Net Framework 4.0+
* .Net Framework 4.5+ **
* Windows Phone Silverlight (8.1+) 
* Xamarin.iOS **
* Xamarin.Android **
* WinRT (Windows Store Apps 8.1) **
* UWP 10+ (Windows 10 Universal Programs) **
* .Net Standard 1.3

** Supports async/await

## Build Status
[![Build status](https://ci.appveyor.com/api/projects/status/ko6t4635hx6rllch?svg=true)](https://ci.appveyor.com/project/Yortw/yort-ntp)

## Available on Nuget

```powershell
    PM> Install-Package Yort.Ntp.Portable
```

[![NuGet Badge](https://buildstats.info/nuget/Yort.Ntp.Portable)](https://www.nuget.org/packages/Yort.Ntp.Portable/)

## Samples
For platforms that support task based async;
```C#
var client = new Yort.Ntp.NtpClient();
var currentTime = await client.RequestTimeAsync();
```

For platforms that do not support async/await or tasks (or if you don't want to use async/await);

```C#
var client = new Yort.Ntp.NtpClient();
client.TimeReceived += Client_TimeReceived;
client.ErrorOccurred += Client_ErrorOccurred;
client.BeginRequestTime();

private void Client_ErrorOccurred(object sender, NtpNetworkErrorEventArgs e)
{
	//TODO: Handle errors here.
}

private void Client_TimeReceived(object sender, NtpTimeReceivedEventArgs e)
{
    //TODO: Use retrieved time here. Time is provided by e.CurrentTime.
	System.Diagnostics.Debug.WriteLine(e.CurrentTime);
}

```



## Attributions
The icon for this project is [Computer Time by Arthur Shlain from the Noun Project](https://thenounproject.com/search/?q=computer+time&i=87580) and is used under the [Creative Commons License](http://creativecommons.org/licenses/by/3.0/us/).

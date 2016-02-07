using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Yort.Ntp
{
	public partial class NtpClient
	{
		partial void SendTimeRequest()
		{
			ExceptionHelper.ThrowYoureDoingItWrong();
		}
	}
}
#if !SUPPORTS_TASKDELAY

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Yort.Ntp
{
	internal static class TaskEx
	{
		public static Task Delay(int milliseconds)
		{
			var tcs = new TaskCompletionSource<object>();
			var timer = new Timer(_ => tcs.SetResult(null), null, milliseconds, System.Threading.Timeout.Infinite);
			return tcs.Task.ContinueWith((pt) => timer.Dispose());
		}
	}
}

#endif
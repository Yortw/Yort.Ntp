using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yort.Ntp
{
	/// <summary>
	/// Provides a set of known NTP server addresses.
	/// </summary>
	public static class KnownNtpServers
	{
		/// <summary>
		/// The server address; pool.ntp.org
		/// </summary>
		public const string PoolOrg = "pool.ntp.org";

		/// <summary>
		/// The server address; asia.pool.ntp.org
		/// </summary>
		public const string Asia = "asia.pool.ntp.org";

		/// <summary>
		/// The server address; europe.pool.ntp.org
		/// </summary>
		public const string Europe = "europe.pool.ntp.org";

		/// <summary>
		/// The server address; north-america.pool.ntp.org
		/// </summary>
		public const string NorthAmericaOrg = "north-america.pool.ntp.org";

		/// <summary>
		/// The server address; oceania.pool.ntp.org
		/// </summary>
		public const string OceaniaOrg = "oceania.pool.ntp.org";

		/// <summary>
		/// The server address; south-america.pool.ntp.org
		/// </summary>
		public const string SouthAmericaOrg = "south-america.pool.ntp.org";

		/// <summary>
		/// The server address; time-a.nist.gov
		/// </summary>
		public const string TimeANist = "time-a.nist.gov";
	}
}

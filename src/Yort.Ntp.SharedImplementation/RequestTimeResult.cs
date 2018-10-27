using System;
using System.Collections.Generic;
using System.Text;

namespace Yort.Ntp
{
    /// <summary>
    /// Contains the result of an asyncronous time request.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public struct RequestTimeResult
    {
        /// <summary>
        /// Time (UTC) received from network server.
        /// </summary>
        public DateTime NtpTime { get; }

        /// <summary>
        /// Time (UTC) of system at reception.
        /// </summary>
        public DateTime SystemTime { get; }

        /// <summary>
        /// Constructs a RequestTimeResult value.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="ntpTime">The date and time received from the NTP server.</param>
        /// <param name="sysTime">The (UTC) date and time of the system upon reception.</param>
        public RequestTimeResult(DateTime ntpTime, DateTime sysTime)
        {
            NtpTime = ntpTime;
            SystemTime = sysTime;
        }

        /// <summary>
        /// Tests for equality between objects.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="obj">The other object to compare against.</param>
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <summary>
        /// Tests for equality between RequestTimeResults.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="other">The other object to compare against.</param>
        public bool Equals(RequestTimeResult other)
        {
            return (NtpTime == other.NtpTime && SystemTime == other.SystemTime);
        }

        /// <summary>
        /// Get the hash code of the object.
        /// </summary>
        /// <remarks>
        /// </remarks>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Tests for equality between two RequestTimeResults.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="left">The first object.</param>
        /// <param name="right">The second object.</param>
        public static bool operator ==(RequestTimeResult left, RequestTimeResult right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Tests for inequality between RequestTimeResults.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="left">The first object.</param>
        /// <param name="right">The second object.</param>
        public static bool operator !=(RequestTimeResult left, RequestTimeResult right)
        {
            return !left.Equals(right);
        }
    }

}
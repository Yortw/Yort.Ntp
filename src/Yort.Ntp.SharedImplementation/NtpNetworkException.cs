using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Yort.Ntp
{
	/// <summary>
	/// Represents a network error that occurred during an NTP operation.
	/// </summary>
#if SUPPORTS_SERIALISATION
	[Serializable]
#endif
	public class NtpNetworkException : Exception
	{

		private readonly int _SocketErrorCode;

		/// <summary>
		/// Default constructor, required by framework. Not recommended for use.
		/// </summary>
		public NtpNetworkException() : this("A network error occurred.") { }
		/// <summary>
		/// Partial constructor. Provides only a text based description of the error.
		/// </summary>
		/// <param name="message">The human readable error message describing the network error.</param>
		public NtpNetworkException(string message) : base(message) { }
		/// <summary>
		/// Partial constructor, recommended. Provides an error message and a socket error code.
		/// </summary>
		/// <param name="message">The human readable error message describing the network error.</param>
		/// <param name="socketErrorCode">An integer specifying a socket error.</param>
		public NtpNetworkException(string message, int socketErrorCode) : this(message)
		{
			_SocketErrorCode = socketErrorCode;
		}
		/// <summary>
		/// Partial constructor, recommended. Provides an error message and a socket error code.
		/// </summary>
		/// <param name="message">The human readable error message describing the network error.</param>
		/// <param name="socketErrorCode">An integer specifying a socket error.</param>
		/// <param name="inner">The original exception that is wrapped by this exception.</param>
		public NtpNetworkException(string message, int socketErrorCode, Exception inner) : this(message, inner)
		{
			_SocketErrorCode = socketErrorCode;
		}
		/// <summary>
		/// Partial constructor, recommended. Provides an error message, a socket error code and a reference to the original exception.
		/// </summary>
		/// <param name="message">The human readable error message describing the network error.</param>
		/// <param name="inner">The original exception that is wrapped by this exception.</param>
		public NtpNetworkException(string message, Exception inner) : base(message, inner) { }

		#if SUPPORTS_SERIALISATION
		/// <summary>
		/// Constructor required for serialisation purposes. Not recommended for direct use.
		/// </summary>
		/// <param name="info">A <see cref="System.Runtime.Serialization.SerializationInfo"/> instance used to deserialise the object.</param>
		/// <param name="context">A <see cref="System.Runtime.Serialization.StreamingContext"/> instance used to deserialise the object.</param>
		protected NtpNetworkException(
		System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
			if (info == null) throw new ArgumentNullException(nameof(info));

			_SocketErrorCode = info.GetInt32("SocketErrorCode");
		}

		/// <summary>
		/// Serialises this object instance.
		/// </summary>
		/// <param name="info">A <see cref="System.Runtime.Serialization.SerializationInfo"/> instance used to serialise the object.</param>
		/// <param name="context">A <see cref="System.Runtime.Serialization.StreamingContext"/> instance used to serialise the object.</param>
		[System.Security.SecurityCritical]
		public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			if (info == null) throw new ArgumentNullException(nameof(info));

			info.AddValue("SocketErrorCode", _SocketErrorCode);

			base.GetObjectData(info, context);
		}
#endif

#pragma warning disable 1574
		/// <summary>
		/// Returns the socket error code (as an integer) for the error that occurred.
		/// </summary>
		/// <remarks>
		/// <para>On platforms that support it, this can be cast to the <see cref="System.Net.Sockets.SocketError"/> enumeration to determine what the error code means.</para>
		/// </remarks>
		public int SocketErrorCode
		{
			get
			{
				return _SocketErrorCode;
			}
		}
#pragma warning restore 1574

	}
}
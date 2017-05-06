using System;
using System.Runtime.Serialization;

namespace SyslogNet.Client
{
#if net46
    [Serializable]
#endif
	public class CommunicationsException : Exception
	{
		public CommunicationsException()
		{
		}

		public CommunicationsException(string message) : base(message)
		{
		}

		public CommunicationsException(string message, Exception inner) : base(message, inner)
		{
		}

#if net46
        protected CommunicationsException(
			SerializationInfo info,
			StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}
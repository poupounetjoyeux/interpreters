using System;

namespace KaraW3B.Interpreters.Models.Exceptions
{
    public sealed class KaraW3BEncodingException : Exception
    {
        public KaraW3BEncodingException(string message) : base(message)
        {
        }
    }
}

using System;

namespace KaraW3B.Interpreters.Models.Exceptions
{
    public sealed class KaraW3BWriterException : Exception
    {
        public KaraW3BWriterException(string message) : base(message)
        {
        }
    }
}

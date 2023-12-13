using System;

namespace NemeChess2.Exceptions
{
    internal class ResponseNullException : Exception
    {
        public ResponseNullException(string? message) : base(message)
        {
        }
    }
}

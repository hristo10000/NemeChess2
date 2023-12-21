using System;

namespace NemeChess2.Exceptions
{
    internal class StreamStoppedUnexpectedlyEception : Exception
    {
        public StreamStoppedUnexpectedlyEception(string? message) : base(message)
        {
        }
    }
}

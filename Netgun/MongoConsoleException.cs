using System;

namespace Netgun
{
    public class MongoConsoleException : Exception
    {
        public MongoConsoleException(string message) : base(message) { }
    }
}
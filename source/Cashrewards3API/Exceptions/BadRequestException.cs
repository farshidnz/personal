using System;

namespace Cashrewards3API.Exceptions
{
    public class BadRequestException : Exception
    {
        public BadRequestException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BadRequestException"/> class.
        /// </summary>
        public BadRequestException() : base()
        {
        }
    }
}
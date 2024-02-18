using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Exceptions
{
    public class ArgumentException : Exception
    {
        public ArgumentException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BadRequestException"/> class.
        /// </summary>
        public ArgumentException() : base()
        {
        }
    }
}

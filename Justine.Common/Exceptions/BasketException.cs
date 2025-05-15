using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Justine.Common.Exceptions
{
    // TODO: Add Logging
    public class BasketException : Exception
    {
        public BasketException(string message) : base(message)
        {
        }

        public BasketException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

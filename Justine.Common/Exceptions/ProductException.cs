using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Justine.Common.Exceptions
{
    // TODO: Add Logging
    public class ProductException : Exception
    {
        public ProductException(string message) : base(message)
        {
        }

        public ProductException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
    
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Justine.Common.Exceptions
{
    // TODO: Add Logging
    public class OrderException : Exception
    {
        public OrderException(string message) : base(message)
        {
        }

        public OrderException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;


namespace lab1_1_net10
{
    public class CurrencyServiceException : Exception
    {
        public CurrencyServiceException(string message) : base(message) { }
        public CurrencyServiceException(string message, Exception innerException) : base(message, innerException) { }
    }
}

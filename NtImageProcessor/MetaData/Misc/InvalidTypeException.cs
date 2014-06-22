using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NtImageProcessor.MetaData.Misc
{
    class InvalidTypeException : Exception
    {
        public InvalidTypeException() { }

        public InvalidTypeException(String message)
            : base(message)
        {
        }
    }
}

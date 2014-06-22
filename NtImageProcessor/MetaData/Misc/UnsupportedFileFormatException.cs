using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NtImageProcessor.MetaData.Misc
{
    class UnsupportedFileFormatException : Exception
    {
        public UnsupportedFileFormatException() { }

        public UnsupportedFileFormatException(String message)
            : base(message)
        {
        }

    }
}

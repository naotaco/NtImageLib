using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NtImageProcessor.MetaData.Misc
{
    public class GpsInformationAlreadyExistsException : Exception
    {
        public GpsInformationAlreadyExistsException() { }

        public GpsInformationAlreadyExistsException(String message)
            : base(message)
        {
        }
    }
}

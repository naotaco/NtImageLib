using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NtImageProcessor.MetaData.Structure
{
    public class UnsignedFraction
    {
        public UnsignedFraction() { }

        public UInt32 Denominator { get; set; }
        public UInt32 Numerator { get; set; }
    }

    public class SignedFraction
    {
        public SignedFraction() { }
        public Int32 Denominator { get; set; }
        public Int32 Numerator { get; set; }
    }
}

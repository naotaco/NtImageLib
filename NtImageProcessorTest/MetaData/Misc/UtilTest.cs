using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NtImageProcessor.MetaData.Misc;
using NtImageProcessor.MetaData.Structure;

namespace NtImageProcessorTest.MetaData.Misc
{
    [TestClass]
    public class UtilTest
    {
        [TestMethod]
        public void IntToByte()
        {
            UInt32 origin = 3;
            var actual = Util.ConvertToByte(origin, 4, false);
            var expected = new byte[] { 0, 0, 0, 3 };

            TestUtil.AreEqual(expected, actual);
            
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kumquat.Utilities.Tests
{
    using Kumquat.Utilities.Attributes;
    using NUnit.Framework;

    [TestFixture]
    public class AttributesTests
    {
        [Test]
        public void EqualsAttributesSuccessTest()
        {
            var attr = new EqualsAttribute("test");
            var output = attr.IsSuccessful("test");

            Assert.AreEqual(true, output);
        }

        [Test]
        public void EqualsAttributesFailTest()
        {
            var attr = new EqualsAttribute("test");
            var output = attr.IsSuccessful("fail");

            Assert.AreEqual(false, output);
        }

        [Test]
        public void NotEmptyAttributesSuccessTest()
        {
            var attr = new NotEmptyAttribute();
            var output = attr.IsSuccessful("test");

            Assert.AreEqual(true, output);
        }

        [Test]
        public void NotEmptyAttributesFailTest()
        {
            var attr = new NotEmptyAttribute();
            var output = attr.IsSuccessful("");

            Assert.AreEqual(false, output);
        }
    }
}

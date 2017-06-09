using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kumquat.Utilities.Tests
{
    using Kumquat.Utilities.Attributes;
    using Kumquat.Utilities.Tests.Model;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using SimpleInjector;

    [TestFixture]
    public class TypeConverterTests
    {
        [Test]
        public void BindToInterfaceTest()
        {
            var item = new TestItem() { TestInt = 12, TestString = "Test" };
            string json = JsonConvert.SerializeObject(item, Formatting.Indented, new TypeConverter<ITestItem>());

            ITestItem t = JsonConvert.DeserializeObject<ITestItem>(json, new TypeConverter<ITestItem>());

            Assert.NotNull(t);
            Assert.AreEqual(12, t.TestInt);
            Assert.AreEqual("Test", t.TestString);
        }

        [Test]
        public void BindToCollectionInterfaceTest()
        {
            var item1 = new TestItem() { TestInt = 12, TestString = "Test" };
            var item2 = new TestItem() { TestInt = 10, TestString = "Test2" };
            var item = new TestItem[] {item1, item2};
            string json = JsonConvert.SerializeObject(item, Formatting.Indented, new TypeConverter<ITestItem>());

            var t = JsonConvert.DeserializeObject<IEnumerable<ITestItem>>(json, new TypeConverter<ITestItem>());

            Assert.NotNull(t);
            CollectionAssert.IsNotEmpty(t);
            Assert.AreEqual(12, t.First().TestInt);
            Assert.AreEqual("Test", t.First().TestString);
        }

        [Test]
        public void BindToIpInterfaceTest()
        {
            var item = new IPSuccessResponse() { Ip = "1.1.1.1" };
            string json = JsonConvert.SerializeObject(item, Formatting.Indented, new TypeConverter<IIPSuccessResponse>());

            IIPSuccessResponse t = JsonConvert.DeserializeObject<IIPSuccessResponse>(json, new TypeConverter<IIPSuccessResponse>());

            Assert.NotNull(t);
            Assert.AreEqual("1.1.1.1", t.Ip);
        }
    }
}

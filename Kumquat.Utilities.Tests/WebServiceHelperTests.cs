using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kumquat.Utilities.Tests
{
    using Kumquat.SAS.SC.Interfaces;
    using Kumquat.Utilities.Tests.Model;
    using Moq;
    using NUnit.Framework;
    using SimpleInjector;

    [TestFixture]
    public class WebServiceHelperTests
    {
        [Test]
        public void NullResponseFailsValidationTest()
        {
            object response = null;
            var logHelper = new Mock<ILogHelper>();
            var helper = new WebServiceHelper(logHelper.Object);
            var output = helper.IsCallSuccessful(response);
            
            Assert.AreEqual(false, output);
        }

        [Test]
        public void NotNullResponsePassesValidationTest()
        {
            object response = "test";
            var logHelper = new Mock<ILogHelper>();
            var helper = new WebServiceHelper(logHelper.Object);
            var output = helper.IsCallSuccessful(response);

            Assert.AreEqual(true, output);
        }

        [Test]
        public void EqualsAttributePassesValidationTest()
        {
            var response = new ValidationSuccessResponse();
            var logHelper = new Mock<ILogHelper>();
            response.Validate = true;
            var helper = new WebServiceHelper(logHelper.Object);
            var output = helper.IsCallSuccessful(response);

            Assert.AreEqual(true, output);
        }

        [Test]
        public void EqualsAttributeFailsValidationTest()
        {
            var response = new ValidationSuccessResponse();
            var logHelper = new Mock<ILogHelper>();
            response.Validate = false;
            var helper = new WebServiceHelper(logHelper.Object);
            var output = helper.IsCallSuccessful(response);

            Assert.AreEqual(false, output);
        }

        [Test]
        public void NotEmptyAttributePassesValidationTest()
        {
            var response = new IPSuccessResponse();
            var logHelper = new Mock<ILogHelper>();
            response.Ip = "1.1.1.1";
            var helper = new WebServiceHelper(logHelper.Object);
            var output = helper.IsCallSuccessful(response);

            Assert.AreEqual(true, output);
        }

        [Test]
        public void NotEmptyAttributeFailsValidationTest()
        {
            var response = new IPSuccessResponse();
            var logHelper = new Mock<ILogHelper>();
            response.Ip = "";
            var helper = new WebServiceHelper(logHelper.Object);
            var output = helper.IsCallSuccessful(response);

            Assert.AreEqual(false, output);
        }

        [Test]
        public void IntegrationIPServiceTest()
        {
            var url = "http://ip.jsontest.com/";
            var logHelper = new Mock<ILogHelper>();
            var helper = new WebServiceHelper(logHelper.Object);
            var result = helper.CallService<IPSuccessResponse, IPFailResponse>(url);

            Assert.NotNull(result);
            Assert.AreEqual(true, result.Success);
        }

        [Test]
        public void IntegrationIPServiceInterfaceTest()
        {
            var url = "http://ip.jsontest.com/";
            var logHelper = new Mock<ILogHelper>();
            var helper = new WebServiceHelper(logHelper.Object);
            var result = helper.CallService<IIPSuccessResponse, IIPFailResponse>(url);

            Assert.NotNull(result);
            Assert.NotNull(result.ResponseItem);
            Assert.AreEqual(true, result.Success);
        }
    }
}

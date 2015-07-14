﻿using System.Xml;
using HmrcTpvsProxy.Domain.Manipulator;
using HmrcTpvsProxy.Domain.Manipulator.Data;
using NUnit.Framework;

namespace HmrcTpvsProxy.Domain.Test.Manipulator
{
    [TestFixture]
    public class HmrcDataManipulatorTest
    {
        [Test]
        [TestCase(RequestType.AR)]
        [TestCase(RequestType.Authorisation)]
        [TestCase(RequestType.NOT)]
        [TestCase(RequestType.Unknown)]
        public void HmrcDataFileIsNotManipulatedForRequestTypeOf(RequestType requestType)
        {
            var testData = GetP9TestData();
            var repository = new EmployeeIdentityRepository();
            var requestTypeResolver = new FakeRequestTypeResolver();
            requestTypeResolver.SetRequestType(requestType);

            var manipulator = new HmrcDataManipulator(repository, requestTypeResolver);

            var alteredData = manipulator.ApplyEmployeeIdentities(testData);

            Assert.AreEqual(testData, alteredData);
        }

        [Test]
        [TestCase(RequestType.P9)]
        [TestCase(RequestType.P6)]
        [TestCase(RequestType.SL1)]
        [TestCase(RequestType.SL2)]
        public void HmrcDataFileIsManipulatedForRequestTypeOf(RequestType requestType)
        {
            var testData = GetP9TestData();
            var repository = new EmployeeIdentityRepository();
            var requestTypeResolver = new FakeRequestTypeResolver();
            requestTypeResolver.SetRequestType(requestType);

            var manipulator = new HmrcDataManipulator(repository, requestTypeResolver);

            var alteredData = manipulator.ApplyEmployeeIdentities(testData);

            Assert.AreNotEqual(testData, alteredData);
        }

        [Test]
        public void HmrcDataOnlyHasNinoAndEmployeeNumberAlteredWhenProcessingP9()
        {
            var testData = GetP9TestData();
            var manipulator = GetManipulator();

            var alteredData = manipulator.ApplyEmployeeIdentities(testData);

            var testDataXml = new XmlDocument();
            testDataXml.LoadXml(testData);
            var testDataNodes = testDataXml.GetElementsByTagName("CodingNoticeP9").Item(0);

            var alteredDataXml = new XmlDocument();
            alteredDataXml.LoadXml(alteredData);
            var alteredDataNodes = alteredDataXml.GetElementsByTagName("CodingNoticeP9").Item(0);

            Assert.AreEqual(testDataNodes["EmployerRef"].InnerText, alteredDataNodes["EmployerRef"].InnerText);
            Assert.AreEqual(testDataNodes["Name"].InnerText, alteredDataNodes["Name"].InnerText);
            Assert.AreEqual(testDataNodes["EffectiveDate"].InnerText, alteredDataNodes["EffectiveDate"].InnerText);
            Assert.AreEqual(testDataNodes["CodingUpdate"].InnerText, alteredDataNodes["CodingUpdate"].InnerText);

            Assert.AreNotEqual(testDataNodes["NINO"].InnerText, alteredDataNodes["NINO"].InnerText);
            Assert.AreNotEqual(testDataNodes["WorksNumber"].InnerText, alteredDataNodes["WorksNumber"].InnerText);
        }

        private string GetP9TestData()
        {
            return TestResponses.P9Response;
        }

        private IHmrcDataManipulator GetManipulator()
        {
            var repository = new EmployeeIdentityRepository();
            var requestTypeResolver = new RequestTypeResolver();

            return new HmrcDataManipulator(repository, requestTypeResolver);
        }

        private class FakeRequestTypeResolver : IRequestTypeResolver
        {
            private RequestType requestType;

            public RequestType GetRequestType(XmlDocument requestXml)
            {
                return requestType;
            }

            public RequestType GetRequestTypeForResponse(XmlDocument responseXml)
            {
                return requestType;
            }

            public void SetRequestType(RequestType requestType)
            {
                this.requestType = requestType;
            }
        }
    }
}

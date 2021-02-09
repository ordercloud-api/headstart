using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using OrderCloud.SDK;

namespace Orchestration.Tests
{
    public class OrderCloudTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void partial_object_maps_to_partial_class()
        {
            var obj = JObject.Parse(@"{ 'Name': 'changed' }").ToObject<PartialSpec>();
            var compare = new PartialSpec() {Name = "changed"};
            Assert.IsTrue(obj.Name == compare.Name);
        }
    }
}

using System;
using System.Diagnostics;
using System.Linq;
using Headstart.Common.Helpers;
using NUnit.Framework;
using OrderCloud.SDK;
using Headstart.Models;
using Headstart.Models.Extended;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json.Linq;

namespace Orchestration.Tests
{
    public class ModelTests
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void ensure_spec_control_type_required()
        {
            var prop = typeof(SpecUI)
                .GetProperty("ControlType")?
                .GetCustomAttributes(typeof(RequiredAttribute), false);
            Assert.IsTrue(prop != null);
        }

        //[Test]
        //public void ensure_spec_ui_xp_required()
        //{
        //    var prop = typeof(OrchestrationSpecXp)
        //        .GetProperty("UI")?
        //        .GetCustomAttributes(typeof(RequiredAttribute), false);
        //    Assert.IsTrue(prop != null);
        //}

        [Test]
        public void ensure_base_orchestration_object_id_required()
        {
            var obj = typeof(IHSObject);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => obj.IsAssignableFrom(p));
            foreach (var type in types)
            {
                var prop = type.GetProperty("ID")?.GetCustomAttributes(typeof(RequiredAttribute), false);
                Assert.IsTrue(prop != null);
            }
        }
    }
}

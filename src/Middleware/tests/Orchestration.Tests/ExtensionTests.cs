using System.Collections;
using NUnit.Framework;
using Headstart.Common.Extensions;
using Headstart.Common.Models;
using Headstart.Models;
using Headstart.Orchestration;

namespace Orchestration.Tests
{
    public class ExtensionTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test, TestCaseSource(typeof(TypeFactory), nameof(TypeFactory.TestCases))]
        public void build_path<T>(RecordType type, T obj) where T : IHSObject
        {
            var model = new OrchestrationObject<T>()
            {
                ClientId = "fake",
                ID = obj.ID,
                Token = "fake",
                Model = obj
            };
            var path = model.BuildPath("supplierid", "clientid");
            Assert.AreEqual(path, $"supplierid/clientid/{obj.Type().ToString().ToLower()}/id");
        }

        [Test, TestCaseSource(typeof(TypeFactory), nameof(TypeFactory.TestCases))]
        public void test_type_deriver<T>(RecordType type, T obj) where T : IHSObject
        {
            Assert.AreEqual(type, obj.Type());
        }
    }

    public class TypeFactory
    {
        public static IEnumerable TestCases
        {
            get
            {
                yield return new TestCaseData(RecordType.Product, new HSProduct()
                {
                    ID = "id"
                });
                yield return new TestCaseData(RecordType.ProductFacet, new HSProductFacet()
                {
                    ID = "id"
                });
                yield return new TestCaseData(RecordType.SpecProductAssignment, new HSSpecProductAssignment()
                {
                    ID = "id"
                });
                yield return new TestCaseData(RecordType.PriceSchedule, new HSPriceSchedule()
                {
                    ID = "id"
                });
                yield return new TestCaseData(RecordType.Spec, new HSSpec()
                {
                    ID = "id"
                });
                yield return new TestCaseData(RecordType.SpecOption, new HSSpecOption()
                {
                    ID = "id"
                });
                yield return new TestCaseData(RecordType.Buyer, new HSBuyer()
                {
                    ID = "id"
                });
                yield return new TestCaseData(RecordType.User, new HSUser()
                {
                    ID = "id"
                });
                yield return new TestCaseData(RecordType.UserGroup, new HSUserGroup()
                {
                    ID = "id"
                });
                yield return new TestCaseData(RecordType.UserGroupAssignment, new HSUserGroupAssignment()
                {
                    ID = "id"
                });
                yield return new TestCaseData(RecordType.CostCenter, new HSCostCenter()
                {
                    ID = "id"
                });
                yield return new TestCaseData(RecordType.Address, new HSAddressBuyer()
                {
                    ID = "id"
                });
                yield return new TestCaseData(RecordType.AddressAssignment, new HSAddressAssignment()
                {
                    ID = "id"
                });
            }
        }
    }
}

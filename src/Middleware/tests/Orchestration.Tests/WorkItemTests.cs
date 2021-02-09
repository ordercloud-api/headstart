using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.IO;
using Headstart.Common.Exceptions;   
using Headstart.Common.Helpers;
using Headstart.Common.Models;
using Headstart.Common.Queries;
using Headstart.Models.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSubstitute;
using NSubstitute.Exceptions;
using NUnit.Framework;
using ordercloud.integrations.library;
using Action = Headstart.Common.Models.Action;

namespace Orchestration.Tests
{
    public class MockObject
    {
        public string ID { get; set; }
        public string Description { get; set; }
        [MaxLength(500), OrchestrationIgnore]
        public string Name { get; set; }
        public MockObjectXp xp { get; set; } = new MockObjectXp();
    }

    public class MockObjectXp
    {
        [OrchestrationIgnore]
        public string ShouldBeIgnored { get; set; }
        public string ShouldBeChanged { get; set; }
        public MockSubObject MockSub { get; set; } = new MockSubObject();  
    }

    public class MockSubObject
    {
        public string ShouldBeChanged { get; set; }
        [OrchestrationIgnore]
        public string ShouldBeIgnored { get; set; }
    }

    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [TestCase("fast/guid/product/id.json")]
        public void construct_workitem(string path)
        {
            var wi = new WorkItem(path);
            Assert.AreEqual("fast", wi.ResourceId);
            Assert.AreEqual(RecordType.Product, wi.RecordType);
            Assert.AreEqual("id", wi.RecordId);
        }

        [TestCase("fast/random/id.json")]
        public void construct_workitem_failure(string path)
        {
            Assert.Throws(typeof(OrchestrationException), () =>
            {
                var wi = new WorkItem(path);
            });
        }

        [Test]
        public void DiffLarge()
        {
            using var currentFile = File.OpenText($"JObjectTests/current.json");
            using var currentReader = new JsonTextReader(currentFile);
            var current = (JObject) JToken.ReadFrom(currentReader);

            using var cacheFile = File.OpenText($"JObjectTests/cache.json");
            using var cacheReader = new JsonTextReader(cacheFile);
            var cache = (JObject)JToken.ReadFrom(cacheReader);

            var wi = new WorkItem()
            {
                Cache = cache,
                Current = current
            };
            wi.Diff = wi.Current.Diff(wi.Cache);
            var action = WorkItemMethods.DetermineAction(wi);
            Assert.IsTrue(action == Action.Patch);
        } 

        [Test, TestCaseSource(typeof(ActionFactory), nameof(ActionFactory.TestCases))]
        public Action determine_action_results(WorkItem wi)
        {
            wi.Diff = wi.Current.Diff(wi.Cache);
            var action = WorkItemMethods.DetermineAction(wi);
            return action;
        }

        [Test, TestCaseSource(typeof(DiffFactory), nameof(DiffFactory.TestCases))]
        public JToken diff_results(WorkItem wi)
        {
            var diff = wi.Current.Diff(wi.Cache);
            return diff;
        }

        [Test, TestCaseSource(typeof(SerializerFactory), nameof(SerializerFactory.TestCases))]
        public void serializer_ignore_results(WorkItem wi)
        {
            var diff = wi.Current.Diff(wi.Cache);
            var customSerializer = diff.ToObject<MockObject>(OrchestrationSerializer.Serializer);
            Assert.IsNotNull(customSerializer.Description);
            Assert.IsNull(customSerializer.Name);
            Assert.IsNotNull(customSerializer.xp.ShouldBeChanged);
            Assert.IsNull(customSerializer.xp.ShouldBeIgnored);
            Assert.IsNull(customSerializer.xp.MockSub.ShouldBeIgnored);
            Assert.IsNotNull(customSerializer.xp.MockSub.ShouldBeChanged);
        }

        [Test, TestCaseSource(typeof(DeletedXpFactory), nameof(DeletedXpFactory.TestCases))]
        public bool has_deleted_xp(WorkItem item)
        {
            var has = item.Cache.HasDeletedXp(item.Current);
            return has;
        }
    }

    
    public class DeletedXpFactory
    {
        public static IEnumerable TestCases
        {
            get
            {
                yield return new TestCaseData(new WorkItem()
                {
                    Current = JObject.Parse(@"{ 'ID': 'id', 'Name': 'name', 'xp': { 'Tax': { 'Category': 'category', 'Code': 'code' }}}"),
                    Cache = JObject.Parse(@"{ 'ID': 'id', 'Name': 'name', 'xp': { 'Tax': { 'Category': 'category', 'Code': 'code', 'Description': 'description' }}}"),
                }).Returns(true);
            }
        }
    }

    public class SerializerFactory
    {
        public static IEnumerable TestCases
        {
            get
            {
                yield return new TestCaseData(new WorkItem()
                {
                    Current = JObject.Parse(
                        @"{ 'ID': 'id', 'Name': 'name is changed', 'Description': 'new value', 'xp': { 'ShouldBeChanged': 'new value', 'ShouldBeIgnored': 'new value', 'MockSub': { 'ShouldBeChanged': 'new category', 'ShouldBeIgnored': 'new value' }}}"),
                    Cache = JObject.Parse(
                        @"{ 'ID': 'id', 'Name': 'name', 'Description': 'old value', 'xp': { 'ShouldBeChanged': 'old value', 'ShouldBeIgnored': 'old value', 'MockSub': { 'ShouldBeChanged': 'old category', 'ShouldBeIgnored': 'old value' }}}"),
                });
            }
        }
    }

    public class DiffFactory
    {
        public static IEnumerable TestCases
        {
            get
            {
                yield return new TestCaseData(new WorkItem()
                {
                    Cache = JObject.Parse(@"{
	'OwnerID': 'FourOver',
	'DefaultPriceScheduleID': '1b9b314d-85f5-4f17-b2dd-ad666ffb4e67',
	'AutoForward': false,
	'ID': '1b9b314d-85f5-4f17-b2dd-ad666ffb4e67',
	'Name': '16PT-ACMATT-5X7',
	'Description': '5\' X 7\' 16PT Matte/Dull Finish Announcement Cards, FLAT - No Scoring',
	'QuantityMultiplier': 1,
	'ShipWeight': 1.0,
	'ShipHeight': 3.0,
	'ShipWidth': 3.0,
	'ShipLength': 9.0,
	'Active': true,
	'SpecCount': 13,
	'VariantCount': 0,
	'ShipFromAddressID': null,
	'Inventory': null,
	'DefaultSupplierID': 'FourOver',
	'xp': {
		'Facets': {
			'category': [
				'Announcement Cards'
			],
			'shape': [
				'Rectangle'
			],
			'size': [
				'5\' x 7\''
			],
			'stock': [
				'16PT C2S'
			],
			'supplier': [
				'4Over'
			],
			'manufacturer': [
				'4Over'
			]
		},
		'IntegrationData': {
			'v': null,
			'sid': '1b9b314d-85f5-4f17-b2dd-ad666ffb4e67',
			'u': 'https://s3.dualstack.us-east-1.amazonaws.com/staticcintas.eretailing.com/images/product/front'
		},
		'Status': 'Draft',
		'HasVariants': false,
		'Note': null,
		'Tax': {
			'Category': 'P0000000',
			'Code': 'SP106914',
			'Description': 'Printing'
		},
		'UnitOfMeasure': {
			'Qty': 1,
			'Unit': 'Box'
		},
		'ProductType': 'Standard',
		'IsResale': false,
		'Accessorials': null,
		'Currency': null
	}
}"),
                    Current = JObject.Parse(@"{
	'OwnerID': 'FourOver',
	'DefaultPriceScheduleID': '1b9b314d-85f5-4f17-b2dd-ad666ffb4e67',
	'AutoForward': false,
	'ID': '1b9b314d-85f5-4f17-b2dd-ad666ffb4e67',
	'Name': '16PT-ACMATT-5X7',
	'Description': '5\' X 7\' 16PT Matte/Dull Finish Announcement Cards, FLAT - No Scoring',
	'QuantityMultiplier': 1,
	'ShipWeight': 1.0,
	'ShipHeight': 3.0,
	'ShipWidth': 3.0,
	'ShipLength': 9.0,
	'Active': true,
	'SpecCount': 13,
	'VariantCount': 0,
	'ShipFromAddressID': null,
	'Inventory': null,
	'DefaultSupplierID': 'FourOver',
	'xp': {
		'Facets': {
			'category': [
				'Announcement Cards'
			],
			'shape': [
				'Rectangle'
			],
			'size': [
				'5\' x 7\''
			],
			'stock': [
				'16PT C2S'
			],
			'supplier': [
				'4Over'
			],
			'manufacturer': [
				'4Over'
			]
		},
		'IntegrationData': {
			'v': null,
			'sid': '1b9b314d-85f5-4f17-b2dd-ad666ffb4e67',
			'u': 'https://s3.dualstack.us-east-1.amazonaws.com/staticcintas.eretailing.com/images/product/front'
		},
		'Status': 'Draft',
		'HasVariants': false,
		'Note': null,
		'Tax': {
			'Category': 'P0000000',
			'Code': 'SP106914',
			'Description': 'Printing'
		},
		'UnitOfMeasure': {
			'Qty': 1,
			'Unit': 'Box'
		},
		'ProductType': 'Standard',
		'IsResale': false,
		'Accessorials': null,
		'Currency': null
	}
}")
                }).Returns(null);
                //yield return new TestCaseData(new WorkItem()
                //{
                //    Cache = JObject.Parse(@"{ 'ID': 'id', 'AutoForward': false}"),
                //    Current = JObject.Parse(@"{ 'ID': 'id', 'AutoForward': true }")
                //}).Returns(JObject.Parse(@"{ 'AutoForward': true }"));

                //yield return new TestCaseData(new WorkItem()
                //{
                //    Cache = JObject.Parse(@"{ 'Token': 'old_token', 'ClientId': 'old_id', 'ID': 'id'}"),
                //    Current = JObject.Parse(@"{ 'Token': 'new_token',  'ClientId': 'new_id','ID': 'id' }")
                //}).Returns(null);

                //yield return new TestCaseData(new WorkItem()
                //{
                //    Cache = JObject.Parse(@"{ 'ID': 'id'}"),
                //    Current = JObject.Parse(@"{ 'ID': 'id' }")
                //}).Returns(null);

                //yield return new TestCaseData(new WorkItem()
                //{
                //    Cache = JObject.Parse(@"{ 'ID': 'id', 'OptionCount': 5 }"),
                //    Current = JObject.Parse(@"{ 'ID': 'id' }")
                //}).Returns(null);

                //yield return new TestCaseData(new WorkItem()
                //{
                //    Cache = JObject.Parse(@"{ 'ID': 'id', 'OptionCount': 5 }"),
                //    Current = JObject.Parse(@"{ 'ID': 'id', 'Required': true }")
                //}).Returns(JObject.Parse(@"{ 'Required': true }"));

                //yield return new TestCaseData(new WorkItem()
                //{
                //    Cache = JObject.Parse(@"{ 'ID': 'id', 'OptionCount': 5, 'xp': { 'key': 'value' }}"),
                //    Current = JObject.Parse(@"{ 'ID': 'id', 'Required': true, 'xp': { 'key': 'change' } }")
                //}).Returns(JObject.Parse(@"{ 'Required': true, 'xp': { 'key': 'change' }}"));

                //yield return new TestCaseData(new WorkItem()
                //{
                //    Cache = JObject.Parse(@"{ 'ID': 'id', 'xp': { 'key': 'value' }}"),
                //    Current = JObject.Parse(@"{ 'ID': 'id', 'xp': { 'key': 'value', 'nested': { 'key': 'value' }}}")
                //}).Returns(JObject.Parse(@"{ 'xp': { 'key': 'value', 'nested': { 'key': 'value' }}}"));

                //yield return new TestCaseData(new WorkItem()
                //{
                //    Cache = JObject.Parse(@"{ 'ID': 'id', 'xp': { 'key': 'value' }}"),
                //    Current = JObject.Parse(@"{ 'ID': 'id', 'xp': { 'key': 'value', 'nested': { 'key': 'value', 'array': ['1','2'] }}}")
                //}).Returns(JObject.Parse(@"{ 'xp': { 'key': 'value', 'nested': { 'key': 'value', 'array': ['1','2'] }}}"));

                //yield return new TestCaseData(new WorkItem()
                //{
                //    Cache = JObject.Parse(@"{ 'ID': 'id', 'xp': { 'key': 'value', 'nested': { 'key': 'value', 'array': ['1','2'] }}}"),
                //    Current = JObject.Parse(@"{ 'ID': 'id', 'xp': { 'key': 'value', 'nested': { 'key': 'value', 'array': ['1','2','3'] }}}")
                //}).Returns(JObject.Parse(@"{ 'xp': { 'key': 'value', 'nested': { 'key': 'value', 'array': ['1','2','3'] }}}"));
            }
        }
    }

    public class ActionFactory
    {
        public static IEnumerable TestCases
        {
            get
            {
                yield return new TestCaseData(new WorkItem()
                {
                    Cache = JObject.Parse(@"{ 'Token': 'old_token', 'ClientId': 'old_id', 'Name': 'name' }"),
                    Current = JObject.Parse(@"{ 'Token': 'new_token', 'ClientId': 'new_id', 'Name': 'name' }"),
                    RecordType = RecordType.Buyer
                }).Returns(Action.Get);

                yield return new TestCaseData(new WorkItem()
                {
                    Cache = JObject.Parse(@"{ 'Name': 'name' }"),
                    Current = JObject.Parse(@"{ 'ID': 'id', 'Name': 'name' }"),
                    RecordType = RecordType.SpecProductAssignment
                }).Returns(Action.Ignore);

                yield return new TestCaseData(new WorkItem()
                {
                    Cache = JObject.Parse(@"{ 'ID': 'id', 'Name': 'name' }"),
                    Current = JObject.Parse(@"{ 'ID': 'id', 'Name': 'name' }"),
                    RecordType = RecordType.SpecProductAssignment
                }).Returns(Action.Ignore);

                // test for cache with xp, current without that results in PATCH
                yield return new TestCaseData(new WorkItem()
                {
                    Cache = JObject.Parse(@"{ 'ID': 'id', 'Name': 'name', 'xp': { 'key': 'value' }}"),
                    Current = JObject.Parse(@"{ 'ID': 'id', 'Name': 'patch'}"),
                    RecordType = RecordType.Product
                }).Returns(Action.Patch);

                yield return new TestCaseData(new WorkItem()
                {
                    Cache = JObject.Parse(@"{ 'ID': 'id', 'OptionCount': 2, 'Name': 'name', 'xp': { 'key': 'value' }}"),
                    Current = JObject.Parse(@"{ 'ID': 'id', 'Name': 'name', 'xp': { 'key': 'updated' }}"),
                    RecordType = RecordType.PriceSchedule
                }).Returns(Action.Patch);

                // cache has additional properties, current has changes
                yield return new TestCaseData(new WorkItem()
                {
                    Cache = JObject.Parse(@"{ 'ID': 'id', 'OptionCount': 5, 'Name': 'name' }"),
                    Current = JObject.Parse(@"{ 'ID': 'id', 'Name': 'patch' }"),
                    RecordType = RecordType.Spec
                }).Returns(Action.Patch);

                // everything the same, except an updated existing property in xp
                yield return new TestCaseData(new WorkItem()
                {
                    Cache = JObject.Parse(@"{ 'ID': 'id', 'Name': 'name', 'xp': { 'key': 'value' }}"),
                    Current = JObject.Parse(@"{ 'ID': 'id', 'Name': 'name', 'xp': { 'key': 'updated' }}"),
                    RecordType = RecordType.SpecOption
                }).Returns(Action.Patch);

                // everything the same, except a missing property in current xp
                yield return new TestCaseData(new WorkItem()
                {
                    Cache = JObject.Parse(@"{ 'ID': 'id', 'Name': 'name', 'xp': { 'key': 'value', 'missing': 'value' }}"),
                    Current = JObject.Parse(@"{ 'ID': 'id', 'Name': 'name', 'xp': { 'key': 'updated' }}"),
                    RecordType = RecordType.PriceSchedule
                }).Returns(Action.Patch);

                // everything the same, except a new property in current xp and missing in cache xp
                yield return new TestCaseData(new WorkItem()
                {
                    Cache = JObject.Parse(@"{ 'ID': 'id', 'Name': 'name', 'xp': { 'key': 'value', 'missing': 'value' }}"),
                    Current = JObject.Parse(@"{ 'ID': 'id', 'Name': 'name', 'xp': { 'key': 'updated', 'extra': 'value' }}"),
                    RecordType = RecordType.ProductFacet
                }).Returns(Action.Patch);

                // base property change, xp with no change
                yield return new TestCaseData(new WorkItem()
                {
                    Cache = JObject.Parse(@"{ 'ID': 'id', 'Name': 'value', 'xp': { 'key': 'value' }}"),
                    Current = JObject.Parse(@"{ 'ID': 'id', 'Name': 'patch', 'xp': { 'key': 'value' }}"),
                    RecordType = RecordType.Product
                }).Returns(Action.Patch);

                // no change from cache with xp
                yield return new TestCaseData(new WorkItem()
                {
                    Cache = JObject.Parse(@"{ 'ID': 'id', 'Name': 'value', 'xp': { 'key': 'value' }}"),
                    Current = JObject.Parse(@"{ 'ID': 'id', 'Name': 'value', 'xp': { 'key': 'value' }}"),
                    RecordType = RecordType.Product
                }).Returns(Action.Get);

                // simple property update on base object with no xp
                yield return new TestCaseData(new WorkItem()
                {
                    Cache = JObject.Parse(@"{ 'ID': 'id', 'Name': 'name' }"),
                    Current = JObject.Parse(@"{ 'ID': 'id', 'Name': 'patch' }"),
                    RecordType = RecordType.Product
                }).Returns(Action.Patch);

                // property change, but not all properties in cache present
                yield return new TestCaseData(new WorkItem()
                {
                    Cache = JObject.Parse(@"{ 'ID': 'id', 'Name': 'name', 'Active': true }"),
                    Current = JObject.Parse(@"{ 'ID': 'id', 'Name': 'patch' }"),
                    RecordType = RecordType.Product
                }).Returns(Action.Patch);

                // property change, but new property not in cache (may not ever be a case since we're caching the OC response)
                yield return new TestCaseData(new WorkItem()
                {
                    Cache = JObject.Parse(@"{ 'ID': 'id', 'Name': 'name' }"),
                    Current = JObject.Parse(@"{ 'ID': 'id', 'Name': 'patch', 'Active': true }"),
                    RecordType = RecordType.Product
                }).Returns(Action.Patch);

                // no change from cache, cache has additional properties
                yield return new TestCaseData(new WorkItem()
                {
                    Cache = JObject.Parse(@"{ 'ID': 'id', 'OptionCount': 5, 'Name': 'name' }"),
                    Current = JObject.Parse(@"{ 'ID': 'id', 'Name': 'name' }"),
                    RecordType = RecordType.Product
                }).Returns(Action.Get);

                // no change from cache
                yield return new TestCaseData(new WorkItem()
                {
                    Cache = JObject.Parse(@"{ 'ID': 'id', 'Name': 'name' }"),
                    Current = JObject.Parse(@"{ 'ID': 'id', 'Name': 'name' }"),
                    RecordType = RecordType.Product
                }).Returns(Action.Get);

                // no cache
                yield return new TestCaseData(new WorkItem()
                {
                    Cache = null,
                    Current = JObject.Parse(@"{ 'ID': 'id', 'Name': 'name' }"),
                    RecordType = RecordType.Product
                }).Returns(Action.Create);
            }
        }
    }
}
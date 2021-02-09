using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Headstart.API.Commands;
using Headstart.Common;
using Headstart.Common.Services.CMS.Models;
using Microsoft.AspNetCore.Http;
using Npoi.Mapper;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NSubstitute;
using NUnit.Framework;
using ordercloud.integrations.easypost;
using ordercloud.integrations.library;
using OrderCloud.SDK;

namespace Orchestration.Tests
{
    public class TemplateTests
    {
        [OneTimeSetUp]
        public void Setup()
        {
            if (File.Exists(RandomFlatFileName)) File.Delete(RandomFlatFileName);
            var mapper = new Mapper();
            mapper.Save($"TemplateTests/{RandomFlatFileName}", RandomFlatProductObjectList(3), "TemplateFlat");

            var products = RandomHydratedProductObjectList(3);
            var hydrated = new Mapper();
            hydrated.Put(products.Select(y => y.Product).ToList(), "Products");
            hydrated.Put(products.Select(z => z.PriceSchedule).ToList(), "PriceSchedules");
            hydrated.Put(products.SelectMany(o => o.SpecOptions).ToList(), "SpecOptions");
            hydrated.Put(products.SelectMany(i => i.Images).ToList(), "Images");
            hydrated.Put(products.SelectMany(a => a.Attachments).ToList(), "Attachments");
            hydrated.Put(products.SelectMany(s => s.Specs).ToList(), "Specs");

            hydrated.Save($"TemplateTests/{RandomRelationalFileName}");
        }

        private TemplateProduct RandomProductObject(string productId)
        {
            var random = new Random();
            return new TemplateProduct()
            {
                Active = true,
                Description = Guid.NewGuid().ToString(),
                ID = productId,
                IsResale = true,
                Name = Guid.NewGuid().ToString(),
                UnitOfMeasure = "uom",
                QuantityMultiplier = random.Next(1, 100),
                ShipHeight = (decimal?)Math.Round(random.NextDouble() * 5, 1, MidpointRounding.AwayFromZero),
                ShipLength = (decimal?)Math.Round(random.NextDouble() * 5, 1, MidpointRounding.AwayFromZero),
                ShipWeight = (decimal?)Math.Round(random.NextDouble() * 5, 1, MidpointRounding.AwayFromZero),
                ShipWidth = (decimal?)Math.Round(random.NextDouble() * 5, 1, MidpointRounding.AwayFromZero),
                TaxCategory = Guid.NewGuid().ToString(),
                TaxCode = Guid.NewGuid().ToString(),
                TaxDescription = Guid.NewGuid().ToString(),
                UnitOfMeasureQty = random.Next(1, 5).ToString()
            };
        }

        private TemplatePriceSchedule RandomPriceScheduleObject(string productId)
        {
            var random = new Random();
            var seed = random.Next(1, 2);
            return new TemplatePriceSchedule()
            {
                ProductID = productId,
                ApplyShipping = true,
                ApplyTax = true,
                ID = Guid.NewGuid().ToString(),
                MaxQuantity = random.Next(1000, 10000),
                Name = Guid.NewGuid().ToString(),
                UseCumulativeQuantity = true,
                RestrictedQuantity = false,
                MinQuantity = random.Next(1, 999),
            };
        }

        private TemplateSpec RandomSpecObject(string productId)
        {
            var random = new Random();
            var seed = random.Next(1, 2);
            return seed == 1 
                ? new TemplateSpec() {
                    ProductID = productId,
                    ID = Guid.NewGuid().ToString(),
                    Name = Guid.NewGuid().ToString(),
                    ListOrder = 0,
                    DefaultValue = Guid.NewGuid().ToString(),
                    Required = false,
                    AllowOpenText = true,
                    DefinesVariant = false
                } 
                : new TemplateSpec()
                {
                    ProductID = productId,
                    ID = Guid.NewGuid().ToString(),
                    Name = Guid.NewGuid().ToString(),
                    ListOrder = 0,
                    DefaultValue = null,
                    Required = true,
                    AllowOpenText = false,
                    DefinesVariant = true
                };
        }

        private TemplateSpecOption RandomSpecOptionObject(string specId)
        {
            return new TemplateSpecOption()
            {
                ID = Guid.NewGuid().ToString(),
                SpecID = specId,
                IsOpenText = false,
                ListOrder = 0,
                PriceMarkupType = PriceMarkupType.NoMarkup,
                PriceMarkup = null,
                Description = Guid.NewGuid().ToString()
            };
        }

        private TemplateAsset RandomTemplateImage(string productId)
        {
            return new TemplateAsset()
            {
                Active = true,
                ProductID = productId,
                Title = Guid.NewGuid().ToString(),
                Url = Guid.NewGuid().ToString(),
                Type = AssetType.Image,
                Tags = new List<string>(),
                FileName = Guid.NewGuid().ToString()
            };
        }

        private TemplateAsset RandomTemplateAttachment(string productId)
        {
            return new TemplateAsset()
            {
                Active = true,
                ProductID = productId,
                Title = Guid.NewGuid().ToString(),
                Url = Guid.NewGuid().ToString(),
                Type = AssetType.Unknown,
                Tags = new List<string>(),
                FileName = Guid.NewGuid().ToString()
            };
        }

        private TemplateProductFlat RandomFlatProductObject()
        {
            var random = new Random();
            return new TemplateProductFlat()
            {
                Type = random.Next(0, 3).To<AssetType>(),
                Active = true,
                ApplyShipping = true,
                ApplyTax = true,
                Description = Guid.NewGuid().ToString(),
                FileName = Guid.NewGuid().ToString(),
                ID = Guid.NewGuid().ToString(),
                ImageTitle = Guid.NewGuid().ToString(),
                IsResale = true,
                MaxQuantity = random.Next(1000, 10000),
                Name = Guid.NewGuid().ToString(),
                UnitOfMeasure = "uom",
                UseCumulativeQuantity = true,
                RestrictedQuantity = false,
                Url = Guid.NewGuid().ToString(),
                MinQuantity = random.Next(1, 999),
                Price = Math.Round(random.NextDouble() * 99, 2, MidpointRounding.AwayFromZero).To<decimal>(),
                QuantityMultiplier = random.Next(1, 100),
                ShipHeight = (decimal?)Math.Round(random.NextDouble() * 5, 1, MidpointRounding.AwayFromZero),
                ShipLength = (decimal?)Math.Round(random.NextDouble() * 5, 1, MidpointRounding.AwayFromZero),
                ShipWeight = (decimal?)Math.Round(random.NextDouble() * 5, 1, MidpointRounding.AwayFromZero),
                ShipWidth = (decimal?)Math.Round(random.NextDouble() * 5, 1, MidpointRounding.AwayFromZero),
                Tags = Guid.NewGuid().ToString(),
                TaxCategory = Guid.NewGuid().ToString(),
                TaxCode = Guid.NewGuid().ToString(),
                TaxDescription = Guid.NewGuid().ToString(),
                UnitOfMeasureQuantity = random.Next(1, 5),
                SizeTier = SizeTier.A
            };
        }

        private List<TemplateProductFlat> RandomFlatProductObjectList(int upper)
        {
            var result = new List<TemplateProductFlat>();
            for (var i = 0; i <= upper; i++)
            {
                result.Add(RandomFlatProductObject());
            }

            return result;
        }

        private List<TemplateHydratedProduct> RandomHydratedProductObjectList(int upper)
        {
            var random = new Random();
            var result = new List<TemplateHydratedProduct>();
            for (var i = 0; i <= upper; i++)
            {
                var productId = Guid.NewGuid().ToString();
                var product = new TemplateHydratedProduct()
                {
                    Product = RandomProductObject(productId),
                    PriceSchedule = RandomPriceScheduleObject(productId),
                    Attachments = new List<TemplateAsset>() { RandomTemplateAttachment(productId) },
                    Images = new List<TemplateAsset>(),
                    Specs = new List<TemplateSpec>(),
                    SpecOptions = new List<TemplateSpecOption>()
                };
                var count = random.Next(1, 10);
                for (var x = 0; x <= count; x++)
                {
                    product.Images.Add(RandomTemplateImage(productId));
                    product.Specs.Add(RandomSpecObject(productId));
                    var options = random.Next(1, 3);
                    for (var y = 0; y <= options; y++)
                    {
                        product.SpecOptions.Add(RandomSpecOptionObject(product.Specs.LastOrDefault()?.ID));
                    }
                }
                result.Add(product);
            }

            return result;
        }

        private const string RandomFlatFileName = "TemplateFlat.xlsx";
        private const string RandomRelationalFileName = "TemplateRelated.xlsx";

        //[TestCase(RandomFlatFileName, "flat")]
        [TestCase(RandomRelationalFileName, "related")]
        public async Task TimerTest(string fileName, string type)
        {
            using var stream = File.Open($"TemplateTests/{fileName}", FileMode.Open);
            var file = Substitute.For<IFormFile>();
            file.OpenReadStream().Returns(stream);
            var command = new ProductTemplateCommand(Substitute.For<AppSettings>());
            var timer = new Stopwatch();
            timer.Start();
            switch (type)
            {
                case "flat":
                    await command.ParseProductTemplateFlat(file, new VerifiedUserContext(new ClaimsPrincipal()));
                    break;
                case "related":
                    await command.ParseProductTemplate(file, new VerifiedUserContext(new ClaimsPrincipal()));
                    break;
            }
            timer.Stop();
            Assert.IsTrue(timer.Elapsed.Minutes < 5, $"Elapsed time for {type}: {timer.Elapsed.Minutes}");
        }

        [TestCase("TemplateSheets.xlsx")]
        public async Task Test(string fileName)
        {
            using var stream = File.Open($"TemplateTests/{fileName}", FileMode.Open);
            IFormFile file = Substitute.For<IFormFile>();
            file.OpenReadStream().Returns(stream);
            var command = new ProductTemplateCommand(Substitute.For<AppSettings>());
            var parsed = await command.ParseProductTemplate(file, new VerifiedUserContext(new ClaimsPrincipal()));
            Assert.IsTrue(parsed.Count == 1); // total product count
            Assert.IsTrue(parsed.FirstOrDefault()?.Product.ID == "example_id");
            // price schedules test
            Assert.IsTrue(parsed.FirstOrDefault()?.PriceSchedule.ID == "example_ps_id");
            Assert.IsTrue(parsed.FirstOrDefault()?.PriceSchedule.ProductID == "example_id");
            // specs test
            Assert.IsTrue(parsed.FirstOrDefault()?.Specs.Count == 2);
            Assert.IsTrue(parsed.FirstOrDefault()?.Specs.All(s => s.ProductID == "example_id"));
            // spec options test
            var spec = parsed.FirstOrDefault()?.Specs.FirstOrDefault(s => s.ID == "example_spec_id_color");
            Assert.IsTrue(parsed.FirstOrDefault()?.SpecOptions.Count == 2);
            // images test
            Assert.IsTrue(parsed.FirstOrDefault()?.Images.Count == 1);
            Assert.IsTrue(parsed.FirstOrDefault()?.Images.FirstOrDefault()?.ProductID == "example_id");
            // attachments test
            Assert.IsTrue(parsed.FirstOrDefault()?.Attachments.Count == 1);
            Assert.IsTrue(parsed.FirstOrDefault()?.Attachments.FirstOrDefault()?.ProductID == "example_id");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Headstart.API.Helpers;
using Headstart.Common.Models;
using Headstart.Integrations.CMS;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.CosmosDB;
using OrderCloud.SDK;

namespace Headstart.API.Commands.Crud
{
    public interface IHSProductCommand
    {
        Task<SuperHSProduct> Get(string id, string token);

        Task<ListPage<SuperHSProduct>> List(ListArgs<HSProduct> args, string token);

        Task<SuperHSProduct> Post(SuperHSProduct product, DecodedToken decodedToken);

        Task<SuperHSProduct> Put(string id, SuperHSProduct product, string token);

        Task Delete(string id, string token);

        Task<HSPriceSchedule> GetPricingOverride(string id, string buyerID, string token);

        Task DeletePricingOverride(string id, string buyerID, string token);

        Task<HSPriceSchedule> UpdatePricingOverride(string id, string buyerID, HSPriceSchedule pricingOverride, string token);

        Task<HSPriceSchedule> CreatePricingOverride(string id, string buyerID, HSPriceSchedule pricingOverride, string token);

        Task<Product> FilterOptionOverride(string id, string supplierID, IDictionary<string, object> facets, DecodedToken decodedToken);
    }

    public class DefaultOptionSpecAssignment
    {
        public string SpecID { get; set; }

        public string OptionID { get; set; }
    }

    public class HSProductCommand : IHSProductCommand
    {
        private readonly IOrderCloudClient oc;
        private readonly AppSettings settings;
        private readonly ISupplierApiClientHelper apiClientHelper;
        private readonly IAssetClient assetClient;

        public HSProductCommand(AppSettings settings, IOrderCloudClient elevatedOc, ISupplierApiClientHelper apiClientHelper, IAssetClient assetClient)
        {
            oc = elevatedOc;
            this.settings = settings;
            this.apiClientHelper = apiClientHelper;
            this.assetClient = assetClient;
        }

        public async Task<HSPriceSchedule> GetPricingOverride(string id, string buyerID, string token)
        {
            var priceScheduleID = $"{id}-{buyerID}";
            var priceSchedule = await oc.PriceSchedules.GetAsync<HSPriceSchedule>(priceScheduleID);
            return priceSchedule;
        }

        public async Task DeletePricingOverride(string id, string buyerID, string token)
        {
            /* must remove the price schedule from the visibility assignments
            * deleting a price schedule with active visibility assignments removes the visbility
            * assignment completely, we want those product to usergroup catalog assignments to remain
            * just without the override */
            var priceScheduleID = $"{id}-{buyerID}";
            await RemovePriceScheduleAssignmentFromProductCatalogAssignments(id, buyerID, priceScheduleID);
            await oc.PriceSchedules.DeleteAsync(priceScheduleID);
        }

        public async Task<HSPriceSchedule> CreatePricingOverride(string id, string buyerID, HSPriceSchedule priceSchedule, string token)
        {
            /* must add the price schedule to the visibility assignments */
            var priceScheduleID = $"{id}-{buyerID}";
            priceSchedule.ID = priceScheduleID;
            var newPriceSchedule = await oc.PriceSchedules.SaveAsync<HSPriceSchedule>(priceScheduleID, priceSchedule);
            await AddPriceScheduleAssignmentToProductCatalogAssignments(id, buyerID, priceScheduleID);
            return newPriceSchedule;
        }

        public async Task<HSPriceSchedule> UpdatePricingOverride(string id, string buyerID, HSPriceSchedule priceSchedule, string token)
        {
            var priceScheduleID = $"{id}-{buyerID}";
            var newPriceSchedule = await oc.PriceSchedules.SaveAsync<HSPriceSchedule>(priceScheduleID, priceSchedule);
            return newPriceSchedule;
        }

        public async Task RemovePriceScheduleAssignmentFromProductCatalogAssignments(string productID, string buyerID, string priceScheduleID)
        {
            var relatedProductCatalogAssignments = await oc.Products.ListAssignmentsAsync(productID: productID, buyerID: buyerID, pageSize: 100);
            await Throttler.RunAsync(relatedProductCatalogAssignments.Items, 100, 5, assignment =>
            {
                return oc.Products.SaveAssignmentAsync(new ProductAssignment
                {
                    BuyerID = buyerID,
                    PriceScheduleID = null,
                    ProductID = productID,
                    UserGroupID = assignment.UserGroupID,
                });
            });
        }

        public async Task AddPriceScheduleAssignmentToProductCatalogAssignments(string productID, string buyerID, string priceScheduleID)
        {
            var relatedProductCatalogAssignments = await oc.Products.ListAssignmentsAsync(productID: productID, buyerID: buyerID, pageSize: 100);
            await Throttler.RunAsync(relatedProductCatalogAssignments.Items, 100, 5, assignment =>
            {
                return oc.Products.SaveAssignmentAsync(new ProductAssignment
                {
                    BuyerID = buyerID,
                    PriceScheduleID = priceScheduleID,
                    ProductID = productID,
                    UserGroupID = assignment.UserGroupID,
                });
            });
        }

        public async Task<SuperHSProduct> Get(string id, string token)
        {
            var product = await oc.Products.GetAsync<HSProduct>(id, token);
            var priceSchedule = new PriceSchedule();
            try
            {
                priceSchedule = await oc.PriceSchedules.GetAsync<PriceSchedule>(product.ID, token);
            }
            catch
            {
                priceSchedule = new PriceSchedule();
            }

            var specs = oc.Products.ListSpecsAsync(id, null, null, null, 1, 100, null, token);
            var variants = oc.Products.ListVariantsAsync<HSVariant>(id, null, null, null, 1, 100, null, token);
            try
            {
                return new SuperHSProduct
                {
                    Product = product,
                    PriceSchedule = priceSchedule,
                    Specs = (await specs).Items,
                    Variants = (await variants).Items,
                };
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<ListPage<SuperHSProduct>> List(ListArgs<HSProduct> args, string token)
        {
            var filterString = args.ToFilterString();
            var productsList = await oc.Products.ListAsync<HSProduct>(
                filters: string.IsNullOrEmpty(filterString) ? null : filterString,
                search: args.Search,
                searchType: SearchType.ExactPhrasePrefix,
                sortBy: args.SortBy.FirstOrDefault(),
                pageSize: args.PageSize,
                page: args.Page,
                accessToken: token);
            var superProductsList = new List<SuperHSProduct> { };
            await Throttler.RunAsync(productsList.Items, 100, 10, async product =>
            {
                var priceSchedule = oc.PriceSchedules.GetAsync(product.DefaultPriceScheduleID, token);
                var specs = oc.Products.ListSpecsAsync(product.ID, null, null, null, 1, 100, null, token);
                var variants = oc.Products.ListVariantsAsync<HSVariant>(product.ID, null, null, null, 1, 100, null, token);
                superProductsList.Add(new SuperHSProduct
                {
                    Product = product,
                    PriceSchedule = await priceSchedule,
                    Specs = (await specs).Items,
                    Variants = (await variants).Items,
                });
            });
            return new ListPage<SuperHSProduct>
            {
                Meta = productsList.Meta,
                Items = superProductsList,
            };
        }

        public async Task<SuperHSProduct> Post(SuperHSProduct superProduct, DecodedToken decodedToken)
        {
            // Determine ID up front so price schedule ID can match
            superProduct.Product.ID = superProduct.Product.ID ?? CosmosInteropID.New();

            // Create Price Schedule
            PriceSchedule priceSchedule = null;

            // All products must have a price schedule for orders to be submitted. The front end provides a default Price of $0 for quote products that don't have one.
            superProduct.PriceSchedule.ID = superProduct.Product.ID;
            try
            {
                priceSchedule = await oc.PriceSchedules.CreateAsync<PriceSchedule>(superProduct.PriceSchedule, decodedToken.AccessToken);
            }
            catch (OrderCloudException ex)
            {
                if (ex.HttpStatus == System.Net.HttpStatusCode.Conflict)
                {
                    throw new Exception($"Product SKU {superProduct.PriceSchedule.ID} already exists.  Please try a different SKU.");
                }
            }

            superProduct.Product.DefaultPriceScheduleID = priceSchedule.ID;

            if (decodedToken.CommerceRole == CommerceRole.Supplier)
            {
                var me = await oc.Me.GetAsync(accessToken: decodedToken.AccessToken);
                var supplierName = await GetSupplierNameForXpFacet(me.Supplier.ID, decodedToken.AccessToken);
                superProduct.Product.xp.Facets.Add("supplier", new List<string>() { supplierName });
            }

            // Create Product
            var product = await oc.Products.CreateAsync<HSProduct>(superProduct.Product, decodedToken.AccessToken);

            // Return the SuperProduct
            return new SuperHSProduct
            {
                Product = product,
                PriceSchedule = priceSchedule,
                Specs = new List<Spec>(),
                Variants = new List<HSVariant>(),
            };
        }

        public async Task<SuperHSProduct> Put(string id, SuperHSProduct superProduct, string token)
        {
            // Update the Product itself
            var updatedProduct = await oc.Products.SaveAsync<HSProduct>(superProduct.Product.ID, superProduct.Product, token);

            // Two spec lists to compare (requestSpecs and existingSpecs)
            IList<Spec> requestSpecs = superProduct.Specs.ToList();
            IList<Spec> existingSpecs = (await oc.Products.ListSpecsAsync(id, accessToken: token)).Items.ToList();

            // Two variant lists to compare (requestVariants and existingVariants)
            IList<HSVariant> requestVariants = superProduct.Variants;
            IList<Variant> existingVariants = (await oc.Products.ListVariantsAsync(id, pageSize: 100, accessToken: token)).Items.ToList();

            // Calculate differences in specs - specs to add, and specs to delete
            var specsToAdd = requestSpecs.Where(s => !existingSpecs.Any(s2 => s2.ID == s.ID)).ToList();
            var specsToDelete = existingSpecs.Where(s => !requestSpecs.Any(s2 => s2.ID == s.ID)).ToList();

            // Get spec options to add -- WAIT ON THESE, RUN PARALLEL THE ADD AND DELETE SPEC REQUESTS
            foreach (var rSpec in requestSpecs)
            {
                foreach (var eSpec in existingSpecs)
                {
                    if (rSpec.ID == eSpec.ID)
                    {
                        await Throttler.RunAsync(rSpec.Options.Where(rso => !eSpec.Options.Any(eso => eso.ID == rso.ID)), 100, 5, o => oc.Specs.CreateOptionAsync(rSpec.ID, o, accessToken: token));
                        await Throttler.RunAsync(eSpec.Options.Where(eso => !rSpec.Options.Any(rso => rso.ID == eso.ID)), 100, 5, o => oc.Specs.DeleteOptionAsync(rSpec.ID, o.ID, accessToken: token));
                    }
                }
            }

            // Create new specs and Delete removed specs
            var defaultSpecOptions = new List<DefaultOptionSpecAssignment>();
            await Throttler.RunAsync(specsToAdd, 100, 5, s =>
            {
                defaultSpecOptions.Add(new DefaultOptionSpecAssignment { SpecID = s.ID, OptionID = s.DefaultOptionID });
                s.DefaultOptionID = null;
                return oc.Specs.SaveAsync<Spec>(s.ID, s, accessToken: token);
            });
            await Throttler.RunAsync(specsToDelete, 100, 5, s => oc.Specs.DeleteAsync(s.ID, accessToken: token));

            // Add spec options for new specs
            foreach (var spec in specsToAdd)
            {
                await Throttler.RunAsync(spec.Options, 100, 5, o => oc.Specs.CreateOptionAsync(spec.ID, o, accessToken: token));
            }

            // Patch Specs with requested DefaultOptionID
            await Throttler.RunAsync(defaultSpecOptions, 100, 10, a => oc.Specs.PatchAsync(a.SpecID, new PartialSpec { DefaultOptionID = a.OptionID }, accessToken: token));

            // Make assignments for the new specs
            await Throttler.RunAsync(specsToAdd, 100, 5, s => oc.Specs.SaveProductAssignmentAsync(new SpecProductAssignment { ProductID = id, SpecID = s.ID }, accessToken: token));
            HandleSpecOptionChanges(requestSpecs, existingSpecs, token);

            // Check if Variants differ
            var variantsAdded = requestVariants.Any(v => !existingVariants.Any(v2 => v2.ID == v.ID));
            var variantsRemoved = existingVariants.Any(v => !requestVariants.Any(v2 => v2.ID == v.ID));
            bool hasVariantChange = false;

            foreach (Variant variant in requestVariants)
            {
                ValidateRequestVariant(variant);
                var currVariant = existingVariants.Where(v => v.ID == variant.ID);
                if (currVariant == null || currVariant.Count() < 1)
                {
                    continue;
                }

                hasVariantChange = HasVariantChange(variant, currVariant.First());
                if (hasVariantChange)
                {
                    break;
                }
            }

            // IF variants differ, then re-generate variants and re-patch IDs to match the user input.
            if (variantsAdded || variantsRemoved || hasVariantChange || requestVariants.Any(v => v.xp.NewID != null))
            {
                // validate variant names before continuing saving.
                await ValidateVariantsAsync(superProduct, token);

                // Re-generate Variants
                await oc.Products.GenerateVariantsAsync(id, overwriteExisting: true, accessToken: token);

                // Patch NEW variants with the User Specified ID (Name,ID), and correct xp values (SKU)
                await Throttler.RunAsync(superProduct.Variants, 100, 5, v =>
                {
                    v.ID = v.xp.NewID ?? v.ID;
                    v.Name = v.xp.NewID ?? v.ID;
                    if (superProduct?.Product?.Inventory?.VariantLevelTracking == true && v.Inventory == null)
                    {
                        v.Inventory = new PartialVariantInventory { QuantityAvailable = 0 };
                    }

                    if (superProduct.Product?.Inventory == null)
                    {
                        // If Inventory doesn't exist on the product, don't patch variants with inventory either.
                        return oc.Products.PatchVariantAsync(id, $"{superProduct.Product.ID}-{v.xp.SpecCombo}", new PartialVariant { ID = v.ID, Name = v.Name, xp = v.xp, Active = v.Active }, accessToken: token);
                    }
                    else
                    {
                        return oc.Products.PatchVariantAsync(id, $"{superProduct.Product.ID}-{v.xp.SpecCombo}", new PartialVariant { ID = v.ID, Name = v.Name, xp = v.xp, Active = v.Active, Inventory = v.Inventory }, accessToken: token);
                    }
                });
            }

            // If applicable, update OR create the Product PriceSchedule
            var tasks = new List<Task>();
            Task<PriceSchedule> priceScheduleReq = null;
            if (superProduct.PriceSchedule != null)
            {
                priceScheduleReq = UpdateRelatedPriceSchedules(superProduct.PriceSchedule, token);
                tasks.Add(priceScheduleReq);
            }

            // List Variants
            var variantsReq = oc.Products.ListVariantsAsync<HSVariant>(id, pageSize: 100, accessToken: token);
            tasks.Add(variantsReq);

            // List Product Specs
            var specsReq = oc.Products.ListSpecsAsync<Spec>(id, accessToken: token);
            tasks.Add(specsReq);

            await Task.WhenAll(tasks);

            return new SuperHSProduct
            {
                Product = updatedProduct,
                PriceSchedule = priceScheduleReq?.Result,
                Specs = specsReq?.Result?.Items,
                Variants = variantsReq?.Result?.Items,
            };
        }

        public async Task Delete(string id, string token)
        {
            var product = await oc.Products.GetAsync<HSProduct>(id);
            var specs = await oc.Products.ListSpecsAsync<Spec>(id, accessToken: token);
            var tasks = new List<Task>()
            {
                Throttler.RunAsync(specs.Items, 100, 5, s => oc.Specs.DeleteAsync(s.ID, accessToken: token)),
                oc.Products.DeleteAsync(id, token),
            };
            if (product?.xp?.Images?.Count() > 0)
            {
                tasks.Add(Throttler.RunAsync(product.xp.Images, 100, 5, i => assetClient.DeleteAssetByUrl(i.Url)));
            }

            if (product?.xp?.Documents != null && product?.xp?.Documents.Count() > 0)
            {
                tasks.Add(Throttler.RunAsync(product.xp.Documents, 100, 5, d => assetClient.DeleteAssetByUrl(d.Url)));
            }

            // Delete images, attachments, and assignments associated with the requested product
            await Task.WhenAll(tasks);
        }

        public async Task<Product> FilterOptionOverride(string id, string supplierID, IDictionary<string, object> facets, DecodedToken decodedToken)
        {
            ApiClient supplierClient = await apiClientHelper.GetSupplierApiClient(supplierID, decodedToken.AccessToken);
            if (supplierClient == null)
            {
                throw new Exception($"Default supplier client not found. SupplierID: {supplierID}");
            }

            var configToUse = new OrderCloudClientConfig
            {
                ApiUrl = decodedToken.ApiUrl,
                AuthUrl = decodedToken.AuthUrl,
                ClientId = supplierClient.ID,
                ClientSecret = supplierClient.ClientSecret,
                GrantType = GrantType.ClientCredentials,
                Roles = new[]
                    {
                        ApiRole.SupplierAdmin,
                        ApiRole.ProductAdmin,
                    },
            };
            var ocClient = new OrderCloudClient(configToUse);
            await ocClient.AuthenticateAsync();
            var token = ocClient.TokenResponse.AccessToken;

            // Format the facet data to change for request body
            var facetDataFormatted = new ExpandoObject();
            var facetDataFormattedCollection = (ICollection<KeyValuePair<string, object>>)facetDataFormatted;
            foreach (var kvp in facets)
            {
                facetDataFormattedCollection.Add(kvp);
            }

            dynamic facetDataFormattedDynamic = facetDataFormatted;

            // Update the product with a supplier token
            var updatedProduct = await ocClient.Products.PatchAsync(
                id,
                new PartialProduct() { xp = new { Facets = facetDataFormattedDynamic } },
                accessToken: token);
            return updatedProduct;
        }

        private async Task ValidateVariantsAsync(SuperHSProduct superProduct, string token)
        {
            List<Variant> allVariants = new List<Variant>();
            if (superProduct.Variants == null || !superProduct.Variants.Any())
            {
                return;
            }

            try
            {
                var allProducts = await oc.Products.ListAllAsync(accessToken: token);

                if (allProducts == null || !allProducts.Any())
                {
                    return;
                }

                foreach (Product product in allProducts)
                {
                    if (product.VariantCount > 0 && product.ID != superProduct.Product.ID)
                    {
                        allVariants.AddRange((await oc.Products.ListVariantsAsync(productID: product.ID, pageSize: 100, accessToken: token)).Items);
                    }
                }
            }
            catch (Exception)
            {
                return;
            }

            foreach (Variant variant in superProduct.Variants)
            {
                if (!allVariants.Any())
                {
                    return;
                }

                List<Variant> duplicateSpecNames = allVariants.Where(currVariant => IsDifferentVariantWithSameName(variant, currVariant)).ToList();
                if (duplicateSpecNames.Any())
                {
                    throw new Exception($"{duplicateSpecNames.First().ID} already exists on a variant. Please use unique names for SKUS and try again.");
                }
            }
        }

        private bool IsDifferentVariantWithSameName(Variant variant, Variant currVariant)
        {
            // Do they have the same SKU
            if (variant.xp.NewID == currVariant.ID)
            {
                if (variant.xp.SpecCombo == currVariant.xp.SpecCombo)
                {
                    // It's most likely the same variant
                    return false;
                }

                return true;
            }

            return false;
        }

        private void ValidateRequestVariant(Variant variant)
        {
            if (variant.xp.NewID == variant.ID)
            {
                // If NewID is same as ID, no changes have happened so NewID shouldn't be populated.
                variant.xp.NewID = null;
            }
        }

        private async Task<PriceSchedule> UpdateRelatedPriceSchedules(PriceSchedule updated, string token)
        {
            var ocAuth = await oc.AuthenticateAsync();
            var initial = await oc.PriceSchedules.GetAsync(updated.ID);
            if (initial.MaxQuantity != updated.MaxQuantity ||
                initial.MinQuantity != updated.MinQuantity ||
                initial.UseCumulativeQuantity != updated.UseCumulativeQuantity ||
                initial.RestrictedQuantity != updated.RestrictedQuantity ||
                initial.ApplyShipping != updated.ApplyShipping ||
                initial.ApplyTax != updated.ApplyTax)
            {
                var patch = new PartialPriceSchedule()
                {
                    MinQuantity = updated.MinQuantity,
                    MaxQuantity = updated.MaxQuantity,
                    UseCumulativeQuantity = updated.UseCumulativeQuantity,
                    RestrictedQuantity = updated.RestrictedQuantity,
                    ApplyShipping = updated.ApplyShipping,
                    ApplyTax = updated.ApplyTax,
                };
                var relatedPriceSchedules = await oc.PriceSchedules.ListAllAsync(filters: $"ID={initial.ID}*");
                var priceSchedulesToUpdate = relatedPriceSchedules.Where(p => p.ID != updated.ID);
                await Throttler.RunAsync(priceSchedulesToUpdate, 100, 5, p =>
                {
                    return oc.PriceSchedules.PatchAsync(p.ID, patch, ocAuth.AccessToken);
                });
            }

            return await oc.PriceSchedules.SaveAsync<PriceSchedule>(updated.ID, updated, token);
        }

        private bool HasVariantChange(Variant variant, Variant currVariant)
        {
            if (variant.Active != currVariant.Active)
            {
                return true;
            }

            if (variant.Description != currVariant.Description)
            {
                return true;
            }

            if (variant.Name != currVariant.Name)
            {
                return true;
            }

            if (variant.ShipHeight != currVariant.ShipHeight)
            {
                return true;
            }

            if (variant.ShipLength != currVariant.ShipLength)
            {
                return true;
            }

            if (variant.ShipWeight != currVariant.ShipWeight)
            {
                return true;
            }

            if (variant.ShipWidth != currVariant.ShipWidth)
            {
                return true;
            }

            if (variant?.Inventory?.LastUpdated != currVariant?.Inventory?.LastUpdated)
            {
                return true;
            }

            if (variant?.Inventory?.QuantityAvailable != currVariant?.Inventory?.QuantityAvailable)
            {
                return true;
            }

            return false;
        }

        private async void HandleSpecOptionChanges(IList<Spec> requestSpecs, IList<Spec> existingSpecs, string token)
        {
            var requestSpecOptions = new Dictionary<string, List<SpecOption>>();
            var existingSpecOptions = new List<SpecOption>();
            foreach (Spec requestSpec in requestSpecs)
            {
                List<SpecOption> specOpts = new List<SpecOption>();
                foreach (SpecOption requestSpecOption in requestSpec.Options)
                {
                    specOpts.Add(requestSpecOption);
                }

                requestSpecOptions.Add(requestSpec.ID, specOpts);
            }

            foreach (Spec existingSpec in existingSpecs)
            {
                foreach (SpecOption existingSpecOption in existingSpec.Options)
                {
                    existingSpecOptions.Add(existingSpecOption);
                }
            }

            foreach (var spec in requestSpecOptions)
            {
                IList<SpecOption> changedSpecOptions = ChangedSpecOptions(spec.Value, existingSpecOptions);
                await Throttler.RunAsync(changedSpecOptions, 100, 5, option => oc.Specs.SaveOptionAsync(spec.Key, option.ID, option, token));
            }
        }

        private IList<SpecOption> ChangedSpecOptions(List<SpecOption> requestOptions, List<SpecOption> existingOptions)
        {
            return requestOptions.FindAll(requestOption => OptionHasChanges(requestOption, existingOptions));
        }

        private bool OptionHasChanges(SpecOption requestOption, List<SpecOption> currentOptions)
        {
            var matchingOption = currentOptions.Find(currentOption => currentOption.ID == requestOption.ID);
            if (matchingOption == null)
            {
                return false;
            }

            if (matchingOption.PriceMarkup != requestOption.PriceMarkup)
            {
                return true;
            }

            if (matchingOption.IsOpenText != requestOption.IsOpenText)
            {
                return true;
            }

            if (matchingOption.ListOrder != requestOption.ListOrder)
            {
                return true;
            }

            if (matchingOption.PriceMarkupType != requestOption.PriceMarkupType)
            {
                return true;
            }

            return false;
        }

        private async Task<string> GetSupplierNameForXpFacet(string supplierID, string accessToken)
        {
            var supplier = await oc.Suppliers.GetAsync(supplierID, accessToken);
            return supplier.Name;
        }
    }
}

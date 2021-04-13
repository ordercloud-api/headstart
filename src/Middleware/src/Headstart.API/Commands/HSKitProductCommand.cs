using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Headstart.Common;
using Headstart.Common.Services.CMS;
using Headstart.Common.Services.CMS.Models;
using Headstart.Models;
using Headstart.Models.Headstart;
using OrderCloud.Catalyst;
using OrderCloud.SDK;


namespace Headstart.API.Commands.Crud
{
    public interface IHSKitProductCommand
    {
        Task<HSKitProduct> Get(string id, string token);
        Task<HSMeKitProduct> GetMeKit(string id, VerifiedUserContext user);
        Task<ListPage<HSKitProduct>> List(ListArgs<Document<HSKitProductAssignment>> args, string token);
        Task<HSKitProduct> Post(HSKitProduct kitProduct, string token);
        Task<HSKitProduct> Put(string id, HSKitProduct kitProduct, string token);
        Task Delete(string id, string token);
    }

    public class HSKitProductCommand : IHSKitProductCommand
    {
        private readonly IOrderCloudClient _oc;
        private readonly ICMSClient _cms;
        private readonly IMeProductCommand _meProductCommand;
        private readonly IAssetClient _assetClient;

        public HSKitProductCommand(
            AppSettings settings,
            ICMSClient cms,
            IOrderCloudClient elevatedOc,
            IMeProductCommand meProductCommand,
            IAssetClient assetClient
        )
        {
            _cms = cms;
            _oc = elevatedOc;
            _meProductCommand = meProductCommand;
            _assetClient = assetClient;
        }

        public async Task<HSKitProduct> Get(string id, string token)
        {
            var _product = await _oc.Products.GetAsync<HSProduct>(id, token);
            var _productAssignments = await _cms.Documents.Get<HSKitProductAssignment>("HSKitProductAssignment", _product.ID, token);

            return new HSKitProduct
            {
                ID = _product.ID,
                Name = _product.Name,
                Product = _product,
                ProductAssignments = await _getKitDetails(_productAssignments.Doc, token)
            };
        }
        public async Task<HSMeKitProduct> GetMeKit(string id, VerifiedUserContext user)
        {
            var _product = await _oc.Me.GetProductAsync<HSMeProduct>(id, user.AccessToken);
            var _productAssignments = await _cms.Documents.Get<HSMeKitProductAssignment>("HSKitProductAssignment", _product.ID, user.AccessToken);
            var meKitProduct = new HSMeKitProduct
            {
                ID = _product.ID,
                Name = _product.Name,
                Product = _product,
                ProductAssignments = await _getMeKitDetails(_productAssignments.Doc, user.AccessToken)
            };
            return await _meProductCommand.ApplyBuyerPricing(meKitProduct, user);
        }

        public async Task<ListPage<HSKitProduct>> List(ListArgs<Document<HSKitProductAssignment>> args, string token)
        {
            var _kitProducts = await _cms.Documents.List<HSKitProductAssignment>("HSKitProductAssignment", args, token);
            var _kitProductList = new List<HSKitProduct>();

            await Throttler.RunAsync(_kitProducts.Items, 100, 10, async product =>
            {
                var parentProduct = await _oc.Products.GetAsync<HSProduct>(product.ID);
                _kitProductList.Add(new HSKitProduct
                {
                    ID = parentProduct.ID,
                    Name = parentProduct.Name,
                    Product = parentProduct,
                    ProductAssignments = await _getKitDetails(product.Doc, token)
                });
            });
            return new ListPage<HSKitProduct>
            {
                Meta = _kitProducts.Meta,
                Items = _kitProductList
            };
        }
        public async Task<HSKitProduct> Post(HSKitProduct kitProduct, string token)
        {
            var _product = await _oc.Products.CreateAsync<HSProduct>(kitProduct.Product, token);
            var kitProductDoc = new Document<HSKitProductAssignment>();
            kitProductDoc.ID = _product.ID;
            kitProductDoc.Doc = kitProduct.ProductAssignments;
            var _productAssignments = await _cms.Documents.Create("HSKitProductAssignment", kitProductDoc, token);
            return new HSKitProduct
            {
                ID = _product.ID,
                Name = _product.Name,
                Product = _product,
                ProductAssignments = await _getKitDetails(_productAssignments.Doc, token)
            };
        }

        public async Task<HSKitProduct> Put(string id, HSKitProduct kitProduct, string token)
        {
            var _updatedProduct = await _oc.Products.SaveAsync<HSProduct>(kitProduct.Product.ID, kitProduct.Product, token);
            var kitProductDoc = new Document<HSKitProductAssignment>();
            kitProductDoc.ID = _updatedProduct.ID;
            kitProductDoc.Doc = kitProduct.ProductAssignments;
            var _productAssignments = await _cms.Documents.Save<HSKitProductAssignment>("HSKitProductAssignment", _updatedProduct.ID, kitProductDoc, token);
            return new HSKitProduct
            {
                ID = _updatedProduct.ID,
                Name = _updatedProduct.Name,
                Product = _updatedProduct,
                ProductAssignments = await _getKitDetails(_productAssignments.Doc, token)
            };
        }

        public async Task<HSKitProductAssignment> _getKitDetails(HSKitProductAssignment kit, string token)
        {
            
            // get product, specs, variants, and images for each product in the kit
            foreach (var p in kit.ProductsInKit)
            {
                try
                {
                    var productRequest = _oc.Products.GetAsync<HSProduct>(p.ID);
                    var specListRequest = _oc.Products.ListAllSpecsAsync(p.ID);
                    var variantListRequest = _oc.Products.ListAllVariantsAsync(p.ID);
                    await Task.WhenAll(specListRequest, variantListRequest);

                    p.Product = await productRequest;
                    p.Specs = await specListRequest;
                    p.Variants = await variantListRequest;

                } catch(Exception)
                {
                    p.Product = null;
                }
            }

            // filter out products in kit that we failed to retrieve details for (product might have been deleted since kit was created)
            kit.ProductsInKit = kit.ProductsInKit.Where(p => p.Product != null).ToList();
            return kit;
        }

        public async Task<HSMeKitProductAssignment> _getMeKitDetails(HSMeKitProductAssignment kit, string token)
        {
            // get product, specs, variants, and images for each product in the kit
            foreach (var p in kit.ProductsInKit)
            {
                try
                {
                    var productRequest = _oc.Me.GetProductAsync<HSMeProduct>(p.ID, token);
                    var specListRequest = _oc.Products.ListAllSpecsAsync(p.ID);
                    var variantListRequest = _oc.Products.ListAllVariantsAsync(p.ID);
                    await Task.WhenAll(specListRequest, variantListRequest);

                    var product = await productRequest;
                    if(product?.PriceSchedule != null)
                    {
                        // set min/max from kit only if its within the bounds of what the product can set
                        // this should be enforced at the admin creation level but may change after initially set
                        var productMax = product.PriceSchedule.MaxQuantity;
                        var productMin = product.PriceSchedule.MinQuantity;
                        var kitMax = p.MaxQty;
                        var kitMin = p.MinQty;

                        // set product min
                        if(kitMin != null && (productMin == null || productMin < kitMin ))
                        {
                            product.PriceSchedule.MinQuantity = kitMin;
                        }

                        // set product max
                        if(kitMax != null && (productMax == null || productMax > kitMax) && kitMax > product.PriceSchedule.MinQuantity) // extra check needed because minqty might have changed
                        {
                            product.PriceSchedule.MaxQuantity = kitMax;
                        }
                    }

                    p.Product = product;
                    p.Specs = await specListRequest;
                    p.Variants = await variantListRequest;
                }
                catch (Exception)
                {
                    p.Product = null;
                }
            }

            // filter out products in kit that we failed to retrieve details for (product might have been deleted since kit was created)
            kit.ProductsInKit = kit.ProductsInKit.Where(p => p.Product != null).ToList();
            return kit;
        }

        public async Task Delete(string id, string token)
        {
            var tasks = new List<Task>()
            {
                _oc.Products.DeleteAsync(id, token)
            };
            var product = await _oc.Products.GetAsync<HSProduct>(id);
            if(product?.xp?.Images?.Count() > 0 )
            {
                tasks.Add(Throttler.RunAsync(product.xp.Images, 100, 5, i => _assetClient.DeleteAssetByUrl(i.Url)));
            }
            if(product?.xp?.Documents.Count() > 0)
            {
                tasks.Add(Throttler.RunAsync(product.xp.Documents, 100, 5, d => _assetClient.DeleteAssetByUrl(d.Url)));
            }
            // Delete images, attachments, and assignments associated with the requested product
            await Task.WhenAll(tasks);
        }
    }
}

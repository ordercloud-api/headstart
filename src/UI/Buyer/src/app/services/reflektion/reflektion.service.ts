import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { HSMeProduct, ReflektionAccessToken, HeadStartSDK } from "@ordercloud/headstart-sdk";
import { intersection } from "lodash";
import { ListPageWithFacets, MetaWithFacets } from "ordercloud-javascript-sdk";
import { ProductSortOption } from "src/app/components/products/sort-products/sort-products.component";
import { AppConfig } from "src/app/models/environment.types";
import { ProductFilters } from "src/app/models/filter-config.types";

@Injectable({
    providedIn: 'root',
  })
  export class ReflektionService {
    constructor(private http: HttpClient, private appConfig: AppConfig) {}
        private reflektionToken: ReflektionAccessToken = null;
        reflektionSortOptions: ProductSortOption[] = [
            { label: "Name: A-Z", value: "name-asc" },
            { label: "Name: Z-A", value: "name-desc" },
            { label: "Price: High to Low", value: "price-desc" },
            { label: "Price: Low to High", value: "price-asc" },
            { label: "Rating: High to Low", value: "review-desc" },
            { label: "Rating: Low to High", value: "review-asc" },
            { label: "Reviews: High to Low", value: "review-desc" },
            { label: "Reviews: Low to High", value: "review-asc" },
            { label: "Featured", value: "featured-desc" },
        ];

        async init() {
            if (this.reflektionToken == null) {
                this.reflektionToken = await HeadStartSDK.Reflektion.GetToken();
            }
        }

        async listReflektionProducts(userID: string, filters: ProductFilters) : Promise<ListPageWithFacets<HSMeProduct>> {
            await this.init();
            var body = this.buildRequest(userID, filters);
            var resp = await this.http.post<any>(this.appConfig.reflektionUrl + "/api/search-rec/3", body, { headers: { Authorization: this.reflektionToken.accessToken } }).toPromise();
            var meProducts = { 
              Meta: this.mapMeta(resp),
              Items:  resp.content.product.value.map(this.mapProduct)
            };
            return meProducts;
        }

        private mapProduct(reflektionProduct): HSMeProduct | any {
            var product = {
                ID: reflektionProduct.sku,
                Name: reflektionProduct.name,
                QuantityMultiplier: 1,
                PriceSchedule: {
                  MinQuantity: 1,
                  PriceBreaks: [{
                    Quantity: 1,
                    Price: reflektionProduct.price
                  }]
                },
                xp: {
                  Currency: "USD",
                  Images: [{
                    Url: reflektionProduct.image_url
                  }]
                }
            };
            return product;
        }

        private mapMeta(response): MetaWithFacets {
            var Page = response.page_number;
            var PageSize = response.content.product.n_item;
            var TotalCount = response.content.product.total_item;
            var itemRangeStart = (((Page - 1) * PageSize) + 1)
            var itemRangeEnd = Math.max(itemRangeStart + PageSize, TotalCount);
            var meta = {
                Facets: [],
                Page,
                PageSize,
                TotalCount,
                TotalPages: response.total_page,
                ItemRange: [itemRangeStart, itemRangeEnd],
            };
            return meta;
        }

        private buildRequest(userID: string, filters: ProductFilters) {
            var sortArray = (filters.sortBy || []).map(value => {
                var [name, order] = value.split("-");
                return { name, order };
            })
            return {        
                data: {
                    n_item: 20,
                    page_number: Number(filters.page),
                    query: {
                        keyphrase: {
                            value: [
                              filters.search ?? ""
                            ]
                        }
                    },
                    context: {
                        user: {
                            uuid: userID
                        }
                    },
                    sort: {
                        value: sortArray
                    },
                    content: {
                        product: {}
                    },
                    force_v2_specs: true
                }
            }
        }
  }
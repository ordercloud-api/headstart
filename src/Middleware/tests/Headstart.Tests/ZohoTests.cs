//using Headstart.Common.Commands.Zoho;
//using Headstart.Common.Services.ShippingIntegration.Models;
//using Headstart.Common.Services.Zoho;
//using Newtonsoft.Json;
//using NUnit.Framework;
//using OrderCloud.SDK;
//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Headstart.Models;
//using Headstart.Models.Models.Marketplace;

//namespace Headstart.Tests
//{
//    public class ZohoTests
//    {
//        private const string TWO_LINEITEMS_SAME_PRODUCT_ID = @"{
//            'Order': {
//                'ID': 'SEB000124',
//                'FromUser': {
//                    'ID': '0003-00004',
//                    'Username': 'billapprover',
//                    'Password': null,
//                    'FirstName': 'bill',
//                    'LastName': 'hickey',
//                    'Email': 'test@test.com',
//                    'Phone': '',
//                    'TermsAccepted': null,
//                    'Active': true,
//                    'xp': null,
//                    'AvailableRoles': null,
//                    'DateCreated': '2020-04-10T21:23:43.137+00:00',
//                    'PasswordLastSetDate': '2020-04-10T21:31:48.04+00:00'
//                },
//                'FromCompanyID': '0003',
//                'ToCompanyID': 'rQYR6T6ZTEqVrgv8x_ei0g',
//                'FromUserID': '0003-00004',
//                'BillingAddressID': '0003-0001',
//                'BillingAddress': {
//                    'ID': '0003-0001',
//                    'DateCreated': null,
//                    'CompanyName': 'Basecamp Burlingame, Ca',
//                    'FirstName': '',
//                    'LastName': '',
//                    'Street1': '261 California Dr',
//                    'Street2': '',
//                    'City': 'Burlingame',
//                    'State': 'CA',
//                    'Zip': '94010',
//                    'Country': 'US',
//                    'Phone': '',
//                    'AddressName': 'Burlingame, CA',
//                    'xp': {
//                        'Coordinates': null,
//                        'Accessorials': null,
//                        'Email': 'BurlingameCA@basecampfitness.com',
//                        'LocationID': 'BCF10001',
//                        'AvalaraCertificateID': null,
//                        'AvalaraCertificateExpiration': null
//                    }
//                },
//                'ShippingAddressID': '0003-0001',
//                'Comments': '',
//                'LineItemCount': 2,
//                'Status': 'Open',
//                'DateCreated': '2020-06-22T16:14:11.343+00:00',
//                'DateSubmitted': '2020-06-22T16:20:12.477+00:00',
//                'DateApproved': null,
//                'DateDeclined': null,
//                'DateCanceled': null,
//                'DateCompleted': null,
//                'Subtotal': 50,
//                'ShippingCost': 20.64,
//                'TaxCost': 4.74,
//                'PromotionDiscount': 0,
//                'Total': 75.38,
//                'IsSubmitted': true,
//                'xp': {
//                    'ExternalTaxTransactionID': '',
//                    'OrderType': 'Standard',
//                    'QuoteOrderInfo': null,
//                    'Currency': 'USD',
//                    'OrderReturnInfo': {
//                        'HasReturn': false
//                    },
//                    'ClaimStatus': 'NoClaim',
//                    'ShippingStatus': 'Processing',
//                    'ApprovalNeeded': ''
//                }
//            },
//            'LineItems': [
//                {
//                    'ID': 'Dnz1uBExm02yDlPIUqizzg',
//                    'ProductID': '500CLASSHOODY',
//                    'Quantity': 1,
//                    'DateAdded': '2020-06-22T16:14:47.63+00:00',
//                    'QuantityShipped': 0,
//                    'UnitPrice': 25,
//                    'PromotionDiscount': 0,
//                    'LineTotal': 25,
//                    'LineSubtotal': 25,
//                    'CostCenter': null,
//                    'DateNeeded': null,
//                    'ShippingAccount': null,
//                    'ShippingAddressID': '0003-0001',
//                    'ShipFromAddressID': '012-02',
//                    'Product': {
//                        'ID': '500CLASSHOODY',
//                        'Name': '500 Class Hoodie',
//                        'Description': null,
//                        'QuantityMultiplier': 1,
//                        'ShipWeight': 0.8,
//                        'ShipHeight': 1,
//                        'ShipWidth': 12,
//                        'ShipLength': 6,
//                        'xp': {
//                            'Facets': {
//                                'supplier': [
//                                    'Self Esteem Brands Distribution'
//                                ],
//                                'size': [
//                                    'Small',
//                                    'Medium',
//                                    'Large',
//                                    'X-Large',
//                                    'X-Small'
//                                ]
//                            },
//                            'IntegrationData': null,
//                            'Status': 'Draft',
//                            'HasVariants': false,
//                            'Note': '',
//                            'Tax': {
//                                'Category': 'P0000000',
//                                'Code': 'PC030000',
//                                'Description': 'Clothing And Related Products (Business-To-Business)'
//                            },
//                            'UnitOfMeasure': {
//                                'Qty': 1,
//                                'Unit': 'each'
//                            },
//                            'ProductType': 'Standard',
//                            'IsResale': true,
//                            'Accessorials': null,
//                            'Currency': 'USD'
//                        }
//                    },
//                    'Variant': {
//                        'ID': '500CLASSHOODY-Medium',
//                        'Name': '500CLASSHOODY-Medium',
//                        'Description': null,
//                        'ShipWeight': null,
//                        'ShipHeight': null,
//                        'ShipWidth': null,
//                        'ShipLength': null,
//                        'xp': {
//                            'SpecCombo': 'Medium',
//                            'SpecValues': [
//                                {
//                                    'SpecName': 'Size',
//                                    'SpecOptionValue': 'Medium',
//                                    'PriceMarkup': ''
//                                }
//                            ],
//                            'NewID': null
//                        }
//                    },
//                    'ShippingAddress': {
//                        'ID': '0003-0001',
//                        'DateCreated': null,
//                        'CompanyName': 'Basecamp Burlingame, Ca',
//                        'FirstName': '',
//                        'LastName': '',
//                        'Street1': '261 California Dr',
//                        'Street2': '',
//                        'City': 'Burlingame',
//                        'State': 'CA',
//                        'Zip': '94010',
//                        'Country': 'US',
//                        'Phone': '',
//                        'AddressName': 'Burlingame, CA',
//                        'xp': {
//                            'Coordinates': null,
//                            'Accessorials': null,
//                            'Email': 'BurlingameCA@basecampfitness.com',
//                            'LocationID': 'BCF10001',
//                            'AvalaraCertificateID': null,
//                            'AvalaraCertificateExpiration': null
//                        }
//                    },
//                    'ShipFromAddress': {
//                        'ID': '012-02',
//                        'DateCreated': null,
//                        'CompanyName': 'Self Esteem Brands Distribution',
//                        'FirstName': '',
//                        'LastName': '',
//                        'Street1': '5805 W 6th Ave',
//                        'Street2': null,
//                        'City': 'Lakewood',
//                        'State': 'CO',
//                        'Zip': '80214-2453',
//                        'Country': 'US',
//                        'Phone': '',
//                        'AddressName': 'SEB Distribution Denver',
//                        'xp': null
//                    },
//                    'SupplierID': '012',
//                    'Specs': [
//                        {
//                            'SpecID': '500CLASSHOODYSize',
//                            'Name': 'Size',
//                            'OptionID': 'Medium',
//                            'Value': 'Medium',
//                            'PriceMarkupType': 'NoMarkup',
//                            'PriceMarkup': null
//                        }
//                    ],
//                    'xp': {
//                        'LineItemStatus': 'Complete',
//                        'LineItemReturnInfo': null,
//                        'LineItemImageUrl': 'https://marketplace-middleware-test.azurewebsites.net/products/500CLASSHOODY/image',
//                        'UnitPriceInProductCurrency': 25
//                    }
//                },
//                {
//                    'ID': 'BnJ3vXvKAki_NmyqUvqSwQ',
//                    'ProductID': '500CLASSHOODY',
//                    'Quantity': 1,
//                    'DateAdded': '2020-06-22T16:14:53.3+00:00',
//                    'QuantityShipped': 0,
//                    'UnitPrice': 25,
//                    'PromotionDiscount': 0,
//                    'LineTotal': 25,
//                    'LineSubtotal': 25,
//                    'CostCenter': null,
//                    'DateNeeded': null,
//                    'ShippingAccount': null,
//                    'ShippingAddressID': '0003-0001',
//                    'ShipFromAddressID': '012-02',
//                    'Product': {
//                        'ID': '500CLASSHOODY',
//                        'Name': '500 Class Hoodie',
//                        'Description': null,
//                        'QuantityMultiplier': 1,
//                        'ShipWeight': 0.8,
//                        'ShipHeight': 1,
//                        'ShipWidth': 12,
//                        'ShipLength': 6,
//                        'xp': {
//                            'Facets': {
//                                'supplier': [
//                                    'Self Esteem Brands Distribution'
//                                ],
//                                'size': [
//                                    'Small',
//                                    'Medium',
//                                    'Large',
//                                    'X-Large',
//                                    'X-Small'
//                                ]
//                            },
//                            'IntegrationData': null,
//                            'Status': 'Draft',
//                            'HasVariants': false,
//                            'Note': '',
//                            'Tax': {
//                                'Category': 'P0000000',
//                                'Code': 'PC030000',
//                                'Description': 'Clothing And Related Products (Business-To-Business)'
//                            },
//                            'UnitOfMeasure': {
//                                'Qty': 1,
//                                'Unit': 'each'
//                            },
//                            'ProductType': 'Standard',
//                            'IsResale': true,
//                            'Accessorials': null,
//                            'Currency': 'USD'
//                        }
//                    },
//                    'Variant': {
//                        'ID': '500CLASSHOODY-XSmall',
//                        'Name': '500CLASSHOODY-XSmall',
//                        'Description': null,
//                        'ShipWeight': null,
//                        'ShipHeight': null,
//                        'ShipWidth': null,
//                        'ShipLength': null,
//                        'xp': {
//                            'SpecCombo': 'XSmall',
//                            'SpecValues': [
//                                {
//                                    'SpecName': 'Size',
//                                    'SpecOptionValue': 'X-Small',
//                                    'PriceMarkup': ''
//                                }
//                            ],
//                            'NewID': null
//                        }
//                    },
//                    'ShippingAddress': {
//                        'ID': '0003-0001',
//                        'DateCreated': null,
//                        'CompanyName': 'Basecamp Burlingame, Ca',
//                        'FirstName': '',
//                        'LastName': '',
//                        'Street1': '261 California Dr',
//                        'Street2': '',
//                        'City': 'Burlingame',
//                        'State': 'CA',
//                        'Zip': '94010',
//                        'Country': 'US',
//                        'Phone': '',
//                        'AddressName': 'Burlingame, CA',
//                        'xp': {
//                            'Coordinates': null,
//                            'Accessorials': null,
//                            'Email': 'BurlingameCA@basecampfitness.com',
//                            'LocationID': 'BCF10001',
//                            'AvalaraCertificateID': null,
//                            'AvalaraCertificateExpiration': null
//                        }
//                    },
//                    'ShipFromAddress': {
//                        'ID': '012-02',
//                        'DateCreated': null,
//                        'CompanyName': 'Self Esteem Brands Distribution',
//                        'FirstName': '',
//                        'LastName': '',
//                        'Street1': '5805 W 6th Ave',
//                        'Street2': null,
//                        'City': 'Lakewood',
//                        'State': 'CO',
//                        'Zip': '80214-2453',
//                        'Country': 'US',
//                        'Phone': '',
//                        'AddressName': 'SEB Distribution Denver',
//                        'xp': null
//                    },
//                    'SupplierID': '012',
//                    'Specs': [
//                        {
//                            'SpecID': '500CLASSHOODYSize',
//                            'Name': 'Size',
//                            'OptionID': 'XSmall',
//                            'Value': 'X-Small',
//                            'PriceMarkupType': 'NoMarkup',
//                            'PriceMarkup': null
//                        }
//                    ],
//                    'xp': {
//                        'LineItemStatus': 'Complete',
//                        'LineItemReturnInfo': null,
//                        'LineItemImageUrl': 'https://marketplace-middleware-test.azurewebsites.net/products/500CLASSHOODY/image',
//                        'UnitPriceInProductCurrency': 25
//                    }
//                }
//            ],
//            'ShipEstimateResponse': {
//                'ShipEstimates': [
//                    {
//                        'ID': '012-02',
//                        'xp': {},
//                        'SelectedShipMethodID': 'FedexParcel-3cbe3fc1-23d2-46f8-8d3b-1f64e906fe02',
//                        'ShipEstimateItems': [
//                            {
//                                'LineItemID': 'Dnz1uBExm02yDlPIUqizzg',
//                                'Quantity': 1
//                            },
//                            {
//                                'LineItemID': 'BnJ3vXvKAki_NmyqUvqSwQ',
//                                'Quantity': 1
//                            }
//                        ],
//                        'ShipMethods': [
//                            {
//                                'ID': 'FedexParcel-3cbe3fc1-23d2-46f8-8d3b-1f64e906fe02',
//                                'Name': 'FEDEX_GROUND',
//                                'Cost': 20.64,
//                                'EstimatedTransitDays': 3,
//                                'xp': {
//                                    'OriginalShipCost': 20.64,
//                                    'OriginalCurrency': 'USD',
//                                    'ExchangeRate': 1,
//                                    'OrderCurrency': 'USD'
//                                }
//                            },
//                            {
//                                'ID': 'FedexParcel-d8a40d0a-68e2-4b32-85c9-f436133b55ba',
//                                'Name': 'FEDEX_2_DAY',
//                                'Cost': 44.04,
//                                'EstimatedTransitDays': 2,
//                                'xp': {
//                                    'OriginalShipCost': 44.04,
//                                    'OriginalCurrency': 'USD',
//                                    'ExchangeRate': 1,
//                                    'OrderCurrency': 'USD'
//                                }
//                            },
//                            {
//                                'ID': 'FedexParcel-54019d30-86fc-4828-a760-fe6b48c52b43',
//                                'Name': 'STANDARD_OVERNIGHT',
//                                'Cost': 108.86,
//                                'EstimatedTransitDays': 1,
//                                'xp': {
//                                    'OriginalShipCost': 108.86,
//                                    'OriginalCurrency': 'USD',
//                                    'ExchangeRate': 1,
//                                    'OrderCurrency': 'USD'
//                                }
//                            }
//                        ]
//                    }
//                ],
//                'HttpStatusCode': 200,
//                'UnhandledErrorBody': null,
//                'xp': {}
//            },
//            'OrderCalculateResponse': {
//                'LineItemOverrides': [],
//                'ShippingTotal': null,
//                'TaxTotal': 4.74,
//                'HttpStatusCode': 200,
//                'UnhandledErrorBody': null,
//                'xp': {}
//            },
//            'OrderSubmitResponse': null
//        }";
//        private const string worksheet = @"{
//		'Order': {
//			'ID': 'SEB000206',
//			'FromUser': {
//				'ID': '0003-00004',
//				'Username': 'billapprover',
//				'Password': null,
//				'FirstName': 'bill',
//				'LastName': 'hickey',
//				'Email': 'test@test.com',
//				'Phone': '',
//				'TermsAccepted': null,
//				'Active': true,
//				'xp': null,
//				'AvailableRoles': null,
//				'DateCreated': '2020-04-10T21:23:43.137+00:00',
//				'PasswordLastSetDate': '2020-04-10T21:31:48.04+00:00'
//			},
//			'FromCompanyID': '0003',
//			'ToCompanyID': 'rQYR6T6ZTEqVrgv8x_ei0g',
//			'FromUserID': '0003-00004',
//			'BillingAddressID': '0003-0003',
//			'BillingAddress': {
//				'ID': '0003-0003',
//				'DateCreated': null,
//				'CompanyName': 'Basecamp Minneapolis, MN',
//				'FirstName': '',
//				'LastName': '',
//				'Street1': '100 Hennepin Ave',
//				'Street2': '',
//				'City': 'Minneapolis',
//				'State': 'MN',
//				'Zip': '55402',
//				'Country': 'US',
//				'Phone': '',
//				'AddressName': 'Minneapolis, MN',
//				'xp': {
//					'Coordinates': null,
//					'Accessorials': null,
//					'Email': '',
//					'AvalaraCertificateID': 53,
//					'AvalaraCertificateExpiration': '2020-05-31T00:00:00+00:00'
//				}
//			},
//			'ShippingAddressID': '0003-0003',
//			'Comments': '',
//			'LineItemCount': 1,
//			'Status': 'Open',
//			'DateCreated': '2020-06-18T15:47:38.163+00:00',
//			'DateSubmitted': '2020-06-18T16:05:45.43+00:00',
//			'DateApproved': null,
//			'DateDeclined': null,
//			'DateCanceled': null,
//			'DateCompleted': null,
//			'Subtotal': 4.12,
//			'ShippingCost': 8.7,
//			'TaxCost': 1.02,
//			'PromotionDiscount': 0.0,
//			'Total': 13.84,
//			'IsSubmitted': true,
//			'xp': {
//				'ExternalTaxTransactionID': '',
//				'OrderType': 'Standard',
//				'QuoteOrderInfo': null,
//				'Currency': 'USD',
//				'OrderReturnInfo': {
//					'HasReturn': false
//				},
//				'ApprovalNeeded': ''
//			}
//		},
//		'LineItems': [
//			{
//				'ID': 'LmeWgQvLIEyT_bIWlUQeGg',
//				'ProductID': 'baseballglove',
//				'Quantity': 1,
//				'DateAdded': '2020-06-18T15:55:50.03+00:00',
//				'QuantityShipped': 0,
//				'UnitPrice': 4.11824,
//				'PromotionDiscount': 0.0,
//				'LineTotal': 4.12,
//				'LineSubtotal': 4.12,
//				'CostCenter': null,
//				'DateNeeded': null,
//				'ShippingAccount': null,
//				'ShippingAddressID': '0003-0003',
//				'ShipFromAddressID': 'Basecamp-01',
//				'Product': {
//					'ID': 'baseballglove',
//					'Name': 'Baseball Glove',
//					'Description': null,
//					'QuantityMultiplier': 1,
//					'ShipWeight': 1.0,
//					'ShipHeight': 1.0,
//					'ShipWidth': 2.0,
//					'ShipLength': 1.0,
//					'xp': {
//						'Facets': {
//							'supplier': [
//								'Basecamp Fitness Distribution'
//							]
//						},
//						'IntegrationData': null,
//						'Status': 'Draft',
//						'HasVariants': false,
//						'Note': '',
//						'Tax': {
//							'Category': 'P0000000',
//							'Code': 'PC030400',
//							'Description': 'Clothing And Related Products (Business-To-Business)-Sports/recreational equipment'
//						},
//						'UnitOfMeasure': {
//							'Qty': 1,
//							'Unit': 'Per'
//						},
//						'ProductType': 'Standard',
//						'IsResale': false,
//						'Accessorials': null,
//						'Currency': 'CHF'
//					}
//				},
//				'Variant': null,
//				'ShippingAddress': {
//					'ID': '0003-0003',
//					'DateCreated': null,
//					'CompanyName': 'Basecamp Minneapolis, MN',
//					'FirstName': '',
//					'LastName': '',
//					'Street1': '100 Hennepin Ave',
//					'Street2': '',
//					'City': 'Minneapolis',
//					'State': 'MN',
//					'Zip': '55402',
//					'Country': 'US',
//					'Phone': '',
//					'AddressName': 'Minneapolis, MN',
//					'xp': {
//						'Coordinates': null,
//						'Accessorials': null,
//						'Email': '',
//						'AvalaraCertificateID': 53,
//						'AvalaraCertificateExpiration': '2020-05-31T00:00:00+00:00'
//					}
//				},
//				'ShipFromAddress': {
//					'ID': 'Basecamp-01',
//					'DateCreated': null,
//					'CompanyName': 'Basecamp Fitness Minneapolis',
//					'FirstName': '',
//					'LastName': '',
//					'Street1': '100 Hennepin Ave',
//					'Street2': null,
//					'City': 'Minneapolis',
//					'State': 'MN',
//					'Zip': '55401-1903',
//					'Country': 'US',
//					'Phone': '',
//					'AddressName': 'Basecamp Fitness Minneapolis',
//					'xp': null
//				},
//				'SupplierID': 'Basecamp',
//				'Specs': [],
//				'xp': {
//					'LineItemStatus': 'Complete',
//					'LineItemReturnInfo': null,
//					'LineItemImageUrl': 'https://marketplace-middleware-test.azurewebsites.net/products/baseballglove/image',
//					'UnitPriceInProductCurrency': 4.0
//				}
//			}
//		],
//		'ShipEstimateResponse': {
//			'ShipEstimates': [
//				{
//					'ID': 'Basecamp-01',
//					'xp': {},
//					'SelectedShipMethodID': 'FedexParcel-5e780d9d-a89c-4ab2-81cf-b48014cfe609',
//					'ShipEstimateItems': [
//						{
//							'LineItemID': 'LmeWgQvLIEyT_bIWlUQeGg',
//							'Quantity': 1
//						}
//					],
//					'ShipMethods': [
//						{
//							'ID': 'FedexParcel-5e780d9d-a89c-4ab2-81cf-b48014cfe609',
//							'Name': 'FEDEX_GROUND',
//							'Cost': 8.7,
//							'EstimatedTransitDays': 1,
//							'xp': {
//								'OriginalShipCost': 8.7,
//								'OriginalCurrency': 'USD',
//								'ExchangeRate': 1.0,
//								'OrderCurrency': 'USD'
//							}
//						},
//						{
//							'ID': 'FedexParcel-7e4c88be-731f-417a-9f1a-eace3bd96845',
//							'Name': 'FEDEX_EXPRESS_SAVER',
//							'Cost': 15.58,
//							'EstimatedTransitDays': 3,
//							'xp': {
//								'OriginalShipCost': 15.58,
//								'OriginalCurrency': 'USD',
//								'ExchangeRate': 1.0,
//								'OrderCurrency': 'USD'
//							}
//						},
//						{
//							'ID': 'FedexParcel-fb0d99fb-2ca3-4a3c-9915-d85d725f215e',
//							'Name': 'FEDEX_2_DAY',
//							'Cost': 17.67,
//							'EstimatedTransitDays': 2,
//							'xp': {
//								'OriginalShipCost': 17.67,
//								'OriginalCurrency': 'USD',
//								'ExchangeRate': 1.0,
//								'OrderCurrency': 'USD'
//							}
//						}
//					]
//				}
//			],
//			'HttpStatusCode': 200,
//			'UnhandledErrorBody': null,
//			'xp': {}
//		},
//		'OrderCalculateResponse': {
//			'LineItemOverrides': [],
//			'ShippingTotal': null,
//			'TaxTotal': 1.02,
//			'HttpStatusCode': 200,
//			'UnhandledErrorBody': null,
//			'xp': {}
//		},
//		'OrderSubmitResponse': null
//	}";

//		// taxes were failing from writing tax on order which caused this worksheet to fail
//		private const string WORKSHEET_WITH_TAX_COST = @"{
//	'Order': {
//		'ID': 'SEB000388X3',
//		'FromUser': {
//			'ID': '0005-00011',
//			'Username': 'SarahWTC',
//			'Password': null,
//			'FirstName': 'Sarah',
//			'LastName': 'Ennis',
//			'Email': 'sennis@four51.com',
//			'Phone': '',
//			'TermsAccepted': null,
//			'Active': true,
//			'xp': {
//				'Country': 'US'
//			},
//			'AvailableRoles': null,
//			'DateCreated': '2020-07-07T16:13:54.84+00:00',
//			'PasswordLastSetDate': '2020-07-07T16:14:47.377+00:00'
//		},
//		'FromCompanyID': '0005',
//		'ToCompanyID': 'rQYR6T6ZTEqVrgv8x_ei0g',
//		'FromUserID': '0005-00011',
//		'BillingAddressID': '0005-0001',
//		'BillingAddress': {
//			'ID': '0005-0001',
//			'DateCreated': null,
//			'CompanyName': 'Waxing the City - Maple Grove',
//			'FirstName': '',
//			'LastName': '',
//			'Street1': '7895 Main St',
//			'Street2': '',
//			'City': 'Maple Grove',
//			'State': 'MN',
//			'Zip': '55369',
//			'Country': 'US',
//			'Phone': '7634162082',
//			'AddressName': 'Maple Grove, MN',
//			'xp': {
//				'Coordinates': null,
//				'Accessorials': null,
//				'Email': 'wtcMG@test.com',
//				'LocationID': 'W7',
//				'AvalaraCertificateID': 61,
//				'AvalaraCertificateExpiration': '2021-07-01T00:00:00+00:00'
//			}
//		},
//		'ShippingAddressID': '0005-0001',
//		'Comments': '',
//		'LineItemCount': 2,
//		'Status': 'Open',
//		'DateCreated': '2020-08-28T15:29:04.07+00:00',
//		'DateSubmitted': '2020-08-28T15:32:06.12+00:00',
//		'DateApproved': null,
//		'DateDeclined': null,
//		'DateCanceled': null,
//		'DateCompleted': null,
//		'Subtotal': 426.95,
//		'ShippingCost': 280.51,
//		'TaxCost': 53.23,
//		'PromotionDiscount': 0,
//		'Total': 760.69,
//		'IsSubmitted': true,
//		'xp': {
//			'ExternalTaxTransactionID': '41a74fb6-6364-42d8-82d6-63f6e968ba16',
//			'OrderType': 'Standard',
//			'QuoteOrderInfo': null,
//			'Currency': 'USD',
//			'Returns': {
//				'HasClaims': false,
//				'HasUnresolvedClaims': false,
//				'Resolutions': []
//			},
//			'ClaimStatus': 'NoClaim',
//			'ShippingStatus': 'Processing',
//			'ApprovalNeeded': '',
//			'ShipFromAddressIDs': [
//				'019-01'
//			],
//			'SupplierIDs': [
//				'019'
//			],
//			'SubmittedOrderStatus': 'Open',
//			'NeedsAttention': true
//		}
//	},
//	'LineItems': [
//		{
//			'ID': 'X001',
//			'ProductID': '27832',
//			'Quantity': 1,
//			'DateAdded': '2020-08-28T15:30:44.08+00:00',
//			'QuantityShipped': 0,
//			'UnitPrice': 224.7094,
//			'PromotionDiscount': 0,
//			'LineTotal': 224.71,
//			'LineSubtotal': 224.71,
//			'CostCenter': null,
//			'DateNeeded': null,
//			'ShippingAccount': null,
//			'ShippingAddressID': '0005-0001',
//			'ShipFromAddressID': '019-01',
//			'Product': {
//				'ID': '27832',
//				'Name': 'Granite Series Sled',
//				'Description': '',
//				'QuantityMultiplier': 1,
//				'ShipWeight': 45,
//				'ShipHeight': 1,
//				'ShipWidth': 1,
//				'ShipLength': 1,
//				'xp': {
//					'Facets': {
//						'supplier': [
//							'Power Systems'
//						],
//						'Color': [
//							'Black'
//						]
//					},
//					'IntegrationData': null,
//					'Status': 'Draft',
//					'HasVariants': false,
//					'Note': '',
//					'Tax': {
//						'Category': 'P0000000',
//						'Code': 'PC030204',
//						'Description': 'Clothing And Related Products (Business-To-Business)-Handbags'
//					},
//					'UnitOfMeasure': {
//						'Qty': 1,
//						'Unit': 'sled'
//					},
//					'ProductType': 'Standard',
//					'IsResale': false,
//					'Accessorials': null,
//					'Currency': 'USD'
//				}
//			},
//			'Variant': null,
//			'ShippingAddress': {
//				'ID': '0005-0001',
//				'DateCreated': null,
//				'CompanyName': 'Waxing the City - Maple Grove',
//				'FirstName': '',
//				'LastName': '',
//				'Street1': '7895 Main St',
//				'Street2': '',
//				'City': 'Maple Grove',
//				'State': 'MN',
//				'Zip': '55369',
//				'Country': 'US',
//				'Phone': '7634162082',
//				'AddressName': 'Maple Grove, MN',
//				'xp': {
//					'Coordinates': null,
//					'Accessorials': null,
//					'Email': 'wtcMG@test.com',
//					'LocationID': 'W7',
//					'AvalaraCertificateID': 61,
//					'AvalaraCertificateExpiration': '2021-07-01T00:00:00+00:00'
//				}
//			},
//			'ShipFromAddress': {
//				'ID': '019-01',
//				'DateCreated': null,
//				'CompanyName': 'Power Systems',
//				'FirstName': '',
//				'LastName': '',
//				'Street1': '5700 Casey Dr',
//				'Street2': null,
//				'City': 'Knoxville',
//				'State': 'TN',
//				'Zip': '37909-1803',
//				'Country': 'US',
//				'Phone': '',
//				'AddressName': 'Power Systems',
//				'xp': null
//			},
//			'SupplierID': '019',
//			'Specs': [],
//			'xp': {
//				'StatusByQuantity': {
//					'Submitted': 1,
//					'Backordered': 0,
//					'CancelRequested': 0,
//					'Complete': 0,
//					'ReturnRequested': 0,
//					'Returned': 0,
//					'Canceled': 0,
//					'Open': 0
//				},
//				'Returns': [],
//				'Cancelations': [],
//				'ImageUrl': 'https://marketplace-middleware-staging.azurewebsites.net/assets/rQYR6T6ZTEqVrgv8x_ei0g/products/27832/thumbnail?size=M'
//			}
//		},
//		{
//			'ID': 'X002',
//			'ProductID': '70085',
//			'Quantity': 1,
//			'DateAdded': '2020-08-28T15:30:44.643+00:00',
//			'QuantityShipped': 0,
//			'UnitPrice': 202.2374,
//			'PromotionDiscount': 0,
//			'LineTotal': 202.24,
//			'LineSubtotal': 202.24,
//			'CostCenter': null,
//			'DateNeeded': null,
//			'ShippingAccount': null,
//			'ShippingAddressID': '0005-0001',
//			'ShipFromAddressID': '019-01',
//			'Product': {
//				'ID': '70085',
//				'Name': 'Power Systems Deck',
//				'Description': '',
//				'QuantityMultiplier': 1,
//				'ShipWeight': 28,
//				'ShipHeight': 20,
//				'ShipWidth': 35,
//				'ShipLength': 30,
//				'xp': {
//					'Facets': {
//						'Color': [
//							'Black'
//						],
//						'supplier': [
//							'Power Systems'
//						]
//					},
//					'IntegrationData': null,
//					'Status': 'Draft',
//					'HasVariants': false,
//					'Note': '',
//					'Tax': {
//						'Category': 'P0000000',
//						'Code': 'P0000000',
//						'Description': 'Tangible Personal Property (TPP)'
//					},
//					'UnitOfMeasure': {
//						'Qty': 1,
//						'Unit': 'per'
//					},
//					'ProductType': 'Standard',
//					'IsResale': false,
//					'Accessorials': null,
//					'Currency': 'USD'
//				}
//			},
//			'Variant': null,
//			'ShippingAddress': {
//				'ID': '0005-0001',
//				'DateCreated': null,
//				'CompanyName': 'Waxing the City - Maple Grove',
//				'FirstName': '',
//				'LastName': '',
//				'Street1': '7895 Main St',
//				'Street2': '',
//				'City': 'Maple Grove',
//				'State': 'MN',
//				'Zip': '55369',
//				'Country': 'US',
//				'Phone': '7634162082',
//				'AddressName': 'Maple Grove, MN',
//				'xp': {
//					'Coordinates': null,
//					'Accessorials': null,
//					'Email': 'wtcMG@test.com',
//					'LocationID': 'W7',
//					'AvalaraCertificateID': 61,
//					'AvalaraCertificateExpiration': '2021-07-01T00:00:00+00:00'
//				}
//			},
//			'ShipFromAddress': {
//				'ID': '019-01',
//				'DateCreated': null,
//				'CompanyName': 'Power Systems',
//				'FirstName': '',
//				'LastName': '',
//				'Street1': '5700 Casey Dr',
//				'Street2': null,
//				'City': 'Knoxville',
//				'State': 'TN',
//				'Zip': '37909-1803',
//				'Country': 'US',
//				'Phone': '',
//				'AddressName': 'Power Systems',
//				'xp': null
//			},
//			'SupplierID': '019',
//			'Specs': [],
//			'xp': {
//				'StatusByQuantity': {
//					'Submitted': 1,
//					'Backordered': 0,
//					'CancelRequested': 0,
//					'Complete': 0,
//					'ReturnRequested': 0,
//					'Returned': 0,
//					'Canceled': 0,
//					'Open': 0
//				},
//				'Returns': [],
//				'Cancelations': [],
//				'ImageUrl': 'https://marketplace-middleware-staging.azurewebsites.net/assets/rQYR6T6ZTEqVrgv8x_ei0g/products/70085/thumbnail?size=M'
//			}
//		}
//	],
//	'ShipEstimateResponse': {
//		'ShipEstimates': [
//			{
//				'ID': '019-01',
//				'xp': {},
//				'SelectedShipMethodID': 'FedexParcel-1a1b4d6f-02de-4042-b4bc-d073b43a920d',
//				'ShipEstimateItems': [
//					{
//						'LineItemID': 'X001',
//						'Quantity': 1
//					},
//					{
//						'LineItemID': 'X002',
//						'Quantity': 1
//					}
//				],
//				'ShipMethods': [
//					{
//						'ID': 'FedexParcel-1a1b4d6f-02de-4042-b4bc-d073b43a920d',
//						'Name': 'FEDEX_GROUND',
//						'Cost': 280.51,
//						'EstimatedTransitDays': 2,
//						'xp': {
//							'OriginalShipCost': 280.51,
//							'OriginalCurrency': 'USD',
//							'ExchangeRate': 1,
//							'OrderCurrency': 'USD'
//						}
//					},
//					{
//						'ID': 'FedexParcel-97f45367-b2bc-43bf-950a-8a1fe2038bde',
//						'Name': 'FEDEX_EXPRESS_SAVER',
//						'Cost': 629.26,
//						'EstimatedTransitDays': 3,
//						'xp': {
//							'OriginalShipCost': 629.26,
//							'OriginalCurrency': 'USD',
//							'ExchangeRate': 1,
//							'OrderCurrency': 'USD'
//						}
//					},
//					{
//						'ID': 'FedexParcel-ef6f22e0-c909-4e9e-9db7-91803014ccd9',
//						'Name': 'STANDARD_OVERNIGHT',
//						'Cost': 1322.68,
//						'EstimatedTransitDays': 1,
//						'xp': {
//							'OriginalShipCost': 1322.68,
//							'OriginalCurrency': 'USD',
//							'ExchangeRate': 1,
//							'OrderCurrency': 'USD'
//						}
//					}
//				]
//			}
//		],
//		'HttpStatusCode': 200,
//		'UnhandledErrorBody': null,
//		'xp': {}
//	}
//}";

//		// order worksheet to recreate SEB-1001
//		private const string SEB_1001 = @"";

//        private ZohoClientConfig _zohoConfig;
//        private OrderCloudClientConfig _ocConfig;
//        private OrderCloudClient _ocBuyer;
//        private OrderCloudClient _ocIntegration;

//		[SetUp]
//        public void Setup()
//        {
//			_zohoConfig = new ZohoClientConfig()
//            {
//				ClientId = "1000.LYTODQT800N5C6UWEMRKKRS3VM7RPH",
//				ClientSecret = "d6c6960a7d742efd8230bd010e83eb86fae6c2dc87",
//				AccessToken = "1000.e9b088c5a817701588daf498a8231d69.467c7b3949d6d37a9982c18d865a2749",
//				ApiUrl = "https://books.zoho.com/api/v3",
//				OrganizationID = "708781679"
//			};
//			_ocConfig = new OrderCloudClientConfig()
//            {
//				AuthUrl = "https://api.ordercloud.io",
//				ApiUrl = "https://api.ordercloud.io",
//				ClientId = "0CC8282F-8EA9-4040-B1D9-BC03AC9FBB6B",
//				ClientSecret = "ulhq0P2DrdvzjBngQhv3DLus15V3VZEGYG0vYuVtBCRrruDNCpQXl11Sfinb",
//				Roles = new []{ ApiRole.FullAccess }
//			};
//			_ocIntegration = new OrderCloudClient(_ocConfig);
//			_ocBuyer = new OrderCloudClient(new OrderCloudClientConfig()
//            {
//				ApiUrl = "https://api.ordercloud.io",
//				AuthUrl = "https://api.ordercloud.io",
//				ClientId = "90E20A6D-F56B-4C0F-A3D3-F4476036032B",
//				GrantType = GrantType.Password,
//				Username = "abishopafowner",
//				Password = "abishop451",
//				Roles = new []{ ApiRole.Shopper }
//			});
//        }

//        [Test]
//        public async Task Test()
//        {
//			// create buyer order
//			var buyerOrder = await _ocBuyer.Orders.CreateAsync<HSOrder>(OrderDirection.Outgoing, new HSOrder()
//            {
//				ID = "__testing__SEB",
//				FromUserID = "0002-00001",
//                BillingAddressID = "0002-0003",
//                ShippingAddressID = "0002-0003",
//                ShippingCost = 322.18m,
//				TaxCost = 0m
//			});
//            await _ocBuyer.LineItems.CreateAsync(OrderDirection.Outgoing, buyerOrder.ID, new HSLineItem()
//            {
//                ID = "X001",
//                ProductID = "212769",
//                Quantity = 2,
//                ShipFromAddressID = "010-01",
//                ShippingAddressID = "0002-0003",
//                SupplierID = "010"
//            });
//            await _ocBuyer.LineItems.CreateAsync(OrderDirection.Outgoing, buyerOrder.ID, new HSLineItem()
//            {
//                ID = "X002",
//                ProductID = "212782",
//                Quantity = 1,
//                ShipFromAddressID = "010-01",
//                ShippingAddressID = "0002-0003",
//                SupplierID = "010",
//				Specs = new List<LineItemSpec>()
//                {
//					new LineItemSpec()
//                    {
//						Name = "Size",
//						OptionID = "Small",
//						Value = "Small",
//						SpecID = "212782Size"
//                    },
//					new LineItemSpec()
//                    {
//						Name = "Color",
//						Value = "Blue",
//						OptionID = "Blue",
//						SpecID = "212782Color"
//                    }
//                }
//            });
//            await _ocBuyer.LineItems.CreateAsync(OrderDirection.Outgoing, buyerOrder.ID, new HSLineItem()
//            {
//                ID = "X003",
//                ProductID = "212782",
//                Quantity = 4,
//                ShipFromAddressID = "010-01",
//                ShippingAddressID = "0002-0003",
//                SupplierID = "010",
//                Specs = new List<LineItemSpec>()
//                {
//                    new LineItemSpec()
//                    {
//                        Name = "Size",
//                        OptionID = "Large",
//                        Value = "Large",
//                        SpecID = "212782Size"
//                    },
//                    new LineItemSpec()
//                    {
//                        Name = "Color",
//                        Value = "White",
//                        OptionID = "White",
//                        SpecID = "212782Color"
//                    }
//                }
//            });
//			await _ocBuyer.LineItems.CreateAsync(OrderDirection.Outgoing, buyerOrder.ID, new HSLineItem()
//            {
//                ID = "X004",
//                ProductID = "212782",
//                Quantity = 7,
//                ShipFromAddressID = "010-01",
//                ShippingAddressID = "0002-0003",
//                SupplierID = "010",
//                Specs = new List<LineItemSpec>()
//                {
//                    new LineItemSpec()
//                    {
//                        Name = "Size",
//                        OptionID = "Medium",
//                        Value = "Medium",
//                        SpecID = "212782Size"
//                    },
//                    new LineItemSpec()
//                    {
//                        Name = "Color",
//                        Value = "Blue",
//                        OptionID = "Blue",
//                        SpecID = "212782Color"
//                    }
//                }
//            });
//			await _ocBuyer.Orders.AddPromotionAsync(OrderDirection.Outgoing, buyerOrder.ID, "zogics10");
//			Assert.That(true);

//    //        var command = new ZohoCommand(_zohoConfig, _ocConfig);
//    //        var wk = JsonConvert.DeserializeObject<HSOrderWorksheet>(SEB_1001);
//    //        try
//    //        {
//				//var zoho_salesorder = await command.CreateSalesOrder(wk);
//    //            var zoho_purchaseorder = await command.CreatePurchaseOrder(zoho_salesorder, updatedSupplierOrders);
//				//var order = await command.CreateSalesOrder(wk);
//    //        }
//    //        catch (Exception ex)
//    //        {
//    //            Console.WriteLine("hi");
//    //        }
//        }
//    }
//}

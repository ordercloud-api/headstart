namespace Headstart.Common.Models.Misc
{ 
	public enum CustomRole
	{
		// Seller/Supplier
		HSBuyerAdmin,
		HSBuyerImpersonator,
		HSCategoryAdmin,
		HSContentAdmin,
		HSMeAdmin,
		HSMeProductAdmin,
		HSMeSupplierAddressAdmin,
		HSMeSupplierAdmin,
		HSMeSupplierUserAdmin,
		HSOrderAdmin,
		HSProductAdmin,
		HSPromotionAdmin,
		HSReportAdmin,
		HSReportReader,
		HSSellerAdmin,
		HSShipmentAdmin,
		HSStorefrontAdmin,
		HSSupplierAdmin,
		HSSupplierUserGroupAdmin,

		// Buyer
		HSBaseBuyer,
		HSLocationAddressAdmin,
		HSLocationOrderApprover,
		HSLocationViewAllOrders,
		HSLocationPermissionAdmin,
		HSLocationNeedsApproval,
		HSLocationCreditCardAdmin,

		// CMS 
		AssetAdmin,
		AssetReader,
		DocumentAdmin,
		DocumentReader,
		SchemaAdmin,
		SchemaReader
	}
}
namespace Headstart.Common.Models.Misc
{ 
	public enum CustomRole
	{
		// Seller/Supplier
		HsBuyerAdmin,
		HsBuyerImpersonator,
		HsCategoryAdmin,
		HsContentAdmin,
		HsMeAdmin,
		HsMeProductAdmin,
		HsMeSupplierAddressAdmin,
		HsMeSupplierAdmin,
		HsMeSupplierUserAdmin,
		HsOrderAdmin,
		HsProductAdmin,
		HsPromotionAdmin,
		HsReportAdmin,
		HsReportReader,
		HsSellerAdmin,
		HsShipmentAdmin,
		HsStorefrontAdmin,
		HsSupplierAdmin,
		HsSupplierUserGroupAdmin,

		// Buyer
		HsBaseBuyer,
		HsLocationAddressAdmin,
		HsLocationOrderApprover,
		HsLocationViewAllOrders,
		HsLocationPermissionAdmin,
		HsLocationNeedsApproval,
		HsLocationCreditCardAdmin,

		// CMS 
		AssetAdmin,
		AssetReader,
		DocumentAdmin,
		DocumentReader,
		SchemaAdmin,
		SchemaReader
	}
}
namespace Headstart.Models.Misc
{
	public enum CustomRole
	{
		// seller/supplier
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

		// buyer
		HSBaseBuyer,
		HSLocationAddressAdmin,
		HSLocationOrderApprover,
		HSLocationViewAllOrders,
		HSLocationPermissionAdmin,
		HSLocationNeedsApproval,
		HSLocationCreditCardAdmin,

		// cms 
		AssetAdmin,
		AssetReader,
		DocumentAdmin,
		DocumentReader,
		SchemaAdmin,
		SchemaReader
	}
}

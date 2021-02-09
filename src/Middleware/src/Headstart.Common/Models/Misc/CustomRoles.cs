namespace Headstart.Models.Misc
{
	public enum CustomRole
	{
		// seller/supplier
		MPMeProductAdmin, 
		MPMeProductReader,
		MPProductAdmin,
		MeProductAdmin,
		MPProductReader,
		MPPromotionReader,
		MPContentAdmin,
		MPPromotionAdmin,
		MPCategoryAdmin,
		MPCategoryReader, 
		MPOrderAdmin,
		MPOrderReader,
		MPShipmentAdmin,
		MPBuyerAdmin, 
		MPBuyerReader,
		MPSellerAdmin,
		MPSupplierAdmin, 
		MPMeSupplierAdmin,
		MPMeSupplierAddressAdmin,
		MPMeSupplierUserAdmin,
		MPSupplierUserGroupAdmin,
		MPReportReader,
		MPReportAdmin,
		MPStoreFrontAdmin,

		// buyer
		MPBaseBuyer,
		MPLocationPermissionAdmin,
		MPLocationCreditCardAdmin,
		MPLocationAddressAdmin,
		MPLocationOrderApprover,
		MPLocationNeedsApproval,
		MPLocationViewAllOrders,
		MPLocationResaleCertAdmin,

		// cms 
		AssetAdmin,
		AssetReader,
		DocumentAdmin,
		DocumentReader,
		SchemaAdmin,
		SchemaReader
	}
}

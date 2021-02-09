using OrderCloud.SDK;
using ordercloud.integrations.avalara;
using System.Threading.Tasks;
using Headstart.Models.Misc;
using Headstart.Models;
using ordercloud.integrations.library;

namespace Headstart.API.Commands
{
    public interface IResaleCertCommand
    {
        Task<TaxCertificate> GetAsync(string locationID, VerifiedUserContext verifiedUser);
        Task<TaxCertificate> CreateAsync(string locationID, TaxCertificate cert, VerifiedUserContext verifiedUser);
        Task<TaxCertificate> UpdateAsync(string locationID, TaxCertificate cert, VerifiedUserContext verifiedUser);
    }

    public class ResaleCertCommand : IResaleCertCommand
    {
        private readonly IOrderCloudClient _oc;
        private readonly IAvalaraCommand _avalara;
        private readonly ILocationPermissionCommand _locationPermissionCommand;

        public ResaleCertCommand(IAvalaraCommand avalara, IOrderCloudClient oc, ILocationPermissionCommand locationPermissionCommand)
        {
			_oc = oc;
            _avalara = avalara;
            _locationPermissionCommand = locationPermissionCommand;
        }

        public async Task<TaxCertificate> GetAsync(string locationID, VerifiedUserContext verifiedUser)
        {
            await EnsureUserCanManageLocationResaleCert(locationID, verifiedUser);
            var buyerID = locationID.Split('-')[0];
            var address = await _oc.Addresses.GetAsync<HSAddressBuyer>(buyerID, locationID);
            if(address.xp.AvalaraCertificateID != null)
            {
                return await _avalara.GetCertificateAsync((int)address.xp.AvalaraCertificateID);
            } else
            {
                return new TaxCertificate();
            }
        }

        public async Task<TaxCertificate> CreateAsync(string locationID, TaxCertificate cert, VerifiedUserContext verifiedUser)
        {
            await EnsureUserCanManageLocationResaleCert(locationID, verifiedUser);
            var buyerID = locationID.Split('-')[0];
            var address = await _oc.Addresses.GetAsync<HSAddressBuyer>(buyerID, locationID);
            var createdCert = await _avalara.CreateCertificateAsync(cert, address);
            var newAddressXP = new
            {
                AvalaraCertificateID = createdCert.ID,
                AvalaraCertificateExpiration = createdCert.ExpirationDate
            };
            var addressPatch = new PartialAddress
            {
                xp = newAddressXP
            };
            await _oc.Addresses.PatchAsync(buyerID, locationID, addressPatch);
            return createdCert;
        }

        public async Task<TaxCertificate> UpdateAsync(string locationID, TaxCertificate cert, VerifiedUserContext verifiedUser)
        {
            await EnsureUserCanManageLocationResaleCert(locationID, verifiedUser);
            var buyerID = locationID.Split('-')[0];
            var address = await _oc.Addresses.GetAsync<HSAddressBuyer>(buyerID, locationID);
            Require.That(address.xp.AvalaraCertificateID == cert.ID, new ErrorCode("Insufficient Access", 403, $"User cannot modofiy this cert"));
            var updatedCert = await _avalara.UpdateCertificateAsync(cert.ID, cert, address);
            return updatedCert;
        }


        private async Task EnsureUserCanManageLocationResaleCert(string locationID, VerifiedUserContext verifiedUser)
        {
            var hasAccess = await _locationPermissionCommand.IsUserInAccessGroup(locationID, UserGroupSuffix.ResaleCertAdmin.ToString(), verifiedUser);
            Require.That(hasAccess, new ErrorCode("Insufficient Access", 403, $"User cannot manage resale certs for: {locationID}"));
        }
    };
}
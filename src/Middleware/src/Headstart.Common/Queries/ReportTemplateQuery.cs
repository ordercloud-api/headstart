using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cosmonaut;
using Cosmonaut.Extensions;
using Headstart.Common.Models;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using OrderCloud.Catalyst;

namespace Headstart.Common.Queries
{
    public interface IReportTemplateQuery<ReportTemplate>
    {
        Task<List<ReportTemplate>> List(ReportTypeEnum reportType, VerifiedUserContext verifiedUser);
        Task<ReportTemplate> Post(ReportTemplate reportTemplate, VerifiedUserContext verifiedUser);
        Task<ReportTemplate> Put(string id, ReportTemplate reportTemplate, VerifiedUserContext verifiedUser);
        Task Delete(string id);
        Task<ReportTemplate> Get(string id, VerifiedUserContext verifiedUser);
    }

    public class ReportTemplateQuery : IReportTemplateQuery<ReportTemplate>
    {
        private readonly ICosmosStore<ReportTemplate> _store;
        public ReportTemplateQuery(ICosmosStore<ReportTemplate> store)
        {
            _store = store;
        }

        public async Task<List<ReportTemplate>> List(ReportTypeEnum reportType, VerifiedUserContext verifiedUser)
        {
            var feedOptions = new FeedOptions() { PartitionKey = new PartitionKey($"{verifiedUser.Seller.ID}") };
            var templates = new List<ReportTemplate>();
            if (verifiedUser.UserType == "admin")
            {
                templates = await _store.Query(feedOptions).Where(x => x.ReportType == reportType).ToListAsync();
            } else if (verifiedUser.UserType == "supplier")
            {
                templates = await _store.Query(feedOptions).Where(x => x.ReportType == reportType && x.AvailableToSuppliers == true).ToListAsync();
            }
            return templates;
        }

        public async Task<ReportTemplate> Post(ReportTemplate reportTemplate, VerifiedUserContext verifiedUser)
        {
            var template = reportTemplate;
            template.SellerID = verifiedUser.Seller.ID;
            var newTemplate = await _store.AddAsync(template);
            return newTemplate;
        }

        public async Task<ReportTemplate> Put(string id, ReportTemplate reportTemplate, VerifiedUserContext verifiedUser)
        {
            var templateToPut = await _store.Query().FirstOrDefaultAsync(template => template.TemplateID == id);
            reportTemplate.id = templateToPut.id;
            var updatedTemplate = await _store.UpdateAsync(reportTemplate);
            return updatedTemplate;
        }

        public async Task Delete(string id)
        {
            await _store.RemoveAsync(template => template.TemplateID == id);
        }

        public async Task<ReportTemplate> Get(string id, VerifiedUserContext verifiedUser)
        {
            var template = await _store.Query().FirstOrDefaultAsync(template => template.TemplateID == id);
            return template;
        }
    }
}

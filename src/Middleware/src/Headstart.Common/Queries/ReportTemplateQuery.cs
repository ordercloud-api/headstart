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
using OrderCloud.SDK;

namespace Headstart.Common.Queries
{
    public interface IReportTemplateQuery<ReportTemplate>
    {
        Task<List<ReportTemplate>> List(ReportTypeEnum reportType, DecodedToken decodedToken);
        Task<ReportTemplate> Post(ReportTemplate reportTemplate, DecodedToken decodedToken);
        Task<ReportTemplate> Put(string id, ReportTemplate reportTemplate, DecodedToken decodedToken);
        Task Delete(string id);
        Task<ReportTemplate> Get(string id, DecodedToken decodedToken);
    }

    public class ReportTemplateQuery : IReportTemplateQuery<ReportTemplate>
    {
        private readonly ICosmosStore<ReportTemplate> _store;
        private readonly IOrderCloudClient _oc;
        public ReportTemplateQuery(ICosmosStore<ReportTemplate> store, IOrderCloudClient oc)
        {
            _store = store;
            _oc = oc;
        }

        public async Task<List<ReportTemplate>> List(ReportTypeEnum reportType, DecodedToken decodedToken)
        {
            var me = await _oc.Me.GetAsync(accessToken: decodedToken.AccessToken);
            var feedOptions = new FeedOptions() { PartitionKey = new PartitionKey($"{me?.Seller?.ID}") };
            var templates = new List<ReportTemplate>();
            if (decodedToken.CommerceRole == CommerceRole.Seller)
            {
                templates = await _store.Query(feedOptions).Where(x => x.ReportType == reportType).ToListAsync();
            } else if (decodedToken.CommerceRole == CommerceRole.Supplier)
            {
                templates = await _store.Query(feedOptions).Where(x => x.ReportType == reportType && x.AvailableToSuppliers == true).ToListAsync();
            }
            return templates;
        }

        public async Task<ReportTemplate> Post(ReportTemplate reportTemplate, DecodedToken decodedToken)
        {
            var me = await _oc.Me.GetAsync(accessToken: decodedToken.AccessToken);
            var template = reportTemplate;
            template.SellerID = me?.Seller?.ID;
            var newTemplate = await _store.AddAsync(template);
            return newTemplate;
        }

        public async Task<ReportTemplate> Put(string id, ReportTemplate reportTemplate, DecodedToken decodedToken)
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

        public async Task<ReportTemplate> Get(string id, DecodedToken decodedToken)
        {
            var template = await _store.Query().FirstOrDefaultAsync(template => template.TemplateID == id);
            return template;
        }
    }
}

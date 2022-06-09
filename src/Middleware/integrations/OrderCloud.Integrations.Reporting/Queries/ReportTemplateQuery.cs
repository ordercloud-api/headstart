using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cosmonaut;
using Cosmonaut.Extensions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.Reporting.Models;
using OrderCloud.SDK;

namespace OrderCloud.Integrations.Reporting.Queries
{
    public interface IReportTemplateQuery<TReportTemplate>
    {
        Task<List<TReportTemplate>> List(ReportTypeEnum reportType, DecodedToken decodedToken);

        Task<TReportTemplate> Post(TReportTemplate reportTemplate, DecodedToken decodedToken);

        Task<TReportTemplate> Put(string id, TReportTemplate reportTemplate, DecodedToken decodedToken);

        Task Delete(string id);

        Task<TReportTemplate> Get(string id, DecodedToken decodedToken);
    }

    public class ReportTemplateQuery : IReportTemplateQuery<ReportTemplate>
    {
        private readonly ICosmosStore<ReportTemplate> store;
        private readonly IOrderCloudClient oc;

        public ReportTemplateQuery(ICosmosStore<ReportTemplate> store, IOrderCloudClient oc)
        {
            this.store = store;
            this.oc = oc;
        }

        public async Task<List<ReportTemplate>> List(ReportTypeEnum reportType, DecodedToken decodedToken)
        {
            var me = await oc.Me.GetAsync(accessToken: decodedToken.AccessToken);
            var feedOptions = new FeedOptions() { PartitionKey = new PartitionKey($"{me?.Seller?.ID}") };
            var templates = new List<ReportTemplate>();
            if (decodedToken.CommerceRole == CommerceRole.Seller)
            {
                templates = await store.Query(feedOptions).Where(x => x.ReportType == reportType).ToListAsync();
            }
            else if (decodedToken.CommerceRole == CommerceRole.Supplier)
            {
                templates = await store.Query(feedOptions).Where(x => x.ReportType == reportType && x.AvailableToSuppliers == true).ToListAsync();
            }

            return templates;
        }

        public async Task<ReportTemplate> Post(ReportTemplate reportTemplate, DecodedToken decodedToken)
        {
            var me = await oc.Me.GetAsync(accessToken: decodedToken.AccessToken);
            var template = reportTemplate;
            template.SellerID = me?.Seller?.ID;
            var newTemplate = await store.AddAsync(template);
            return newTemplate;
        }

        public async Task<ReportTemplate> Put(string id, ReportTemplate reportTemplate, DecodedToken decodedToken)
        {
            var templateToPut = await store.Query().FirstOrDefaultAsync(template => template.TemplateID == id);
            reportTemplate.id = templateToPut.id;
            var updatedTemplate = await store.UpdateAsync(reportTemplate);
            return updatedTemplate;
        }

        public async Task Delete(string id)
        {
            await store.RemoveAsync(template => template.TemplateID == id);
        }

        public async Task<ReportTemplate> Get(string id, DecodedToken decodedToken)
        {
            var template = await store.Query().FirstOrDefaultAsync(template => template.TemplateID == id);
            return template;
        }
    }
}

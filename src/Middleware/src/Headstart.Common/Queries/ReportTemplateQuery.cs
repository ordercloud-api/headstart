using Cosmonaut;
using System.Linq;
using OrderCloud.SDK;
using OrderCloud.Catalyst;
using Cosmonaut.Extensions;
using System.Threading.Tasks;
using Headstart.Common.Models;
using Microsoft.Azure.Documents;
using System.Collections.Generic;
using Microsoft.Azure.Documents.Client;

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
			MeUser me = await _oc.Me.GetAsync(accessToken: decodedToken.AccessToken);
			FeedOptions feedOptions = new FeedOptions() { PartitionKey = new PartitionKey($"{me?.Seller?.ID}") };
			List<ReportTemplate> templates = new List<ReportTemplate>();
			if (decodedToken.CommerceRole == CommerceRole.Seller)
			{
				templates = await _store.Query(feedOptions).Where(x => x.ReportType == reportType).ToListAsync();
			}
			else if (decodedToken.CommerceRole == CommerceRole.Supplier)
			{
				templates = await _store.Query(feedOptions).Where(x => x.ReportType == reportType && x.AvailableToSuppliers == true).ToListAsync();
			}
			return templates;
		}

		public async Task<ReportTemplate> Post(ReportTemplate reportTemplate, DecodedToken decodedToken)
		{
			MeUser me = await _oc.Me.GetAsync(accessToken: decodedToken.AccessToken);
			ReportTemplate template = reportTemplate;
			template.SellerId = me?.Seller?.ID;
			return await _store.AddAsync(template);
		}

		public async Task<ReportTemplate> Put(string id, ReportTemplate reportTemplate, DecodedToken decodedToken)
		{
			ReportTemplate templateToPut = await _store.Query().FirstOrDefaultAsync(template => template.TemplateId == id);
			reportTemplate.id = templateToPut.id;
			return await _store.UpdateAsync(reportTemplate);
		}

		public async Task Delete(string id)
		{
			await _store.RemoveAsync(template => template.TemplateId == id);
		}

		public async Task<ReportTemplate> Get(string id, DecodedToken decodedToken)
		{
			return await _store.Query().FirstOrDefaultAsync(template => template.TemplateId == id);
		}
	}
}
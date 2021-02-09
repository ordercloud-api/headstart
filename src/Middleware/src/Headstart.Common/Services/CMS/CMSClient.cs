using Flurl;
using Flurl.Http;
using Headstart.Common.Services.CMS.Models;
using ordercloud.integrations.library;
using OrderCloud.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Headstart.Common.Services.CMS
{
	public interface ICMSClient
	{
		IAssetResource Assets { get; }
		IDocumentResource Documents { get; }
		ISchemaResource Schemas { get; }
	}

	public class CMSClientConfig 
	{
		public string BaseUrl { get; set; }
	}

	public interface IAssetResource
	{
		Task<ListPage<Asset>> List(ListArgs<Asset> args, string token);
		Task<Asset> Get(string assetID, string token);
		Task<Asset> Save(string assetID, Asset asset, string token);
		Task Delete(string assetID, string token);
		Task SaveAssetAssignment(AssetAssignment assignment, string token);
		// this one is wierd
		//Task DeleteAssetAssignment(AssetAssignment assignment, string token);
		Task<ListPage<Asset>> ListAssets(ResourceType type, string ID, ListArgsPageOnly args, string token);
		Task<ListPage<Asset>> ListAssetsOnChild(ParentResourceType parentType, string parentID, ResourceType type, string ID, ListArgsPageOnly args, string token);
	}

	public interface IDocumentResource
	{
		Task<ListPage<Document<T>>> List<T>(string schemaID, ListArgs<Document<T>> args, string token);
		Task<Document<T>> Get<T>(string schemaID, string documentID, string token);
		Task<Document<T>> Create<T>(string schemaID, Document<T> document, string token);
		Task<Document<T>> Save<T>(string schemaID, string documentID, Document<T> document, string token);
		Task Delete(string schemaID, string documentID, string token);
		Task<ListPage<DocumentAssignment>> ListAssignments(string schemaID, ListArgs<DocumentAssignment> args, string token);
		Task SaveAssignment(string schemaID, DocumentAssignment assignment, string token);
		// This one is weird
		// Task DeleteAssignment(string schemaID, DocumentAssignment assignment, string token);
		Task<ListPage<Document<T>>> ListDocuments<T>(string schemaID, ResourceType type, string ID, ListArgsPageOnly args, string token);
		Task<ListPage<Document<T>>> ListDocumentsOnChild<T>(string schemaID, ParentResourceType parentType, string parentID, ResourceType type, string ID, ListArgsPageOnly args, string token);
	}

	public interface ISchemaResource
	{
		Task<ListPage<DocSchema>> List(ListArgs<DocSchema> args, string token);
		Task<DocSchema> Get(string schemaID, string token);
		Task<DocSchema> Create(DocSchema schema, string token);
		Task<DocSchema> Save(string schemaID, DocSchema schema, string token);
		Task Delete(string schemaID, string token);
	}

	public class CMSClient : ICMSClient
	{
		public IAssetResource Assets { get; private set; }
		public IDocumentResource Documents { get; private set; }
		public ISchemaResource Schemas { get; private set; }

		public CMSClient(CMSClientConfig config)
		{
			Assets = new AssetResource(config);
			Documents = new DocumentResource(config);
			Schemas =  new SchemaResource(config);
		}
	}

	public class CMSResource
	{
		protected readonly CMSClientConfig _config;
		public CMSResource(CMSClientConfig config)
		{
			_config = config;
		}

		protected async Task<ListPage<T>> ListAsync<T>(ListArgs<T> args, string token, params string[] pathSegments)
		{
			var filters = args.Filters.Select(f => {
				var param = f.QueryParams.FirstOrDefault();
				return new { key = param.Item1, value = param.Item2 };
			});

			var queryParams = new Dictionary<string, string>()
			{
				{ "search", args.Search },
				{ "page", args.Page.ToString() },
				{ "pageSize", args.PageSize.ToString() }
			};
			if(args.SortBy.Any())
            {
				queryParams.Add("sortBy", string.Join(",", args.SortBy));
			}

			return await BuildRequest(token, pathSegments)
				.SetQueryParams(queryParams)
				.SetQueryParams(filters)
				.GetJsonAsync<ListPage<T>>();
		}

		protected async Task<ListPage<T>> ListAsync<T>(ListArgsPageOnly args, string token, params string[] pathSegments)
		{
			return await BuildRequest(token, pathSegments)
				.SetQueryParams(new { page = args.Page, pageSize = args.PageSize })
				.GetJsonAsync<ListPage<T>>();
		}

		protected async  Task<T> GetAsync<T>(string token, params string[] pathSegments)
		{
			return await BuildRequest(token, pathSegments).GetJsonAsync<T>();
		}

		protected async Task<T> PostAsync<T>(T obj, string token, params string[] pathSegments)
		{
			return await BuildRequest(token, pathSegments).PostJsonAsync(obj).ReceiveJson<T>();
		}

		protected async Task<T> PutAsync<T>(T obj, string token, params string[] pathSegments)
		{
			return await BuildRequest(token, pathSegments).PutJsonAsync(obj).ReceiveJson<T>();
		}

		protected async Task DeleteAsync(string token, params string[] pathSegments)
		{
			await BuildRequest(token,pathSegments).DeleteAsync();
		}

		private IFlurlRequest BuildRequest(string token, params string[] pathSegments)
		{
			return $"{_config.BaseUrl}/{string.Join("/", pathSegments)}".WithOAuthBearerToken(token);
		}
	}

	public class DocumentResource : CMSResource, IDocumentResource
	{
		public DocumentResource(CMSClientConfig config): base(config) {}

		public Task<ListPage<Document<T>>> List<T>(string schemaID, ListArgs<Document<T>> args, string token) => ListAsync(args, token, "schemas", schemaID, "documents");
		public Task<Document<T>> Get<T>(string schemaID, string documentID, string token) => GetAsync<Document<T>>(token, "schemas", schemaID, "documents", documentID);
		public Task<Document<T>> Create<T>(string schemaID, Document<T> document, string token) => PostAsync(document, token, "schemas", schemaID, "documents");
		public Task<Document<T>> Save<T>(string schemaID, string documentID, Document<T> document, string token) => PutAsync(document, token, "schemas", schemaID, "documents", documentID);
		public Task Delete(string schemaID, string documentID, string token) => DeleteAsync(token, "schemas", schemaID, "documents", documentID);
		public Task<ListPage<DocumentAssignment>> ListAssignments(string schemaID, ListArgs<DocumentAssignment> args, string token) => ListAsync(args, token, "schemas", schemaID, "documents", "assignments");
		public Task SaveAssignment(string schemaID, DocumentAssignment assignment, string token) => PostAsync(assignment, token, "schemas", schemaID, "documents", "assignments");
		// This one is weird
		// public Task DeleteAssignment(string schemaID, DocumentAssignment assignment, string token) => DeleteAsync(token, )
		public Task<ListPage<Document<T>>> ListDocuments<T>(string schemaID, ResourceType type, string ID, ListArgsPageOnly args, string token) =>
			ListAsync<Document<T>>(args, token, "schemas", schemaID, "documents", type.ToString(), ID);
		public Task<ListPage<Document<T>>> ListDocumentsOnChild<T>(string schemaID, ParentResourceType parentType, string parentID, ResourceType type, string ID, ListArgsPageOnly args, string token) =>
			ListAsync<Document<T>>(args, token, "schemas", schemaID, "documents", parentType.ToString(), parentID, type.ToString(), ID);

	}

	public class SchemaResource : CMSResource, ISchemaResource
	{
		public SchemaResource(CMSClientConfig config) : base(config) { }

		public Task<ListPage<DocSchema>> List(ListArgs<DocSchema> args, string token) => ListAsync(args, token, "schemas");
		public Task<DocSchema> Get(string schemaID, string token) => GetAsync<DocSchema>(token, "schemas", schemaID);
		public Task<DocSchema> Create(DocSchema schema, string token) => PostAsync(schema, token, "schemas");
		public Task<DocSchema> Save(string schemaID, DocSchema schema, string token) => PutAsync<DocSchema>(schema, token, "schemas", schemaID);
		public Task Delete(string schemaID, string token) => DeleteAsync(token, "schemas", schemaID);
	}

	public class AssetResource : CMSResource, IAssetResource
	{
		public AssetResource(CMSClientConfig config) : base(config) { }

		public Task<ListPage<Asset>> List(ListArgs<Asset> args, string token) => ListAsync(args, token, "assets");
		public Task<Asset> Get(string assetID, string token) => GetAsync<Asset>(token, "assets", assetID);
		public Task<Asset> Save(string assetID, Asset asset, string token) => PutAsync(asset, token, "assets", assetID);
		public Task Delete(string assetID, string token) => DeleteAsync(token, "assets");
		public Task SaveAssetAssignment(AssetAssignment assignment, string token) =>
			PostAsync(assignment, token, "assets", "assignments");
		// this one is wierd
		//public Task DeleteAssetAssignment(AssetAssignment assignment, string token);
		public Task<ListPage<Asset>> ListAssets(ResourceType type, string ID, ListArgsPageOnly args, string token) =>
			ListAsync<Asset>(args, token, "assets", type.ToString(), ID);
		public Task<ListPage<Asset>> ListAssetsOnChild(ParentResourceType parentType, string parentID, ResourceType type, string ID, ListArgsPageOnly args, string token) =>
			ListAsync<Asset>(args, token, "assets", parentType.ToString(), parentID, type.ToString(), ID);

	}
}

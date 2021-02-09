using System;
using System.Reflection;
using System.Threading.Tasks;
using Headstart.Common.Models;
using Newtonsoft.Json.Linq;
using Headstart.Common.Queries;
using OrderCloud.SDK;
using Action = Headstart.Common.Models.Action;
using Headstart.Common.Services.CMS;
using Headstart.Common;

namespace Headstart.Orchestration
{
    public interface IWorkItemCommand
    {
        Task<JObject> CreateAsync(WorkItem wi);
        Task<JObject> PatchAsync(WorkItem wi);
        Task<JObject> UpdateAsync(WorkItem wi);
        Task<JObject> DeleteAsync(WorkItem wi);
        Task<JObject> GetAsync(WorkItem wi);
    }

    public interface ISyncCommand
    {
        Task<JObject> Dispatch(WorkItem wi);
    }

    public class SyncCommand : ISyncCommand
    {
        private const string ASSEMBLY = "Headstart.Common.Commands.";
        protected readonly AppSettings _settings;
        protected readonly LogQuery _log;
        private readonly IOrderCloudClient _client;
        private readonly ICMSClient _cms;
        
        public SyncCommand(AppSettings settings, IOrderCloudClient client, ICMSClient cms, LogQuery log) : this(settings, client, log)
        {
            _cms = cms;
        }

        public SyncCommand(AppSettings settings, IOrderCloudClient client, LogQuery log)
        {
            _settings = settings;
            _log = log;
            _client = client;
        }

        public bool IdExists(OrderCloudException ex)
        {
            return ex.Errors[0].ErrorCode == OcError.IdExists;
        }

        public async Task<JObject> Dispatch(WorkItem wi)
        {
            if (wi.Action == Action.Ignore) return null;
            _client.Config.ClientId = wi.ClientId;
            var type = Type.GetType($"{ASSEMBLY}{wi.RecordType}SyncCommand", true);
            var constructors = type.GetConstructors()[0].GetParameters().Length;
            switch (constructors)
            {
                case 3:
                {
                    var command = (IWorkItemCommand)Activator.CreateInstance(type, _settings, _log, _client);
                    var method = command.GetType()
                        .GetMethod($"{wi.Action}Async", BindingFlags.Public | BindingFlags.Instance);
                    if (method == null) throw new MissingMethodException($"{wi.RecordType}SyncCommand is missing");
                    return await (Task<JObject>) method.Invoke(command, new object[] { wi });
                }
                case 5:
                {
                    var command = (IWorkItemCommand)Activator.CreateInstance(type, _settings, _log, _client, _cms);
                    var method = command.GetType()
                        .GetMethod($"{wi.Action}Async", BindingFlags.Public | BindingFlags.Instance);
                    if (method == null) throw new MissingMethodException($"{wi.RecordType}SyncCommand is missing");
                    return await (Task<JObject>) method.Invoke(command, new object[] { wi });
                }
                default:
                    throw new MissingMethodException($"{wi.RecordType}SyncCommand is missing");
            }
        }
    }

    public static class OcError
    {
        public static string IdExists => "IdExists";
        public static string NotFound => "NotFound";
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ordercloud.integrations.library
{
    public class ListArgsModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata.ModelType.WithoutGenericArgs() != typeof(ListArgs<>))
                return null;
            return new ListArgsModelBinder();
        }
    }

    public class ListArgsModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
                throw new ArgumentNullException(nameof(bindingContext));

            if (bindingContext.ModelType.WithoutGenericArgs() != typeof(ListArgs<>))
                return Task.CompletedTask;

            var listArgs = (IListArgs)Activator.CreateInstance(bindingContext.ModelType);
            LoadFromQueryString(bindingContext.HttpContext.Request.Query, listArgs);
            listArgs.ValidateAndNormalize();
            bindingContext.Model = listArgs;
            bindingContext.Result = ModelBindingResult.Success(listArgs);
            return Task.CompletedTask;
        }

        public virtual void LoadFromQueryString(IQueryCollection query, IListArgs listArgs)
        {
            listArgs.Filters = new List<ListFilter>();
            foreach (var (key, value) in query)
            {
                int i;
                switch (key.ToLower())
                {
                    case "sortby":
                        listArgs.SortBy = value.ToString().Split(',').Distinct().ToArray();
                        break;
                    case "page":
                        if (int.TryParse(value, out i) && i >= 1)
                            listArgs.Page = i;
                        else
                            throw new OrderCloudIntegrationException.UserErrorException("page must be an integer greater than or equal to 1.");
                        break;
                    case "pagesize":
                        if (int.TryParse(value, out i) && i >= 1 && i <= 100)
                            listArgs.PageSize = i;
                        else
                            throw new OrderCloudIntegrationException.UserErrorException($"pageSize must be an integer between 1 and 100.");
                        break;
                    case "search":
                        listArgs.Search = value.ToString();
                        break;
                    case "searchon":
                        listArgs.SearchOn = value.ToString();
                        break;
                    default:
                        listArgs.Filters.Add(ListFilter.Parse(key, value));
                        break;
                }
            }
        }
    }
}

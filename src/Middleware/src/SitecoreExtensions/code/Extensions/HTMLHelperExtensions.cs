using System;
using System.Web;
using Sitecore.Data;
using System.Web.Mvc;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Mvc.Helpers;
using System.Linq.Expressions;
using DynamicPlaceholders.Mvc.Extensions;

namespace Sitecore.Foundation.SitecoreExtensions.Extensions
{
	public static class HtmlHelperExtensions
	{
		/// <summary>
		/// Common re-usable ImageField() extension method
		/// </summary>
		/// <param name="helper"></param>
		/// <param name="fieldId"></param>
		/// <param name="mh"></param>
		/// <param name="mw"></param>
		/// <param name="cssClass"></param>
		/// <param name="disableWebEditing"></param>
		/// <returns>The ImageField HtmlString value</returns>
		public static HtmlString ImageField(this SitecoreHelper helper, ID fieldId, int mh = 0, int mw = 0, string cssClass = null, bool disableWebEditing = false)
		{
			return helper.Field(fieldId.ToString(), new
			{
				mh,
				mw,
				DisableWebEdit = disableWebEditing,
				@class = cssClass ?? ""
			});
		}

		/// <summary>
		/// Common re-usable ImageField() extension method
		/// </summary>
		/// <param name="helper"></param>
		/// <param name="fieldId"></param>
		/// <param name="item"></param>
		/// <param name="mh"></param>
		/// <param name="mw"></param>
		/// <param name="cssClass"></param>
		/// <param name="disableWebEditing"></param>
		/// <returns>The ImageField HtmlString value</returns>
		public static HtmlString ImageField(this SitecoreHelper helper, ID fieldId, Item item, int mh = 0, int mw = 0, string cssClass = null, bool disableWebEditing = false)
		{
			return helper.Field(fieldId.ToString(), item, new
			{
				mh,
				mw,
				DisableWebEdit = disableWebEditing,
				@class = cssClass ?? ""
			});
		}

		/// <summary>
		/// Common re-usable ImageField() extension method
		/// </summary>
		/// <param name="helper"></param>
		/// <param name="fieldName"></param>
		/// <param name="item"></param>
		/// <param name="mh"></param>
		/// <param name="mw"></param>
		/// <param name="cssClass"></param>
		/// <param name="disableWebEditing"></param>
		/// <returns>The ImageField HtmlString value</returns>
		public static HtmlString ImageField(this SitecoreHelper helper, string fieldName, Item item, int mh = 0, int mw = 0, string cssClass = null, bool disableWebEditing = false)
		{
			return helper.Field(fieldName, item, new
			{
				mh,
				mw,
				DisableWebEdit = disableWebEditing,
				@class = cssClass ?? ""
			});
		}

		/// <summary>
		/// Common re-usable DynamicPlaceholder() extension method
		/// </summary>
		/// <param name="helper"></param>
		/// <param name="placeholderName"></param>
		/// <param name="useStaticPlaceholderNames"></param>
		/// <returns>The DynamicPlaceholder HtmlString value</returns>
		public static HtmlString DynamicPlaceholder(this SitecoreHelper helper, string placeholderName, bool useStaticPlaceholderNames = false)
		{
			return useStaticPlaceholderNames ? helper.Placeholder(placeholderName) : SitecoreHelperExtensions.DynamicPlaceholder(helper, placeholderName);
		}

		/// <summary>
		/// Common re-usable Field() extension method
		/// </summary>
		/// <param name="helper"></param>
		/// <param name="fieldId"></param>
		/// <returns>The Field HtmlString value</returns>
		public static HtmlString Field(this SitecoreHelper helper, ID fieldId)
		{
			Assert.ArgumentNotNullOrEmpty(fieldId, nameof(fieldId));
			return helper.Field(fieldId.ToString());
		}

		/// <summary>
		/// Common re-usable Field() extension method
		/// </summary>
		/// <param name="helper"></param>
		/// <param name="fieldId"></param>
		/// <param name="parameters"></param>
		/// <returns>The Field HtmlString value</returns>
		public static HtmlString Field(this SitecoreHelper helper, ID fieldId, object parameters)
		{
			Assert.ArgumentNotNullOrEmpty(fieldId, nameof(fieldId));
			Assert.IsNotNull(parameters, nameof(parameters));
			return helper.Field(fieldId.ToString(), parameters);
		}

		/// <summary>
		/// Common re-usable Field() extension method
		/// </summary>
		/// <param name="helper"></param>
		/// <param name="fieldId"></param>
		/// <param name="item"></param>
		/// <param name="parameters"></param>
		/// <returns>The Field HtmlString value</returns>
		public static HtmlString Field(this SitecoreHelper helper, ID fieldId, Item item, object parameters)
		{
			Assert.ArgumentNotNullOrEmpty(fieldId, nameof(fieldId));
			Assert.IsNotNull(item, nameof(item));
			Assert.IsNotNull(parameters, nameof(parameters));
			return helper.Field(fieldId.ToString(), item, parameters);
		}

		/// <summary>
		/// Common re-usable Field() extension method
		/// </summary>
		/// <param name="helper"></param>
		/// <param name="fieldId"></param>
		/// <param name="item"></param>
		/// <returns>The Field HtmlString value</returns>
		public static HtmlString Field(this SitecoreHelper helper, ID fieldId, Item item)
		{
			Assert.ArgumentNotNullOrEmpty(fieldId, nameof(fieldId));
			Assert.IsNotNull(item, nameof(item));
			return helper.Field(fieldId.ToString(), item);
		}

		/// <summary>
		/// Common re-usable ValidationErrorFor() extension method
		/// </summary>
		/// <typeparam name="TModel"></typeparam>
		/// <typeparam name="TProperty"></typeparam>
		/// <param name="htmlHelper"></param>
		/// <param name="expression"></param>
		/// <param name="error"></param>
		/// <returns>The ValidationErrorFor boolean status as a MvcHtmlString value</returns>
		public static MvcHtmlString ValidationErrorFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, string error)
		{
			return htmlHelper.HasError(ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData), ExpressionHelper.GetExpressionText(expression)) ? new MvcHtmlString(error) : null;
		}

		/// <summary>
		/// Common re-usable HasError() extension method
		/// </summary>
		/// <param name="htmlHelper"></param>
		/// <param name="modelMetadata"></param>
		/// <param name="expression"></param>
		/// <returns>The HasError boolean status from the htmlHelper object</returns>
		public static bool HasError(this HtmlHelper htmlHelper, ModelMetadata modelMetadata, string expression)
		{
			var modelName = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(expression);
			var formContext = htmlHelper.ViewContext.FormContext;
			if (formContext == null)
			{
				return false;
			}

			if (!htmlHelper.ViewData.ModelState.ContainsKey(modelName))
			{
				return false;
			}

			var modelState = htmlHelper.ViewData.ModelState[modelName];
			var modelErrors = modelState?.Errors;
			return modelErrors?.Count > 0;
		}
	}
}
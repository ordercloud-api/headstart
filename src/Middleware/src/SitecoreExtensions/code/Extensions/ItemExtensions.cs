using System;
using System.Linq;
using Sitecore.Data;
using Sitecore.Links;
using Sitecore.Data.Items;
using Sitecore.Publishing;
using Sitecore.Data.Fields;
using Sitecore.Diagnostics;
using Sitecore.Data.Managers;
using Sitecore.Resources.Media;
using System.Collections.Generic;

namespace Sitecore.Foundation.SitecoreExtensions.Extensions
{
	public static class ItemExtensions
	{
		/// <summary>
		/// Common re-usable GetMediaItemImage() extension method
		/// </summary>
		/// <param name="mediaItem"></param>
		/// <param name="imageCss"></param>
		/// <returns>The Image Html string from the mediaItem Item object</returns>
		public static string GetMediaItemImage(this MediaItem mediaItem, string imageCss = "")
		{
			var imgSrcUrl = mediaItem == null ? string.Empty : StringUtil.EnsurePrefix('/', MediaManager.GetMediaUrl(mediaItem));
			var imgAlt = mediaItem == null ? string.Empty : mediaItem.Alt.Trim();
			return $@"<img class'{imageCss}' src='{imgSrcUrl}' alt='{imgAlt}' />";
		}

		/// <summary>
		/// Common re-usable ImageUrl() extension method
		/// </summary>
		/// <param name="imageField"></param>
		/// <returns>The Image Url string value from the imageField Item object</returns>
		[Obsolete]
		public static string ImageUrl(this ImageField imageField)
		{
			if (imageField?.MediaItem == null)
			{
				throw new ArgumentNullException(nameof(imageField));
			}

			var options = MediaUrlOptions.Empty;
			if (int.TryParse(imageField.Width, out var width))
			{
				options.Width = width;
			}

			if (int.TryParse(imageField.Height, out var height))
			{
				options.Height = height;
			}
			return imageField.ImageUrl(options);
		}

		/// <summary>
		/// Common re-usable ImageUrl() extension method with options
		/// </summary>
		/// <param name="imageField"></param> 
		/// <param name="options"></param>
		/// <returns>The Image Url string value from the imageField Item object</returns>
		[Obsolete]
		public static string ImageUrl(this ImageField imageField, MediaUrlOptions options)
		{
			if (imageField?.MediaItem == null)
			{
				throw new ArgumentNullException(nameof(imageField));
			}
			return options == null ? imageField.ImageUrl() : HashingUtils.ProtectAssetUrl(MediaManager.GetMediaUrl(imageField.MediaItem, options));
		}

		/// <summary>
		/// Common re-usable IsChecked() extension method
		/// </summary>
		/// <param name="checkboxField"></param>
		/// <returns>The IsChecked boolean status from the checkboxField object</returns>
		public static bool IsChecked(this Field checkboxField)
		{
			if (checkboxField == null)
			{
				throw new ArgumentNullException(nameof(checkboxField));
			}
			return MainUtil.GetBool(checkboxField.Value, false);
		}

		/// <summary>
		/// Common re-usable Url() extension method
		/// </summary>
		/// <param name="contextItem"></param>
		/// <param name="options"></param>
		/// <returns>The Url string value from the contextItem Item object</returns>
		[Obsolete]
		public static string Url(this Item contextItem, UrlOptions options = null)
		{
			if (contextItem == null) 
			{ 
				throw new ArgumentNullException(nameof(contextItem));
			}

			if (options != null)
			{
				return LinkManager.GetItemUrl(contextItem, options);
			}
			return !contextItem.Paths.IsMediaItem ? LinkManager.GetItemUrl(contextItem) : MediaManager.GetMediaUrl(contextItem);
		}

		/// <summary>
		/// Common re-usable ImageUrl() extension method
		/// </summary>
		/// <param name="contextItem"></param>
		/// <param name="imageFieldId"></param>
		/// <param name="options"></param>
		/// <returns>The ImageUrl string value from the contextItem Item object</returns>
		[Obsolete]
		public static string ImageUrl(this Item contextItem, ID imageFieldId, MediaUrlOptions options = null)
		{
			if (contextItem == null)
			{
				throw new ArgumentNullException(nameof(contextItem));
			}
			var imageField = FieldExtensions.GetImageField(contextItem, imageFieldId);
			return imageField?.MediaItem == null ? string.Empty : imageField.ImageUrl(options);
		}

		/// <summary>
		/// Common re-usable ImageUrl() extension method
		/// </summary>
		/// <param name="mediaItem"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns>The ImageUrl string value from the mediaItem Item object</returns>
		[Obsolete]
		public static string ImageUrl(this MediaItem mediaItem, int width, int height)
		{
			if (mediaItem == null)
			{
				throw new ArgumentNullException(nameof(mediaItem));
			}
			var options = new MediaUrlOptions { Height = height, Width = width };
			var url = MediaManager.GetMediaUrl(mediaItem, options);
			var cleanUrl = StringUtil.EnsurePrefix('/', url);
			var hashedUrl = HashingUtils.ProtectAssetUrl(cleanUrl);
			return hashedUrl;
		}

		/// <summary>
		/// Common re-usable TargetItem() extension method
		/// </summary>
		/// <param name="contextItem"></param>
		/// <param name="linkFieldId"></param>
		/// <returns>The TargetItem Item object from the mediaItem Item object</returns>
		public static Item TargetItem(this Item contextItem, ID linkFieldId)
		{
			if (contextItem == null)
			{
				throw new ArgumentNullException(nameof(contextItem));
			}

			if (contextItem.Fields[linkFieldId] == null || !contextItem.Fields[linkFieldId].HasValue)
			{
				return null;
			}

			var linkField = (LinkField)contextItem.Fields[linkFieldId];
			var referenceField = (ReferenceField)contextItem.Fields[linkFieldId];
			return linkField.TargetItem ?? referenceField.TargetItem;
		}

		/// <summary>
		/// Common re-usable MediaUrl() extension method
		/// </summary>
		/// <param name="contextItem"></param>
		/// <param name="mediaFieldId"></param>
		/// <param name="options"></param>
		/// <returns>The MediaUrl string value from the contextItem Item object</returns>
		[Obsolete]
		public static string MediaUrl(this Item contextItem, ID mediaFieldId, MediaUrlOptions options = null)
		{
			var targetItem = contextItem.TargetItem(mediaFieldId);
			return targetItem == null ? string.Empty : (MediaManager.GetMediaUrl(targetItem) ?? string.Empty);
		}

		/// <summary>
		/// Common re-usable IsImage() extension method
		/// </summary>
		/// <param name="contextItem"></param>
		/// <returns>The IsImage boolean status from the contextItem Item object</returns>
		public static bool IsImage(this Item contextItem)
		{
			return new MediaItem(contextItem).MimeType.StartsWith("image/", StringComparison.InvariantCultureIgnoreCase);
		}

		/// <summary>
		/// Common re-usable IsVideo() extension method
		/// </summary>
		/// <param name="contextItem"></param>
		/// <returns>The IsVideo boolean status from the contextItem Item object</returns>
		public static bool IsVideo(this Item contextItem)
		{
			return new MediaItem(contextItem).MimeType.StartsWith("video/", StringComparison.InvariantCultureIgnoreCase);
		}

		/// <summary>
		/// Common re-usable GetAncestorOrSelfOfTemplate() extension method
		/// </summary>
		/// <param name="contextItem"></param>
		/// <param name="templateId"></param>
		/// <returns>The AncestorOrSelfOfTemplate Item object from the contextItem Item object</returns>
		public static Item GetAncestorOrSelfOfTemplate(this Item contextItem, ID templateId)
		{
			if (contextItem == null)
			{
				throw new ArgumentNullException(nameof(contextItem));
			}
			return contextItem.IsDerived(templateId) ? contextItem : contextItem.Axes.GetAncestors().LastOrDefault(i => i.IsDerived(templateId));
		}

		/// <summary>
		/// Common re-usable GetAncestorsAndSelfOfTemplate() extension method
		/// </summary>
		/// <param name="contextItem"></param>
		/// <param name="templateId"></param>
		/// <returns>The List of AncestorOrSelfOfTemplate Items from the contextItem Item object</returns>
		public static IList<Item> GetAncestorsAndSelfOfTemplate(this Item contextItem, ID templateId)
		{
			if (contextItem == null)
			{
				throw new ArgumentNullException(nameof(contextItem));
			}

			var returnValue = new List<Item>();
			if (contextItem.IsDerived(templateId))
			{
				returnValue.Add(contextItem);
			}
			returnValue.AddRange(contextItem.Axes.GetAncestors().Reverse().Where(i => i.IsDerived(templateId)));
			return returnValue;
		}

		/// <summary>
		/// Common re-usable LinkFieldUrl() extension method
		/// </summary>
		/// <param name="contextItem"></param>
		/// <param name="fieldId"></param>
		/// <returns>The LinkFieldUrl string value from the contextItem Item object</returns>
		public static string LinkFieldUrl(this Item contextItem, ID fieldId)
		{
			if (contextItem == null)
			{
				throw new ArgumentNullException(nameof(contextItem));
			}
			if (ID.IsNullOrEmpty(fieldId))
			{
				throw new ArgumentNullException(nameof(fieldId));
			}

			var linkField = FieldExtensions.GetLinkField(contextItem, fieldId);
			if (linkField == null)
			{
				return string.Empty;
			}
			switch (linkField.LinkType.ToLower())
			{
				case "internal":
					// Use LinkMananger for internal links, if link is not empty
					return linkField.TargetItem != null ? LinkManager.GetItemUrl(linkField.TargetItem) : string.Empty;
				case "media":
					// Use MediaManager for media links, if link is not empty
					return linkField.TargetItem != null ? MediaManager.GetMediaUrl(linkField.TargetItem) : string.Empty;
				case "external":
					// Just return external links
					return linkField.Url;
				case "anchor":
					// Prefix anchor link with # if link if not empty
					return !string.IsNullOrEmpty(linkField.Anchor) ? "#" + linkField.Anchor : string.Empty;
				case "mailto":
					// Just return mailto link
					return linkField.Url;
				case "javascript":
					// Just return javascript
					return linkField.Url;
				default:
					// Just please the compiler, this
					// condition will never be met
					return linkField.Url;
			}
		}

		/// <summary>
		/// Common re-usable LinkFieldTarget() extension method
		/// </summary>
		/// <param name="contextItem"></param>
		/// <param name="fieldId"></param>
		/// <returns>The LinkFieldTarget string value from the contextItem Item object</returns>
		public static string LinkFieldTarget(this Item contextItem, ID fieldId)
		{
			return contextItem.LinkFieldOptions(fieldId, LinkFieldOption.Target);
		}

		/// <summary>
		/// Common re-usable LinkFieldOptions() extension method
		/// </summary>
		/// <param name="contextItem"></param>
		/// <param name="fieldId"></param>
		/// <param name="option"></param>
		/// <returns>The LinkFieldOptions string value from the contextItem Item object</returns>
		public static string LinkFieldOptions(this Item contextItem, ID fieldId, LinkFieldOption option)
		{
			XmlField field = contextItem.Fields[fieldId];
			switch (option)
			{
				case LinkFieldOption.Text:
					return field?.GetAttribute("text");
				case LinkFieldOption.LinkType:
					return field?.GetAttribute("linktype");
				case LinkFieldOption.Class:
					return field?.GetAttribute("class");
				case LinkFieldOption.Alt:
					return field?.GetAttribute("title");
				case LinkFieldOption.Target:
					return field?.GetAttribute("target");
				case LinkFieldOption.QueryString:
					return field?.GetAttribute("querystring");
				default:
					throw new ArgumentOutOfRangeException(nameof(option), option, null);
			}
		}

		/// <summary>
		/// Common re-usable HasLayout() extension method
		/// </summary>
		/// <param name="contextItem"></param>
		/// <returns>The HasLayout boolean status from the contextItem Item object</returns>
		public static bool HasLayout(this Item contextItem)
		{
			return contextItem?.Visualization?.Layout != null;
		}

		/// <summary>
		/// Common re-usable IsDerived() extension method
		/// </summary>
		/// <param name="contextItem"></param>
		/// <param name="templateId"></param>
		/// <returns>The IsDerived boolean status from the contextItem Item object</returns>
		public static bool IsDerived(this Item contextItem, ID templateId)
		{
			if (contextItem == null)
			{
				return false;
			}
			return !templateId.IsNull && contextItem.IsDerived(contextItem.Database.Templates[templateId]);
		}

		/// <summary>
		/// Common re-usable IsDerived() extension method
		/// </summary>
		/// <param name="contextItem"></param>
		/// <param name="templateItem"></param>
		/// <returns>The IsDerived boolean status from the contextItem Item object</returns>
		private static bool IsDerived(this Item contextItem, Item templateItem)
		{
			if (contextItem == null)
			{
				return false;
			}

			if (templateItem == null)
			{
				return false;
			}
			var itemTemplate = TemplateManager.GetTemplate(contextItem);
			return itemTemplate != null && (itemTemplate.ID == templateItem.ID || itemTemplate.DescendsFrom(templateItem.ID));
		}

		/// <summary>
		/// Common re-usable FieldHasValue() extension method
		/// </summary>
		/// <param name="contextItem"></param>
		/// <param name="fieldKey"></param>
		/// <returns>The FieldHasValue boolean status from the contextItem Item object</returns>
		public static bool FieldHasValue(this Item contextItem, string fieldKey)
		{
			return FieldExtensions.IsValidFieldValueByKeyHasValue(contextItem, fieldKey);
		}

		/// <summary>
		/// Common re-usable FieldHasValue() extension method
		/// </summary>
		/// <param name="contextItem"></param>
		/// <param name="fieldId"></param>
		/// <returns>The FieldHasValue boolean status from the contextItem Item object</returns>
		public static bool FieldHasValue(this Item contextItem, ID fieldId)
		{
			return FieldExtensions.IsValidFieldValueByKeyHasValue(contextItem, fieldId);
		}

		/// <summary>
		/// Common re-usable GetInteger() extension method
		/// </summary>
		/// <param name="contextItem"></param>
		/// <param name="fieldKey"></param>
		/// <returns>The Field Item's Integer value from the contextItem Item object</returns>
		public static int GetInteger(this Item contextItem, string fieldKey)
		{
			int.TryParse(FieldExtensions.GetFieldValueByKey(contextItem, fieldKey), out var result);
			return result;
		}

		/// <summary>
		/// Common re-usable GetInteger() extension method
		/// </summary>
		/// <param name="contextItem"></param>
		/// <param name="fieldId"></param>
		/// <returns>The Field Item's Integer value from the contextItem Item object</returns>
		public static int GetInteger(this Item contextItem, ID fieldId)
		{
			int.TryParse(FieldExtensions.GetFieldValueByKey(contextItem, fieldId), out var result);
			return result;
		}

		/// <summary>
		/// Common re-usable GetMultiListValueItems() extension method
		/// </summary>
		/// <param name="contextItem"></param>
		/// <param name="fieldKey"></param>
		/// <returns>The List of MultiListValueItems from the contextItem Item object</returns>
		public static IEnumerable<Item> GetMultiListValueItems(this Item contextItem, string fieldKey)
		{
			return FieldExtensions.IsValidFieldValueByKeyHasValue(contextItem, fieldKey) 
				? FieldExtensions.GetMultiListField(contextItem, fieldKey).GetItems().ToList()
				: new List<Item>();
		}

		/// <summary>
		/// Common re-usable GetMultiListValueItems() extension method
		/// </summary>
		/// <param name="contextItem"></param>
		/// <param name="fieldId"></param>
		/// <returns>The List of MultiListValueItems from the contextItem Item object</returns>
		public static IEnumerable<Item> GetMultiListValueItems(this Item contextItem, ID fieldId)
		{
			return FieldExtensions.IsValidFieldValueByKeyHasValue(contextItem, fieldId)
				? FieldExtensions.GetMultiListField(contextItem, fieldId).GetItems().ToList() 
				: new List<Item>();
		}


		/// <summary>
		/// Common re-usable GetImageFieldItem() extension method
		/// </summary>
		/// <param name="contextItem"></param>
		/// <param name="fieldKey"></param>
		/// <returns>The List of GetImageFieldItem from the contextItem Item object</returns>
		public static ImageField GetImageFieldItem(this Item contextItem, string fieldKey)
		{
			return FieldExtensions.GetImageField(contextItem, fieldKey);
		}

		/// <summary>
		/// Common re-usable GetImageFieldItem() extension method
		/// </summary>
		/// <param name="contextItem"></param>
		/// <param name="fieldId"></param>
		/// <returns>The List of GetImageFieldItem from the contextItem Item object</returns>
		public static ImageField GetImageFieldItem(this Item contextItem, ID fieldId)
		{
			return FieldExtensions.GetImageField(contextItem, fieldId);
		}

		/// <summary>
		/// Common re-usable HasContextLanguage() extension method
		/// </summary>
		/// <param name="contextItem"></param>
		/// <returns>The HasContextLanguage from the contextItem Item object</returns>
		public static bool HasContextLanguage(this Item contextItem)
		{
			var latestVersion = contextItem.Versions.GetLatestVersion();
			return latestVersion?.Versions.Count > 0;
		}

		/// <summary>
		/// Common re-usable ReferencedFieldItem() extension method
		/// </summary>
		/// <param name="contextItem"></param>
		/// <param name="fieldKey"></param>
		/// <returns>The ReferencedFieldItem object from the contextItem Item object</returns>
		public static Item ReferencedFieldItem(this Item contextItem, string fieldKey)
		{
			Item targetItem = null;
			var referenceField = FieldExtensions.GetReferenceField(contextItem, fieldKey);
			if (referenceField?.TargetItem != null)
			{
				targetItem = referenceField.TargetItem;
			}
			return targetItem;
		}

		/// <summary>
		/// Common re-usable ReferencedFieldItem() extension method
		/// </summary>
		/// <param name="contextItem"></param>
		/// <param name="fieldId"></param>
		/// <returns>The ReferencedFieldItem object from the contextItem Item object</returns>
		public static Item ReferencedFieldItem(this Item contextItem, ID fieldId)
		{
			Item targetItem = null;
			var referenceField = FieldExtensions.GetReferenceField(contextItem, fieldId);
			if (referenceField?.TargetItem != null)
			{
				targetItem = referenceField.TargetItem;
			}
			return targetItem;
		}

		/// <summary>
		/// Common re-usable FirstChildDerivedFromTemplate() extension method
		/// </summary>
		/// <param name="contextItem"></param>
		/// <param name="templateId"></param>
		/// <returns>The FirstChildDerivedFromTemplate Item or null object from the contextItem Item object</returns>
		public static Item FirstChildDerivedFromTemplate(this Item contextItem, ID templateId)
		{
			if (contextItem == null || contextItem.HasChildren != true)
			{
				return null;
			}
			foreach (Item child in contextItem.Children)
			{
				if (child.IsDerived(templateId) == true)
				{
					return child;
				}
			}
			return null;
		}

		/// <summary>
		/// Common re-usable PublishItem() extension method
		/// </summary>
		/// <param name="item"></param>
		/// <param name="publishMode"></param>
		/// <param name="dbTarget"></param>
		/// <param name="publishAsync"></param>
		/// <param name="deepPublish"></param>
		/// <param name="publishRelatedItems"></param>
		/// <param name="compareRevisions"></param>
		public static void PublishItem(this Item item, PublishMode publishMode, string dbTarget, bool publishAsync = false, bool deepPublish = false, bool publishRelatedItems = false, bool compareRevisions = false)
		{
			try
			{
				var publishOptions = new PublishOptions(item.Database, Database.GetDatabase(dbTarget), publishMode, item.Language, DateTime.Now)
				{
					RootItem = item,
					Deep = deepPublish,
					PublishRelatedItems = publishRelatedItems,
					CompareRevisions = compareRevisions
				};
				var publisher = new Publisher(publishOptions);
				if (publishAsync)
				{
					publisher.PublishAsync();
				}
				else
				{
					publisher.Publish();
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException("CustomSitecore", Helpers.GetMethodName(), @"Exception Error:", ex.Message, ex.StackTrace, new object());
			}
		}
	}

	/// <summary>
	/// LinkFieldOption enum object
	/// </summary>
	public enum LinkFieldOption
	{
		Text,
		LinkType,
		Class,
		Alt,
		Target,
		QueryString
	}
}
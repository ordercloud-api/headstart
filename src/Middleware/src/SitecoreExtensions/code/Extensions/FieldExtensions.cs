using System;
using System.Globalization;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Resources.Media;

namespace Sitecore.Foundation.SitecoreExtensions.Extensions
{
    public static class FieldExtensions
    {
        /// <summary>
        /// Common re-usable GetImageField() extension method - used to return Sitecore item's ImageField field value or null, based on passed in fieldKey value
        /// </summary>
        /// <param name="contextItem"></param>
        /// <param name="fieldKey"></param>
        /// <returns>The ImageField Item object from the contextItem Item object</returns>
        public static ImageField GetImageField(Item contextItem, string fieldKey)
        {
            ImageField field = null;
            if (IsValidFieldValueByKeyHasValue(contextItem, fieldKey))
            {
                field = !(FieldTypeManager.GetField(contextItem.Fields[fieldKey]) is ImageField) ? contextItem.Fields[fieldKey] : null;
            }
            return field;
        }

        /// <summary>
        /// Common re-usable GetImageField() extension method - used to return Sitecore item's ImageField field value or null, based on passed in fieldKey value
        /// </summary>
        /// <param name="contextItem"></param>
        /// <param name="fieldId"></param>
        /// <returns>The ImageField Item object from the contextItem Item object</returns>
        public static ImageField GetImageField(Item contextItem, ID fieldId)
        {
            ImageField field = null;
            if (IsValidFieldValueByKeyHasValue(contextItem, fieldId))
            {
                field = !(FieldTypeManager.GetField(contextItem.Fields[fieldId]) is ImageField) ? contextItem.Fields[fieldId] : null;
            }
            return field;
        }

        /// <summary>
        /// Common re-usable GetMediaItem() extension method
        /// </summary>
        /// <param name="imageField"></param>
        /// <returns>The MediaItem Item object from the contextItem Item object</returns>
        public static MediaItem GetMediaItem(ImageField imageField)
        {
            if (imageField?.MediaItem == null)
            {
                return null;
            }
            var mediaItem = new MediaItem(imageField.MediaItem);
            return mediaItem;
        }

        /// <summary>
        /// Common re-usable GetImageUrlFromItem() extension method
        /// </summary>
        /// <param name="contextItem"></param>
        /// <param name="fieldKey"></param>
        /// <returns>The Image Url string value or empty string from the contextItem Item object</returns>
        public static string GetImageUrlFromItem(Item contextItem, string fieldKey)
        {
            var imageItem = GetImageField(contextItem, fieldKey);
            if (imageItem?.MediaItem == null)
            {
                return string.Empty;
            }

            var imageMedia = new MediaItem(imageItem.MediaItem);
            var imageUrl = StringUtil.EnsurePrefix('/', MediaManager.GetMediaUrl(imageMedia));
            return imageUrl;
        }

        /// <summary>
        /// Common re-usable GetFieldValueByKey() extension method - used to return Sitecore item's field value or empty string, based on passed in fieldKey value
        /// </summary>
        /// <param name="contextItem"></param>
        /// <param name="fieldKey"></param>
        /// <returns>The contextItem's field value</returns>
        public static string GetFieldValueByKey(Item contextItem, string fieldKey)
        {
            return IsValidFieldValueByKeyHasValue(contextItem, fieldKey) ? contextItem.Fields[fieldKey].Value : string.Empty;
        }

        /// <summary>
        /// Common re-usable GetFieldValueByKey() extension method - used to return Sitecore item's field value or empty string, based on passed in fieldKey value
        /// </summary>
        /// <param name="contextItem"></param>
        /// <param name="fieldId"></param>
        /// <returns>The contextItem's field value</returns>
        public static string GetFieldValueByKey(Item contextItem, ID fieldId)
        {
            return IsValidFieldValueByKeyHasValue(contextItem, fieldId) ? contextItem.Fields[fieldId].Value : string.Empty;
        }

        /// <summary>
        /// Common re-usable GetCheckboxField() extension method - used to return Sitecore item's CheckboxField field item or null, based on passed in fieldKey value
        /// </summary>
        /// <param name="contextItem"></param>
        /// <param name="fieldKey"></param>
        /// <returns>The CheckboxField Item object from the contextItem Item object</returns>
        public static CheckboxField GetCheckboxField(Item contextItem, string fieldKey)
        {
            CheckboxField checkbox = null;
            if (IsValidFieldValueByKey(contextItem, fieldKey))
            {
                checkbox = contextItem.Fields[fieldKey];
            }
            return checkbox;
        }

        /// <summary>
        /// Common re-usable GetCheckboxField() extension method - used to return Sitecore item's CheckboxField field item or null, based on passed in fieldKey value
        /// </summary>
        /// <param name="contextItem"></param>
        /// <param name="fieldId"></param>
        /// <returns>The CheckboxField Item object from the contextItem Item object</returns>
        public static CheckboxField GetCheckboxField(Item contextItem, ID fieldId)
        {
            CheckboxField checkbox = null;
            if (IsValidFieldValueByKey(contextItem, fieldId))
            {
                checkbox = !(FieldTypeManager.GetField(contextItem.Fields[fieldId]) is CheckboxField) ? contextItem.Fields[fieldId] : null;
            }
            return checkbox;
        }

        /// <summary>
        /// Common re-usable GetDateField() extension method - used to return Sitecore item's DateField field value or null, based on passed in fieldKey value
        /// </summary>
        /// <param name="contextItem"></param>
        /// <param name="fieldKey"></param>
        /// <returns>The DateField Item object from the contextItem Item object</returns>
        public static DateField GetDateField(Item contextItem, string fieldKey)
        {
            DateField date = null;
            if (IsValidFieldValueByKey(contextItem, fieldKey))
            {
                date = !(FieldTypeManager.GetField(contextItem.Fields[fieldKey]) is DateField) ? contextItem.Fields[fieldKey] : null;
            }
            return date;
        }


        /// <summary>
        /// Common re-usable GetDateField() extension method - used to return Sitecore item's DateField field value or null, based on passed in fieldKey value
        /// </summary>
        /// <param name="contextItem"></param>
        /// <param name="fieldId"></param>
        /// <returns>The DateField Item object from the contextItem Item object</returns>
        public static DateField GetDateField(Item contextItem, ID fieldId)
        {
            DateField date = null;
            if (IsValidFieldValueByKey(contextItem, fieldId))
            {
                date = !(FieldTypeManager.GetField(contextItem.Fields[fieldId]) is DateField) ? contextItem.Fields[fieldId] : null;
            }
            return date;
        }

        /// <summary>
        /// Common re-usable GetMultiListField() extension method - used to return Sitecore item's MultilistField field value or null, based on passed in fieldKey value
        /// </summary>
        /// <param name="contextItem"></param>
        /// <param name="fieldKey"></param>
        /// <returns>The Multi-listField Item object from the contextItem Item object</returns>
        public static MultilistField GetMultiListField(Item contextItem, string fieldKey)
        {
            MultilistField field = null;
            if (IsValidFieldValueByKey(contextItem, fieldKey))
            {
                field = !(FieldTypeManager.GetField(contextItem.Fields[fieldKey]) is MultilistField) ? contextItem.Fields[fieldKey] : null;
            }
            return field;
        }

        /// <summary>
        /// Common re-usable GetMultiListField() extension method - used to return Sitecore item's MultilistField field value or null, based on passed in fieldKey value
        /// </summary>
        /// <param name="contextItem"></param>
        /// <param name="fieldId"></param>
        /// <returns>The Multi-listField Item object from the contextItem Item object</returns>
        public static MultilistField GetMultiListField(Item contextItem, ID fieldId)
        {
            MultilistField field = null;
            if (IsValidFieldValueByKey(contextItem, fieldId))
            {
                field = !(FieldTypeManager.GetField(contextItem.Fields[fieldId]) is MultilistField) ? contextItem.Fields[fieldId] : null;
            }
            return field;
        }

        /// <summary>
        /// Common re-usable GetLinkField() extension method - used to return Sitecore item's LinkField field value or null, based on passed in fieldKey value
        /// </summary>
        /// <param name="contextItem"></param>
        /// <param name="fieldKey"></param>
        /// <returns>The LinkField Item object from the contextItem Item object</returns>
        public static LinkField GetLinkField(Item contextItem, string fieldKey)
        {
            LinkField field = null;
            if (IsValidFieldValueByKey(contextItem, fieldKey))
            {
                field = !(FieldTypeManager.GetField(contextItem.Fields[fieldKey]) is LinkField) ? contextItem.Fields[fieldKey] : null;
            }
            return field;
        }

        /// <summary>
        /// Common re-usable GetLinkField() extension method - used to return Sitecore item's LinkField field value or null, based on passed in fieldKey value
        /// </summary>
        /// <param name="contextItem"></param>
        /// <param name="fieldId"></param>
        /// <returns>The LinkField Item object from the contextItem Item object</returns>
        public static LinkField GetLinkField(Item contextItem, ID fieldId)
        {
            LinkField field = null;
            if (IsValidFieldValueByKey(contextItem, fieldId))
            {
                field = !(FieldTypeManager.GetField(contextItem.Fields[fieldId]) is LinkField) ? contextItem.Fields[fieldId] : null;
            }
            return field;
        }


        /// <summary>
        /// Common re-usable GetReferenceField() extension method - used to return Sitecore item's LinkField field value or null, based on passed in fieldKey value
        /// </summary>
        /// <param name="contextItem"></param>
        /// <param name="fieldKey"></param>
        /// <returns>The ReferenceField Item object from the contextItem Item object</returns>
        public static ReferenceField GetReferenceField(Item contextItem, string fieldKey)
        {
            ReferenceField field = null;
            if (IsValidFieldValueByKey(contextItem, fieldKey))
            {
                field = !(FieldTypeManager.GetField(contextItem.Fields[fieldKey]) is ReferenceField) ? contextItem.Fields[fieldKey] : null;
            }
            return field;
        }

        /// <summary>
        /// Common re-usable GetReferenceField() extension method - used to return Sitecore item's LinkField field value or null, based on passed in fieldKey value
        /// </summary>
        /// <param name="contextItem"></param>
        /// <param name="fieldId"></param>
        /// <returns>The ReferenceField Item object from the contextItem Item object</returns>
        public static ReferenceField GetReferenceField(Item contextItem, ID fieldId)
        {
            ReferenceField field = null;
            if (IsValidFieldValueByKey(contextItem, fieldId))
            {
                field = !(FieldTypeManager.GetField(contextItem.Fields[fieldId]) is ReferenceField) ? contextItem.Fields[fieldId] : null;
            }
            return field;
        }

        /// <summary>
        /// Common re-usable GetLinkFieldTextAndUrl() extension method
        /// </summary>
        /// <param name="contextItem"></param>
        /// <param name="fieldKey"></param>
        /// <param name="linkText"></param>
        /// <param name="linkUrl"></param>
        /// <returns>The linkText and linkUrl output params from the contextItem Item object</returns>
        public static void GetLinkFieldTextAndUrl(Item contextItem, string fieldKey, out string linkText, out string linkUrl)
        {
            linkText = string.Empty;
            linkUrl = string.Empty;
            var field = GetLinkField(contextItem, fieldKey);
            if (field == null)
            {
                return;
            }
            linkText = field.Text;
            linkUrl = field.GetFriendlyUrl();
        }

        /// <summary>
        /// Common re-usable GetLinkField() extension method
        /// </summary>
        /// <param name="contextItem"></param>
        /// <param name="fieldKey"></param>
        /// <returns>The Link Field Url value from the contextItem Item object</returns>
        public static string GetLinkFieldUrl(Item contextItem, string fieldKey)
        {
            var linkUrl = string.Empty;
            var field = GetLinkField(contextItem, fieldKey);
            if (field != null)
            {
                linkUrl = field.GetFriendlyUrl();
            }
            return linkUrl;
        }

        /// <summary>
        /// Common re-usable IsValidFieldValueByKey() extension method - used to return true if a Sitecore item has a field based on passed in fieldKey value
        /// or false if it doe not have a field based on passed in fieldKey value
        /// </summary>
        /// <param name="contextItem"></param>
        /// <param name="fieldKey"></param>
        /// <returns>The IsValidFieldValueByKey boolean status from the contextItem Item object</returns>
        public static bool IsValidFieldValueByKey(Item contextItem, string fieldKey)
        {
            return contextItem?.Fields[fieldKey] != null;
        }

        /// <summary>
        /// Common re-usable IsValidFieldValueByKey() extension method - used to return true if a Sitecore item has a field based on passed in fieldKey value
        /// or false if it does not have a field based on passed in fieldKey value
        /// </summary>
        /// <param name="contextItem"></param>
        /// <param name="fieldKey"></param>
        /// <returns>The IsValidFieldValueByKeyHasValue boolean status from the contextItem Item object</returns>
        public static bool IsValidFieldValueByKeyHasValue(Item contextItem, string fieldKey)
        {
            return IsValidFieldValueByKey(contextItem, fieldKey) && contextItem.Fields[fieldKey].HasValue;
        }

        /// <summary>
        /// Common re-usable IsValidFieldValueByKey() extension method - used to return true if a Sitecore item has a field based on passed in fieldKey value
        /// or false if it does not have a field based on passed in fieldKey value
        /// </summary>
        /// <param name="contextItem"></param>
        /// <param name="fieldId"></param>
        /// <returns>The IsValidFieldValueByKey boolean status from the contextItem Item object</returns>
        public static bool IsValidFieldValueByKey(Item contextItem, ID fieldId)
        {
            return contextItem?.Fields[fieldId] != null;
        }

        /// <summary>
        /// Common re-usable IsValidFieldValueByKey() extension method - used to return true if a Sitecore item has a field based on passed in fieldKey value
        /// or false if it does not have a field based on passed in fieldKey value
        /// </summary>
        /// <param name="contextItem"></param>
        /// <param name="fieldId"></param>
        /// <returns>The IsValidFieldValueByKeyHasValue boolean status from the contextItem Item object</returns>
        public static bool IsValidFieldValueByKeyHasValue(Item contextItem, ID fieldId)
        {
            return contextItem?.Fields[fieldId] != null && contextItem.Fields[fieldId].HasValue;
        }

        /// <summary>
        /// Common re-usable SetFieldValueByKey() extension method - used to set Sitecore item's field value, based on passed in fieldKey value 
        /// and fieldValue string value
        /// </summary>
        /// <param name="contextItem"></param>
        /// <param name="fieldKey"></param>
        /// <param name="fieldValue"></param>
        /// <returns>The contextItem after setting the contextItem's field value</returns>
        public static Item SetFieldValueByKey(Item contextItem, string fieldKey, string fieldValue)
        {
            if (IsValidFieldValueByKey(contextItem, fieldKey) && (!string.IsNullOrEmpty(fieldValue)))
            {
                contextItem.Fields[fieldKey].Value = fieldValue;
            }
            return contextItem;
        }

        /// <summary>
        /// Common re-usable SetFieldValueByKey() extension method - used to set Sitecore item's field value, based on passed in fieldKey value 
        /// and fieldValue datetime value
        /// </summary>
        /// <param name="contextItem"></param>
        /// <param name="fieldKey"></param>
        /// <param name="fieldValue"></param>
        /// <returns>The contextItem after setting the contextItem's field value</returns>
        public static Item SetFieldValueByKey(Item contextItem, string fieldKey, DateTime fieldValue)
        {
            var date = GetDateField(contextItem, fieldKey);
            if (date == null || !IsValidDateTimeValue(fieldValue))
            {
                return contextItem;
            }

            var dateChanged = date.Value == fieldValue.ToString(@"MM/dd/yyyy");
            date.Value = dateChanged ? fieldValue.ToString(@"MM/dd/yyyy") : date.Value;
            return contextItem;
        }

        /// <summary>
        /// Common re-usable SetFieldValueByKey() extension method - used to set Sitecore item's field value, based on passed in fieldKey value 
        /// and fieldValue boolean value
        /// </summary>
        /// <param name="contextItem"></param>
        /// <param name="fieldKey"></param>
        /// <param name="fieldValue"></param>
        /// <returns>The contextItem after setting the contextItem's field value</returns>
        public static Item SetFieldValueByKey(Item contextItem, string fieldKey, bool fieldValue)
        {
            var checkbox = GetCheckboxField(contextItem, fieldKey);
            if (checkbox != null)
            {
                checkbox.Checked = (checkbox.Checked != fieldValue) ? fieldValue : checkbox.Checked;
            }
            return contextItem;
        }

        /// <summary>
        /// Common re-usable SetFieldValueByKey() extension method - used to return true if passed in fieldValue value is a valid datetime value 
        /// and false if it is not a valid datetime value
        /// </summary>
        /// <param name="fieldValue"></param>
        /// <returns>The IsValidDateTimeValue boolean status</returns>
        private static bool IsValidDateTimeValue(DateTime fieldValue)
        {
            return fieldValue != DateTime.MinValue && !string.IsNullOrEmpty(fieldValue.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Common re-usable InsertNoFollowStatus() extension method
        /// </summary>
        /// <param name="anchorUrl"></param>
        /// <param name="qsKey"></param>
        /// <returns>The rel="nofollow" attribute or empty string</returns>
        public static string InsertNoFollowStatus(string anchorUrl, string qsKey)
        {
            var nofollowAttribute = string.Empty;
            var attrExists = ((!string.IsNullOrEmpty(anchorUrl)) && (!string.IsNullOrEmpty(qsKey)));
            if (!attrExists)
            {
                return nofollowAttribute;
            }

            var hasNoFollow = anchorUrl.ToLower().Trim().IndexOf(qsKey.ToLower().Trim(), StringComparison.Ordinal) > -1;
            if (hasNoFollow)
            {
                nofollowAttribute = qsKey.ToLower().Trim();
            }
            return nofollowAttribute;
        }

        /// <summary>
        /// Common re-usable HyperLinkFor() extension method - returns anchorElement or empty string
        /// </summary>
        /// <param name="anchorText"></param>
        /// <param name="anchorUrl"></param>
        /// <param name="anchorId"></param>
        /// <param name="anchorCss"></param>
        /// <param name="anchorAttributes"></param>
        /// <returns>The Hyperlink Html string or empty string</returns>
        public static string HyperLinkFor(string anchorText, string anchorUrl, string anchorId, string anchorCss = "", string anchorAttributes = "")
        {
            if (string.IsNullOrEmpty(anchorText) || string.IsNullOrEmpty(anchorUrl) || string.IsNullOrEmpty(anchorId))
            {
                return string.Empty;
            }
            return $@"<a href='{anchorUrl}' pageId='{anchorId}' class='{anchorCss}' {anchorAttributes}>{anchorText}</a>";
        }

        /// <summary>
        /// Common re-usable GetRitchTextContent() extension method - returns HtmlString or empty string
        /// </summary>
        /// <param name="contextItem"></param>
        /// <param name="fieldKey"></param>
        /// <returns>The Html string from the contextItem Item object</returns>
        public static string GetRitchTextContent(Item contextItem, string fieldKey)
        {
            var ritchTextContent = GetFieldValueByKey(contextItem, fieldKey);
            return string.IsNullOrEmpty(ritchTextContent.GetCleanRitchTextContent()) ? string.Empty : ritchTextContent;
        }

        /// <summary>
        /// Common re-usable ImageFor() extension method - returns imageElement or empty string
        /// </summary>
        /// <param name="contextItem"></param>
        /// <param name="fieldKey"></param>
        /// <param name="imageCss"></param>
        /// <returns>The Image Html string from the contextItem Item object</returns>
        public static string ImageFor(Item contextItem, string fieldKey, string imageCss)
        {
            if (contextItem == null || string.IsNullOrEmpty(fieldKey) || !IsValidFieldValueByKey(contextItem, fieldKey))
            {
                return string.Empty;
            }

            var imageItem = GetImageField(contextItem, fieldKey);
            var menuLogoUrl = imageItem == null ? string.Empty : imageItem.ImageUrl();
            var menuLogoAltText = imageItem == null ? string.Empty : imageItem.Alt.Trim();
            var pageId = imageItem == null ? string.Empty : imageItem.MediaID.Guid.ToString().RemoveSpecifiedChars("[{}]", true);
            return $@"<img src='{menuLogoUrl}' pageId='{pageId}' class='{imageCss}' alt='{menuLogoAltText}' />";
        }

        /// <summary>
        /// Common re-usable ImageLinkFor() extension method - returns a linkable imageElement or empty string
        /// </summary>
        /// <param name="contextItem"></param>
        /// <param name="fieldKey"></param>
        /// <param name="imageCss"></param>
        /// <param name="anchorUrl"></param>
        /// <param name="anchorId"></param>
        /// <param name="achorCss"></param>
        /// <returns>The Image Link Html string from the contextItem Item object</returns>
        public static string ImageLinkFor(Item contextItem, string fieldKey, string imageCss, string anchorUrl, string anchorId, string achorCss = "")
        {
            var imageLinkedElement = string.Empty;
            var imageElement = ImageFor(contextItem, fieldKey, imageCss);
            if (string.IsNullOrEmpty(imageElement))
            {
                return imageLinkedElement;
            }

            var imageItem = GetImageField(contextItem, fieldKey);
            var menuLogoAltText = (imageItem != null) ? imageItem.Alt : string.Empty;
            imageLinkedElement = HyperLinkFor(imageElement, anchorUrl, anchorId, achorCss, $@"aria-label='{menuLogoAltText}'");
            return imageLinkedElement;
        }

        /// <summary>
        /// Common re-usable IsItemForScheduledDisplay() extension method
        /// </summary>
        /// <param name="id"></param>
        /// <returns>The IsItemForScheduledDisplay boolean status</returns>
        public static bool IsItemForScheduledDisplay(ID id)
        {
            DateTime? startDateTime = null, endDateTime = null;
            if (id.IsNull)
            {
                return false;
            }

            var item = Context.Database.GetItem(id);
            if (IsValidFieldValueByKeyHasValue(item, @"Display Start DateTime") && IsValidFieldValueByKeyHasValue(item, @"Display End DateTime"))
            {
                startDateTime = new DateTime(GetDateField(item, @"Display Start DateTime").DateTime.Year, GetDateField(item, @"Display Start DateTime").DateTime.Month,
                    GetDateField(item, @"Display Start DateTime").DateTime.Day, GetDateField(item, @"Display Start DateTime").DateTime.ToLocalTime().Hour,
                    GetDateField(item, @"Display Start DateTime").DateTime.ToLocalTime().Minute, 0);
                endDateTime = new DateTime(GetDateField(item, @"Display End DateTime").DateTime.Year, GetDateField(item, "Display End DateTime").DateTime.Month,
                    GetDateField(item, @"Display End DateTime").DateTime.Day, GetDateField(item, @"Display End DateTime").DateTime.ToLocalTime().Hour,
                    GetDateField(item, @"Display End DateTime").DateTime.ToLocalTime().Minute, 0);
            }
            var currentDateTime = DateTime.Now;
            return currentDateTime >= startDateTime && currentDateTime <= endDateTime;
        }

        /// <summary>
        /// Common re-usable GetSelectedItemFromDroplistField() extension method
        /// </summary>
        /// <param name="contextItem"></param>
        /// <param name="fieldKey"></param>
        /// <returns>The Item object in a DroplistField Item object</returns>
        public static Item GetSelectedItemFromDroplistField(Item contextItem, string fieldKey)
        {
            var field = contextItem.Fields[fieldKey];
            if (field == null || string.IsNullOrEmpty(field.Value))
            {
                return null;
            }

            var fieldSource = field.Source ?? string.Empty;
            var selectedItemPath = $@"{fieldSource.TrimEnd('/')}/{field.Value}";
            return contextItem.Database.GetItem(selectedItemPath);
        }

        /// <summary>
        ///  Common re-usable GetDroplistDictionaryValue() extension method
        /// </summary>
        /// <param name="contextItem"></param>
        /// <param name="fieldKey"></param>
        /// <param name="isGuidValue"></param>
        /// <returns>The DropListItemVal string value or empty</returns>
        public static string GetDroplistDictionaryValue(Item contextItem, string fieldKey, bool isGuidValue = false)
        {
            var dropListItemVal = string.Empty;
            var dropListItem = GetSelectedItemFromDroplistField(contextItem, fieldKey);
            if (dropListItem == null)
            {
                return dropListItemVal;
            }
            dropListItemVal = GetFieldValueByKey(dropListItem, @"Phrase");
            if (!isGuidValue)
            {
                return dropListItemVal;
            }
            ID.TryParse(dropListItemVal, out var templateId);
            dropListItemVal = (!templateId.IsNull) ? templateId.ToString().Trim() : string.Empty;
            return dropListItemVal;
        }
    }
}
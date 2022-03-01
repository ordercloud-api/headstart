namespace Sitecore.Foundation.SitecoreExtensions.Extensions
{
    using System;
    using Sitecore.Data;
    using Sitecore.Data.Items;
    using Sitecore.Data.Fields;
    using Sitecore.Resources.Media;

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
                field = (ImageField)contextItem.Fields[fieldKey];
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
            MediaItem mediaItem = null;
            if (imageField != null && imageField.MediaItem != null)
            {
                mediaItem = new MediaItem(imageField.MediaItem);
            }
            return mediaItem;
        }

        /// <summary>
        /// Common re-usable GetImageUrlFromItem() extension method
        /// </summary>
        /// <param name="item"></param>
        /// <param name="fieldKey"></param>
        /// <returns>The Image Url string value or empty string from the contextItem Item object</returns>
        public static string GetImageUrlFromItem(Item contextItem, string fieldKey)
        {
            string imageUrl = string.Empty;
            if (IsValidFieldValueByKeyHasValue(contextItem, fieldKey))
            {
                var imageItem = GetImageField(contextItem, fieldKey);
                if (imageItem != null && imageItem.MediaItem != null)
                {
                    MediaItem imageMedia = new MediaItem(imageItem.MediaItem);
                    imageUrl = StringUtil.EnsurePrefix('/', MediaManager.GetMediaUrl(imageMedia));
                }
            }            
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
            return (IsValidFieldValueByKey(contextItem, fieldKey)) ? contextItem.Fields[fieldKey].Value : string.Empty;
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
        /// Common re-usable GetDateField() extension method - used to return Sitecore item's DateField field value or null, based on passed in fieldKey value
        /// </summary>
        /// <param name="contextItem"></param>
        /// <param name="fieldKey"></param>
        /// <returns>The DateField Item object from the contextItem Item object</returns>
        public static DateField GetDateField(Item contextItem, string fieldKey)
        {
            DateField date = null;
            if (IsValidFieldValueByKeyHasValue(contextItem, fieldKey))
            {
                date = contextItem.Fields[fieldKey];
            }
            return date;
        }

        /// <summary>
        /// Common re-usable GetMultiListField() extension method - used to return Sitecore item's MultilistField field value or null, based on passed in fieldKey value
        /// </summary>
        /// <param name="contextItem"></param>
        /// <param name="fieldKey"></param>
        /// <returns>The MultilistField Item object from the contextItem Item object</returns>
        public static MultilistField GetMultiListField(Item contextItem, string fieldKey)
        {
            MultilistField field = null;
            if (IsValidFieldValueByKey(contextItem, fieldKey))
            {
                field = (MultilistField)contextItem.Fields[fieldKey];
            }
            return field;
        }

        /// <summary>
        /// Common re-usable GetMultiListField() extension method - used to return Sitecore item's MultilistField field value or null, based on passed in fieldKey value
        /// </summary>
        /// <param name="contextItem"></param>
        /// <param name="fieldId"></param>
        /// <returns>The MultilistField Item object from the contextItem Item object</returns>
        public static MultilistField GetMultiListField(Item contextItem, ID fieldId)
        {
            MultilistField field = null;
            if (IsValidFieldValueByKey(contextItem, fieldId))
            {
                field = (MultilistField)contextItem.Fields[fieldId];
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
            LinkField field = (IsValidFieldValueByKey(contextItem, fieldKey))
                ? ((LinkField)contextItem.Fields[fieldKey]) : null;
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

            LinkField field = GetLinkField(contextItem, fieldKey);
            if (field != null)
            {
                linkText = field.Text;
                linkUrl = field.GetFriendlyUrl();
            }
        }

        /// <summary>
        /// Common re-usable GetLinkField() extension method
        /// </summary>
        /// <param name="contextItem"></param>
        /// <param name="fieldKey"></param>
        /// <returns>The Link Field Url value from the contextItem Item object</returns>
        public static string GetLinkFieldUrl(Item contextItem, string fieldKey)
        {
            string linkUrl = string.Empty;
            LinkField field = GetLinkField(contextItem, fieldKey);
            if (field != null)
            {
                linkUrl = field.GetFriendlyUrl();
            }
            return linkUrl;
        }

        /// <summary>
        /// Common re-usable IsValidFieldValueByKey() extension method - used to return true if a Sitecore item has a field based on passed in fieldKey value
        /// or false if it doee not have a field based on passed in fieldKey value
        /// </summary>
        /// <param name="contextItem"></param>
        /// <param name="fieldKey"></param>
        /// <returns>The IsValidFieldValueByKey boolean status from the contextItem Item object</returns>
        public static bool IsValidFieldValueByKey(Item contextItem, string fieldKey)
        {
            return ((contextItem != null) && (contextItem.Fields[fieldKey] != null)) ? true : false;
        }

        /// <summary>
        /// Common re-usable IsValidFieldValueByKey() extension method - used to return true if a Sitecore item has a field based on passed in fieldKey value
        /// or false if it doee not have a field based on passed in fieldKey value
        /// </summary>
        /// <param name="contextItem"></param>
        /// <param name="fieldKey"></param>
        /// <returns>The IsValidFieldValueByKeyHasValue boolean status from the contextItem Item object</returns>
        public static bool IsValidFieldValueByKeyHasValue(Item contextItem, string fieldKey)
        {
            return ((contextItem != null) && (contextItem.Fields[fieldKey] != null) && (contextItem.Fields[fieldKey].HasValue)) ? true : false;
        }

        /// <summary>
        /// Common re-usable IsValidFieldValueByKey() extension method - used to return true if a Sitecore item has a field based on passed in fieldKey value
        /// or false if it doee not have a field based on passed in fieldKey value
        /// </summary>
        /// <param name="contextItem"></param>
        /// <param name="fieldId"></param>
        /// <returns>The IsValidFieldValueByKey boolean status from the contextItem Item object</returns>
        public static bool IsValidFieldValueByKey(Item contextItem, ID fieldId)
        {
            return ((contextItem != null) && (contextItem.Fields[fieldId] != null)) ? true : false;
        }

        /// <summary>
        /// Common re-usable IsValidFieldValueByKey() extension method - used to return true if a Sitecore item has a field based on passed in fieldKey value
        /// or false if it doee not have a field based on passed in fieldKey value
        /// </summary>
        /// <param name="contextItem"></param>
        /// <param name="fieldId"></param>
        /// <returns>The IsValidFieldValueByKeyHasValue boolean status from the contextItem Item object</returns>
        public static bool IsValidFieldValueByKeyHasValue(Item contextItem, ID fieldId)
        {
            return ((contextItem != null) && (contextItem.Fields[fieldId] != null) && (contextItem.Fields[fieldId].HasValue)) ? true : false;
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
            DateField date = GetDateField(contextItem, fieldKey);
            if ((date != null) && (IsValidDateTimeValue(fieldValue)))
            {
                var dateChanged = (date.Value.ToString() == fieldValue.ToString($@"MM/dd/yyyy")) ? true : false;
                date.Value = (dateChanged) ? fieldValue.ToString($@"MM/dd/yyyy") : date.Value;
            }
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
            CheckboxField checkbox = GetCheckboxField(contextItem, fieldKey);
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
            if ((fieldValue != null) && (fieldValue != DateTime.MinValue) && (!string.IsNullOrEmpty(fieldValue.ToString())))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Common re-usable InsertNoFollowStatus() extension method
        /// </summary>
        /// <param name="anchorUrl"></param>
        /// <param name="qsKey"></param>
        /// <returns>The rel="nofollow" attribute or empty string</returns>
        public static string InsertNoFollowStatus(string anchorUrl, string qsKey)
        {
            string nofollowAttribute = string.Empty;
            bool attrExists = ((!string.IsNullOrEmpty(anchorUrl)) && (!string.IsNullOrEmpty(qsKey)));
            if (attrExists)
            {
                bool hasNoFollow = (anchorUrl.ToLower().Trim().IndexOf(qsKey.ToLower().Trim()) > -1);
                if (hasNoFollow)
                {
                    nofollowAttribute = qsKey.ToLower().Trim();
                }
            }
            return nofollowAttribute;
        }

        /// <summary>
        /// Common re-usable HyperLinkFor() extension method - returns achorElement or empty string
        /// </summary>
        /// <param name="achorText"></param>
        /// <param name="anchorUrl"></param>
        /// <param name="anchorId"></param>
        /// <param name="achorCSS"></param>
        /// <param name="achorAttributes"></param>
        /// <returns>The Hyperlink Html string or empty string</returns>
        public static string HyperLinkFor(string achorText, string anchorUrl, string anchorId, string achorCSS, string achorAttributes)
        {
            string achorElement = "<a href='{0}' pageid='{1}' class='{2}' {3}>{4}</a>";
            if (string.IsNullOrEmpty(achorText) || string.IsNullOrEmpty(anchorUrl) || string.IsNullOrEmpty(anchorId))
            {
                return string.Empty;
            }
            else
            {
                achorElement = string.Format(achorElement, anchorUrl, anchorId, achorCSS, achorAttributes, achorText);
            }
            return  achorElement;
        }

        /// <summary>
        /// Common re-usable GetRitchTextContent() extension method - returns HtmlString or empty string
        /// </summary>
        /// <param name="contextItem"></param>
        /// <param name="fieldKey"></param>
        /// <returns>The Html string from the contextItem Item object</returns>
        public static string GetRitchTextContent(Item contextItem, string fieldKey)
        {
            string ritchTextContent = GetFieldValueByKey(contextItem, fieldKey);
            if (string.IsNullOrEmpty(ritchTextContent.GetCleanRitchTextContent()))
            {
                return string.Empty;
            }
            return ritchTextContent;
        }

        /// <summary>
        /// Common re-usable ImageFor() extension method - returns imageElement or empty string
        /// </summary>
        /// <param name="contextItem"></param>
        /// <param name="fieldKey"></param>
        /// <param name="imageCSS"></param>
        /// <returns>The Image Html string from the contextItem Item object</returns>
        public static string ImageFor(Item contextItem, string fieldKey, string imageCSS)
        {
            string imageElement = "<img src='{0}' pageid='{1}' class='{2}' alt='{3}'></>";
            if (contextItem == null || string.IsNullOrEmpty(fieldKey) || !IsValidFieldValueByKey(contextItem, fieldKey))
            {
                return string.Empty;
            }
            else
            {
                var imageItem = GetImageField(contextItem, fieldKey);
                string menuLogoUrl = (imageItem != null) ? imageItem.ImageUrl() : string.Empty;
                string menuLogoAltText = (imageItem != null) ? imageItem.Alt : string.Empty;
                imageElement = string.Format(imageElement, menuLogoUrl, imageItem.MediaID, imageCSS, menuLogoAltText);
            }
            return imageElement;
        }

        /// <summary>
        /// Common re-usable ImageLinkFor() extension method - returns a linkable imageElement or empty string
        /// </summary>
        /// <param name="contextItem"></param>
        /// <param name="fieldKey"></param>
        /// <param name="imageCSS"></param>
        /// <param name="anchorUrl"></param>
        /// <param name="anchorId"></param>
        /// <param name="achorCSS"></param>
        /// <returns>The Image Link Html string from the contextItem Item object</returns>
        public static string ImageLinkFor(Item contextItem, string fieldKey, string imageCSS, string anchorUrl, string anchorId, string achorCSS = "")
        {
            string imageElement = ImageFor(contextItem, fieldKey, imageCSS);
            string imageLinkedElement = string.Empty;
            
            if (!string.IsNullOrEmpty(imageElement))
            {
                var imageItem = GetImageField(contextItem, fieldKey);
                string menuLogoAltText = (imageItem != null) ? imageItem.Alt : string.Empty;
                imageLinkedElement = HyperLinkFor(imageElement, anchorUrl, anchorId, achorCSS, string.Format(@"aria-label='{0}'", menuLogoAltText));
            }
            return imageLinkedElement;
        }

        /// <summary>
        /// Common re-usable IsItemForScheduledDisplay() extension method
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>The IsItemForScheduledDisplay boolean status</returns>
        public static bool IsItemForScheduledDisplay(ID Id)
        {
            DateTime? startDateTime = null, endDateTime = null;
            bool retValue = false;
            if (!Id.IsNull)
            {
                var item = Context.Database.GetItem(Id);

                if (IsValidFieldValueByKeyHasValue(item, "Display Start DateTime") && IsValidFieldValueByKeyHasValue(item, "Display End DateTime"))
                {
                    startDateTime = new DateTime(GetDateField(item, "Display Start DateTime").DateTime.Year, GetDateField(item, "Display Start DateTime").DateTime.Month,
                        GetDateField(item, "Display Start DateTime").DateTime.Day, GetDateField(item, "Display Start DateTime").DateTime.ToLocalTime().Hour,
                        GetDateField(item, "Display Start DateTime").DateTime.ToLocalTime().Minute, 0);
                    endDateTime = new DateTime(GetDateField(item, "Display End DateTime").DateTime.Year, GetDateField(item, "Display End DateTime").DateTime.Month,
                        GetDateField(item, "Display End DateTime").DateTime.Day, GetDateField(item, "Display End DateTime").DateTime.ToLocalTime().Hour,
                        GetDateField(item, "Display End DateTime").DateTime.ToLocalTime().Minute, 0);
                }

                var currentDateTime = DateTime.Now;
                retValue = (currentDateTime >= startDateTime && currentDateTime <= endDateTime);
            }
            return retValue;
        }

        /// <summary>
        /// Common re-usable GetSelectedItemFromDroplistField() extension method
        /// </summary>
        /// <param name="contextItem"></param>
        /// <param name="fieldKey"></param>
        /// <returns>The Item object in a DroplistField Item object</returns>
        public static Item GetSelectedItemFromDroplistField(Item contextItem, string fieldKey)
        {
            Field field = contextItem.Fields[fieldKey];
            if (field == null || string.IsNullOrEmpty(field.Value))
            {
                return null;
            }
            var fieldSource = field.Source ?? string.Empty;
            var selectedItemPath = fieldSource.TrimEnd('/') + "/" + field.Value;
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
            var dropListItem = GetSelectedItemFromDroplistField(contextItem, fieldKey);
            string dropListItemVal = string.Empty;
            if (dropListItem != null)
            {
                dropListItemVal = GetFieldValueByKey(dropListItem, "Phrase");
                if (isGuidValue)
                {
                    ID.TryParse(dropListItemVal, out ID templateId);
                    dropListItemVal = (!templateId.IsNull) ? templateId.ToString().Trim() : string.Empty;
                }
            }
            return dropListItemVal;
        }
    }
}
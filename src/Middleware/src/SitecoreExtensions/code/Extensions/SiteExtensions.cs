using System;
using System.IO;
using Sitecore.Data;
using Sitecore.Sites;
using Sitecore.Data.Items;

namespace Sitecore.Foundation.SitecoreExtensions.Extensions
{
	public static class SiteExtensions
	{
		/// <summary>
		/// Common re-usable GetContextItem() extension method
		/// </summary>
		/// <param name="site"></param>
		/// <param name="derivedFromTemplateId"></param>
		/// <returns>The Sitecore site's CurrentContextItem from the SiteContext object</returns>
		public static Item GetContextItem(this SiteContext site, ID derivedFromTemplateId)
		{
			if (site == null)
			{
				throw new ArgumentNullException(nameof(site));
			}
			var startItem = site.GetStartItem();
			return startItem?.GetAncestorOrSelfOfTemplate(derivedFromTemplateId);
		}

		/// <summary>
		/// Common re-usable GetRootItem() extension method
		/// </summary>
		/// <param name="site"></param>
		/// <returns>The Sitecore site's RootContextItem from the SiteContext object</returns>
		public static Item GetRootItem(this SiteContext site)
		{
			if (site == null) 
			{ 
				throw new ArgumentNullException(nameof(site));
			}
			return site.Database.GetItem(Context.Site.RootPath);
		}

		/// <summary>
		/// Common re-usable GetStartItem() extension method
		/// </summary>
		/// <param name="site"></param>
		/// <returns>The Sitecore site's StartContextItem from the SiteContext object</returns>
		private static Item GetStartItem(this SiteContext site)
		{
			if (site == null)
			{
				throw new ArgumentNullException(nameof(site));
			}
			return site.Database.GetItem(Context.Site.StartPath);
		}

		/// <summary>
		/// Common re-usable GetItemsInReverse() extension method
		/// </summary>
		/// <param name="items"></param>
		/// <param name="setInReverseOrder"></param>
		/// <returns>The current array of items in reverse order or default set order</returns>
		public static Item[] GetItemsInReverse(Item[] items, bool setInReverseOrder = false)
		{
			if (setInReverseOrder)
			{
				Array.Reverse(items);
			}
			return items;
		}

		/// <summary>
		/// Common re-usable CreateOrUpdateRobotsTxtSettings() extension method
		/// <param name="webrootPath"></param>
		/// <param name="siteItem"></param>
		/// </summary>
		public static void CreateOrUpdateRobotsTxtSettings(string webrootPath, Item siteItem)
		{
			var robotsFilePath = $@"{webrootPath}\{siteItem.Name.Replace(" ", "-").ToLower().Trim()}_robots.txt";
			var fileItem = new FileInfo(robotsFilePath);

			// Check if file already exists. If yes, delete it.     
			if (fileItem.Exists)
			{
				fileItem.Delete();
			}

			var robotsFileSettings = string.Empty;
			if (FieldExtensions.IsValidFieldValueByKeyHasValue(siteItem, @"RobotsFileSettings"))
			{
				robotsFileSettings = FieldExtensions.GetFieldValueByKey(siteItem, @"RobotsFileSettings").GetCleanRitchTextContent();
			}

			// Create a new file     
			using (StreamWriter sw = fileItem.CreateText())
			{
				sw.WriteLine(robotsFileSettings);
			}
		}
	}
}
using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.smartystreets
{
	// todo: add test for this function 
	// todo: should this live in the extensions project?
	public static class PatchHelper
	{
		public static T PatchObject<T>(T patch, T existing)
		{
			var patchType = patch.GetType();
			var propertiesInPatch = patchType.GetProperties();
			foreach (var property in propertiesInPatch)
			{
				var patchValue = property.GetValue(patch);
				if (patchValue != null)
				{
					property.SetValue(existing, patchValue, null);
				}
			}
			return existing;
		}
	}
}

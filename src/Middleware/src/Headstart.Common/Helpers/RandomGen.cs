using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Headstart.Common.Helpers
{
	public static class RandomGen
	{
		public static string GetString(string allowedChars, int length)
		{
			Random rng = new Random();
			char[] result  = new char[length];
			for (var i = 0; i < length; i++)
			{
				var randomIndex = rng.Next(0, allowedChars.Length - 1);
				result[i] = allowedChars[randomIndex];
			}
			return new string(result);
		}
	}
}

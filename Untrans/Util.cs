using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Retchelf
{
	public static class Extensions
	{
		public static string StripMargin(this string s)
		{
			return Regex.Replace(s, @"([\r\n]+)\s*\|", "$1");
		}

		public static Dictionary<U, V> ToDictionary<U, V>(this IEnumerable<KeyValuePair<U, V>> e)
		{
			return e.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
		}
	}
}
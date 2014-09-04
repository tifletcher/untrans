using System.Text.RegularExpressions;

namespace Retchelf
{
	public static class Extensions
	{
		public static string StripMargin(this string s)
		{
			return Regex.Replace(s, @"([\r\n]+)\s*\|", "$1");
		}

	}
}
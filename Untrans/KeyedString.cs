using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Xml.Linq;

namespace Untrans
{
	public class TranslateableString : KeyedString
	{
		public bool Translated { get; set; }

		public TranslateableString()
		{
			Translated = false;
		}
	}

	public class TranslatedString : KeyedString
	{
		public bool Stale { get; set; }

		public TranslatedString()
		{
			Stale = true;
		}
	}

	public static class KeyedStringExtension
	{
		public static SortedSet<String> ExtractKeys(this Dictionary<String, KeyedString> inputSet)
		{
			return KeyedString.ExtractKeys(inputSet);
		}
		public static SortedSet<String> ExtractKeys(this Dictionary<String, TranslatedString> inputSet)
		{
			return KeyedString.ExtractKeys(inputSet);
		}
		public static SortedSet<String> ExtractKeys(this Dictionary<String, TranslateableString> inputSet)
		{
			return KeyedString.ExtractKeys(inputSet);
		}
	}

	public class KeyedString : IComparable
	{
		public string Key { get; set; }
		public string String { get; set; }

		public int CompareTo(object obj)
		{
			var other = obj as KeyedString;
			return String.Compare(Key, other.Key); // blow up if obj can't be coerced
		}

		public static Dictionary<String, T> ReadFile<T>(string filename) where T : KeyedString, new()
		{
			var doc = XDocument.Load(filename);
			var hash = new Dictionary<string, T>();
			foreach (var dataNode in doc.Descendants("data"))
			{
				var key = dataNode.Attribute("name").Value;
				hash.Add(key, new T
				{
					Key = key,
					String = dataNode.Descendants("value").First().Value
				});
			}
			return hash;
		}

		public static SortedSet<String> ExtractKeys<T>(Dictionary<String, T> inputHash) where T : KeyedString
		{
			var outputSet = new SortedSet<String>();
			foreach (var el in inputHash)
			{
				outputSet.Add(el.Key);
			}
			return outputSet;
		}
	}

}

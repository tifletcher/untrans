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
			Stale = false;
		}
	}

	public static class KeyedStringExtension
	{
		public static SortedSet<String> ExtractKeys(this SortedSet<KeyedString> inputSet)
		{
			return KeyedString.ExtractKeys(inputSet);
		}
		public static SortedSet<String> ExtractKeys(this SortedSet<TranslatedString> inputSet)
		{
			return KeyedString.ExtractKeys(inputSet);
		}
		public static SortedSet<String> ExtractKeys(this SortedSet<TranslateableString> inputSet)
		{
			return KeyedString.ExtractKeys(inputSet);
		}
	}

	public class KeyedString : IComparable
	{
		public string Key { get; set; }
		public string String { set; private get; }

		public int CompareTo(object obj)
		{
			var other = obj as KeyedString;
			return String.Compare(Key, other.Key); // blow up if obj can't be coerced
		}

		public static SortedSet<T> ReadFile<T>(string filename) where T : KeyedString, new()
		{
			var doc = XDocument.Load(filename);
			var set = new SortedSet<T>();
			foreach (var dataNode in doc.Descendants("data"))
			{
				set.Add(new T
				{
					Key = dataNode.Attribute("name").Value,
					String = dataNode.Descendants("value").First().Value
				});
			}
			return set;
		}

		public static SortedSet<String> ExtractKeys<T>(SortedSet<T> inputSet) where T : KeyedString
		{
			var outputSet = new SortedSet<String>();
			foreach (var ks in inputSet)
			{
				outputSet.Add(ks.Key);
			}
			return outputSet;
		}
	}

}

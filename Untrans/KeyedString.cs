using System;
using System.Collections.Generic;
using System.Linq;
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

	public class KeyedString
	{
		public string Key { get; set; }
		public string String { get; set; }

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
	}
}

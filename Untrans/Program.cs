using System;
using System.Collections.Generic;
using System.Linq;

using NDesk.Options;

namespace Untrans
{
	internal static class Config
	{
		public static string DataPath = "";
		public static string BaseFilename = "PorchlightStrings";
		public static string Suffix = "resx";
		public static string GermanFilename = DataPath + "PorchlightStrings.de.resx";
		public static string EnglishFilename = DataPath + BaseFilename + "." + Suffix;

		public enum TranslationTargets
		{
			German,
			French,
			Japanese,
			Chinese
		}

		public class TranslationInfo
		{
			public string Name { get; set; }
			public string Code { get; set; }

			public string Filename
			{
				get { return DataPath + BaseFilename + "." + Code + "." + Suffix; }
			}
		}

		public static Dictionary<TranslationTargets, TranslationInfo> Translations =
			new Dictionary<TranslationTargets, TranslationInfo>
			{
				{TranslationTargets.German,   new TranslationInfo{ Name="German",   Code="de" }},
				{TranslationTargets.French,   new TranslationInfo{ Name="French",   Code="fr" }},
				{TranslationTargets.Japanese, new TranslationInfo{ Name="Japanese", Code="ja" }},
				{TranslationTargets.Chinese,  new TranslationInfo{ Name="Chinese",  Code="zh" }},
			};
	}


	class Program
	{
		private class Options
		{
			public string BaseFilename { get; set; }
			public TargetTypes TargetType { get; set; }
		}

		private enum TargetTypes
		{
			UI,
		}

		static void Main(string[] args)
		{
			var options = new Options
			{
				BaseFilename = @"c:\Users\tfletcher\Porchlight\Main\UI\dist\std\",
				TargetType = TargetTypes.UI
			};
			var optionSet = new OptionSet()
			{
				{
					"p:|path:", path =>
					{
						if (!String.IsNullOrWhiteSpace(path)) options.BaseFilename = path;
					}
				},
				{
					"UI", _ => options.TargetType = TargetTypes.UI;
				}
			};
			optionSet.Parse(args);

			var porchlightStrings = KeyedString.ReadFile<TranslateableString>(options.BaseFilename + Config.EnglishFilename);
			var germanTranslations = KeyedString.ReadFile<TranslatedString>(options.BaseFilename + Config.GermanFilename);

			var translations =
				Config.Translations.Aggregate(new Dictionary<Config.TranslationTargets, SortedSet<TranslatedString>>(),
					(accumulator, translation) =>
					{
						var translationSet = KeyedString.ReadFile<TranslatedString>(
							options.BaseFilename + Config.Translations[translation.Key].Filename
							);
						accumulator.Add(translation.Key, translationSet);
						return accumulator;
					});

			Console.Write("All Strings:");
			Console.WriteLine(porchlightStrings.Count());

			Console.Write("German");
			Console.WriteLine(germanTranslations.Count());

			var untranslatedStrings =
				porchlightStrings.Where(translateble => !(germanTranslations.Any(translation => translation.Key == translateble.Key)));
			Console.WriteLine(untranslatedStrings.Count());

			var TranslatebleKeys = porchlightStrings.ExtractKeys();

			Console.WriteLine("----");
			foreach (var translation in translations)
			{
				Console.Write(Config.Translations[translation.Key].Name + ": ");
				Console.WriteLine(translation.Value.Count());
			}




			Console.ReadKey();
		}
	}
}

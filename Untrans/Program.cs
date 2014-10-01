﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NDesk.Options;

using Retchelf;

namespace Untrans
{
	internal static class Config
	{
		public static string DataPath = "\\";
		public static string BaseFilename = "PorchlightStrings";
		public static string Suffix = "resx";
		public static string EnglishFilename = DataPath + BaseFilename + "." + Suffix;

		public static HashSet<TranslationInfo> Translations =
			new HashSet<TranslationInfo>
			{
				new TranslationInfo{ Name="Spanish",  Code="es" },
				new TranslationInfo{ Name="German",   Code="de" },
				new TranslationInfo{ Name="French",   Code="fr" },
				new TranslationInfo{ Name="Japanese", Code="ja" },
				new TranslationInfo{ Name="Chinese",  Code="zh" },

				// probably obsolete forever. existed in 1402, gone in 1403
				new TranslationInfo{ Name="Mexican Spanish",  Code="es-mx" },
				new TranslationInfo{ Name="Simplified Chinese",  Code="es-mx" },
			};
	}

	public class TranslationInfo
	{
		public string Name { get; set; }
		public string Code { get; set; }

		public string Filename
		{
			get { return Config.DataPath + Config.BaseFilename + "." + Code + "." + Config.Suffix; }
		}
	}


	class Program
	{
		private class Options
		{
			public string BasePath { get; set; }
			public string IgnoreFilePath { get; set; }
			public bool Report { get; set; }
		}

		static void Main(string[] args)
		{

			#region options and help
			bool showHelp = false;
			var options = new Options
			{
				Report = false,
				IgnoreFilePath = "./ignore.regx.txt",
			};
			var optionSet = new OptionSet
			{
				{
					"p|path:", path =>
					{
						if (!String.IsNullOrWhiteSpace(path)) options.BasePath = path;
					}
				},
				{
					"i|ignore:", path =>
					{
						if (!String.IsNullOrWhiteSpace(path)) options.IgnoreFilePath = path;
					}
				},
				{
					"report-only", _ =>
					{
						options.Report = true;
					}
				},
				{
					"h|?|help", _ =>
					{
						showHelp = true;
					}
				}
			};
			optionSet.Parse(args);

			if (showHelp || !args.Any() || options.BasePath == null)
			{
				var help = @" Untrans
					| Show info about translation staleness or print list of untranlated strings in a porchlight build.

					| Arguments:
					| --path=<directory containing PorchlightStrings.resx>

					| Options:
					| --ignore=<path to ignore file>. Default is ""ignore.regx.txt""
					| --report-only Prints a translation report instead of listing untranslated strings
					| "
					.StripMargin();

				Console.Write(help);
				Environment.Exit(0);
			}
			#endregion

			#region read ignore files
			if (!File.Exists(options.IgnoreFilePath))
			{
				Console.WriteLine("Ignore file \"{0}\" not found.", options.IgnoreFilePath);
				Environment.Exit(0);
			}
			var globalIgnore = new HashSet<String>(File.ReadAllLines(options.IgnoreFilePath)).Where(line => !String.IsNullOrEmpty(line));
			#endregion

			#region read / preen translations
			// read in porchlight strings and translations
			var porchlightStringsPath = options.BasePath + Config.EnglishFilename;
			if (!File.Exists(porchlightStringsPath))
			{
				Console.WriteLine("\"{0}\" not found. Is this the path to a release build?", porchlightStringsPath);
				Environment.Exit(0);
			}
			var rawPorchlightStrings = KeyedString.ReadFile<TranslateableString>(porchlightStringsPath);
			var porchlightStrings = rawPorchlightStrings
				.Where(kvp => !globalIgnore.Any(exp => Regex.IsMatch(kvp.Value.String, "^" + exp + "$")))
				.ToDictionary();

			var translations =
				Config.Translations.Aggregate(new Dictionary<TranslationInfo, Dictionary<String, TranslatedString>>(),
					(accumulator, translationinfo) =>
					{
						var translationDict = KeyedString.ReadFile<TranslatedString>(options.BasePath + translationinfo.Filename);
						if (translationDict != null)
						{
							accumulator.Add(translationinfo, translationDict);
						}
						return accumulator;
					});

			// mark fully translated strings and non-stale translations
			foreach (var key in porchlightStrings.Keys)
			{
				int translationsFound = 0;

				foreach (var translation in translations)
				{
					if (translation.Value.ContainsKey(key))
					{
						translation.Value[key].Stale = false;
						translationsFound++;
					}
				}

				if (translationsFound == translations.Count)
				{
					porchlightStrings[key].Translated = true;
				}
			}
			#endregion

			if (options.Report)
			{
				Console.WriteLine("Total strings before filtering: " + rawPorchlightStrings.Count);
				Console.WriteLine("{0} unique ignore expressions found in \"{1}\"", globalIgnore.Count(), options.IgnoreFilePath);
				Console.WriteLine("Total translateable strings: " + porchlightStrings.Count);
				Console.WriteLine();

				Console.WriteLine("Untranslated: " + porchlightStrings.Count(s => !s.Value.Translated));
				Console.WriteLine();

				Console.WriteLine("Stale keys:");
				foreach (var translation in translations.OrderBy(translationInfo => translationInfo.Key.Name))
				{
					var stale = translation.Value.Where(s => s.Value.Stale);
					Console.WriteLine("-- {0}: {1} of {2} strings are stale", translation.Key.Name, stale.Count(), translation.Value.Count);
				}
			}
			else
			{
				var untranslatedStrings = porchlightStrings.Where(s => !s.Value.Translated);
				foreach (var untranslatedString in untranslatedStrings)
				{
					Console.WriteLine(untranslatedString.Key + "\t" + untranslatedString.Value.String);
				}
			}

		}
	}
}

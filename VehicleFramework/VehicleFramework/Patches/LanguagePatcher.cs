using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(LanguageSDF))]
    public class LanguageSDFPatcher
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(LanguageSDF.Initialize))]
        public static void LanguageSDFInitializeHarmonyPrefix(Language __instance)
        {
            LanguagePatcher.InsertCustomLines(ref __instance);
        }
    }

    [HarmonyPatch(typeof(Language))]
    public class LanguagePatcher
    {
        private const string FallbackLanguage = "English";
        private static readonly Dictionary<string, Dictionary<string, string>> _customLines = new Dictionary<string, Dictionary<string, string>>();
        private static string _currentLanguage => Language.main.currentLanguage;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Language.GetKeysFor))]
        public static void LanguageGetKeysForHarmonyPrefix(Language __instance)
        {
            InsertCustomLines(ref __instance);
        }
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Language.TryGet))]
        public static void LanguageTryGetHarmonyPrefix(Language __instance, string key)
        {
            RepatchCheck(ref __instance, key);
        }
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Language.Contains))]
        public static void LanguageContainsHarmonyPrefix(Language __instance, string key)
        {
            RepatchCheck(ref __instance, key);
        }
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Language.LoadLanguageFile))]
        internal static void RepatchCheck(ref Language __instance, string language)
        {
            if (string.IsNullOrWhiteSpace(language))
            {
                return;
            }

            if ((!_customLines.TryGetValue(_currentLanguage, out var customStrings) || !customStrings.TryGetValue(language, out var customValue)) &&
                (!_customLines.TryGetValue(FallbackLanguage, out customStrings) || !customStrings.TryGetValue(language, out customValue)))
            {
                return;
            }

            if (!__instance.strings.TryGetValue(language, out string currentValue) || customValue != currentValue)
            {
                InsertCustomLines(ref __instance);
            }
        }
        internal static void InsertCustomLines(ref Language __instance)
        {
            if (!_customLines.TryGetValue(FallbackLanguage, out var fallbackStrings) & !_customLines.TryGetValue(_currentLanguage, out var currentStrings))
            {
                return;
            }

            if(fallbackStrings == null)
            {
                fallbackStrings = new Dictionary<string, string>();
            }
            if (currentStrings == null)
            {
                currentStrings = new Dictionary<string, string>();
            }
            foreach (var fallbackString in fallbackStrings)
            {
                // Allow mixed-in English if the current language doesn't have a translation for a key. 
                if (currentStrings.TryGetValue(fallbackString.Key, out var currentValue))
                    __instance.strings[fallbackString.Key] = currentValue;
                else
                    __instance.strings[fallbackString.Key] = fallbackString.Value;
            }

            if (_currentLanguage == FallbackLanguage)
            {
                __instance.ParseMetaData();
                return;
            }

            var diffStrings = currentStrings.Except(fallbackStrings);

            // Just in case there are current language strings that aren't in the fallback language, we implement them as well.
            foreach (var currentOnlyString in diffStrings)
            {
                __instance.strings[currentOnlyString.Key] = currentOnlyString.Value;
            }

            __instance.ParseMetaData();
        }
        internal static void AddCustomLanguageLine(string lineId, string text, string language)
        {
            if (string.IsNullOrWhiteSpace(language))
            {
                language = FallbackLanguage;
            }

            if (!_customLines.ContainsKey(language))
                _customLines[language] = new Dictionary<string, string>();

            _customLines[language][lineId] = text;
        }
        internal static void AddCustomLanguageLines(string language, Dictionary<string, string> languageStrings)
        {
            if (string.IsNullOrWhiteSpace(language))
            {
                language = FallbackLanguage;
            }

            if (!_customLines.ContainsKey(language))
                _customLines[language] = new Dictionary<string, string>();

            var customStrings = _customLines[language];

            foreach (var languageString in languageStrings)
            {
                customStrings[languageString.Key] = languageString.Value;
            }
        }
        public static void SetLanguageLine(string lineId, string text, string language = "English")
        {
            AddCustomLanguageLine(lineId, text, language);
        }
    }
}

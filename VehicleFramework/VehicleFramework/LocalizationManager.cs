using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using System.Reflection;

namespace VehicleFramework.Localization
{
    public enum EnglishString
    {
        EmptyHover,
        HeadLightsHover,
        FloodLightsHover,
        NavLightsHover,
        InteriorLightsHover,
        DefaultColorHover,
        PowerHover,
        AutoPilotHover,
        OpenStorage,
        VehicleBattery,
        AutoPilotBattery,
        EnterVehicle,
        ExitVehicle,
        Vehicle,
        StartPiloting,
        TooSteep,
        TooFast,
        Depth1FriendlyString,
        Depth1Description,
        Depth2FriendlyString,
        Depth2Description,
        Depth3FriendlyString,
        Depth3Description,
        MainExterior,
        PrimaryAccent,
        SecondaryAccent,
        NameLabel,
        MVModules,
        MVDepthModules
    }
    public enum SupportedLanguage
    {
        bg_BG,
        zh_CN,
        zh_Hant,
        hr_HR,
        cs_CZ,
        da_DK,
        nl_BE,
        en_US,
        et_EE,
        fi_FI,
        fr_FR,
        de_DE,
        el_GR,
        hu_HU,
        ga_IE,
        it_IT,
        ja_JP,
        ko_KR,
        lv_LV,
        lt_LT,
        nb_NO,
        pl_PL,
        pt_BR,
        pt_PT,
        ro_RO,
        ru_RU,
        sr_Cyrl,
        sk_SK,
        es_ES,
        es_MX,
        es_419,
        sv_SE,
        th_TH,
        tr_TR,
        uk_UA,
        vi_VN
    }
    public static class LocalizationManager
    {
        private static Dictionary<SupportedLanguage, Dictionary<EnglishString, string>> _localizedStrings;

        static LocalizationManager()
        {
            _localizedStrings = new Dictionary<SupportedLanguage, Dictionary<EnglishString, string>>();
            foreach (SupportedLanguage lang in Enum.GetValues(typeof(SupportedLanguage)))
            {
                LoadLanguageFile(lang);
            }
        }

        private static void LoadLanguageFile(SupportedLanguage lang)
        {
            string modPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string filePath = Path.Combine(modPath, "Localization", $"{lang}.txt");
            if (File.Exists(filePath))
            {
                var strings = new Dictionary<EnglishString, string>();
                string[] lines = File.ReadAllLines(filePath);
                foreach (string line in lines)
                {
                    string[] keyValue = line.Split('=');
                    if (keyValue.Length == 2)
                    {
                        if (Enum.TryParse(keyValue[0], out EnglishString key))
                        {
                            // This permits using newlines in the language files.
                            strings[key] = keyValue[1].Replace("\\n","\n");
                        }
                    }
                }
                _localizedStrings[lang] = strings;
            }
            else
            {
                Debug.LogError($"Language file not found: {filePath}");
            }
        }
        public static SupportedLanguage ConvertToSL(string curLang)
        {
            string modifiedCurLang = curLang.Replace('-', '_');
            if (Enum.TryParse(modifiedCurLang, out SupportedLanguage result))
            {
                return result;
            }
            else
            {
                Logger.Error($"Invalid language code: {curLang}. Defaulting to en_US.");
                return SupportedLanguage.en_US;
            }
        }
        public static string GetIsoCode(string language)
        {
            foreach (KeyValuePair<string, string> pair in Language.cultureToLanguage)
            {
                if (pair.Value.Equals(language, StringComparison.OrdinalIgnoreCase))
                {
                    return pair.Key;
                }
            }
            Logger.Error($"Invalid language: {language}. Defaulting to English.");
            return "en-US";
        }
        public static String GetString(EnglishString str)
        {
            SupportedLanguage lang = ConvertToSL(GetIsoCode(Language.main.currentLanguage));
            if (_localizedStrings.TryGetValue(lang, out Dictionary<EnglishString, string> languageDict))
            {
                if (languageDict.TryGetValue(str, out string localizedString))
                {
                    return localizedString;
                }
            }
            return str.ToString(); // Default to the English string name if not found
        }
    }
}
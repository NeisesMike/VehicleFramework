using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using System.Reflection;

namespace VehicleFramework.Localization
{
    internal enum SupportedLanguage
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
    internal class LocalDict<TEnum> : Dictionary<SupportedLanguage, Dictionary<TEnum, string>>
    {
    }

    internal static class LocalizationManager
    {
        internal static LocalDict<TEnum> LoadLanguageFiles<TEnum>(string modPath) where TEnum : struct, Enum
        {
            LocalDict<TEnum> result = new();
            foreach (SupportedLanguage lang in Enum.GetValues(typeof(SupportedLanguage)))
            {
                string filePath = Path.Combine(modPath, "Localization", $"{lang}.txt");
                if (File.Exists(filePath))
                {
                    Dictionary<TEnum, string> strings = new();
                    string[] lines = File.ReadAllLines(filePath);
                    foreach (string line in lines)
                    {
                        string[] keyValue = line.Split('=');
                        if (keyValue.Length == 2)
                        {
                            if (Enum.TryParse(keyValue[0], out TEnum key))
                            {
                                // This permits using newlines in the language files.
                                strings[key] = keyValue[1].Replace("\\n", "\n");
                            }
                        }
                    }
                    result[lang] = strings;
                }
                else
                {
                    Debug.LogError($"Language file not found: {filePath}");
                }
            }
            return result;
        }
        private static SupportedLanguage ConvertToSL(string curLang)
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
        private static string GetIsoCode(string language)
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
        internal static SupportedLanguage GetSupportedLanguage()
        {
            return ConvertToSL(GetIsoCode(Language.main.currentLanguage));
        }
        internal static string GetString(EnglishString str)
        {
            return Localizer<EnglishString>.GetString(str);
        }
    }
}
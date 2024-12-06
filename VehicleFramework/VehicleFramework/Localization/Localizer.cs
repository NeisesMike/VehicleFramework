using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using System.Reflection;

namespace VehicleFramework.Localization
{
    internal enum EnglishString
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
    public static class Localizer<TEnum> where TEnum: struct, Enum
    {
        private static LocalDict<TEnum> main = null;
        public static string GetString(TEnum value)
        {
            if(main == null)
            {
                string modPath = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
                main = LocalizationManager.LoadLanguageFiles<TEnum>(modPath);
            }
            SupportedLanguage lang = LocalizationManager.GetSupportedLanguage();
            if (main.TryGetValue(lang, out Dictionary<TEnum, string> languageDict))
            {
                if (languageDict.TryGetValue(value, out string localizedString))
                {
                    return localizedString;
                }
                else
                {
                    Logger.Error("Couldn't get a localized value for that enum: " + value.ToString());
                    foreach(var but in languageDict)
                    {
                        Logger.Warn(but.Key.ToString() + " : " + but.Value);
                    }
                }
            }
            else
            {
                Logger.Error("Couldn't get a localized dictionary for that language: " + lang.ToString());
            }
            return value.ToString(); // Default to the English string name if not found
        }
    }
}
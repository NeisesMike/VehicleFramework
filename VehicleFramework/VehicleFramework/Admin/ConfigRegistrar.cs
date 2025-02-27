using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using BepInEx.Configuration;

namespace VehicleFramework.Admin
{
    public class ExternalVehicleConfig<T>
    {
        internal string MyName = "";
        internal Dictionary<string, ConfigEntry<T>> ExternalConfigs = new Dictionary<string, ConfigEntry<T>>();

        internal static Dictionary<string, ExternalVehicleConfig<T>> main = new Dictionary<string, ExternalVehicleConfig<T>>();
        internal static ExternalVehicleConfig<T> SeamothConfig = null;
        internal static ExternalVehicleConfig<T> PrawnConfig = null;
        internal static ExternalVehicleConfig<T> CyclopsConfig = null;
        private static ExternalVehicleConfig<T> AddNew(ModVehicle mv)
        {
            var thisConf = new ExternalVehicleConfig<T>
            {
                MyName = mv.GetType().ToString()
            };
            main.Add(thisConf.MyName, thisConf);
            return thisConf;
        }
        public T GetValue(string name)
        {
            if (ExternalConfigs.ContainsKey(name))
            {
                return ExternalConfigs[name].Value;
            }
            throw new ArgumentException($"External config for {MyName} does not have a config entry of name {name}.");
        }
        public static ExternalVehicleConfig<T> GetModVehicleConfig(string vehicleName)
        {
            var MVs = VehicleManager.vehicleTypes.Where(x => x.name.Equals(vehicleName, StringComparison.OrdinalIgnoreCase));
            if (!MVs.Any())
            {
                string mvNames = VehicleManager.vehicleTypes.Select(x => x.name).Aggregate((x, y) => $"{x}, {y}");
                throw new ArgumentException($"RegisterForModVehicle: vehicle name does not identify a ModVehicle: {vehicleName}. Options are: {mvNames}");
            }
            if (MVs.Count() > 1)
            {
                string mvNames = MVs.Select(x => x.name).Aggregate((x, y) => $"{x}, {y}");
                throw new ArgumentException($"RegisterForModVehicle: vehicle name does not uniquely identify a ModVehicle: {vehicleName}. There were {MVs.Count()} matches: {mvNames}");
            }
            ModVehicle mv = MVs.First().mv;
            if (!main.ContainsKey(mv.GetType().ToString()))
            {
                AddNew(mv);
            }
            return main[mv.GetType().ToString()];
        }
        public static ExternalVehicleConfig<T> GetSeamothConfig()
        {
            if(SeamothConfig == null)
            {
                SeamothConfig = new ExternalVehicleConfig<T>
                {
                    MyName = ConfigRegistrar.SeamothName
                };
            }
            return SeamothConfig;
        }
        public static ExternalVehicleConfig<T> GetPrawnConfig()
        {
            if (PrawnConfig == null)
            {
                PrawnConfig = new ExternalVehicleConfig<T>
                {
                    MyName = ConfigRegistrar.PrawnName
                };
            }
            return PrawnConfig;
        }
        public static ExternalVehicleConfig<T> GetCyclopsConfig()
        {
            if (CyclopsConfig == null)
            {
                CyclopsConfig = new ExternalVehicleConfig<T>
                {
                    MyName = ConfigRegistrar.CyclopsName
                };
            }
            return CyclopsConfig;
        }
    }
    public static class ConfigRegistrar
    {
        internal const string SeamothName = "VanillaSeaMoth";
        internal const string PrawnName = "VanillaPrawn";
        internal const string CyclopsName = "VanillaCyclops";
        public static void LogAllVehicleNames()
        {
            UWE.CoroutineHost.StartCoroutine(LogAllVehicleNamesInternal());
        }
        private static IEnumerator LogAllVehicleNamesInternal()
        {
            while (Player.main == null)
            {
                // wait until the player exists, so that we're sure every vehicle is done with registration
                yield return null;
            }
            var boolNames = ExternalVehicleConfig<bool>.main.Keys;
            var floatNames = ExternalVehicleConfig<float>.main.Keys;
            var keyNames = ExternalVehicleConfig<KeyboardShortcut>.main.Keys;
            var result = boolNames.Concat(floatNames).Concat(keyNames).Distinct().ToList();
            result.Add(SeamothName);
            result.Add(PrawnName);
            result.Add(CyclopsName);
            Logger.Log("Logging all vehicle type names:");
            result.ForEach(x => Logger.Log(x));
        }
        public static void RegisterForAllModVehicles<T>(string name, ConfigDescription description, T defaultValue, Action<TechType, T> OnChange = null, ConfigFile configFile = null)
        {
            UWE.CoroutineHost.StartCoroutine(RegisterForAllInternal<T>(name, description, defaultValue, OnChange, configFile));
        }
        public static void RegisterForModVehicle<T>(string vehicleName, string name, ConfigDescription description, T defaultValue, Action<TechType, T> OnChange = null, ConfigFile configFile = null)
        {
            UWE.CoroutineHost.StartCoroutine(RegisterForVehicleInternal<T>(vehicleName, name, description, defaultValue, OnChange, configFile));
        }
        public static void RegisterForSeamoth<T>(string name, ConfigDescription description, T defaultValue, Action<T> OnChange = null, ConfigFile configFile = null)
        {
            UWE.CoroutineHost.StartCoroutine(RegisterForSeamothInternal<T>(name, description, defaultValue, OnChange, configFile));
        }
        public static void RegisterForPrawn<T>(string name, ConfigDescription description, T defaultValue, Action<T> OnChange = null, ConfigFile configFile = null)
        {
            UWE.CoroutineHost.StartCoroutine(RegisterForPrawnInternal<T>(name, description, defaultValue, OnChange, configFile));
        }
        public static void RegisterForCyclops<T>(string name, ConfigDescription description, T defaultValue, Action<T> OnChange = null, ConfigFile configFile = null)
        {
            UWE.CoroutineHost.StartCoroutine(RegisterForCyclopsInternal<T>(name, description, defaultValue, OnChange, configFile));
        }
        private static IEnumerator RegisterForAllInternal<T>(string name, ConfigDescription description, T defaultValue, Action<TechType, T> OnChange = null, ConfigFile configFile = null)
        {
            if (typeof(T) != typeof(bool) && typeof(T) != typeof(float) && typeof(T) != typeof(KeyboardShortcut))
            {
                Logger.Error($"ConfigRegistrar only accepts type parameters: bool, float, KeyboardShortcut, but you supplied the type parameter {typeof(T)}.");
                yield break;
            }
            while (Player.main == null)
            {
                // wait until the player exists, so that we're sure every vehicle is done with registration
                yield return null;
            }
            foreach (var pair in ExternalVehicleConfig<T>.main)
            {
                ConfigFile config = configFile;
                if (config == null)
                {
                    config = MainPatcher.Instance.Config;
                }
                var vConf = pair.Value;
                string vehicleName = pair.Key;
                ConfigEntry<T> thisConf;
                try
                {
                    thisConf = config.Bind<T>(vehicleName, name, defaultValue, description);
                }
                catch (Exception e)
                {
                    Logger.Error("ConfigRegistrar: Could not bind that config option. Probably you chose a non-unique name. Error follows:\n" + e.Message);
                    yield break;
                }
                if (OnChange != null)
                {
                    void DoThisAction(object sender, EventArgs e)
                    {
                        foreach (ModVehicle innerMV in VehicleManager.vehicleTypes.Select(x => x.mv))
                        {
                            if(innerMV.GetType().ToString() == vehicleName)
                            {
                                OnChange(innerMV.TechType, thisConf.Value);
                                break;
                            }
                        }
                    }
                    thisConf.SettingChanged += DoThisAction;
                }
                vConf.ExternalConfigs.Add(name, thisConf);
            }
        }
        private static IEnumerator RegisterForVehicleInternal<T>(string vehicleName, string name, ConfigDescription description, T defaultValue, Action<TechType, T> OnChange, ConfigFile configFile)
        {
            if (typeof(T) != typeof(bool) && typeof(T) != typeof(float) && typeof(T) != typeof(KeyboardShortcut))
            {
                Logger.Error($"ConfigRegistrar only accepts type parameters: bool, float, KeyboardShortcut, but you supplied the type parameter {typeof(T)}.");
                yield break;
            }
            while (Player.main == null)
            {
                // wait until the player exists, so that we're sure every vehicle is done with registration
                yield return null;
            }
            var MVs = VehicleManager.vehicleTypes.Where(x => x.name.ToLower().Contains(vehicleName.ToLower()));
            if (!MVs.Any())
            {
                throw new ArgumentException($"RegisterForModVehicle: vehicle name does not identify a ModVehicle: {vehicleName}");
            }
            if (MVs.Count() > 1)
            {
                throw new ArgumentException($"RegisterForModVehicle: vehicle name does not uniquely identify a ModVehicle: {vehicleName}. There were {MVs.Count()} matches.");
            }
            ModVehicle mv = MVs.First().mv;
            ConfigFile config = configFile;
            if (config == null)
            {
                config = MainPatcher.Instance.Config;
            }
            var vConf = ExternalVehicleConfig<T>.GetModVehicleConfig(vehicleName);
            ConfigEntry<T> thisConf;
            try
            {
                thisConf = config.Bind<T>(vConf.MyName, name, defaultValue, description);
            }
            catch (Exception e)
            {
                Logger.Error("ConfigRegistrar: Could not bind that config option. Probably you chose a non-unique name. Error follows:\n" + e.Message);
                yield break;
            }
            if (OnChange != null)
            {
                void DoThisAction(object sender, EventArgs e)
                {
                    OnChange(mv.TechType, thisConf.Value);
                }
                thisConf.SettingChanged += DoThisAction;
            }
            vConf.ExternalConfigs.Add(name, thisConf);
        }
        private static IEnumerator RegisterForSeamothInternal<T>(string name, ConfigDescription description, T defaultValue, Action<T> onChange, ConfigFile configFile)
        {
            if (typeof(T) != typeof(bool) && typeof(T) != typeof(float) && typeof(T) != typeof(KeyboardShortcut))
            {
                Logger.Error($"ConfigRegistrar only accepts type parameters: bool, float, KeyboardShortcut, but you supplied the type parameter {typeof(T)}.");
                yield break;
            }
            while (Player.main == null)
            {
                // wait until the player exists, so that we're sure every vehicle is done with registration
                yield return null;
            }
            ConfigFile config = configFile;
            if (config == null)
            {
                config = MainPatcher.Instance.Config;
            }
            var vConf = ExternalVehicleConfig<T>.GetSeamothConfig();
            ConfigEntry<T> thisConf;
            try
            {
                thisConf = config.Bind<T>(SeamothName, name, defaultValue, description);
            }
            catch (Exception e)
            {
                Logger.Error("ConfigRegistrar: Could not bind that config option. Probably you chose a non-unique name. Error follows:\n" + e.Message);
                yield break;
            }
            if (onChange != null)
            {
                void DoThisAction(object sender, EventArgs e)
                {
                    onChange(thisConf.Value);
                }
                thisConf.SettingChanged += DoThisAction;
            }
            vConf.ExternalConfigs.Add(name, thisConf);
        }
        private static IEnumerator RegisterForPrawnInternal<T>(string name, ConfigDescription description, T defaultValue, Action<T> onChange, ConfigFile configFile)
        {
            if (typeof(T) != typeof(bool) && typeof(T) != typeof(float) && typeof(T) != typeof(KeyboardShortcut))
            {
                Logger.Error($"ConfigRegistrar only accepts type parameters: bool, float, KeyboardShortcut, but you supplied the type parameter {typeof(T)}.");
                yield break;
            }
            while (Player.main == null)
            {
                // wait until the player exists, so that we're sure every vehicle is done with registration
                yield return null;
            }
            ConfigFile config = configFile;
            if (config == null)
            {
                config = MainPatcher.Instance.Config;
            }
            var vConf = ExternalVehicleConfig<T>.GetPrawnConfig();
            ConfigEntry<T> thisConf;
            try
            {
                thisConf = config.Bind<T>(PrawnName, name, defaultValue, description);
            }
            catch (Exception e)
            {
                Logger.Error("ConfigRegistrar: Could not bind that config option. Probably you chose a non-unique name. Error follows:\n" + e.Message);
                yield break;
            }
            if (onChange != null)
            {
                void DoThisAction(object sender, EventArgs e)
                {
                    onChange(thisConf.Value);
                }
                thisConf.SettingChanged += DoThisAction;
            }
            vConf.ExternalConfigs.Add(name, thisConf);
        }
        private static IEnumerator RegisterForCyclopsInternal<T>(string name, ConfigDescription description, T defaultValue, Action<T> onChange, ConfigFile configFile)
        {
            if (typeof(T) != typeof(bool) && typeof(T) != typeof(float) && typeof(T) != typeof(KeyboardShortcut))
            {
                Logger.Error($"ConfigRegistrar only accepts type parameters: bool, float, KeyboardShortcut, but you supplied the type parameter {typeof(T)}.");
                yield break;
            }
            while (Player.main == null)
            {
                // wait until the player exists, so that we're sure every vehicle is done with registration
                yield return null;
            }
            ConfigFile config = configFile;
            if (config == null)
            {
                config = MainPatcher.Instance.Config;
            }
            var vConf = ExternalVehicleConfig<T>.GetCyclopsConfig();
            ConfigEntry<T> thisConf;
            try
            {
                thisConf = config.Bind<T>(CyclopsName, name, defaultValue, description);
            }
            catch (Exception e)
            {
                Logger.Error("ConfigRegistrar: Could not bind that config option. Probably you chose a non-unique name. Error follows:\n" + e.Message);
                yield break;
            }
            if (onChange != null)
            {
                void DoThisAction(object sender, EventArgs e)
                {
                    onChange(thisConf.Value);
                }
                thisConf.SettingChanged += DoThisAction;
            }
            vConf.ExternalConfigs.Add(name, thisConf);
        }
    }
}

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using HarmonyLib;
using System.Runtime.CompilerServices;
using System.Collections;
using SMLHelper.V2.Options.Attributes;
using SMLHelper.V2.Options;
using SMLHelper.V2.Json;
using SMLHelper.V2.Handlers;
using QModManager.API.ModLoading;
using SMLHelper.V2.Utility;
using SMLHelper.V2.Json.Attributes;

using upgrades = System.Collections.Generic.Dictionary<string, TechType>;
using innateStorages = System.Collections.Generic.List<System.Tuple<UnityEngine.Vector3, System.Collections.Generic.List<TechType>>>;
using modularStorages = System.Collections.Generic.List<System.Tuple<int, System.Collections.Generic.List<TechType>>>;
using batteries = System.Collections.Generic.List<System.Tuple<TechType, float>>;

namespace VehicleFramework
{
    public static class Logger
    {
        public static void Log(string message)
        {
            UnityEngine.Debug.Log("[VehicleFramework] " + message);
        }
        public static void Log(string format, params object[] args)
        {
            UnityEngine.Debug.Log("[VehicleFramework] " + string.Format(format, args));
        }
        public static void Output(string msg)
        {
            BasicText message = new BasicText(500, 0);
            message.ShowMessage(msg, 5);
        }
        public static void OutputLong(string msg)
        {
            BasicText message = new BasicText(0, 100);
            message.ShowMessage(msg, 5);
        }
    }
    [QModCore]
    public static class MainPatcher
    {
        internal static VehicleFrameworkConfig Config { get; private set; }
        internal static SaveData VehicleSaveData { get; private set; }

        [QModPrePatch]
        public static void PrePatch()
        {
        }

        [QModPatch]
        public static void Patch()
        {
            SaveData saveData = SaveDataHandler.Main.RegisterSaveDataCache<SaveData>();

            // Update the player position before saving it
            saveData.OnStartedSaving += (object sender, JsonFileEventArgs e) =>
            {
                VehicleManager.SaveVehicles(sender, e);
            };

            saveData.OnFinishedSaving += (object sender, JsonFileEventArgs e) =>
            {
                //SaveData data = e.Instance as SaveData;
                //Logger.Output(VehicleManager.VehiclesInPlay.Count.ToString());
                //Logger.Output(data.UpgradeList.Keys.ToString());
            };

            saveData.OnFinishedLoading += (object sender, JsonFileEventArgs e) =>
            {
                VehicleSaveData = e.Instance as SaveData; 
            };




            Config = OptionsPanelHandler.Main.RegisterModOptions<VehicleFrameworkConfig>();
            var harmony = new Harmony("com.mikjaw.subnautica.vehicleframework.mod");
            harmony.PatchAll();
        }

        [QModPostPatch]
        public static void PostPatch()
        {
            VehicleBuilder.PatchCraftables();
        }
    }

    [Menu("Vehicle Framework Options")]
    public class VehicleFrameworkConfig : ConfigFile
    {
        [Toggle("temp")]
        public bool temp = false;
    }


    [FileName("vehicle_storage")]
    internal class SaveData : SaveDataCache
    {
        public List<Tuple<Vector3, bool>> IsPlayerInside { get; set; }
        public List<Tuple<Vector3, upgrades>> UpgradeLists { get; set; }
        public List<Tuple<Vector3, innateStorages>> InnateStorages { get; set; }
        public List<Tuple<Vector3, modularStorages>> ModularStorages { get; set; }
        
        public List<Tuple<Vector3, batteries>> Batteries { get; set; }
        public List<Tuple<Vector3, batteries>> BackupBatteries { get; set; }

        // todo: maybe this?
        // save a few lines in the output json?
        public List<Tuple<Vector3, Tuple<upgrades, innateStorages, modularStorages, batteries>>> AllVehiclesStorages { get; set; }
    }
}

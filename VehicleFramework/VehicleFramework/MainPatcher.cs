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
    }
    [QModCore]
    public static class MainPatcher
    {
        internal static VehicleFrameworkConfig Config { get; private set; }
        
        [QModPrePatch]
        public static void PrePatch()
        {
        }

        [QModPatch]
        public static void Patch()
        {
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
}

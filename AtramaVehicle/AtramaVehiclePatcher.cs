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

namespace AtramaVehicle
{
    public static class Logger
    {
        public static void Log(string message)
        {
            UnityEngine.Debug.Log("[AtramaVehicle] " + message);
        }

        public static void Log(string format, params object[] args)
        {
            UnityEngine.Debug.Log("[AtramaVehicle] " + string.Format(format, args));
        }

        public static void output(string msg)
        {
            BasicText message = new BasicText(500, 0);
            message.ShowMessage(msg, 5);
        }
    }

    [QModCore]
    public static class AtramaVehiclePatcher
    {
        internal static MyConfig Config { get; private set; }
        internal static AtramaCraftable atramaPrefab { get; private set; }

        [QModPatch]
        public static void Patch()
        {
            string atramaDescription = "Sized somewhere between the Seamoth and the Cyclops, the Atrama is a robust construction vehicle capable of safely navigating great depths.";
            atramaPrefab = new AtramaCraftable("Atrama", "Atrama Construction Vehicle", atramaDescription);
            atramaPrefab.Patch();

            Config = OptionsPanelHandler.Main.RegisterModOptions<MyConfig>();
            var harmony = new Harmony("com.mikjaw.subnautica.atramavehicle.mod");
            harmony.PatchAll();
        }
    }

    [Menu("Atrama Vehicle Options")]
    public class MyConfig : ConfigFile
    {
        [Toggle("Automatic Leveling")]
        public KeyCode levelButton = KeyCode.Backspace;
    }

   
}

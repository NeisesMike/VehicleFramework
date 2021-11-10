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

namespace OdysseyVehicle
{
    public static class Logger
    {
        public static void Log(string message)
        {
            UnityEngine.Debug.Log("[OdysseyVehicle] " + message);
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
        [QModPatch]
        public static void Patch()
        {
            var harmony = new Harmony("com.mikjaw.subnautica.odyssey.mod");
            harmony.PatchAll();
            Odyssey.Register();
        }
    }
}

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
using Nautilus.Options.Attributes;
using Nautilus.Options;
using Nautilus.Json;
using Nautilus.Handlers;
using Nautilus.Utility;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Bootstrap;

namespace OdysseyVehicle
{
    public static class Logger
    {
        public static ManualLogSource MyLog { get; set; }
        public static void Warn(string message)
        {
            MyLog.LogWarning("[OdysseyVehicle] " + message);
        }
        public static void Log(string message)
        {
            MyLog.LogInfo("[OdysseyVehicle] " + message);
        }
        public static void Output(string msg)
        {
            BasicText message = new BasicText(500, 0);
            message.ShowMessage(msg, 5);
        }
    }

    public class CoroutineHelper : MonoBehaviour
    {
        public static CoroutineHelper main { get; set; }
        public static Coroutine Starto(IEnumerator func)
        {
            return main.StartCoroutine(func);
        }
    }

    [BepInPlugin("com.mikjaw.subnautica.odyssey.mod", "Odyssey", "1.3.2")]
    [BepInDependency("com.mikjaw.subnautica.vehicleframework.mod")]
    [BepInDependency("com.snmodding.nautilus")]
    public class MainPatcher : BaseUnityPlugin
    {
        public void Start()
        {
            CoroutineHelper.main = (new GameObject()).EnsureComponent<CoroutineHelper>();
            OdysseyVehicle.Logger.MyLog = base.Logger;
            var harmony = new Harmony("com.mikjaw.subnautica.odyssey.mod");
            harmony.PatchAll();
            CoroutineHelper.Starto(Odyssey.Register());
        }
    }
}

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


namespace Atrama
{
    public static class Logger
    {
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
    [BepInPlugin("com.mikjaw.subnautica.atrama.mod", "AtramaVehicle", "1.3.2")]
    [BepInDependency("com.mikjaw.subnautica.vehicleframework.mod")]
    [BepInDependency("com.snmodding.nautilus")]

    public class MainPatcher : BaseUnityPlugin
    {
        //internal static AtramaConfig Config { get; private set; }

        public void Start()
        {
            var harmony = new Harmony("com.mikjaw.subnautica.atrama.mod");
            harmony.PatchAll();
            CoroutineHelper.main = (new GameObject()).EnsureComponent<CoroutineHelper>();
            CoroutineHelper.Starto(Atrama.Register());
        }
    }
    /*
    [Menu("Atrama Options")]
    public class AtramaConfig : ConfigFile
    {
        [Toggle("temp")]
        public bool temp = false;
    }
    */
}

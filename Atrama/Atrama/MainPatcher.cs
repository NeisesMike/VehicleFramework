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
using SMLHelper.V2.Utility;
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
    [BepInPlugin("com.mikjaw.subnautica.atrama.mod", "AtramaVehicle", "1.0")]
    [BepInDependency("com.mikjaw.subnautica.vehicleframework.mod", BepInDependency.DependencyFlags.HardDependency)]
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

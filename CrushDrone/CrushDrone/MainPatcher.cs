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
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Crafting;
using Nautilus.Json.Attributes;

using innateStorage = System.Collections.Generic.List<System.Tuple<System.String, float>>;

namespace CrushDrone
{
    public static class Logger
    {
        public static void Log(string message)
        {
            UnityEngine.Debug.Log("[CricketVehicle] " + message);
        }
        public static void Output(string msg)
        {
            BasicText message = new BasicText(500, 0);
            message.ShowMessage(msg, 5);
        }
    }

    [BepInPlugin("com.mikjaw.subnautica.crush.mod", "CrushDrone", "1.0.0")]
    [BepInDependency("com.mikjaw.subnautica.vehicleframework.mod")]
    [BepInDependency("com.snmodding.nautilus")]
    public class MainPatcher : BaseUnityPlugin
    {
        //internal static CrushConfig config { get; private set; }
        public void Awake()
        {
            Crush.GetAssets();
        }
        public void Start()
        {
            //config = OptionsPanelHandler.RegisterModOptions<CrushConfig>();
            var harmony = new Harmony("com.mikjaw.subnautica.crush.mod");
            harmony.PatchAll();
            UWE.CoroutineHost.StartCoroutine(Crush.Register());
        }
    }
    [FileName("cricket_containers")]
    internal class SaveData : SaveDataCache
    {
        public List<Tuple<Vector3, innateStorage>> InnateStorages { get; set; }
    }

    /*
    [Menu("Crush Drone Options")]
    public class CrushConfig : ConfigFile
    {

    }
    */
}

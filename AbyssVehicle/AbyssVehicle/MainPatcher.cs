using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using UnityEngine;
using System.Runtime.CompilerServices;
using System.Collections;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Bootstrap;
using Nautilus.Json;
using Nautilus.Options.Attributes;
using Nautilus.Handlers;

/*
 * "Pilot Seat" (https://skfb.ly/ot766) by ali_hidayat is licensed under Creative Commons Attribution (http://creativecommons.org/licenses/by/4.0/).
 * */

namespace AbyssVehicle
{
    public static class Logger
    {
        public static ManualLogSource MyLog { get; set; }
        public static void Warn(string message)
        {
            MyLog.LogWarning("[AbyssVehicle] " + message);
        }
        public static void Log(string message)
        {
            MyLog.LogInfo("[AbyssVehicle] " + message);
        }
    }

    [BepInPlugin("com.mikjaw.subnautica.abyss.mod", "AbyssVehicle", "1.3.2")]
    [BepInDependency("com.mikjaw.subnautica.vehicleframework.mod")]
    [BepInDependency("com.snmodding.nautilus")]
    public class MainPatcher : BaseUnityPlugin
    {
        internal static AbyssVehicleConfig config { get; private set; }

        public void Start()
        {
            config = OptionsPanelHandler.RegisterModOptions<AbyssVehicleConfig>();
            AbyssVehicle.Logger.MyLog = base.Logger;
            var harmony = new Harmony("com.mikjaw.subnautica.abyss.mod");
            harmony.PatchAll();
            UWE.CoroutineHost.StartCoroutine(Abyss.Register());
        }
    }
    [Menu("Abyss Vehicle Options")]
    public class AbyssVehicleConfig : ConfigFile
    {
        [Keybind("Next Camera")]
        public KeyCode nextCamera = KeyCode.F;

        [Keybind("Previous Camera")]
        public KeyCode previousCamera = KeyCode.T;

        [Keybind("Exit Camera")]
        public KeyCode exitCamera = KeyCode.V;

        [Slider("Abyss GUI Size", Min =0.1f, Max =3f, Step =0.01f)]
        public float guiSize = 0.8f;

        [Slider("Abyss GUI X Position", Min = -900f, Max = 900f, Step = 0.1f)]
        public float guiXPosition = 820;

        [Slider("Abyss GUI Y Position", Min = -500f, Max = 500f, Step = 0.1f)]
        public float guiYPosition = -150f;
    }
}

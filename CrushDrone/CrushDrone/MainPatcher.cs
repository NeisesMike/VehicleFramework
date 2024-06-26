using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using HarmonyLib;
using System.Collections;
using Nautilus.Utility;
using BepInEx;
using VehicleFramework.VehicleTypes;
using VehicleFramework;

using innateStorage = System.Collections.Generic.List<System.Tuple<System.String, float>>;

namespace CrushDrone
{
    [BepInPlugin("com.mikjaw.subnautica.crush.mod", "CrushDrone", "1.0.0")]
    [BepInDependency(VehicleFramework.PluginInfo.PLUGIN_GUID)]
    [BepInDependency(Nautilus.PluginInfo.PLUGIN_GUID)]
    public class MainPatcher : BaseUnityPlugin
    {
        //internal static CrushConfig config { get; private set; }
        public static VehicleFramework.Assets.VehicleAssets assets;
        public void Awake()
        {
            GetAssets();
        }
        public void Start()
        {
            //config = OptionsPanelHandler.RegisterModOptions<CrushConfig>();
            var harmony = new Harmony("com.mikjaw.subnautica.crush.mod");
            harmony.PatchAll();
            UWE.CoroutineHost.StartCoroutine(Register());
        }
        public static void GetAssets()
        {
            string directoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string bundlePath = Path.Combine(directoryPath, "crush");
            assets = VehicleFramework.Assets.AssetBundleManager.GetVehicleAssetsFromBundle(bundlePath, "Crush", "SpriteAtlas", "DronePing", "CrafterSprite", "ArmFragment");
        }
        public static IEnumerator Register()
        {
            Drone crush = assets.model.EnsureComponent<Crush>() as Drone;
            yield return UWE.CoroutineHost.StartCoroutine(VehicleRegistrar.RegisterVehicle(crush));
        }
    }

    /*
    [Menu("Crush Drone Options")]
    public class CrushConfig : ConfigFile
    {

    }
    */
}

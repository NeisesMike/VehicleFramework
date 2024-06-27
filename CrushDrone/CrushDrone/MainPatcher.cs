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
using BiomeData = LootDistributionData.BiomeData;

using innateStorage = System.Collections.Generic.List<System.Tuple<System.String, float>>;

namespace CrushDrone
{
    [BepInPlugin("com.mikjaw.subnautica.crush.mod", "CrushDrone", "1.0.0")]
    [BepInDependency(VehicleFramework.PluginInfo.PLUGIN_GUID)]
    [BepInDependency(Nautilus.PluginInfo.PLUGIN_GUID)]
    public class MainPatcher : BaseUnityPlugin
    {
        //internal static CrushConfig config { get; private set; }
        public static TechType CrushArmTechType;
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
        public static TechType RegisterCrushArmFragment(ModVehicle unlockVehicle)
        {
            const string classID = "CrushArmFragment";
            const string displayName = "Crush Arm Fragment";
            const string description = "A Scannable fragment of the Crush Drone";
            var useBiomes = new List<BiomeData>();
            for (int i=400; i<432; i++)
            {
                // Add a small chance to spawn over all the MushroomForest biomes
                useBiomes.Add(new BiomeData { biome = (BiomeType)i, count = 1, probability = 0.1f });
            }
            for (int i = 600; i < 612; i++)
            {
                // Add a small chance to spawn over all the JellyShroom caves
                useBiomes.Add(new BiomeData { biome = (BiomeType)i, count = 1, probability = 0.1f });
            }
            return VehicleFramework.Assets.FragmentManager.RegisterFragment(assets.fragment, unlockVehicle, classID, displayName, description, assets.crafter, useBiomes);
        }
        public static void GetAssets()
        {
            string directoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string bundlePath = Path.Combine(directoryPath, "crush");
            assets = VehicleFramework.Assets.AssetBundleManager.GetVehicleAssetsFromBundle(bundlePath, "Crush", "SpriteAtlas", "DronePing", "CrafterSprite", "ArmFragment", "UnlockSprite");
        }
        public static IEnumerator Register()
        {
            Drone crush = assets.model.EnsureComponent<Crush>() as Drone;
            yield return UWE.CoroutineHost.StartCoroutine(VehicleRegistrar.RegisterVehicle(crush));
            CrushArmTechType = RegisterCrushArmFragment(crush);
        }
    }

    /*
    [Menu("Crush Drone Options")]
    public class CrushConfig : ConfigFile
    {

    }
    */
}

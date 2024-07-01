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
using VehicleFramework.Assets;

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

            AbstractBiomeData abd = new AbstractBiomeData()
                .WithBiome(AbstractBiomeType.MushroomForest)
                .WithBiome(AbstractBiomeType.JellyshroomCaves);

            return FragmentManager.RegisterFragment(assets.fragment, unlockVehicle, classID, displayName, description, assets.unlock, abd.Get(), "Crush");
        }
        public static void GetAssets()
        {
            assets = AssetBundleInterface.GetVehicleAssetsFromBundle("crush", "Crush", "SpriteAtlas", "DronePing", "CrafterSprite", "ArmFragment", "UnlockSprite");
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

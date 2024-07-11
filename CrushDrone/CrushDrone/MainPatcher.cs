using HarmonyLib;
using System.Collections;
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
        internal static CrushConfig config { get; private set; }
        public static TechType CrushArmTechType;
        public static VehicleFramework.Assets.VehicleAssets assets;
        public void Awake()
        {
            GetAssets();
        }
        public void Start()
        {
            config = Nautilus.Handlers.OptionsPanelHandler.RegisterModOptions<CrushConfig>();
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

            //Nautilus.Handlers.StoryGoalHandler.RegisterBiomeGoal("CrushMushroomForest", Story.GoalType.Story, biomeName: "mushroomForest", minStayDuration: 3f, delay: 3f);
            //Nautilus.Handlers.StoryGoalHandler.RegisterBiomeGoal("CrushJellyShroom", Story.GoalType.Story, biomeName: "jellyshroomCaves", minStayDuration: 3f, delay: 3f);
            //Nautilus.Handlers.StoryGoalHandler.RegisterCompoundGoal("CrushBiomes", Story.GoalType.PDA, delay: 3f, new string[]{ "CrushMushroomForest", "CrushJellyShroom" });

            /*
            Nautilus.Handlers.StoryGoalHandler.RegisterBiomeGoal("CrushBiomes", Story.GoalType.Radio, biomeName: "mushroomForest", minStayDuration: 3f, delay: 3f);
            Nautilus.Handlers.StoryGoalHandler.RegisterBiomeGoal("CrushBiomes", Story.GoalType.Radio, biomeName: "jellyshroomCaves", minStayDuration: 3f, delay: 3f);
            Nautilus.Handlers.PDAHandler.AddLogEntry("CrushBiomes", "CrushBiomes", "soundpath", null); // TODO add a sound
            Nautilus.Handlers.LanguageHandler.SetLanguageLine("CrushBiomes", "You did it", "English");
            Nautilus.Handlers.StoryGoalHandler.RegisterCustomEvent("CrushBiomes", () =>
            {
            });
            */

            /*
            var but = UWE.PrefabDatabase.GetPrefabForFilenameAsync("WorldEntities/Environment/Wrecks/life_pod_exploded_13.prefab");
            but = UWE.PrefabDatabase.GetPrefabForFilenameAsync("WorldEntities/Environment/Wrecks/life_pod_exploded_12.prefab");
            but = UWE.PrefabDatabase.GetPrefabForFilenameAsync("WorldEntities/Environment/Wrecks/life_pod_exploded_3.prefab");
            but = UWE.PrefabDatabase.GetPrefabForFilenameAsync("WorldEntities/Environment/Wrecks/life_pod_exploded_19.prefab");
            but = UWE.PrefabDatabase.GetPrefabForFilenameAsync("WorldEntities/Environment/Wrecks/life_pod_exploded_17.prefab");
            but = UWE.PrefabDatabase.GetPrefabForFilenameAsync("WorldEntities/Environment/Wrecks/life_pod_exploded_2.prefab");
            but = UWE.PrefabDatabase.GetPrefabForFilenameAsync("WorldEntities/Environment/Wrecks/life_pod_exploded_6.prefab");
            but = UWE.PrefabDatabase.GetPrefabForFilenameAsync("WorldEntities/Environment/Wrecks/life_pod_exploded_7.prefab");
            but = UWE.PrefabDatabase.GetPrefabForFilenameAsync("WorldEntities/Environment/Wrecks/life_pod_exploded_4.prefab");
            */
        }

    }

    [Nautilus.Options.Attributes.Menu("Crush Drone Options")]
    public class CrushConfig : Nautilus.Json.ConfigFile
    {
        [Nautilus.Options.Attributes.Toggle("Fragment Experience", Tooltip = "Leave checked for the fragment experience.\nLeave unchecked to unlock Crush automatically.\nMust reboot Subnautica to take effect.")]
        public bool isFragmentExperience = true;
    }
}

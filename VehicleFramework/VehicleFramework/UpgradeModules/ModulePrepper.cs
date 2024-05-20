using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using BiomeData = LootDistributionData.BiomeData;
using UnityEngine;
using Nautilus.Handlers;

namespace VehicleFramework.UpgradeModules
{
    public static class ModulePrepper
    {
        public static List<Action<ModVehicle, List<string>, int, TechType, bool>> upgradeOnAddedActions = new List<Action<ModVehicle, List<string>, int, TechType, bool>>();
        //public static List<ToggleAction> upgradeOnToggleActions = new List<ToggleAction>();

        // action, time-to-first, repeatRate, energyCostPerActivation
        public static List<Tuple<Action<ModVehicle, int>, TechType, float, float, float>> upgradeToggleActions = new List<Tuple<Action<ModVehicle, int>, TechType, float, float, float>>();

        // action, cooldown, energy_cost
        public static List<Tuple<Func<ModVehicle, int, TechType, bool>, float, float>> upgradeOnUseActions = new List<Tuple<Func<ModVehicle, int, TechType, bool>, float, float>>();

        // action, max_charge, energy_cost
        public static List<Tuple<Action<ModVehicle, int, TechType, float, float>, float, float>> upgradeOnUseChargeableActions = new List<Tuple<Action<ModVehicle, int, TechType, float, float>, float, float>>();


        public static void RegisterModVehicleDepthModules()
        {
            TechType depth1 = RegisterDepth1();
            TechType depth2 = RegisterDepth2(depth1);
            TechType depth3 = RegisterDepth3(depth2);
        }
        public static TechType RegisterModuleGeneric(List<CraftData.Ingredient> recipe, string classId, string displayName, string description, QuickSlotType qst, Atlas.Sprite icon=null, string tabName="MVCM")
        {
            Nautilus.Crafting.RecipeData moduleRecipe = new Nautilus.Crafting.RecipeData();
            moduleRecipe.Ingredients.AddRange(recipe);
            PrefabInfo module_info = PrefabInfo.WithTechType(classId, displayName, description, unlockAtStart: false);
            if(icon is null)
            {
                module_info.WithIcon(MainPatcher.ModVehicleIcon);
            }
            else
            {
                module_info.WithIcon(icon);
            }
            CustomPrefab module_CustomPrefab = new CustomPrefab(module_info);
            PrefabTemplate moduleTemplate = new CloneTemplate(module_info, TechType.SeamothElectricalDefense)
            {
                ModifyPrefab = prefab => prefab.GetComponentsInChildren<Renderer>().ForEach(r => r.materials.ForEach(m => m.color = Color.red))
            };
            module_CustomPrefab.SetGameObject(moduleTemplate);
            module_CustomPrefab.SetRecipe(moduleRecipe).WithCraftingTime(3).WithFabricatorType(CraftTree.Type.Workbench).WithStepsToFabricatorTab(new string[] { "MVUM", tabName });
            module_CustomPrefab.SetPdaGroupCategory(TechGroup.VehicleUpgrades, TechCategory.VehicleUpgrades);
            module_CustomPrefab.SetUnlock(TechType.BaseUpgradeConsole);
            module_CustomPrefab.SetEquipment(VehicleBuilder.ModuleType).WithQuickSlotType(qst);
            module_CustomPrefab.Register();
            return module_info.TechType;
        }
        public static TechType RegisterDepth1()
        {
            string classId = "ModVehicleDepthModule1";
            string displayName = LocalizationManager.GetString(EnglishString.Depth1FriendlyString);
            string description = LocalizationManager.GetString(EnglishString.Depth1Description);
            List<CraftData.Ingredient> recipe = new List<CraftData.Ingredient>()
                {
                    new CraftData.Ingredient(TechType.TitaniumIngot, 1),
                    new CraftData.Ingredient(TechType.Magnetite, 3),
                    new CraftData.Ingredient(TechType.Glass, 3),
                    new CraftData.Ingredient(TechType.AluminumOxide, 3)
                };
            return RegisterModuleGeneric(recipe, classId, displayName, description, QuickSlotType.Passive, tabName:"MVDM");
        }
        public static TechType RegisterDepth2(TechType TT1)
        {
            string classId = "ModVehicleDepthModule2";
            string displayName = LocalizationManager.GetString(EnglishString.Depth2FriendlyString);
            string description = LocalizationManager.GetString(EnglishString.Depth2Description);
            List<CraftData.Ingredient> recipe = new List<CraftData.Ingredient>()
                {
                    new CraftData.Ingredient(TT1, 1),
                    new CraftData.Ingredient(TechType.TitaniumIngot, 3),
                    new CraftData.Ingredient(TechType.Lithium, 3),
                    new CraftData.Ingredient(TechType.EnameledGlass, 3),
                    new CraftData.Ingredient(TechType.AluminumOxide, 5)
                };
            return RegisterModuleGeneric(recipe, classId, displayName, description, QuickSlotType.Passive, tabName: "MVDM");
        }
        public static TechType RegisterDepth3(TechType TT2)
        {
            string classId = "ModVehicleDepthModule3";
            string displayName = LocalizationManager.GetString(EnglishString.Depth3FriendlyString);
            string description = LocalizationManager.GetString(EnglishString.Depth3Description);
            List<CraftData.Ingredient> recipe = new List<CraftData.Ingredient>()
                {
                    new CraftData.Ingredient(TT2, 1),
                    new CraftData.Ingredient(TechType.PlasteelIngot, 3),
                    new CraftData.Ingredient(TechType.Nickel, 3),
                    new CraftData.Ingredient(TechType.EnameledGlass, 3),
                    new CraftData.Ingredient(TechType.Kyanite, 3)
                };
            return RegisterModuleGeneric(recipe, classId, displayName, description, QuickSlotType.Passive, tabName: "MVDM");
        }


    }
}

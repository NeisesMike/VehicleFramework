using System;
using System.Collections.Generic;
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
    static class ModulePrepper
    {
        public static void RegisterModVehicleDepthModules()
        {
            TechType depth1 = RegisterDepth1();
            TechType depth2 = RegisterDepth2(depth1);
            TechType depth3 = RegisterDepth3(depth2);
        }
        private static TechType RegisterDepthModuleGeneric(List<CraftData.Ingredient> recipe, string classId, string displayName, string description)
        {
            Nautilus.Crafting.RecipeData modulerRecipe = new Nautilus.Crafting.RecipeData();
            modulerRecipe.Ingredients.AddRange(recipe);
            PrefabInfo module_info = PrefabInfo.WithTechType(classId, displayName, description, unlockAtStart: false);

            //module_info.TechType = ??

            module_info.WithIcon(MainPatcher.ModVehicleIcon);
            CustomPrefab module_CustomPrefab = new CustomPrefab(module_info);
            PrefabTemplate moduleTemplate = new CloneTemplate(module_info, TechType.SeamothElectricalDefense)
            {
                ModifyPrefab = prefab => prefab.GetComponentsInChildren<Renderer>().ForEach(r => r.materials.ForEach(m => m.color = Color.red))
            };
            module_CustomPrefab.SetGameObject(moduleTemplate);
            module_CustomPrefab.SetRecipe(modulerRecipe).WithCraftingTime(3).WithFabricatorType(CraftTree.Type.SeamothUpgrades).WithStepsToFabricatorTab(new string[] { "MVUM", "MVDM" });
            module_CustomPrefab.SetPdaGroupCategory(TechGroup.VehicleUpgrades, TechCategory.VehicleUpgrades);
            module_CustomPrefab.SetUnlock(TechType.BaseUpgradeConsole);
            module_CustomPrefab.SetEquipment(VehicleBuilder.ModuleType);
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
            return RegisterDepthModuleGeneric(recipe, classId, displayName, description);
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
            return RegisterDepthModuleGeneric(recipe, classId, displayName, description);
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
            return RegisterDepthModuleGeneric(recipe, classId, displayName, description);
        }

    }
}

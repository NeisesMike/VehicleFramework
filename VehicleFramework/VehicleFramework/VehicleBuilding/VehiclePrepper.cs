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

namespace VehicleFramework
{
    static class VehiclePrepper
    { 
        //public static TechType RegisterVehicle(string classId, string displayName, string description, Dictionary<TechType,int> recipe, string encyEntry)
        public static TechType RegisterVehicle(VehicleEntry vehicle)
        {
            Nautilus.Crafting.RecipeData modulerRecipe = new Nautilus.Crafting.RecipeData();
            modulerRecipe.Ingredients.AddRange(convertRecipe(vehicle.recipe));
            PrefabInfo vehicle_info = PrefabInfo.WithTechType(vehicle.prefab.name, vehicle.prefab.name, vehicle.description);
            PDAEncyclopedia.EntryData entry = new PDAEncyclopedia.EntryData
            {
                key = vehicle.prefab.name,
                path = "Tech/Vehicles",
                nodes = new[] { "Tech", "Vehicles" },
                unlocked = true,
                popup = null,
                image = null,
            };
            LanguageHandler.SetLanguageLine("Ency_" + vehicle.prefab.name, vehicle.prefab.name);
            LanguageHandler.SetLanguageLine("EncyDesc_" + vehicle.prefab.name, vehicle.encyEntry);
            Nautilus.Handlers.PDAHandler.AddEncyclopediaEntry(entry);

            vehicle_info.WithIcon(MainPatcher.ModVehicleIcon);
            CustomPrefab module_CustomPrefab = new CustomPrefab(vehicle_info);
            vehicle.prefab.EnsureComponent<TechTag>().type = vehicle_info.TechType;
            vehicle.prefab.EnsureComponent<PrefabIdentifier>().ClassId = vehicle.prefab.name;
            module_CustomPrefab.SetGameObject(vehicle.prefab);

            module_CustomPrefab.SetRecipe(modulerRecipe).WithCraftingTime(3).WithFabricatorType(CraftTree.Type.Constructor).WithStepsToFabricatorTab(new string[] { "Vehicles" });
            module_CustomPrefab.SetPdaGroupCategory(TechGroup.Constructor, TechCategory.Constructor);
            module_CustomPrefab.SetUnlock(TechType.Constructor);
            module_CustomPrefab.Register();
            return vehicle_info.TechType;
        }

        private static List<CraftData.Ingredient> convertRecipe(Dictionary<TechType,int> dict)
        {
            List<CraftData.Ingredient> output = new List<CraftData.Ingredient>();
            foreach(var pair in dict)
            {
                output.Add(new CraftData.Ingredient(pair.Key, pair.Value));
            }
            return output;
        }
    }
}

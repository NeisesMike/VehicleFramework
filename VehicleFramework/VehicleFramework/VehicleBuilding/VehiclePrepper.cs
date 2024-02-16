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
            modulerRecipe.Ingredients.AddRange(convertRecipe(vehicle.mv.Recipe));
            PrefabInfo vehicle_info = PrefabInfo.WithTechType(vehicle.mv.name, vehicle.mv.name, vehicle.mv.Description);
            vehicle_info.WithIcon(vehicle.mv.CraftingSprite);
            PDAEncyclopedia.EntryData entry = new PDAEncyclopedia.EntryData
            {
                key = vehicle.mv.name,
                path = "Tech/Vehicles",
                nodes = new[] { "Tech", "Vehicles" },
                unlocked = true,
                popup = null,
                image = null,
            };
            LanguageHandler.SetLanguageLine("Ency_" + vehicle.mv.name, vehicle.mv.name);
            LanguageHandler.SetLanguageLine("EncyDesc_" + vehicle.mv.name, vehicle.mv.EncyclopediaEntry);
            Nautilus.Handlers.PDAHandler.AddEncyclopediaEntry(entry);

            CustomPrefab module_CustomPrefab = new CustomPrefab(vehicle_info);
            vehicle.mv.VehicleModel.EnsureComponent<TechTag>().type = vehicle_info.TechType;
            vehicle.mv.VehicleModel.EnsureComponent<PrefabIdentifier>().ClassId = vehicle.mv.name;
            module_CustomPrefab.SetGameObject(vehicle.mv.VehicleModel);

            module_CustomPrefab.SetRecipe(modulerRecipe).WithCraftingTime(3).WithFabricatorType(CraftTree.Type.Constructor).WithStepsToFabricatorTab(new string[] { "Vehicles" });
            module_CustomPrefab.SetPdaGroupCategory(TechGroup.Constructor, TechCategory.Constructor);
            module_CustomPrefab.SetUnlock(vehicle.mv.UnlockedWith);
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

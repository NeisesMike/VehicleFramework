using System.Linq;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using BiomeData = LootDistributionData.BiomeData;
using Nautilus.Handlers;
using System.Reflection;
using System.IO;
using Nautilus.Utility;
using Nautilus.Crafting;
using VehicleFramework.Assets;

namespace VehicleFramework
{
    static class VehiclePrepper
    { 
        //public static TechType RegisterVehicle(string classId, string displayName, string description, Dictionary<TechType,int> recipe, string encyEntry)
        public static TechType RegisterVehicle(VehicleEntry vehicle)
        {
            PrefabInfo vehicle_info = PrefabInfo.WithTechType(vehicle.mv.name, vehicle.mv.name, vehicle.mv.Description);
            vehicle_info.WithIcon(vehicle.mv.CraftingSprite ?? StaticAssets.ModVehicleIcon);
            if (vehicle.mv.EncyclopediaEntry != null && vehicle.mv.EncyclopediaEntry.Length > 0)
            {
                PDAEncyclopedia.EntryData entry = new PDAEncyclopedia.EntryData
                {
                    key = vehicle.mv.name,
                    path = "Tech/Vehicles",
                    nodes = new[] { "Tech", "Vehicles" },
                    unlocked = true,
                    popup = null,
                    image = vehicle.mv.EncyclopediaImage?.texture,
                };
                LanguageHandler.SetLanguageLine("Ency_" + vehicle.mv.name, vehicle.mv.name);
                LanguageHandler.SetLanguageLine("EncyDesc_" + vehicle.mv.name, vehicle.mv.EncyclopediaEntry);
                PDAHandler.AddEncyclopediaEntry(entry);
            }

            CustomPrefab module_CustomPrefab = new CustomPrefab(vehicle_info);
            vehicle.mv.VehicleModel.EnsureComponent<TechTag>().type = vehicle_info.TechType;
            vehicle.mv.VehicleModel.EnsureComponent<PrefabIdentifier>().ClassId = vehicle.mv.name;
            module_CustomPrefab.SetGameObject(vehicle.mv.VehicleModel);
            string jsonRecipeFileName = Path.Combine(
                                            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                                            "recipes",
                                            vehicle.mv.name + "_recipe.json");
            RecipeData modulerRecipe = JsonUtils.Load<RecipeData>(jsonRecipeFileName, false, new Nautilus.Json.Converters.CustomEnumConverter());
            if(modulerRecipe.Ingredients.Count() == 0)
            {
                modulerRecipe.Ingredients.AddRange(vehicle.mv.Recipe.Select(x => new CraftData.Ingredient(x.Key, x.Value)).ToList());
                JsonUtils.Save<RecipeData>(modulerRecipe, jsonRecipeFileName, new Nautilus.Json.Converters.CustomEnumConverter());
                modulerRecipe = JsonUtils.Load<RecipeData>(jsonRecipeFileName, false, new Nautilus.Json.Converters.CustomEnumConverter());
            }

            module_CustomPrefab.SetRecipe(modulerRecipe).WithCraftingTime(3).WithFabricatorType(CraftTree.Type.Constructor).WithStepsToFabricatorTab(new string[] { "Vehicles" });
            module_CustomPrefab.SetPdaGroupCategory(TechGroup.Constructor, TechCategory.Constructor);
            var scanningGadget = module_CustomPrefab.SetUnlock(vehicle.mv.UnlockedWith);
            if (vehicle.mv.UnlockedSprite != null)
            {
                scanningGadget.WithAnalysisTech(vehicle.mv.UnlockedSprite, unlockMessage: vehicle.mv.UnlockedMessage);
            }
            module_CustomPrefab.Register();
            return vehicle_info.TechType;
        }

    }
}

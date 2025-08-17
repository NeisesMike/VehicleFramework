using System.Linq;
using Nautilus.Assets.Gadgets;
using System.Reflection;
using System.IO;
using VehicleFramework.Assets;

namespace VehicleFramework
{
    internal static class VehicleNautilusInterface
    {
        internal static TechType RegisterVehicle(VehicleEntry vehicle)
        {
            string vehicleKey = vehicle.mv.name;
            Nautilus.Assets.PrefabInfo vehicle_info = Nautilus.Assets.PrefabInfo.WithTechType(vehicleKey, vehicleKey, vehicle.mv.Description);
            vehicle_info.WithIcon(vehicle.mv.CraftingSprite ?? StaticAssets.ModVehicleIcon);

            Nautilus.Assets.CustomPrefab module_CustomPrefab = new(vehicle_info);
            Nautilus.Utility.PrefabUtils.AddBasicComponents(vehicle.mv.VehicleModel, vehicleKey, vehicle_info.TechType, LargeWorldEntity.CellLevel.Global);
            module_CustomPrefab.SetGameObject(vehicle.mv.VehicleModel);
            string jsonRecipeFileName = Path.Combine(
                                            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                                            "recipes",
                                            $"{vehicleKey}_recipe.json");
            Nautilus.Crafting.RecipeData vehicleRecipe = Nautilus.Utility.JsonUtils.Load<Nautilus.Crafting.RecipeData>(jsonRecipeFileName, false, new Nautilus.Json.Converters.CustomEnumConverter());
            if (vehicleRecipe.Ingredients.Count() == 0)
            {
                // If the custom recipe file doesn't exist, go ahead and make it using the default recipe.
                vehicleRecipe.Ingredients.AddRange(vehicle.mv.Recipe.Select(x => new Ingredient(x.Key, x.Value)).ToList());
                Nautilus.Utility.JsonUtils.Save<Nautilus.Crafting.RecipeData>(vehicleRecipe, jsonRecipeFileName, new Nautilus.Json.Converters.CustomEnumConverter());
            }
            if (VehicleConfig.GetConfig(vehicle.mv).UseCustomRecipe.Value)
            {
                vehicleRecipe = Nautilus.Utility.JsonUtils.Load<Nautilus.Crafting.RecipeData>(jsonRecipeFileName, false, new Nautilus.Json.Converters.CustomEnumConverter());
            }
            else
            {
                vehicleRecipe = new Nautilus.Crafting.RecipeData();
                vehicleRecipe.Ingredients.AddRange(vehicle.mv.Recipe.Select(x => new Ingredient(x.Key, x.Value)).ToList());
            }

            module_CustomPrefab.SetRecipe(vehicleRecipe).WithFabricatorType(CraftTree.Type.Constructor).WithStepsToFabricatorTab(new string[] { "Vehicles" });
            var scanningGadget = module_CustomPrefab.SetUnlock(vehicle.mv.UnlockedWith)
                .WithPdaGroupCategory(TechGroup.Constructor, TechCategory.Constructor);

            if (!string.IsNullOrEmpty(vehicle.mv.EncyclopediaEntry))
            {
                Nautilus.Handlers.LanguageHandler.SetLanguageLine($"Ency_{vehicleKey}", vehicleKey);
                Nautilus.Handlers.LanguageHandler.SetLanguageLine($"EncyDesc_{vehicleKey}", vehicle.mv.EncyclopediaEntry);
                scanningGadget.WithEncyclopediaEntry("Tech/Vehicles", null, vehicle.mv.EncyclopediaImage?.texture);
                Nautilus.Handlers.StoryGoalHandler.RegisterItemGoal(vehicleKey, Story.GoalType.Encyclopedia, vehicle.mv.UnlockedWith);
            }

            if (vehicle.mv.UnlockedSprite != null)
            {
                scanningGadget.WithAnalysisTech(vehicle.mv.UnlockedSprite, unlockMessage: vehicle.mv.UnlockedMessage);
            }
            module_CustomPrefab.Register();
            return vehicle_info.TechType;
        }

        internal static void PatchCraftable(ref VehicleEntry ve, bool verbose)
        {
            try
            {
                TechType techType = VehicleNautilusInterface.RegisterVehicle(ve);
                VehicleRegistrar.VerboseLog(VehicleRegistrar.LogType.Log, verbose, $"Patched the {ve.name} Craftable");
                VehicleEntry newVE = new(ve.mv, ve.unique_id, ve.pt, ve.ping_sprite, techType);
                VehicleManager.vehicleTypes.Add(newVE);
            }
            catch(System.Exception e)
            {
                Logger.LogException($"VehicleNautilusInterface Error: Failed to Register Vehicle {ve.name}. Error follows:", e);
                Logger.LoopMainMenuError($"Failed registration. See log.", ve.name);
            }
        }
    }
}

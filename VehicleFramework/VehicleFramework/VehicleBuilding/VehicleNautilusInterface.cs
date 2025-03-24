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
            Nautilus.Assets.PrefabInfo vehicle_info = Nautilus.Assets.PrefabInfo.WithTechType(vehicle.mv.name, vehicle.mv.name, vehicle.mv.Description);
            vehicle_info.WithIcon(vehicle.mv.CraftingSprite ?? StaticAssets.ModVehicleIcon);
            if (!vehicle.mv.EncyclopediaEntry.Equals(string.Empty))
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
                Patches.LanguagePatcher.SetLanguageLine("Ency_" + vehicle.mv.name, vehicle.mv.name);
                Patches.LanguagePatcher.SetLanguageLine("EncyDesc_" + vehicle.mv.name, vehicle.mv.EncyclopediaEntry);
                Admin.Utils.AddEncyclopediaEntry(entry);
            }

            Nautilus.Assets.CustomPrefab module_CustomPrefab = new Nautilus.Assets.CustomPrefab(vehicle_info);
            vehicle.mv.VehicleModel.EnsureComponent<TechTag>().type = vehicle_info.TechType;
            vehicle.mv.VehicleModel.EnsureComponent<PrefabIdentifier>().ClassId = vehicle.mv.name;
            module_CustomPrefab.SetGameObject(vehicle.mv.VehicleModel);
            string jsonRecipeFileName = Path.Combine(
                                            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                                            "recipes",
                                            vehicle.mv.name + "_recipe.json");
            Nautilus.Crafting.RecipeData modulerRecipe = Nautilus.Utility.JsonUtils.Load<Nautilus.Crafting.RecipeData>(jsonRecipeFileName, false, new Nautilus.Json.Converters.CustomEnumConverter());
            if(modulerRecipe.Ingredients.Count() == 0)
            {
                modulerRecipe.Ingredients.AddRange(vehicle.mv.Recipe.Select(x => new CraftData.Ingredient(x.Key, x.Value)).ToList());
                Nautilus.Utility.JsonUtils.Save<Nautilus.Crafting.RecipeData>(modulerRecipe, jsonRecipeFileName, new Nautilus.Json.Converters.CustomEnumConverter());
                modulerRecipe = Nautilus.Utility.JsonUtils.Load<Nautilus.Crafting.RecipeData>(jsonRecipeFileName, false, new Nautilus.Json.Converters.CustomEnumConverter());
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

        internal static void PatchCraftable(ref VehicleEntry ve, bool verbose)
        {
            TechType techType = VehicleNautilusInterface.RegisterVehicle(ve);
            VehicleRegistrar.VerboseLog(VehicleRegistrar.LogType.Log, verbose, "Patched the " + ve.name + " Craftable");
            VehicleEntry newVE = new VehicleEntry(ve.mv, ve.unique_id, ve.pt, ve.ping_sprite, techType);
            VehicleManager.vehicleTypes.Add(newVE);
        }

    }
}

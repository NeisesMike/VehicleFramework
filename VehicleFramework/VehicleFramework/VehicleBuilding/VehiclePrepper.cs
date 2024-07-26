using System;
using System.Collections.Generic;
using BiomeData = LootDistributionData.BiomeData;
using UnityEngine;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;

namespace VehicleFramework
{
    public class VehicleCraftable : Craftable
    {
        VehicleEntry ve;
        public VehicleCraftable(VehicleEntry inputVE) : base(inputVE.name, inputVE.mv.name, inputVE.mv.Description)
        {
            ve = inputVE;
        }

        //===============================
        // Craftable overrides
        //===============================
        public override CraftTree.Type FabricatorType => CraftTree.Type.Constructor;
        public override string[] StepsToFabricatorTab => new[] { "Vehicles" };
        public override float CraftingTime => 10f;


        //===============================
        // PDAItem overrides
        //===============================
        public override TechType RequiredForUnlock => TechType.Constructor;
        public override TechGroup GroupForPDA => TechGroup.Constructor;
        public override TechCategory CategoryForPDA => TechCategory.Constructor;
        public override PDAEncyclopedia.EntryData EncyclopediaEntryData
        {
            get
            {
                PDAEncyclopedia.EntryData entry = new PDAEncyclopedia.EntryData
                {
                    key = ClassID,
                    path = "Tech/Vehicles",
                    nodes = new[] { "Tech", "Vehicles" },
                    unlocked = true,
                    popup = null,
                    image = null
                };
                LanguageHandler.SetLanguageLine("Ency_" + ClassID, ClassID);
                LanguageHandler.SetLanguageLine("EncyDesc_" + ClassID, ve.mv.EncyclopediaEntry);
                return entry;
            }
        }

        protected override TechData GetBlueprintRecipe()
        {
            List<Ingredient> ingredients = new List<Ingredient>();
            foreach (KeyValuePair<TechType, int> pair in ve.mv.Recipe)
            {
                ingredients.Add(new Ingredient(pair.Key, pair.Value));
            }
            return new TechData
            {
                Ingredients = ingredients,
                craftAmount = 1
            };
        }

        //===============================
        // Spawnable overrides
        //===============================
        protected override Atlas.Sprite GetItemSprite()
        {
            return MainPatcher.ModVehicleIcon;
        }

        //===============================
        // ModPrefab overrides
        //===============================
        public override GameObject GetGameObject()
        {
            foreach (VehicleEntry ve in VehicleManager.vehicleTypes)
            {
                if (ve.mv.name == ClassID)
                {
                    GameObject thisVehicle = GameObject.Instantiate(ve.mv.VehicleModel);
                    thisVehicle.EnsureComponent<TechTag>().type = TechType;
                    thisVehicle.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
                    thisVehicle.SetActive(true);
                    return thisVehicle;
                }
            }
            Logger.Error("Craftable failed to find the prefab for: " + ClassID);
            return null;
        }
    }
    /*
    static class VehiclePrepper
    { 
        //public static TechType RegisterVehicle(string classId, string displayName, string description, Dictionary<TechType,int> recipe, string encyEntry)
        public static TechType RegisterVehicle(VehicleEntry vehicle)
        {
            SMLHelper.V2.Crafting.RecipeData modulerRecipe = new SMLHelper.V2.Crafting.RecipeData();
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
            SMLHelper.V2.Handlers.PDAHandler.AddEncyclopediaEntry(entry);

            CustomPrefab module_CustomPrefab = new CustomPrefab(vehicle_info);
            vehicle.mv.VehicleModel.EnsureComponent<TechTag>().type = vehicle_info.TechType;
            vehicle.mv.VehicleModel.EnsureComponent<PrefabIdentifier>().ClassId = vehicle.mv.name;
            module_CustomPrefab.SetGameObject(vehicle.mv.VehicleModel);

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
    */
}

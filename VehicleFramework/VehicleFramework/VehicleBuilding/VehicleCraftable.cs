using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using UnityEngine;
using System.IO;

namespace VehicleFramework
{
    public class VehicleCraftable : Craftable
    {
        public VehicleCraftable(string classId, string friendlyName, string description) : base(classId, friendlyName, description)
        {
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
        //public override TechType RequiredForUnlock => TechType.Constructor;
        public override bool UnlockedAtStart => true;
        public override TechGroup GroupForPDA => TechGroup.Constructor;
        public override TechCategory CategoryForPDA => TechCategory.Constructor;
        public override PDAEncyclopedia.EntryData EncyclopediaEntryData
        {
            get
            {
                PDAEncyclopedia.EntryData entry = new PDAEncyclopedia.EntryData();
                entry.key = ClassID;
                entry.path = "Tech/Vehicles";
                entry.nodes = new[] { "Tech", "Vehicles" };
                entry.unlocked = false;
                return entry;
            }
        }

        protected override TechData GetBlueprintRecipe()
        {
            return new TechData
            {
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(TechType.TitaniumIngot, 1),
                    new Ingredient(TechType.PlasteelIngot, 1),
                    new Ingredient(TechType.Lubricant, 1),
                    new Ingredient(TechType.AdvancedWiringKit, 1),
                    new Ingredient(TechType.Lead, 2),
                    new Ingredient(TechType.EnameledGlass, 2)
                },
                craftAmount = 1
            };
        }

        //===============================
        // Spawnable overrides
        //===============================
        protected override Atlas.Sprite GetItemSprite()
        {
            return null;
        }

        //===============================
        // ModPrefab overrides
        //===============================
        public override GameObject GetGameObject()
        {
            foreach(VehicleEntry ve in VehicleManager.vehicleTypes)
            {
                if(ve.prefab.name == ClassID)
                {
                    GameObject thisVehicle = GameObject.Instantiate(ve.prefab);
                    thisVehicle.EnsureComponent<TechTag>().type = TechType;
                    thisVehicle.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
                    return thisVehicle;
                }
            }
            Logger.Log("Craftable failed to find the prefab for: " + ClassID);
            return null;
        }
    }
}

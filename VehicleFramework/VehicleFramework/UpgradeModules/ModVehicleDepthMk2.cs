using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using UnityEngine;
using System.IO;
using System.Reflection;
using UnityEngine.Sprites;

namespace VehicleFramework.UpgradeModules
{
    public class ModVehicleDepthMk2 : Equipable
    {
        public ModVehicleDepthMk2() : base(
            classId: "ModVehicleDepthModule2",
            friendlyName: "Vehicle Depth Module MK2",
            description: "Increases Crush Depth to 800m over Base. Does Not Stack.")
        {
        }

        public override EquipmentType EquipmentType => VehicleBuilder.ModuleType;

        public override TechType RequiredForUnlock => TechType.BaseUpgradeConsole;

        public override TechGroup GroupForPDA => TechGroup.VehicleUpgrades;

        public override TechCategory CategoryForPDA => TechCategory.VehicleUpgrades;

        public override CraftTree.Type FabricatorType => CraftTree.Type.Workbench;

        //public override string[] StepsToFabricatorTab => new string[] { "SeamothMenu", "ModVehicle", "Depth" };
        public override string[] StepsToFabricatorTab => new string[] { "MVUM", "MVDM" };
        public override QuickSlotType QuickSlotType => QuickSlotType.Passive;

        public override GameObject GetGameObject()
        {
            // Get the ElectricalDefense module prefab and instantiate it
            string path = "WorldEntities/Tools/SeamothElectricalDefense";
            GameObject prefab = Resources.Load<GameObject>(path);
            GameObject obj = GameObject.Instantiate(prefab);

            // Get the TechTags and PrefabIdentifiers
            TechTag techTag = obj.GetComponent<TechTag>();
            PrefabIdentifier prefabIdentifier = obj.GetComponent<PrefabIdentifier>();

            // Change them so they fit to our requirements.
            techTag.type = TechType;
            prefabIdentifier.ClassId = ClassID;

            return obj;
        }
        protected override TechData GetBlueprintRecipe()
        {
            return new TechData()
            {
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(MainPatcher.modVehicleDepthModule1.TechType, 1),
                    new Ingredient(TechType.TitaniumIngot, 3),
                    new Ingredient(TechType.Lithium, 3),
                    new Ingredient(TechType.EnameledGlass, 3),
                    new Ingredient(TechType.AluminumOxide, 5)
                },
                craftAmount = 1
            };
        }

        protected override Atlas.Sprite GetItemSprite()
        {
            return MainPatcher.ModVehicleIcon;
        }
    }
}

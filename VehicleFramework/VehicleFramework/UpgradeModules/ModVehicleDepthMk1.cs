using System;
using System.Collections.Generic;
using System.Collections;
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
    public class ModVehicleDepthMk1 : Equipable
    {
        public ModVehicleDepthMk1() : base(
            classId: "ModVehicleDepthModule1",
            friendlyName: "Vehicle Depth Module MK1",
            description: "Increases Crush Depth to 400m over Base. Does Not Stack.")
        {
            //OnStartedPatching += () => CraftTreeHandler.AddTabNode(CraftTree.Type.Workbench, "Depth", "Depth Modules", MainPatcher.ModVehicleIcon, stepsToDepthTab);
            OnStartedPatching += () => CraftTreeHandler.AddTabNode(CraftTree.Type.Workbench, "MVUM", "ModVehicle Upgrade Modules", MainPatcher.ModVehicleIcon);
            string[] stepsToDepthTab = { "MVUM" };
            OnStartedPatching += () => CraftTreeHandler.AddTabNode(CraftTree.Type.Workbench, "MVDM", "ModVehicle Depth Modules", MainPatcher.ModVehicleIcon, stepsToDepthTab);
            
        }

        public override EquipmentType EquipmentType => VehicleBuilder.ModuleType;

        public override TechType RequiredForUnlock => TechType.BaseUpgradeConsole;

        public override TechGroup GroupForPDA => TechGroup.VehicleUpgrades;

        public override TechCategory CategoryForPDA => TechCategory.VehicleUpgrades;

        public override CraftTree.Type FabricatorType => CraftTree.Type.Workbench;

        //public override string[] StepsToFabricatorTab => new string[] { "SeamothMenu", "ModVehicle", "Depth" };
        public override string[] StepsToFabricatorTab => new string[] { "MVUM", "MVDM" };
        public override QuickSlotType QuickSlotType => QuickSlotType.Passive;

        private static GameObject _internalGameObject;
        public static GameObject InternalGameObject
        {
            get
            {
                return _internalGameObject;
            }
            private set
            {
                _internalGameObject = value;
            }
        }
        public IEnumerator ExtractGameObject()
        {
            while (!LargeWorldStreamer.main || !LargeWorldStreamer.main.IsReady() || !LargeWorldStreamer.main.IsWorldSettled())
            {
                yield return new WaitForSecondsRealtime(1f);
            }
            // Get the ElectricalDefense module prefab and instantiate it
            TaskResult<GameObject> result = new TaskResult<GameObject>();
            yield return CoroutineHelper.Starto(CraftData.InstantiateFromPrefabAsync(TechType.SeamothElectricalDefense, result, false));
            GameObject obj = result.Get();

            // Get the TechTags and PrefabIdentifiers
            TechTag techTag = obj.GetComponent<TechTag>();
            PrefabIdentifier prefabIdentifier = obj.GetComponent<PrefabIdentifier>();

            // Change them so they fit to our requirements.
            techTag.type = TechType;
            prefabIdentifier.ClassId = ClassID;

            InternalGameObject = obj;
            yield break;
        }
        public override GameObject GetGameObject()
        {
            return InternalGameObject;
        }
        protected override TechData GetBlueprintRecipe()
        {
            return new TechData()
            {
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(TechType.TitaniumIngot, 1),
                    new Ingredient(TechType.Magnetite, 3),
                    new Ingredient(TechType.Glass, 3),
                    new Ingredient(TechType.AluminumOxide, 3)
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

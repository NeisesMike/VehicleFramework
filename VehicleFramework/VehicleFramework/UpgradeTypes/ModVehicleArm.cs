using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework.UpgradeTypes
{
    public abstract class ModVehicleArm : SelectableUpgrade
    {
        public override string Description => "This is an arm module.";
        public override QuickSlotType QuickSlotType => QuickSlotType.Selectable;
        public override Atlas.Sprite Icon => MainPatcher.ArmIcon;
        public abstract IEnumerator GetArmPrefab(IOut<GameObject> arm);
        public virtual void OnArmSelected(ArmActionParams param)
        {
            Logger.Log("Selecting arm: " + ClassId + " on ModVehicle: " + param.vehicle.subName.name + " in slotID: " + param.slotID.ToString());
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VehicleFramework.VehicleComponents;
using VehicleFramework.Admin;

namespace VehicleFramework.UpgradeTypes
{
    public struct ArmEnergyCosts
    {
        public bool spendOnDown;
        public bool spendOnUp;
        public bool spendOnHeld;
        public float downEnergyCost;
        public float upEnergyCost;
        public float heldEnergyCost;
    }
    public struct ArmCooldowns
    {
        public bool coolOnDown;
        public bool coolOnUp;
        public float downCooldown;
        public float upCooldown;
    }
    public abstract class ModVehicleArm : ModVehicleUpgrade
    {
        internal GameObject armPrefab = null; // this gets set by UpgradeRegistrar.RegisterArmUpgradeActions using this.GetArmPrefab
        public override string Description => "This is an arm module.";
        public override QuickSlotType QuickSlotType => QuickSlotType.Selectable; // must be either selectable or selectablechargeable (only matters for exosuit?)
        public virtual ArmEnergyCosts EnergyCosts => default;
        public virtual ArmCooldowns ArmCooldowns => default;
        public override Atlas.Sprite Icon => MainPatcher.ArmIcon;
        public abstract IEnumerator GetArmPrefab(IOut<GameObject> arm);
        public virtual bool OnArmDown(ArmActionParams param, out float cooldown) // return true OnUseSuccess (false if you refuse to do the action)
        {
            Logger.Log("Selecting arm: " + ClassId + " on ModVehicle: " + param.vehicle.subName.name + " in slotID: " + param.slotID.ToString());
            cooldown = 0;
            return true;
        }
        public virtual bool OnArmHeld(ArmActionParams param, out float cooldown) // return true OnHoldSuccess (false if you don't have "hold" logic)
        {
            Logger.Log("Holding arm: " + ClassId + " on ModVehicle: " + param.vehicle.subName.name + " in slotID: " + param.slotID.ToString());
            cooldown = 0;
            return true;
        }
        public virtual bool OnArmUp(ArmActionParams param, out float cooldown) // return true OnReleaseSuccess (basically always)
        {
            Logger.Log("Releasing arm: " + ClassId + " on ModVehicle: " + param.vehicle.subName.name + " in slotID: " + param.slotID.ToString());
            cooldown = 0;
            return true;
        }
        public virtual bool OnArmAltUse(ArmActionParams param)
        {
            Logger.Log("Releasing arm: " + ClassId + " on ModVehicle: " + param.vehicle.subName.name + " in slotID: " + param.slotID.ToString());
            return false;
        }

        #region ExosuitOnlyMethods
        public virtual void Update(GameObject arm, Vehicle vehicle, ref Quaternion aimDirection)
        {
            // take action every frame, and possible update the arm's Aim Direction.
        }
        public virtual void OnPilotExit(GameObject arm, Vehicle vehicle)
        {
            // reset the arm if need be
        }
        public virtual GameObject GetInteractableRoot(GameObject arm, Vehicle vehicle, GameObject target)
        {
            // target is the camera's LookTarget.
            // the return value becomes Exosuit.activeTarget,
            // which influences OnHandHover GUI notifications,
            // and can be useful in arm logic
            return null;
        }
        #endregion
    }
}

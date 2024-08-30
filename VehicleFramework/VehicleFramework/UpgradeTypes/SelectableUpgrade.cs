using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework.UpgradeTypes
{
    public abstract class SelectableUpgrade : ModVehicleUpgrade
    {
        public override string Description => "This is a selectable upgrade module.";
        public override QuickSlotType QuickSlotType => QuickSlotType.Selectable;
        public virtual float Cooldown => 0;
        public virtual float EnergyCost => 0;
        public virtual void OnSelected(SelectableActionParams param)
        {
            Logger.Log("Selecting " + ClassId + " on ModVehicle: " + param.mv.subName.name + " in slotID: " + param.slotID.ToString());
        }
    }
}

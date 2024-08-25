using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework
{
    public abstract class ModVehicleArm
    {
        public TechType techType { get; internal set; }
        public abstract string GetClassId();
        public abstract string GetDisplayName();
        public virtual List<Tuple<TechType, int>> GetRecipe()
        {
            var list = new List<Tuple<TechType, int>>
            {
                new Tuple<TechType, int>(TechType.Titanium, 1)
            };
            return list;
        }
        public virtual string GetDescription()
        {
            return "This is an arm module;";
        }
        public virtual Atlas.Sprite GetIcon()
        {
            return MainPatcher.ArmIcon;
        }
        public virtual string GetTabName()
        {
            return "MVCM";
        }
        public virtual float GetCooldown()
        {
            return 0;
        }
        public virtual float GetEnergyCost()
        {
            return 0;
        }
        public virtual GameObject GetPrefab()
        {
            return null;
        }
        public virtual void OnAdded(ModVehicle mv, List<string> upgrades, int slotID)
        {
            Logger.Log("Adding " + GetClassId() + " to ModVehicle: " + mv.subName.name + " in slotID: " + slotID.ToString());
        }
        public virtual void OnRemoved(ModVehicle mv, List<string> upgrades, int slotID)
        {
            Logger.Log("Removing " + GetClassId() + " to ModVehicle: " + mv.subName.name + " in slotID: " + slotID.ToString());
        }
        public virtual void OnSelected(ModVehicle mv, GameObject arm, int slotID)
        {
            Logger.Log("Selecting " + GetClassId() + " on ModVehicle: " + mv.subName.name + " in slotID: " + slotID.ToString());
        }
    }
}

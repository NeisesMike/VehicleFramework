using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework.UpgradeTypes
{
    public struct AddActionParams
    {
        public ModVehicle mv;
        public int slotID;
        public TechType techType;
        public bool isAdded;
    }
    public struct ToggleActionParams
    {
        public ModVehicle mv;
        public int slotID;
        public TechType techType;
        public bool active;
    }
    public struct SelectableChargeableActionParams
    {
        public ModVehicle mv;
        public int slotID;
        public TechType techType;
        public float charge;
        public float slotCharge;
    }
    public struct SelectableActionParams
    {
        public ModVehicle mv;
        public int slotID;
        public TechType techType;
    }
    public struct ArmActionParams
    {
        public ModVehicle mv;
        public int slotID;
        public TechType techType;
        public GameObject arm;
    }
}

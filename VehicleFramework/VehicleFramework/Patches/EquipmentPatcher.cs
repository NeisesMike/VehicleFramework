using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace VehicleFramework
{
    [HarmonyPatch(typeof(Equipment))]
    public class EquipmentPatcher
    {
        /*
         * Atrama has 8 total upgrade slots
         * 6 module slots : "AtramaModuleX" where X int in [0,5]
         * 2 arm slots : "AtramaArmX" where X in {Left, Right}
         */

        /*
         * This collection of patches ensures our new upgrade slots interact nicely with the base game's Equipment class.
         */

        public static List<string> vehicleModuleSlots = new List<string> { "VehicleModule0", "VehicleModule1", "VehicleModule2",
                                                                           "VehicleModule3", "VehicleModule4", "VehicleModule5",
                                                                           "VehicleModule6", "VehicleModule7", "VehicleModule8",
                                                                           "VehicleModule9", "VehicleModule10", "VehicleModule11"};
        public static List<string> vehicleArmSlots = new List<string> { "VehicleArmLeft", "VehicleArmRight" };

        public static Dictionary<EquipmentType, List<string>> vehicleTypeToSlots = new Dictionary<EquipmentType, List<string>>
                {
                    { VehicleBuilder.ModuleType, vehicleModuleSlots },
                    { VehicleBuilder.ArmType, vehicleArmSlots }
                };


        [HarmonyPrefix]
        [HarmonyPatch("SetLabel")]
        public static bool SetLabelPrefix(Equipment __instance, string l, ref Dictionary<EquipmentType, List<string>> ___typeToSlots)
        {
            if (!l.Contains("Vehicle"))
            {
                return true;
            }
            ___typeToSlots = vehicleTypeToSlots;
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("AddSlot")]
        public static bool AddSlotPrefix(string slot, ref Dictionary<EquipmentType, List<string>> ___typeToSlots)
        {
            if (!slot.Contains("Vehicle"))
            {
                return true;
            }
            ___typeToSlots = vehicleTypeToSlots;
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("GetCompatibleSlotDefault")]
        public static bool GetCompatibleSlotDefaultPrefix(EquipmentType itemType, ref Dictionary<EquipmentType, List<string>> ___typeToSlots)
        {
            if (itemType != VehicleBuilder.ModuleType && itemType != VehicleBuilder.ArmType)
            {
                return true;
            }
            ___typeToSlots = vehicleTypeToSlots;
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("GetFreeSlot")]
        public static bool GetFreeSlotPrefix(EquipmentType type, ref Dictionary<EquipmentType, List<string>> ___typeToSlots)
        {
            if (type != VehicleBuilder.ModuleType && type != VehicleBuilder.ArmType)
            {
                return true;
            }
            ___typeToSlots = vehicleTypeToSlots;
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("GetSlots")]
        public static bool GetSlotsPrefix(EquipmentType itemType, ref Dictionary<EquipmentType, List<string>> ___typeToSlots)
        {
            if (itemType != VehicleBuilder.ModuleType && itemType != VehicleBuilder.ArmType)
            {
                return true;
            }
            ___typeToSlots = vehicleTypeToSlots;
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("RemoveSlot")]
        public static bool RemoveSlot(string slot, ref Dictionary<EquipmentType, List<string>> ___typeToSlots)
        {
            if (!slot.Contains("Vehicle"))
            {
                return true;
            }
            ___typeToSlots = vehicleTypeToSlots;
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("GetSlotType")]
        public static bool GetSlotTypePrefix(string slot, ref EquipmentType __result, Dictionary<EquipmentType, List<string>> ___typeToSlots)
        {
            if (vehicleModuleSlots.Contains(slot))
            {
                __result = VehicleBuilder.ModuleType;
                return false;
            }
            if (vehicleArmSlots.Contains(slot))
            {
                __result = VehicleBuilder.ArmType;
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("IsCompatible")]
        public static bool IsCompatiblePrefix(EquipmentType itemType, EquipmentType slotType, ref bool __result)
        {
            __result = itemType == slotType || (itemType == EquipmentType.VehicleModule && (slotType == EquipmentType.SeamothModule || slotType == EquipmentType.ExosuitModule || slotType == VehicleBuilder.ModuleType));
            if (__result)
            {
                return false;
            }
            return true;
        }
    }
}

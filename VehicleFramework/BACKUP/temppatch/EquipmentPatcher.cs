using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace AtramaVehicle
{
    [HarmonyPatch(typeof(Equipment))]
    public class EquipmentPatcher
    {
        /*
         * Atrama has 8 total upgrade slots
         * 6 module slots : "AtramaModuleX" where X int in [0,5]
         * 2 arm slots : "AtramaArmX" where X in {Left, Right}
         */

        public static List<string> atramaModuleSlots = new List<string> { "AtramaModule1", "AtramaModule2", "AtramaModule3", "AtramaModule4", "AtramaModule5", "AtramaModule6" };
        public static List<string> atramaArmSlots = new List<string> { "AtramaArmLeft", "AtramaArmRight"};

        public static Dictionary<EquipmentType, List<string>> atramaTypeToSlots = new Dictionary<EquipmentType, List<string>>
                {
                    { AtramaManager.atramaModuleType, atramaModuleSlots },
                    { AtramaManager.atramaArmType, atramaArmSlots }
                };


        [HarmonyPrefix]
        [HarmonyPatch("SetLabel")]
        public static bool SetLabelPrefix(Equipment __instance, string l, ref Dictionary<EquipmentType, List<string>> ___typeToSlots)
        {
            if (!l.Contains("Atrama"))
            {
                return true;
            }
            ___typeToSlots = atramaTypeToSlots;
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("AddSlot")]
        public static bool AddSlotPrefix(string slot, ref Dictionary<EquipmentType, List<string>> ___typeToSlots)
        {
            if (!slot.Contains("Atrama"))
            {
                return true;
            }
            ___typeToSlots = atramaTypeToSlots;
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("GetCompatibleSlotDefault")]
        public static bool GetCompatibleSlotDefaultPrefix(EquipmentType itemType, ref Dictionary<EquipmentType, List<string>> ___typeToSlots)
        {
            if ((int)itemType != 625 && (int)itemType != 626)
            {
                return true;
            }
            ___typeToSlots = atramaTypeToSlots;
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("GetFreeSlot")]
        public static bool GetFreeSlotPrefix(EquipmentType type, ref Dictionary<EquipmentType, List<string>> ___typeToSlots)
        {
            if ((int)type != 625 && (int)type != 626)
            {
                return true;
            }
            ___typeToSlots = atramaTypeToSlots;
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("GetSlots")]
        public static bool GetSlotsPrefix(EquipmentType itemType, ref Dictionary<EquipmentType, List<string>> ___typeToSlots)
        {
            if ((int)itemType != 625 && (int)itemType != 626)
            {
                return true;
            }
            ___typeToSlots = atramaTypeToSlots;
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("RemoveSlot")]
        public static bool RemoveSlot(string slot, ref Dictionary<EquipmentType, List<string>> ___typeToSlots)
        {
            if (!slot.Contains("Atrama"))
            {
                return true;
            }
            ___typeToSlots = atramaTypeToSlots;
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("GetSlotType")]
        public static bool GetSlotTypePrefix(string slot, ref EquipmentType __result, Dictionary<EquipmentType, List<string>> ___typeToSlots)
        {
            if (atramaModuleSlots.Contains(slot))
            {
                __result = AtramaManager.atramaModuleType;
                return false;
            }
            if (atramaArmSlots.Contains(slot))
            {
                __result = AtramaManager.atramaArmType;
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("IsCompatible")]
        public static bool IsCompatiblePrefix(EquipmentType itemType, EquipmentType slotType, ref bool __result)
        {
            __result = itemType == slotType || (itemType == EquipmentType.VehicleModule && (slotType == EquipmentType.SeamothModule || slotType == EquipmentType.ExosuitModule || slotType == AtramaManager.atramaModuleType));
            if(__result)
            {
                return false;
            }
            return true;
        }

    }
}

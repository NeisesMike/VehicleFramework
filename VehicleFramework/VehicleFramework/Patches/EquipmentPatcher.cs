using System.Collections.Generic;
using HarmonyLib;

// PURPOSE: allow ModVehicle upgrade slots to mesh with the game systems
// VALUE: High

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
         * 
         * At first glance, it appears problematic that I overwrite Equipment.typeToSlots,
         * but typeToSlots is an instance field, and I only overwrite it for ModVehicles.
         */

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Equipment.SetLabel))]
        public static void SetLabelPrefix(Equipment __instance, string l, ref Dictionary<EquipmentType, List<string>> ___typeToSlots)
        {
            if (!l.Contains("VehicleModule") && !l.Contains("VehicleArm"))
            {
                return;
            }
            ModVehicle mv = __instance.owner.GetComponentInParent<ModVehicle>();
            if (mv == null)
            {
                return;
            }
            ___typeToSlots = mv.VehicleTypeToSlots;
            return;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Equipment.AddSlot))]
        public static void AddSlotPrefix(Equipment __instance, string slot, ref Dictionary<EquipmentType, List<string>> ___typeToSlots)
        {
            if (!slot.Contains("VehicleModule") && !slot.Contains("VehicleArm"))
            {
                return;
            }
            ModVehicle mv = __instance.owner.GetComponentInParent<ModVehicle>();
            if (mv == null)
            {
                return;
            }
            ___typeToSlots = mv.VehicleTypeToSlots;
            return;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Equipment.GetCompatibleSlotDefault))]
        public static void GetCompatibleSlotDefaultPrefix(Equipment __instance, EquipmentType itemType, ref Dictionary<EquipmentType, List<string>> ___typeToSlots)
        {
            if (itemType != VehicleBuilder.ModuleType && itemType != VehicleBuilder.ArmType)
            {
                return;
            }
            ModVehicle mv = __instance.owner.GetComponentInParent<ModVehicle>();
            if(mv == null)
            {
                return;
            }
            ___typeToSlots = mv.VehicleTypeToSlots;
            return;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Equipment.GetFreeSlot))]
        public static void GetFreeSlotPrefix(Equipment __instance, EquipmentType type, ref Dictionary<EquipmentType, List<string>> ___typeToSlots)
        {
            if (type != VehicleBuilder.ModuleType && type != VehicleBuilder.ArmType)
            {
                return;
            }
            ModVehicle mv = __instance.owner.GetComponentInParent<ModVehicle>();
            if (mv == null)
            {
                return;
            }
            ___typeToSlots = mv.VehicleTypeToSlots;
            return;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Equipment.GetSlots))]
        public static void GetSlotsPrefix(Equipment __instance, EquipmentType itemType, ref Dictionary<EquipmentType, List<string>> ___typeToSlots)
        {
            if (itemType != VehicleBuilder.ModuleType && itemType != VehicleBuilder.ArmType)
            {
                return;
            }
            ModVehicle mv = __instance.owner.GetComponentInParent<ModVehicle>();
            if (mv == null)
            {
                return;
            }
            ___typeToSlots = mv.VehicleTypeToSlots;
            return;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Equipment.RemoveSlot))]
        public static void RemoveSlot(Equipment __instance, string slot, ref Dictionary<EquipmentType, List<string>> ___typeToSlots)
        {
            if (!slot.Contains("VehicleModule") && !slot.Contains("VehicleArm"))
            {
                return;
            }
            ModVehicle mv = __instance.owner.GetComponentInParent<ModVehicle>();
            if (mv == null)
            {
                return;
            }
            ___typeToSlots = mv.VehicleTypeToSlots;
            return;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Equipment.GetSlotType))]
        public static bool GetSlotTypePrefix(string slot, ref EquipmentType __result, Dictionary<EquipmentType, List<string>> ___typeToSlots)
        {
            if(slot.Contains(ModuleBuilder.ModVehicleModulePrefix))
            {
                __result = VehicleBuilder.ModuleType;
                return false;
            }
            else if(slot == ModuleBuilder.RightArmSlotName || slot == ModuleBuilder.LeftArmSlotName)
            {
                __result = VehicleBuilder.ArmType;
                return false;
            }
            else
            {
                return true;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Equipment.IsCompatible))]
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

using System.Collections.Generic;
using System.Collections;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using VehicleFramework.VehicleBuilding;

// PURPOSE: PDA displays ModVehicle upgrades correctly
// VALUE: Very high.

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(uGUI_Equipment))]
    public class UGUI_EquipmentPatcher
    {
        /*
         * This collection of patches ensures our upgrade slots mesh well
         * with the base-game uGUI_Equipment system.
         * That is, we ensure here that our PDA displays ModVehicle upgrades correctly
         */

        private static void GetAllSlotsWithVFSlots(ref Dictionary<string, uGUI_EquipmentSlot> existingSlots)
        {
            ModuleBuilder.BuildAllSlots();
            foreach (var kvp in ModuleBuilder.vehicleAllSlots)
            {
                if(existingSlots.ContainsKey(kvp.Key))
                {
                    continue;
                }
                existingSlots.Add(kvp.Key, kvp.Value);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(uGUI_Equipment.Awake))]
        public static void AwakePostfix(ref Dictionary<string, uGUI_EquipmentSlot> ___allSlots)
        {
            GetAllSlotsWithVFSlots(ref ___allSlots);
        }
        [HarmonyPrefix]
        [HarmonyPatch(nameof(uGUI_Equipment.OnDragHoverEnter))]
        public static void OnDragHoverEnterPatch(ref Dictionary<string, uGUI_EquipmentSlot> ___allSlots)
        {
            GetAllSlotsWithVFSlots(ref ___allSlots);
        }
        [HarmonyPrefix]
        [HarmonyPatch(nameof(uGUI_Equipment.OnDragHoverStay))]
        public static void OnDragHoverStayPatch(ref Dictionary<string, uGUI_EquipmentSlot> ___allSlots)
        {
            GetAllSlotsWithVFSlots(ref ___allSlots);
        }
        [HarmonyPrefix]
        [HarmonyPatch(nameof(uGUI_Equipment.OnDragHoverExit))]
        public static void OnDragHoverExitPatch(ref Dictionary<string, uGUI_EquipmentSlot> ___allSlots)
        {
            GetAllSlotsWithVFSlots(ref ___allSlots);
        }
        [HarmonyPrefix]
        [HarmonyPatch(nameof(uGUI_Equipment.Init))]
        public static void OnDragHoverExitPatch(Equipment equipment, ref Dictionary<string, uGUI_EquipmentSlot> ___allSlots)
        {
            GetAllSlotsWithVFSlots(ref ___allSlots);
            equipment.owner.GetComponent<ModVehicle>()?.UnlockDefaultModuleSlots();
        }
    }
}

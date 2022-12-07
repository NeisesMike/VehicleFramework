using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace VehicleFramework
{
    [HarmonyPatch(typeof(uGUI_Equipment))]
    public class uGUI_EquipmentPatcher
    {
        /*
         * This collection of patches ensures our upgrade slots mesh well
         * with the base-game uGUI_Equipment system.
         * That is, we ensure here that our PDA displays ModVehicle upgrades correctly
         */
        [HarmonyPostfix]
        [HarmonyPatch("Awake")]
        public static void AwakePostfix(ref Dictionary<string, uGUI_EquipmentSlot> ___allSlots)
        {
            if (!ModuleBuilder.main.haveWeCalledBuildAllSlots)
            {
                ModuleBuilder.main.haveWeCalledBuildAllSlots = true;
                ModuleBuilder.main.isEquipmentInit = true;
                ModuleBuilder.main.vehicleAllSlots = ___allSlots;
                ModuleBuilder.main.BuildAllSlots();
                ___allSlots = ModuleBuilder.main.vehicleAllSlots;
            }
        }
        [HarmonyPrefix]
        [HarmonyPatch("OnDragHoverEnter")]
        public static void OnDragHoverEnterPatch(ref Dictionary<string, uGUI_EquipmentSlot> ___allSlots)
        {
            ___allSlots = ModuleBuilder.main.vehicleAllSlots;
        }
        [HarmonyPrefix]
        [HarmonyPatch("OnDragHoverStay")]
        public static void OnDragHoverStayPatch(ref Dictionary<string, uGUI_EquipmentSlot> ___allSlots)
        {
            ___allSlots = ModuleBuilder.main.vehicleAllSlots;
        }
        [HarmonyPrefix]
        [HarmonyPatch("OnDragHoverExit")]
        public static void OnDragHoverExitPatch(ref Dictionary<string, uGUI_EquipmentSlot> ___allSlots)
        {
            ___allSlots = ModuleBuilder.main.vehicleAllSlots;
        }
    }
}

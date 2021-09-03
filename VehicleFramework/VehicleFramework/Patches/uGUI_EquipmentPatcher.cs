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
        public static bool isInit = false;

        [HarmonyPostfix]
        [HarmonyPatch("Awake")]
        public static void AwakePostfix(uGUI_Equipment __instance, ref Dictionary<string, uGUI_EquipmentSlot> ___allSlots)
        {
            ModuleBuilder.vehicleAllSlots = ___allSlots;
            ModuleBuilder.main.BuildAllSlots();
            isInit = true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnDragHoverEnter")]
        public static void OnDragHoverEnterPatch(ref Dictionary<string, uGUI_EquipmentSlot> ___allSlots)
        {
            ___allSlots = ModuleBuilder.vehicleAllSlots;
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnDragHoverStay")]
        public static void OnDragHoverStayPatch(ref Dictionary<string, uGUI_EquipmentSlot> ___allSlots)
        {
            ___allSlots = ModuleBuilder.vehicleAllSlots;
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnDragHoverExit")]
        public static void OnDragHoverExitPatch(ref Dictionary<string, uGUI_EquipmentSlot> ___allSlots)
        {
            ___allSlots = ModuleBuilder.vehicleAllSlots;
        }
    }
}

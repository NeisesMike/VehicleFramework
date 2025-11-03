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

        private static Dictionary<string, uGUI_EquipmentSlot> GetAllSlotsWithVFSlots(Dictionary<string, uGUI_EquipmentSlot> existingSlots)
        {
            return existingSlots
                            .Concat(ModuleBuilder.vehicleAllSlots.Where(kvp => !existingSlots.ContainsKey(kvp.Key)))
                            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(uGUI_Equipment.Awake))]
        public static void AwakePostfix(ref Dictionary<string, uGUI_EquipmentSlot> ___allSlots)
        {
            if (!ModuleBuilder.haveWeCalledBuildAllSlots)
            {
                ModuleBuilder.haveWeCalledBuildAllSlots = true;
                ModuleBuilder.main = Player.main.gameObject.AddComponent<ModuleBuilder>();
                ModuleBuilder.main.GrabComponents();
                ModuleBuilder.main.isEquipmentInit = true;
                ModuleBuilder.vehicleAllSlots = ___allSlots;
                ModuleBuilder.main.BuildAllSlots();
                ___allSlots = GetAllSlotsWithVFSlots(___allSlots);
            }
        }
        [HarmonyPrefix]
        [HarmonyPatch(nameof(uGUI_Equipment.OnDragHoverEnter))]
        public static void OnDragHoverEnterPatch(ref Dictionary<string, uGUI_EquipmentSlot> ___allSlots)
        {
            ___allSlots = GetAllSlotsWithVFSlots(___allSlots);
        }
        [HarmonyPrefix]
        [HarmonyPatch(nameof(uGUI_Equipment.OnDragHoverStay))]
        public static void OnDragHoverStayPatch(ref Dictionary<string, uGUI_EquipmentSlot> ___allSlots)
        {
            ___allSlots = GetAllSlotsWithVFSlots(___allSlots);
        }
        [HarmonyPrefix]
        [HarmonyPatch(nameof(uGUI_Equipment.OnDragHoverExit))]
        public static void OnDragHoverExitPatch(ref Dictionary<string, uGUI_EquipmentSlot> ___allSlots)
        {
            ___allSlots = GetAllSlotsWithVFSlots(___allSlots);
        }
        [HarmonyPrefix]
        [HarmonyPatch(nameof(uGUI_Equipment.Init))]
        public static void OnDragHoverExitPatch(uGUI_Equipment __instance, Equipment equipment)
        {
            equipment.owner.GetComponent<ModVehicle>()?.UnlockDefaultModuleSlots();
            // The following was an attempt to fix the "slots don't appear unless the PDA has be opened once already" issue. Unsuccessful.
            /*
            ModVehicle? mv = equipment.owner.GetComponent<ModVehicle>();
            if (mv == null)
            {
                return;
            }
            if (ModuleBuilder.slotExtenderHasGreenLight)
            {
                mv.UnlockDefaultModuleSlots();
            }
            else
            {
                IEnumerator UnlockDefaultSlotsASAP()
                {
                    yield return new WaitUntil(() => ModuleBuilder.slotExtenderHasGreenLight);
                    __instance.Init(equipment);
                }
                Admin.SessionManager.StartCoroutine(UnlockDefaultSlotsASAP());
            }
            */
        }
    }
}

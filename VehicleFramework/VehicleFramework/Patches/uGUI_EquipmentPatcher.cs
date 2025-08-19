using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
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
                ___allSlots = ModuleBuilder.vehicleAllSlots;
            }
        }
        [HarmonyPrefix]
        [HarmonyPatch(nameof(uGUI_Equipment.OnDragHoverEnter))]
        public static void OnDragHoverEnterPatch(ref Dictionary<string, uGUI_EquipmentSlot> ___allSlots)
        {
            ___allSlots.Where(x => !ModuleBuilder.vehicleAllSlots.Contains(x)).ForEach(x => ModuleBuilder.vehicleAllSlots.Add(x.Key, x.Value));
            ___allSlots = ModuleBuilder.vehicleAllSlots;
        }
        [HarmonyPrefix]
        [HarmonyPatch(nameof(uGUI_Equipment.OnDragHoverStay))]
        public static void OnDragHoverStayPatch(ref Dictionary<string, uGUI_EquipmentSlot> ___allSlots)
        {
            ___allSlots.Where(x => !ModuleBuilder.vehicleAllSlots.Contains(x)).ForEach(x => ModuleBuilder.vehicleAllSlots.Add(x.Key, x.Value));
            ___allSlots = ModuleBuilder.vehicleAllSlots;
        }
        [HarmonyPrefix]
        [HarmonyPatch(nameof(uGUI_Equipment.OnDragHoverExit))]
        public static void OnDragHoverExitPatch(ref Dictionary<string, uGUI_EquipmentSlot> ___allSlots)
        {
            ___allSlots.Where(x => !ModuleBuilder.vehicleAllSlots.Contains(x)).ForEach(x => ModuleBuilder.vehicleAllSlots.Add(x.Key, x.Value));
            ___allSlots = ModuleBuilder.vehicleAllSlots;
        }
    }
}

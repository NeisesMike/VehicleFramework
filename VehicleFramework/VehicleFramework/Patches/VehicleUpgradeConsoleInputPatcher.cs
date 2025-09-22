using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using System.Reflection.Emit;
using System.Linq;
using VehicleFramework.MiscComponents;
using VehicleFramework.Admin;

// PURPOSE: Prevent Drones from accessing upgrades. Display upgrade module models when appropriate. Display custom upgrade-background images.
// VALUE: High. Drones would have odd behavior otherwise, and the other functions are important developer utilities.

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(VehicleUpgradeConsoleInput))]
    class VehicleUpgradeConsoleInputPatcher
    {
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(VehicleUpgradeConsoleInput.OnHandClick))]
        public static IEnumerable<CodeInstruction> VehicleUpgradeConsoleInputOnHandClickHarmonyTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            return Core.DroneTranspilerHelper.SkipForDrones(instructions, generator);
        }
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(VehicleUpgradeConsoleInput.OnHandHover))]
        public static IEnumerable<CodeInstruction> VehicleUpgradeConsoleInputOnHandHoverHarmonyTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            return Core.DroneTranspilerHelper.SkipForDrones(instructions, generator);
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(VehicleUpgradeConsoleInput.UpdateVisuals))]
        public static void UpdateVisualsPostfix(VehicleUpgradeConsoleInput __instance)
        {
            if (__instance.GetComponentInParent<ModVehicle>() != null && __instance.GetComponentInChildren<UpgradeProxy>() != null && __instance.GetComponentInChildren<UpgradeProxy>().slots != null)
            {
                __instance.slots = __instance.GetComponentInChildren<UpgradeProxy>().slots.ToArray();
                for (int i = 0; i < __instance.slots.Length; i++)
                {
                    VehicleUpgradeConsoleInput.Slot slot = __instance.slots[i];
                    GameObject model = slot.model;
                    if (model != null)
                    {
                        bool active = __instance.equipment != null && __instance.equipment.GetTechTypeInSlot(slot.id) > TechType.None;
                        model.SetActive(active);
                    }
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(VehicleUpgradeConsoleInput.OnHandClick))]
        public static void VehicleUpgradeConsoleInputOnHandClickHarmonyPostfix(VehicleUpgradeConsoleInput __instance)
        {
            foreach (var mv in VehicleManager.VehiclesInPlay.Where(x => x != null))
            {
                if (mv.upgradesInput == __instance)
                {
                    if(VehicleBuilding.ModuleBuilder.main == null)
                    {
                        throw Admin.SessionManager.Fatal("ModuleBuilder.main is null! Cannot set background image for VehicleUpgradeConsoleInput.");
                    }
                    VehicleBuilding.ModuleBuilder.main.BackgroundSprite = mv.ModuleBackgroundImage;
                    break;
                }
            }
        }
    }
}

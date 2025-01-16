using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using System.Reflection.Emit;

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(VehicleUpgradeConsoleInput))]
    class VehicleUpgradeConsoleInputPatcher
    {
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(VehicleUpgradeConsoleInput.OnHandClick))]
        public static IEnumerable<CodeInstruction> VehicleUpgradeConsoleInputOnHandClickHarmonyTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            return DroneTranspilerHelper.SkipForDrones(instructions, generator);
        }
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(VehicleUpgradeConsoleInput.OnHandHover))]
        public static IEnumerable<CodeInstruction> VehicleUpgradeConsoleInputOnHandHoverHarmonyTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            return DroneTranspilerHelper.SkipForDrones(instructions, generator);
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(VehicleUpgradeConsoleInput.UpdateVisuals))]
        public static void UpdateVisualsPostfix(VehicleUpgradeConsoleInput __instance)
        {
            if (__instance.GetComponentInParent<ModVehicle>() != null)
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




    }
}

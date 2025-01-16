using System.Collections.Generic;
using HarmonyLib;
using System.Reflection.Emit;

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(BulkheadDoor))]
    public class BulkheadDoorPatcher
    {
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(BulkheadDoor.OnHandHover))]
        public static IEnumerable<CodeInstruction> BulkheadDoorOnHandHoverTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            return DroneTranspilerHelper.SkipForDrones(instructions, generator);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(BulkheadDoor.OnHandClick))]
        public static IEnumerable<CodeInstruction> BulkheadDoorOnHandClickTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            return DroneTranspilerHelper.SkipForDrones(instructions, generator);
        }
    }
}

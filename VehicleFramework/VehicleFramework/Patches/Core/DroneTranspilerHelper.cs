using System;
using System.Collections.Generic;
using HarmonyLib;
using System.Reflection.Emit;

namespace VehicleFramework.Patches.Core
{
    public static class DroneTranspilerHelper
    {
        public static bool IsPlayerUsingDrone()
        {
            return VehicleTypes.Drone.MountedDrone != null;
        }

        public static IEnumerable<CodeInstruction> SkipForDrones(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var newInstructions = new CodeMatcher(instructions, generator)
                .Start()
                .CreateLabel(out Label labelAfterRet)
                .InsertAndAdvance(Transpilers.EmitDelegate<Func<bool>>(IsPlayerUsingDrone))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Brfalse_S, labelAfterRet))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ret));

            return newInstructions.InstructionEnumeration();
        }
    }
}

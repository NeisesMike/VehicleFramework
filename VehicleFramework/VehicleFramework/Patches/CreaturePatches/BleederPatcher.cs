using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

// PURPOSE: ensure bleeders deal damage in an intuitive way
// VALUE: high

namespace VehicleFramework.Patches.CreaturePatches
{
    [HarmonyPatch(typeof(AttachAndSuck))]
    class BleederPatcher
    {
        /* This patch is intended to ensure bleeder's take the player's life and not the vehicle's life.
         * It works by adding a check for whether the player is in a modvehicle before it attaches
         * Without this, the bleeder will collide with the modvehicle, find that it has a player, but that the player isn't piloting or in a cyclops (so he's vulnerable)
         * Then it will attach to the player but deal damage to the modvehicle
         */
        public static bool IsPlayerInsideModVehicle()
        {
            return (Player.main.GetVehicle() is ModVehicle);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(AttachAndSuck.OnCollisionEnter))]
        public static IEnumerable<CodeInstruction> AttachAndSuckOnCollisionEnterTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var isInSub = AccessTools.Method(typeof(Player), "IsInSub");
            CodeMatch IsInSubMatch = new(i => i.opcode == OpCodes.Callvirt && i.operand as MethodInfo == isInSub);

            var matcher = new CodeMatcher(instructions, generator)
                .MatchForward(true, IsInSubMatch)
                .Advance(1); // Move to the brtrue

            var originalBrTrue = matcher.Instruction;
            var clonedBrTrue = originalBrTrue.Clone();

            matcher.Advance(1)
                .InsertAndAdvance(Transpilers.EmitDelegate<Func<bool>>(IsPlayerInsideModVehicle))
                .Insert(clonedBrTrue);

            return matcher.InstructionEnumeration();
        }
    }
}

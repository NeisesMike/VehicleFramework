using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
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
        [HarmonyPatch(nameof(AttachAndSuck.OnCollisionEnter))]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // Add an extra check to see if the player is in a mod vehicle.
            // Add it to the existing checks by way of LOGICAL OR.
            List<CodeInstruction> codes = new(instructions);
            List<CodeInstruction> newCodes = new(codes.Count + 4);
            CodeInstruction myNOP = new(OpCodes.Nop);
            for (int i = 0; i < codes.Count; i++)
            {
                newCodes.Add(myNOP);
            }
            int j = 0; // newCodes index
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Callvirt)
                {
                    if (codes[i].operand.ToString().Contains("IsInSub"))
                    {
                        newCodes[j] = codes[i];
                        newCodes[j + 1] = codes[i + 1];
                        newCodes[j + 2] = CodeInstruction.Call(typeof(BleederPatcher), nameof(IsPlayerInsideModVehicle));
                        newCodes[j + 3] = new(codes[i + 1]);
                        j += 3;
                        i += 1;
                        continue;
                    }
                }
                newCodes[j] = codes[i];
            }
            return newCodes.AsEnumerable();
        }
    }
}

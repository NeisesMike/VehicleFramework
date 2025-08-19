using System;
using System.Collections.Generic;
using HarmonyLib;
using System.Reflection.Emit;

// PURPOSE: Allow ModVehicles to use (and have displayed) custom ping sprites.
// VALUE: Very high.

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(uGUI_Pings))]
    class UGUI_PingsPatcher
    {
        /*
         * This transpiler ensure our ping sprites are used properly by the base-game systems,
         * so that we may display our custom ping sprites on the HUD
         */
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(uGUI_Pings.OnAdd))]
        static IEnumerable<CodeInstruction> UGUI_PingsOnAddTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatch GetPingTypeMatch = new(i => i.opcode == OpCodes.Callvirt && i.operand.ToString().Contains("System.String Get(PingType)"));
            var newInstructions = new CodeMatcher(instructions)
                .MatchForward(false, GetPingTypeMatch)
                .Repeat(x =>
                    x.RemoveInstruction()
                    .InsertAndAdvance(Transpilers.EmitDelegate<Func<CachedEnumString<PingType>, PingType, string>>(VehicleBuilding.VehicleBuilder.GetPingTypeString))
                    .Advance(1)
                    .RemoveInstruction()
                    .Insert(Transpilers.EmitDelegate<Func<SpriteManager.Group, string, UnityEngine.Sprite, UnityEngine.Sprite>>(VehicleBuilding.VehicleBuilder.GetPingTypeSprite))
                );
            return newInstructions.InstructionEnumeration();
        }
    }
}

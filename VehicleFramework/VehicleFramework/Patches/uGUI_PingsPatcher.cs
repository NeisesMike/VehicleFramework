using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using System.Reflection.Emit;

namespace VehicleFramework
{
    [HarmonyPatch(typeof(uGUI_Pings))]
    class uGUI_PingsPatcher
    {
        /*
         * This transpiler ensure our ping sprites are used properly by the base-game systems,
         * so that we may display our custom ping sprites on the HUD
         */
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(uGUI_Pings.OnAdd))]
        static IEnumerable<CodeInstruction> uGUI_PingsOnAddTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatch GetPingTypeMatch = new CodeMatch(i => i.opcode == OpCodes.Callvirt && i.operand.ToString().Contains("System.String Get(PingType)"));
            var newInstructions = new CodeMatcher(instructions)
                .MatchStartForward(GetPingTypeMatch)
                .Repeat(x =>
                    x.RemoveInstruction()
                    .InsertAndAdvance(Transpilers.EmitDelegate<Func<CachedEnumString<PingType>, PingType, string>>(VehicleBuilder.GetPingTypeString))
                    .RemoveInstruction()
                    .Insert(Transpilers.EmitDelegate<Func<SpriteManager.Group, string, Atlas.Sprite>>(VehicleBuilder.GetPingTypeSprite))
                );
            return newInstructions.InstructionEnumeration();
        }
    }
}

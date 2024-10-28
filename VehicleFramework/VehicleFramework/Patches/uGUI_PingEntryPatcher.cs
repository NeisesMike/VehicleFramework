using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using System.Reflection.Emit;

namespace VehicleFramework
{
    [HarmonyPatch(typeof(uGUI_PingEntry))]
    class uGUI_PingEntryPatcher
    {
        /*
         * This transpiler ensure our ping sprites are used properly by the base-game systems,
         * so that we may display our custom ping sprites on the HUD
         */
        [HarmonyPatch(nameof(uGUI_PingEntry.SetIcon))]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            List<CodeInstruction> newCodes = new List<CodeInstruction>(codes.Count);
            CodeInstruction myNOP = new CodeInstruction(OpCodes.Nop);
            for (int i = 0; i < codes.Count; i++)
            {
                newCodes.Add(myNOP);
            }
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Callvirt)
                {
                    if (codes[i].operand.ToString() == "System.String Get(PingType)")
                    {
                        newCodes[i] = CodeInstruction.Call(typeof(VehicleBuilder), nameof(VehicleBuilder.GetPingTypeString));
                        newCodes[i + 1] = CodeInstruction.Call(typeof(VehicleBuilder), nameof(VehicleBuilder.GetPingTypeSprite));
                        i++;
                        continue;
                    }
                }
                newCodes[i] = codes[i];
            }
            return newCodes.AsEnumerable();
        }
    }
}

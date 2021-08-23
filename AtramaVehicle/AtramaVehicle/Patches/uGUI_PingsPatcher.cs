using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using System.Reflection.Emit;

namespace AtramaVehicle
{
    [HarmonyPatch(typeof(uGUI_Pings))]
    class uGUI_PingsPatcher
    {
        [HarmonyPatch("OnAdd")]
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
                        newCodes[i] = CodeInstruction.Call(typeof(AtramaManager), nameof(AtramaManager.getPingTypeString));
                        newCodes[i + 1] = CodeInstruction.Call(typeof(AtramaManager), nameof(AtramaManager.getPingTypeSprite));
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

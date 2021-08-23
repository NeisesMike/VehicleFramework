using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using System.Reflection.Emit;

namespace AtramaVehicle
{
    /*
    [HarmonyPatch(typeof(uGUI_SeamothHUD))]
    public class uGUI_SeamothHUDPatcher
    {
        [HarmonyPatch("Update")]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            List<CodeInstruction> newCodes = new List<CodeInstruction>(codes.Count+1);
            bool havePlacedOurInstr = false;

            CodeInstruction myNOP = new CodeInstruction(OpCodes.Nop);
            for (int i = 0; i < codes.Count + 1; i++)
            {
                newCodes.Add(myNOP);
            }

            for (int i = 0; i < codes.Count; i++)
            {
                if (!havePlacedOurInstr && codes[i].opcode == OpCodes.Stloc_3)
                {
                    newCodes[i] = CodeInstruction.Call(typeof(AtramaManager), nameof(AtramaManager.isPlayerAtramaPilotNotPDA));
                    newCodes[i+1] = codes[i];
                    havePlacedOurInstr = true;
                    continue;
                }
                if(havePlacedOurInstr)
                {
                    newCodes[i + 1] = codes[i];
                }
                else
                {
                    newCodes[i] = codes[i];
                }
            }
            return newCodes.AsEnumerable();
        }


        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        public static void UpdatePostfix(uGUI_SeamothHUD __instance)
        {
            if (AtramaManager.isPlayerAtramaPilot())
            {
                Logger.Log("bing setting HUD values");
                __instance.textHealth.text = IntStringCache.GetStringForInt(0);
                __instance.textPower.text = IntStringCache.GetStringForInt(0);
                __instance.textTemperature.text = IntStringCache.GetStringForInt(0);
            }
            else
            {
                Logger.Log("bing not setting HUD values");
            }
        }
    }
    */
}

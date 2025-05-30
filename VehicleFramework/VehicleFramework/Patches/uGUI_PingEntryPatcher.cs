using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using System.Reflection.Emit;

// PURPOSE: Allow ModVehicles to use (and have displayed) custom ping sprites.
// VALUE: Very high.

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

        // This prefix ensures ModVehicles have their names displayed correctly in the ping tab.
        [HarmonyPrefix]
        [HarmonyPatch(nameof(uGUI_PingEntry.UpdateLabel))]
        public static bool uGUI_PingEntryUpdateLabelPrefix(uGUI_PingEntry __instance, PingType type, string name)
        {
            if(VehicleManager.mvPings.Select(x=>x.pingType).Contains(type))
            {
                foreach (var mvType in VehicleManager.vehicleTypes)
                {
                    if (mvType.pt == type)
                    {
                        __instance.label.text = string.Format("{0} - {1}", mvType.name, name); ;
                        break;
                    }
                }
                return false;
            }
            return true;
        }
    }
}

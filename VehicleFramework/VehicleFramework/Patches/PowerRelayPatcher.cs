using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using System.Reflection.Emit;


namespace VehicleFramework.Patches
{
    /* This set of patches allows battery chargers to work inside Submarines.
     * Submarines have a PowerRelay, and these patches ensure it does mostly nothing.
     * Here, it acts as an interface to EnergyInterface.
     */

    [HarmonyPatch(typeof(PowerRelay))]
    public static class PowerRelayPatcher
    {
        // We do not want the PowerRelay to do all the things it normally does.
        // In Start, make coroutines are invoked in repeating.
        // So we skip it.
        [HarmonyPrefix]
        [HarmonyPatch(nameof(PowerRelay.Start))]
        public static bool StartPrefix(PowerRelay __instance)
        {
            ModVehicle mv = __instance.gameObject.GetComponent<ModVehicle>();
            if (mv != null)
            {
                return false;
            }
            return true;
        }
        [HarmonyPrefix]
        [HarmonyPatch(nameof(PowerRelay.GetPower))]
        public static bool GetPowerPrefix(PowerRelay __instance, ref float __result)
        {
            ModVehicle mv = __instance.gameObject.GetComponent<ModVehicle>();
            if (mv != null)
            {
                __result = mv.energyInterface.TotalCanProvide(out _);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Charger))]
    public static class PowerSystemPatcher
    {
        public static bool MaybeConsumeEnergy(PowerRelay mvpr, float amount, out float amountConsumed)
        {
            ModVehicle mv = mvpr.gameObject.GetComponent<ModVehicle>();
            if (mv == null)
            {
                mvpr.ConsumeEnergy(amount, out amountConsumed);
            }
            else
            {
                amountConsumed = mv.energyInterface.ConsumeEnergy(amount);
            }
            return true;
        }

        /* This transpiler simply replaces one method call with another.
         * It calls the method above, which is generic over the replaced method.
         * It allows special handling in the case of a ModVehicle (which does not have a good PowerRelay)
         */
        [HarmonyPatch(nameof(Charger.Update))]
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
                if (codes[i].opcode == OpCodes.Call && (codes[i].operand.ToString().Contains("ConsumeEnergy")))
                {
                    newCodes[i] = CodeInstruction.Call(typeof(PowerSystemPatcher), nameof(PowerSystemPatcher.MaybeConsumeEnergy));
                }
                else
                {
                    newCodes[i] = codes[i];
                }
            }
            return newCodes.AsEnumerable();
        }
    }
}


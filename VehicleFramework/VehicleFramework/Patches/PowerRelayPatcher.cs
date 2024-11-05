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
     * Submarines have a PowerRelay, but they have SubRoot.powerRelay = null.
     * The PowerRelay acts as an interface to EnergyInterface.
     * These patches ensure the PowerRelay does not Start.
     * 
         * Why doesn't ModVehicle set its powerRelay?
         * We don't set powerRelay because we need CurrentSub.
         * And CurrentSub calls OnPlayerEntered.
         * And OnPlayerEntered plays a voice notification we don't set up,
         * but only when powerRelay is not null.
         * So this avoids an error appearing
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
                __instance.InvokeRepeating("UpdatePowerState", UnityEngine.Random.value, 0.5f);
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
        [HarmonyPrefix]
        [HarmonyPatch(nameof(PowerRelay.GetMaxPower))]
        public static bool GetMaxPowerPrefix(PowerRelay __instance, ref float __result)
        {
            ModVehicle mv = __instance.gameObject.GetComponent<ModVehicle>();
            if (mv != null && mv.energyInterface != null && mv.energyInterface.sources != null)
            {
                float totalCapacity = 0f;
                for (int i = 0; i < mv.energyInterface.sources.Length; i++)
                {
                    EnergyMixin energyMixin = mv.energyInterface.sources[i];
                    if (energyMixin != null)
                    {
                        totalCapacity += energyMixin.capacity;
                    }
                }
                __result = totalCapacity;
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
         * It allows special handling in the case of a ModVehicle (which does not have a good PowerRelay, see top of file)
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


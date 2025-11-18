using System.Linq;
using HarmonyLib;

// PURPOSE: Allow battery charges (and Power Relay in general) to work in expected ways on ModVehicles
// VALUE: High.

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
         * but only when powerRelay != null.
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
                if (mv.energyInterface != null)
                {
                    __result = mv.energyInterface.TotalCanProvide(out _);
                }
                else
                {
                    __result = 0;
                }
                return false;
            }
            return true;
        }
        [HarmonyPrefix]
        [HarmonyPatch(nameof(PowerRelay.GetMaxPower))]
        public static bool GetMaxPowerPrefix(PowerRelay __instance, ref float __result)
        {
            if (__instance == null || __instance.gameObject == null) return true;
            ModVehicle mv = __instance.gameObject.GetComponent<ModVehicle>();
            if (mv == null) return true;
            if (mv.energyInterface == null || mv.energyInterface.sources == null)
            {
                __result = 0;
                return false;
            }
            __result = mv.energyInterface.sources.Where(x => x != null).Select(x => x.capacity).Sum();
            return false;
        }
        [HarmonyPrefix]
        [HarmonyPatch(nameof(PowerRelay.ModifyPower))]
        public static bool PowerRelayModifyPowerPrefix(PowerRelay __instance, float amount, ref float modified, ref bool __result)
        {
            ModVehicle mv = __instance.gameObject.GetComponent<ModVehicle>();
            if (mv == null)
            {
                return true;
            }
            if (mv.energyInterface?.sources == null)
            {
                __result = false;
                return false;
            }
            if (!GameModeUtils.RequiresPower())
            {
                modified = 0f;
                __result = true;
                return false;
            }
            var canProvide = mv.energyInterface.TotalCanProvide(out _);
            var canConsume = mv.energyInterface.TotalCanConsume(out _);
            if (amount < 0 && canProvide < -amount)
            {
                Logger.DebugLog($"Insufficient power: {-amount} >= {canProvide}");
                __result = false;
                return false;
            }
            if (amount > 0 && canConsume < amount)
            {
                Logger.DebugLog($"Insufficient capacity to receive: {amount} >= {canConsume}");
                __result = false;
                return false;
            }
            var rs = modified = amount;
            mv.energyInterface.ModifyCharge(amount);
            __result = true;
            return false;
        }
    }
}


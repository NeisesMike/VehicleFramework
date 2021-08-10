using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace AtramaVehicle
{
    [HarmonyPatch(typeof(VehicleUpgradeConsoleInput))]
    public class VehicleUpgradeConsoleInputPatcher
    {
        [HarmonyPrefix]
        [HarmonyPatch("UpdateVisuals")]
        public static bool UpdateVisualsPrefix(VehicleUpgradeConsoleInput __instance)
        {
            if (__instance.transform.parent == null || __instance.transform.parent.name == null || !__instance.transform.parent.name.Contains("Atrama"))
            {
                return true;
            }
            return false;
        }
    }
}

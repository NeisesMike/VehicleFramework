using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(VehicleUpgradeConsoleInput))]
    class VehicleUpgradeConsoleInputPatcher
    {
        // this is slotextender compat shit
        [HarmonyPrefix]
        [HarmonyPatch("OpenPDA")]
        public static void OpenPDAPrefix(VehicleUpgradeConsoleInput __instance)
        {
            // configure the appropriate AllSlots
            if (__instance.GetComponentInParent<ModVehicle>() != null)
            {
                //ModuleBuilder.main.vehicleAllSlots = null;
            }
            else
            {
                //ModuleBuilder.main.vehicleAllSlots = null;
            }
        }
    }
}

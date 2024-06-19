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
        [HarmonyPrefix]
        [HarmonyPatch(nameof(VehicleUpgradeConsoleInput.OnHandClick))]
        public static bool OnHandClickPrefix(VehicleUpgradeConsoleInput __instance)
        {
            return VehicleTypes.Drone.mountedDrone == null;
        }
        [HarmonyPrefix]
        [HarmonyPatch(nameof(VehicleUpgradeConsoleInput.OnHandHover))]
        public static bool OnHandHoverPrefix(VehicleUpgradeConsoleInput __instance)
        {
            return VehicleTypes.Drone.mountedDrone == null;
        }

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

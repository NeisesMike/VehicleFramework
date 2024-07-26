using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using System.Reflection.Emit;
using UnityEngine;

namespace VehicleFramework
{
    [HarmonyPatch(typeof(BulkheadDoor))]
    public class BulkheadDoorPatcher
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(BulkheadDoor.OnHandClick))]
        public static bool OnHandClickPrefix(BulkheadDoor __instance)
        {
            return VehicleTypes.Drone.mountedDrone == null;
        }
        [HarmonyPrefix]
        [HarmonyPatch(nameof(BulkheadDoor.OnHandHover))]
        public static bool OnHandHoverPrefix(BulkheadDoor __instance)
        {
            return VehicleTypes.Drone.mountedDrone == null;
        }
    }
}

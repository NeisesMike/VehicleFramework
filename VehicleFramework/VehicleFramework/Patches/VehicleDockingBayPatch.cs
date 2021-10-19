using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(VehicleDockingBay))]
    class VehicleDockingBayPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("OnTriggerEnter")]
        public static bool OnTriggerEnterPrefix(VehicleDockingBay __instance, Collider other)
        {
            if(UWE.Utils.GetComponentInHierarchy<ModVehicle>(other.gameObject) != null)
            {
                return false;
            }
            return true;
        }
    }
}

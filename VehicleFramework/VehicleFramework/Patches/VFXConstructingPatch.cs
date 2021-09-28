using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(VFXConstructing))]
    public static class VFXConstructingPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("StartConstruction")]
        public static void StartConstructionPostfix(VFXConstructing __instance)
        {
            if (__instance.GetComponent<ModVehicle>() != null)
            {
                // TODO : make this configurable
                // Seamoth : 10 seconds
                // Cyclops : 20
                // Rocket Base : 25
                __instance.timeToConstruct = 20f;
            }
        }
    }
}

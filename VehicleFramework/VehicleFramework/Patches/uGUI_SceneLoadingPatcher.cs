using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace VehicleFramework
{
    [HarmonyPatch(typeof(uGUI_SceneLoading))]
    public static class uGUI_SceneLoadingPatcher
    {
        /*
         * This system of patches ensures that our ModVehicles load into the game correctly.
         * This call to VehicleManager.LoadVehicles() was a very difficult one to place.
         * The end of Scene-Load is the appropriate time to load the vehicles.
         */
        [HarmonyPrefix]
        [HarmonyPatch("End")]
        public static void End(uGUI_SceneLoading __instance, bool fade, ref bool __state)
        {
            __state = __instance.isLoading;
        }

        [HarmonyPostfix]
        [HarmonyPatch("End")]
        public static void End(uGUI_SceneLoading __instance, bool fade, bool __state)
        {
            if(__state && !__instance.isLoading)
            {
                VehicleManager.LoadVehicles();
            }
        }
    }
}

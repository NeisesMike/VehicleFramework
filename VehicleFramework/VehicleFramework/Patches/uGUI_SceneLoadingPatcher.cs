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

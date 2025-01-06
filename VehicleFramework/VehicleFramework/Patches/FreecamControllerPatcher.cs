using HarmonyLib;
using UnityEngine;

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(FreecamController))]
    public class FreecamControllerPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(FreecamController.FreecamToggle))]
        public static void FreecamControllerFreecamTogglePostfix(FreecamController __instance)
        {
            if(!__instance.mode && __instance.ghostMode)
            {
                __instance.ghostMode = false;
            }
        }
    }
}

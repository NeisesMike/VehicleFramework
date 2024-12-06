using HarmonyLib;
using UnityEngine;

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(FreecamController))]
    public class FreecamControllerPatcher
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(FreecamController.OnConsoleCommand_ghost))]
        public static bool FreecamControllerOnConsoleCommand_ghostPrefix(FreecamController __instance)
        {
            __instance.speed = 8f;
            __instance.toggleNextFrame = true;
            __instance.ghostMode = !__instance.ghostMode;
            return false;
        }
    }
}

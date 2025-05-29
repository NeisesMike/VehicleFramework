using HarmonyLib;
using UnityEngine;

namespace VehicleFramework
{
    [HarmonyPatch(typeof(LiveMixin))]
    public class LiveMixinPatcher
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(LiveMixin.Start))]
        public static void StartPrefix(LiveMixin __instance, GameObject ___loopingDamageEffectObj)
        {
            if (__instance.gameObject?.GetComponent<ModVehicle>() != null)
            {
                __instance.player = Player.main;
            }
        }
    }
}

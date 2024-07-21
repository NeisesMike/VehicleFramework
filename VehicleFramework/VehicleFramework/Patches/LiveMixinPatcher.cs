using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        [HarmonyPostfix]
        [HarmonyPatch(nameof(LiveMixin.TakeDamage))]
        public static void TakeDamagePostfix(LiveMixin __instance, float originalDamage, Vector3 position, DamageType type, GameObject dealer)
        {
            VehicleComponents.VehicleDamageTracker vdt = __instance.gameObject?.GetComponent<ModVehicle>()?.GetComponent<VehicleComponents.VehicleDamageTracker>();
            if (vdt != null)
            {
                vdt.TakeDamagePostfix(originalDamage, position, type, dealer);
            }
        }
    }
}

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
        [HarmonyPatch("Start")]
        public static bool StartPrefix(LiveMixin __instance, GameObject ___loopingDamageEffectObj)
        {
            if(__instance.gameObject == null || __instance.gameObject.name == null || !__instance.gameObject.name.Contains("Vehicle"))
            {
                return true;
            }
            __instance.player = Player.main;
            return true;
		}
    }
}

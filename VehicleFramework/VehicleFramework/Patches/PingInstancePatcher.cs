using HarmonyLib;
using UnityEngine;

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(PingInstance))]
    public class PingInstancePatcher
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(PingInstance.GetPosition))]
        public static void PingInstanceGetPositionPrefix(PingInstance __instance)
        {
            if(__instance.origin == null)
            {
                Logger.Warn("Found null origin for ping instance on object: " + __instance.name + ". Pick up or delete this object!");
            }
        }
    }
}

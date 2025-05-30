using HarmonyLib;

// PURPOSE: Force a fix on PingInstances that have a null origin.
// VALUE: High, unfortunately. If GetPosition has an error, it causes subsequent ping instances to not be displayed, which looks like a Vehicle Framework bug. See uGUI_Pings.UpdatePings for more.

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(PingInstance))]
    public class PingInstancePatcher
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(PingInstance.GetPosition))]
        public static void PingInstanceGetPositionPrefix(PingInstance __instance)
        {
            if (__instance.origin == null)
            {
                __instance.origin = __instance.transform;
                Logger.Warn($"Found null origin for ping instance on object: {__instance.name}. Setting origin to itself. Otherwise, some ping sprites would not be displayed.");
            }
        }
    }
}

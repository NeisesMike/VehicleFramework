using HarmonyLib;
using UnityEngine;
using VehicleFramework.Admin;
using VehicleFramework.VehicleTypes;

// PURPOSE: Allow ModVehicles to use (and have displayed) custom ping sprites.
// VALUE: Very high.

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(uGUI_Pings))]
    class UGUI_PingsPatcher
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(uGUI_Pings.OnAdd))]
        public static bool UGUI_PingsOnAddHarmonyPrefix(uGUI_Pings __instance, PingInstance instance)
        {
            // If the ping is not a ModVehicle, we let the base game handle it.
            ModVehicle? mv = instance.gameObject.GetComponent<ModVehicle>();
            if (mv == null) return true;

            if (instance == null)
            {
                Logger.Warn("uGUI_Pings.OnAdd called with null PingInstance! This should not happen, but we will handle it gracefully.");
                return false;
            }

            // If the ping is a ModVehicle, we handle it ourselves.
            uGUI_Ping uGUI_Ping = __instance.poolPings.Get();
            uGUI_Ping.Initialize();
            uGUI_Ping.SetVisible(instance.visible);
            uGUI_Ping.SetColor(PingManager.colorOptions[instance.colorIndex]);
            string pingName = VFPingManager.VFGetCachedPingTypeString(instance.pingType);
            Sprite pingSprite = VFPingManager.VFGetPingTypeSprite(pingName);
            uGUI_Ping.SetIcon(pingSprite);
            uGUI_Ping.SetLabel(instance.GetLabel());
            uGUI_Ping.SetIconAlpha(0f);
            uGUI_Ping.SetTextAlpha(0f);
            __instance.pings.Add(instance.Id, uGUI_Ping);
            return false;
        }
    }
}

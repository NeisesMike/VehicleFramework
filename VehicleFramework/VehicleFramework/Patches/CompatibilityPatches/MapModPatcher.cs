using HarmonyLib;
using System.Reflection;
using UnityEngine;

// PURPOSE: ensure ModVehicles are displayed correctly in the Map mod
// VALUE: High. The Map is a great mod!

namespace VehicleFramework.Patches.CompatibilityPatches
{
    public static class MapModPatcher
    {
        /*
         * This patch is specifically for the Map Mod.
         * It ensures that our ModVehicles are displayed correctly as their Ping Sprites.
         * Without this patch, the Map Mod dies completely.
         */
        [HarmonyPrefix]
        public static bool Prefix(object __instance)
        {
            FieldInfo? field = __instance.GetType()?.GetField("ping");
            PingInstance? ping = field?.GetValue(__instance) as PingInstance;
            if(ping == null)
            {
                return true; // If we can't get the ping, we don't need to do anything.
            }
            foreach (var mvPIs in VehicleManager.mvPings)
            {
                if (mvPIs.pingType == ping.pingType)
                {
                    FieldInfo? field2 = __instance.GetType()?.GetField("icon");
                    uGUI_Icon? icon = field2?.GetValue(__instance) as uGUI_Icon;
                    if(icon == null)
                    {
                        return true; // If we can't get the icon, we don't need to do anything.
                    }
                    icon.sprite = SpriteManager.Get(TechType.Exosuit);
                    foreach (var mvType in VehicleManager.vehicleTypes)
                    {
                        if (mvType.pt == ping.pingType)
                        {
                            icon.sprite = mvType.ping_sprite;
                            break;
                        }
                    }
                    RectTransform rectTransform = icon.rectTransform;
                    rectTransform.sizeDelta = Vector2.one * 28f;
                    rectTransform.localPosition = Vector3.zero;
                    return false;
                }
            }
            return true;
        }
    }
}

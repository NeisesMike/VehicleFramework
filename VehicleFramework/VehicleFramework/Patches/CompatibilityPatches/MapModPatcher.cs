using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

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
            FieldInfo field = __instance.GetType().GetField("ping");
            PingInstance ping = field.GetValue(__instance) as PingInstance;
            foreach(var mvPIs in VehicleManager.mvPings)
            {
                if (mvPIs.pingType == ping.pingType)
                {
                    FieldInfo field2 = __instance.GetType().GetField("icon");
                    uGUI_Icon icon = field2.GetValue(__instance) as uGUI_Icon;
                    icon.sprite = SpriteManager.Get(TechType.Exosuit);
                    foreach (var mvType in VehicleManager.vehicleTypes)
                    {
                        if (mvType.pt == ping.pingType)
                        {
                            icon.sprite = mvType.mv.PingSprite;
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

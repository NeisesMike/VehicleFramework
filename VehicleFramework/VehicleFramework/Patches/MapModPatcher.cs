using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace VehicleFramework
{
    public static class MapModPatcher
    {
        [HarmonyPrefix]
        public static bool Prefix(object __instance)
        {
            FieldInfo field = __instance.GetType().GetField("ping");
            PingInstance ping = field.GetValue(__instance) as PingInstance;
            Logger.Log("bing: " + ping.name);
            foreach(var mvPIs in VehicleManager.mvPings)
            {
                if (mvPIs.pingType == ping.pingType)
                {
                    Logger.Log("bong");
                    FieldInfo field2 = __instance.GetType().GetField("icon");
                    uGUI_Icon icon = field2.GetValue(__instance) as uGUI_Icon;
                    icon.sprite = SpriteManager.Get(TechType.Exosuit);
                    foreach (var mvType in VehicleBuilder.vehicleTypes)
                    {
                        if (mvType.pt == ping.pingType)
                        {
                            Logger.Log("beep!");
                            icon.sprite = mvType.ping_sprite;
                            //icon.color = Color.black;
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

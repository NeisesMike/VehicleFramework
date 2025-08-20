using HarmonyLib;
using UnityEngine;
using VehicleFramework.Admin;
using VehicleFramework.BaseVehicle;
using VehicleFramework.VehicleBuilding;

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
            string pingName = VFGetCachedPingTypeString(instance.pingType);
            Sprite pingSprite = VFGetPingTypeSprite(pingName);
            uGUI_Ping.SetIcon(pingSprite);
            uGUI_Ping.SetLabel(instance.GetLabel());
            uGUI_Ping.SetIconAlpha(0f);
            uGUI_Ping.SetTextAlpha(0f);
            __instance.pings.Add(instance.Id, uGUI_Ping);
            return false;
        }

        private static string VFGetCachedPingTypeString(PingType inputType)
        {
            foreach (VehicleEntry ve in VehicleManager.vehicleTypes)
            {
                if (ve.pt == inputType)
                {
                    return ve.name;
                }
            }
            foreach (var pair in Assets.SpriteHelper.PingSprites)
            {
                if (pair.Item2 == inputType)
                {
                    return pair.Item1;
                }
            }
            throw SessionManager.Fatal($"Could not find ModVehicle PingType {inputType} in VehicleManager.vehicleTypes or Assets.SpriteHelper.PingSprites!");
        }
        private static Sprite VFGetPingTypeSprite(string name)
        {
            foreach (VehicleEntry ve in VehicleManager.vehicleTypes)
            {
                if (ve.name == name)
                {
                    return ve.ping_sprite;
                }
            }
            foreach (var pair in Assets.SpriteHelper.PingSprites)
            {
                if (pair.Item1 == name)
                {
                    return pair.Item3;
                }
            }
            return Assets.StaticAssets.DefaultPingSprite ?? throw SessionManager.Fatal($"Could not find ModVehicle PingType {name} in VehicleManager.vehicleTypes or Assets.SpriteHelper.PingSprites, and DefaultPingSprite is null!");
        }
    }
}

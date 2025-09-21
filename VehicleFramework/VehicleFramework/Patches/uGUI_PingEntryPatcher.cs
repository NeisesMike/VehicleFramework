using System.Linq;
using HarmonyLib;
using VehicleFramework.Admin;

// PURPOSE: Allow ModVehicles to use (and have displayed) custom ping sprites.
// VALUE: Very high.

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(uGUI_PingEntry))]
    class UGUI_PingEntryPatcher
    {
        /*
         * Search through our own collection of pings, and if the ping type is one of ours, override the sprite with our own.
         */
        [HarmonyPrefix]
        [HarmonyPatch(nameof(uGUI_PingEntry.SetIcon))]
        public static bool UGUI_PingEntrySetIconPatcher(uGUI_PingEntry __instance, PingType type)
        {
            if(Admin.VFPingManager.mvPings.Select(x => x.pingType).Contains(type))
            {
                UnityEngine.Sprite returnSprite = VFPingManager.VFGetPingTypeSprite(VFPingManager.VFGetCachedPingTypeString(type));
                __instance.icon.SetForegroundSprite(returnSprite);
                return false;
            }
            return true;
        }

        // This prefix ensures ModVehicles have their names displayed correctly in the ping tab.
        [HarmonyPrefix]
        [HarmonyPatch(nameof(uGUI_PingEntry.UpdateLabel))]
        public static bool UGUI_PingEntryUpdateLabelPrefix(uGUI_PingEntry __instance, PingType type, string name)
        {
            if(Admin.VFPingManager.mvPings.Select(x=>x.pingType).Contains(type))
            {
                foreach (var mvType in Admin.VehicleManager.vehicleTypes)
                {
                    if (mvType.pt == type)
                    {
                        __instance.label.text = string.Format("{0} - {1}", mvType.name, name); ;
                        break;
                    }
                }
                return false;
            }
            return true;
        }
    }
}

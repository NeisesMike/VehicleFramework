using HarmonyLib;
using UnityEngine;
using VehicleFramework.Assets;

// PURPOSE: Prevent switching hotbar items while looking at a drone station. Allows for saner usage of the drone station controls.
// VALUE: Moderate. Could find another control scheme that doesn't use "select next, select previous."

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(uGUI_QuickSlots))]
    public class UGUI_QuickSlotsPatcher
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(uGUI_QuickSlots.HandleInput))]
        public static bool HandleInputPrefix()
        {
            if (!Builder.isPlacing) // SnapBuilder compat
            {
                Targeting.GetTarget(Player.main.gameObject, 6f, out GameObject target, out float _);
                if (target?.GetComponentInParent<DroneStation>() != null)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
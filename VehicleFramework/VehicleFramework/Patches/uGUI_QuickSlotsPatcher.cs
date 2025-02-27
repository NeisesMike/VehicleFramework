using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace VehicleFramework
{
    [HarmonyPatch(typeof(uGUI_QuickSlots))]
    public class uGUI_QuickSlotsPatcher
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
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace VehicleFramework
{
    [HarmonyPatch(typeof(uGUI_EquipmentSlot))]
    public class uGUI_EquipmentSlotPatcher
    {
        /*
         * This patch ensures that SetState will not be called before its dependency (the background image) is available,
         * just so that we can dodge some errors in the log.
         */
        [HarmonyPrefix]
        [HarmonyPatch(nameof(uGUI_EquipmentSlot.SetState))]
        public static bool SetStatePrefix(uGUI_EquipmentSlot.State newState, uGUI_EquipmentSlot __instance)
        {
            if (!__instance.transform.name.Contains("Vehicle"))
            {
                return true;
            }
            if (ModuleBuilder.main.areModulesReady)
            {
                if(__instance.background == null)
                {
                    Logger.Warn("Warning: modules were ready, but background was null: " + __instance.name);
                    return false;
                }
                return true;
            }
            Logger.Warn("Warning: " + __instance.name + ".SetState() : Vehicle module not ready. Passing.");
			return false;
		}
    }
}

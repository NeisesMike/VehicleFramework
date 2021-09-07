using System;
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
        [HarmonyPrefix]
        [HarmonyPatch("SetState")]
		public static bool SetStatePrefix(uGUI_EquipmentSlot.State newState, uGUI_EquipmentSlot __instance)
        {
            if (!__instance.transform.name.Contains("Vehicle"))
            {
                return true;
            }
            if (ModuleBuilder.main.areModulesReady)
            {
                //Logger.Log(__instance.name + " : SetState() : Vehicle module");
                if(__instance.background == null)
                {
                    Logger.Log("modules were ready, but background was null: " + __instance.name);
                    return false;
                }
                return true;
            }
            Logger.Log(__instance.name + " : SetState() : Vehicle module not ready. Passing.");
			return false;
		}
	}
}

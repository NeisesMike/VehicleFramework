using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace AtramaVehicle
{
    [HarmonyPatch(typeof(uGUI_EquipmentSlot))]
    public class uGUI_EquipmentSlotPatcher
    {
        public static bool hasInited = false;

        [HarmonyPrefix]
        [HarmonyPatch("SetState")]
		public static bool SetStatePrefix(uGUI_EquipmentSlot.State newState, uGUI_EquipmentSlot __instance)
        {
            Logger.Log(__instance.name);
            if (!__instance.transform.name.Contains("Atrama") || hasInited)
            {
                Logger.Log("good input");
				return true;
            }
            Logger.Log("bad input");
			return false;
		}
	}
}

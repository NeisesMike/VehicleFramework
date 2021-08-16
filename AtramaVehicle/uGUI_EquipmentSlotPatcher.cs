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
			if(!__instance.gameObject.name.Contains("Atrama") || (uGUI_EquipmentPatcher.hasInited && hasInited))
            {
				return true;
            }
			return false;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using VehicleFramework.VehicleTypes;

namespace VehicleFramework.Patches
{
	[HarmonyPatch(typeof(IngameMenu))]
	public static class IngameMenuPatcher
	{
		[HarmonyPrefix]
		[HarmonyPatch(nameof(IngameMenu.GetAllowSaving))]
		public static bool GetAllowSavingPrefix(ref bool __result)
		{
            if(Drone.mountedDrone != null)
			{
				__result = false;
				return false;
			}
			return true;
		}
	}
}

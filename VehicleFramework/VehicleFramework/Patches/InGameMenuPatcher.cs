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
		[HarmonyPatch("GetAllowSaving")]
		public static bool GetAllowSavingPrefix(ref bool __result)
		{
			bool isPilotingMV = false;
			foreach(ModVehicle mv in VehicleManager.VehiclesInPlay)
            {
				if (mv as Submarine != null)
				{
					if ((mv as Submarine).IsPlayerPiloting())
					{
						isPilotingMV = true;
						break;
					}
					continue;
				}
				if (mv.IsPlayerDry)
				{
					isPilotingMV = true;
					break;
				}
			}
			if (isPilotingMV)
            {
				__result = false;
				return false;
            }
			return true;
		}
	}
}

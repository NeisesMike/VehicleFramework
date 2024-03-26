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
			ModVehicle mv = Player.main.GetVehicle() as ModVehicle;
			if(mv == null)
            {
				return true;
            }
			else if(mv as Submarine != null)
            {
				if(mv.IsPlayerDry && (mv as Submarine).isPilotSeated)
                {
					__result = false;
					return false;
                }
                else
                {
					return true;
                }
            }
            else
			{
				if (mv.IsPlayerDry)
				{
					__result = false;
					return false;
				}
				else
				{
					return true;
				}
			}
		}
	}
}

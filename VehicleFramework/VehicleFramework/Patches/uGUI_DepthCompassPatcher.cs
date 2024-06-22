using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace VehicleFramework.Patches
{
	[HarmonyPatch(typeof(uGUI_DepthCompass))]

	class uGUI_DepthCompassPatcher
	{
		[HarmonyPrefix]
		[HarmonyPatch(nameof(uGUI_DepthCompass.IsCompassEnabled))]
		public static bool IsCompassEnabledPrefix(uGUI_DepthCompass __instance, ref bool __result)
		{
			if (VehicleTypes.Drone.mountedDrone != null)
			{
				__result = true;
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch(nameof(uGUI_DepthCompass.GetDepthInfo))]
		public static bool GetDepthInfoPrefix(uGUI_DepthCompass __instance, ref int depth, ref int crushDepth, ref uGUI_DepthCompass.DepthMode __result)
		{
			if (VehicleTypes.Drone.mountedDrone != null)
			{
				__result = uGUI_DepthCompass.DepthMode.MapRoomCamera;
				VehicleTypes.Drone.mountedDrone.GetDepth(out depth, out crushDepth);
				return false;
			}
			return true;
		}
	}
}

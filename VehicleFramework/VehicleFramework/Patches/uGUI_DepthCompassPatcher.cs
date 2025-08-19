using HarmonyLib;

// PURPOSE: Ensure drones always have a functional compass on the GUI
// VALUE: Moderate. Could pass this off to individual drones.

namespace VehicleFramework.Patches
{
	[HarmonyPatch(typeof(uGUI_DepthCompass))]
	class UGUI_DepthCompassPatcher
	{
		[HarmonyPostfix]
		[HarmonyPatch(nameof(uGUI_DepthCompass.IsCompassEnabled))]
		public static void IsCompassEnabledPostfix(ref bool __result)
		{
			if (VehicleTypes.Drone.MountedDrone != null)
			{
				__result = true;
			}
		}
		[HarmonyPostfix]
		[HarmonyPatch(nameof(uGUI_DepthCompass.GetDepthInfo))]
		public static void GetDepthInfoPostfix(ref int depth, ref int crushDepth, ref uGUI_DepthCompass.DepthMode __result)
		{
			if (VehicleTypes.Drone.MountedDrone != null)
			{
				__result = uGUI_DepthCompass.DepthMode.Submersible;
				VehicleTypes.Drone.MountedDrone.GetDepth(out depth, out crushDepth);
			}
		}
	}
}

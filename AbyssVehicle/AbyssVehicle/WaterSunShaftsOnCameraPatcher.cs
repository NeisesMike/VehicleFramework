using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace AbyssVehicle
{
	[HarmonyPatch(typeof(WaterSunShaftsOnCamera))]
	public static class WaterSunShaftsOnCameraPatcher
	{
		[HarmonyPrefix]
		[HarmonyPatch("Awake")]
		public static bool AwakePrefix(WaterSunShaftsOnCamera __instance)
		{
			if(__instance.shader is null)
            {
				__instance.shader = MainCamera.camera.gameObject.GetComponent<WaterSunShaftsOnCamera>().shader;
			}
			return true;
		}
	}
}

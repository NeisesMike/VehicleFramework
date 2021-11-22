using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UWE;
using UnityEngine;

namespace VehicleFramework.Patches
{
	[HarmonyPatch(typeof(FreezeTime))]
	public static class FreezeTimePatcher
	{
		[HarmonyPostfix]
		[HarmonyPatch("PauseSound")]
		public static void PauseSoundPostfix(bool pause)
		{
			foreach (ModVehicle mv in VehicleManager.VehiclesInPlay)
			{
				mv.voice.PauseSpeakers(pause);
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace CricketVehicle
{
	[HarmonyPatch(typeof(ConstructorInput))]
	public static class ConstructorInputPatcher
	{
		[HarmonyPrefix]
		[HarmonyPatch("OnHandClick")]
		public static bool OnHandClickPrefix(ConstructorInput __instance, GUIHand hand)
		{
			if (__instance.constructor.building && __instance.constructor.buildTarget?.GetComponent<CricketContainer>() != null)
			{
				return false;
			}
			return true;
		}

		[HarmonyPostfix]
		[HarmonyPatch(nameof(ConstructorInput.GetCraftTransform))]
		public static void GetCraftTransformPostfix(ConstructorInput __instance, TechType techType, ref Vector3 position, ref Quaternion rotation)
		{
			if (techType == MainPatcher.cricketContainerTT)
			{
				position = __instance.constructor.GetItemSpawnPoint(TechType.Seamoth).position;
				rotation = __instance.constructor.GetItemSpawnPoint(TechType.Seamoth).rotation;
				return;
			}
			var ListofCrickets = VehicleFramework.VehicleManager.vehicleTypes.Where(x => x.mv as Cricket != null);
			if (ListofCrickets.Count() < 1)
			{
				return;
			}
			var CricketPrefab = ListofCrickets.First();
			if (techType == CricketPrefab.mv.GetComponent<TechTag>().type)
			{
				position = __instance.constructor.GetItemSpawnPoint(TechType.Seamoth).position;
				rotation = __instance.constructor.GetItemSpawnPoint(TechType.Seamoth).rotation;
				return;
			}
		}
	}
}

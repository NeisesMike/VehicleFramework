using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace CrushDrone
{
	[HarmonyPatch(typeof(BreakableResource))]
	public static class BreakableResourcePatcher
	{
		[HarmonyPrefix]
		[HarmonyPatch(nameof(BreakableResource.OnHandClick))]
		public static bool OnHandClickPrefix(BreakableResource __instance, GUIHand hand)
		{
			if (hand.GetComponent<Crush>())
			{
				__instance.BreakIntoResources();
				if (hand.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Armature|Idle"))
				{
					hand.GetComponent<Animator>().SetTrigger("Strike");
				}
				return false;
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(Inventory))]
	public static class InventoryPatcher
	{
		[HarmonyPostfix]
		[HarmonyPatch(nameof(Inventory.Pickup))]
		public static void OnHandClickPostfix(Pickupable __instance, bool __result)
		{
			VehicleFramework.VehicleTypes.Drone drone = VehicleFramework.VehicleTypes.Drone.mountedDrone;
			if (drone != null)
			{
				if (drone.GetComponent<Crush>() && __result)
				{
					if (drone.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Armature|Idle"))
					{
						drone.GetComponent<Animator>().SetTrigger("Grab");
					}
				}
			}
		}
	}
}

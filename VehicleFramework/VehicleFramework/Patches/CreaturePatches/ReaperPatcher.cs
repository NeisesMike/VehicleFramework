using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace VehicleFramework.Patches.LeviathanPatches
{
    [HarmonyPatch(typeof(ReaperMeleeAttack))]
    class ReaperMeleeAttackPatcher
	{
		/*
		 * This patch allows Reaper Leviathans to grab a ModVehicle the same way they grab a Seamoth.
		 * The Reaper's grab-anchor point is the root GameObject's transform.
		 * So if when grabbed, the Reaper is too close or too far away,
		 * all child GameObjects must be moved in relation to the root GameObject.
		 */
        [HarmonyPostfix]
		[HarmonyPatch(nameof(ReaperMeleeAttack.OnTouch))]
		public static void OnTouchPostfix(ReaperMeleeAttack __instance, Collider collider)
        {
			// This postfix basically executes the OnTouch function again but only for the GrabModVehicle case.
			if(collider.gameObject.GetComponent<Player>() != null)
            {
				ModVehicle maybeMV = collider.gameObject.GetComponent<Player>().GetModVehicle();
				if (maybeMV != null)
                {
					// Don't let the reaper grab the player from inside the ModVehicle
					return;
                }
			}
			ModVehicle mv = collider.gameObject.GetComponentInParent<ModVehicle>();
            if (mv != null)
            {
				if (__instance.liveMixin.IsAlive() && Time.time > __instance.timeLastBite + __instance.biteInterval)
				{
					Creature component = __instance.GetComponent<Creature>();
					if (component.Aggression.Value >= 0.5f)
					{
						ReaperLeviathan component2 = __instance.GetComponent<ReaperLeviathan>();
						if (!component2.IsHoldingVehicle() && !__instance.playerDeathCinematic.IsCinematicModeActive())
						{
							if (component2.GetCanGrabVehicle() && mv.CanLeviathanGrab)
							{
								component2.GrabVehicle(mv, ReaperLeviathan.VehicleType.Seamoth);
							}
							__instance.OnTouch(collider);
							component.Aggression.Value -= 0.25f;
						}
					}
				}
			}
        }
	}

	[HarmonyPatch(typeof(ReaperLeviathan))]
	class ReaperPatcher
	{
		[HarmonyPostfix]
		[HarmonyPatch(nameof(ReaperLeviathan.Update))]
		public static void UpdatePostfix(ReaperLeviathan __instance)
		{
			if ((__instance.holdingVehicle as ModVehicle) != null)
			{
				Vector3 diff = (__instance.holdingVehicle as ModVehicle).LeviathanGrabPoint.transform.position - (__instance.holdingVehicle as ModVehicle).transform.position;
				__instance.holdingVehicle.transform.position -= diff;
			}
		}
	}
}

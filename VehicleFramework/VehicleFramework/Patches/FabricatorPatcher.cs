using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace VehicleFramework.Patches
{


	[HarmonyPatch(typeof(GhostCrafter))]
	public static class FabricatorPatcher
	{
		[HarmonyPrefix]
		[HarmonyPatch("HasEnoughPower")]
		public static bool HasEnoughPowerPrefix(GhostCrafter __instance, ref bool __result)
		{
			ModVehicle mv = __instance.GetComponentInParent<ModVehicle>();
			if (mv is null || !GameModeUtils.RequiresPower())
			{
				return true;
			}
			PowerManager.PowerStatus goodPS = new PowerManager.PowerStatus
			{
				hasFuel = true,
				isPowered = true
			};
			__result = mv.GetComponent<PowerManager>().EvaluatePowerStatus() == goodPS;
			return false;
		}
		[HarmonyPrefix]
		[HarmonyPatch("CanDeconstruct")]
		public static bool CanDeconstructPrefix(GhostCrafter __instance, string reason, ref bool __result)
		{
			ModVehicle mv = __instance.GetComponentInParent<ModVehicle>();
			if (mv is null)
			{
				return true;
			}
			__result = false;
			return false;
		}
	}

	[HarmonyPatch(typeof(CrafterLogic))]
	public static class CrafterLogicPatcher
	{
		[HarmonyPrefix]
		[HarmonyPatch("ConsumeEnergy")]
		public static bool ConsumeEnergyPrefix(CrafterLogic __instance, ref bool __result, PowerRelay powerRelay, float amount)
		{
			if (!GameModeUtils.RequiresPower())
			{
				// If this is Creative or something, don't even bother
				return true;
			}
			if (powerRelay is null)
			{
				// if powerRelay was null, we must be talking about a ModVehicle
				// so let's check for one
				ModVehicle mv = null;
				foreach (ModVehicle tempMV in VehicleManager.VehiclesInPlay)
				{
					if (tempMV.IsPlayerDry)
					{
						mv = tempMV;
						break;
					}
				}
				if (mv is null)
				{
					Logger.Error("ConsumeEnergyPrefix ERROR: PowerRelay was null, but we weren't in a ModVehicle.");
					return true;
				}
				else
				{
					// we found the ModVehicle from whose fabricator we're trying to drain power
					__result = 5 <= mv.powerMan.TrySpendEnergy(5f);
					return false;
				}
			}
			// we should never make it here... so let the base game throw an error :shrug:
			return true;
		}
	}

	[HarmonyPatch(typeof(ConstructorInput))]
	public static class ConstructorInputPatcher
	{
		[HarmonyPrefix]
		[HarmonyPatch("OnHandClick")]
		public static bool OnHandClickPrefix(ConstructorInput __instance, GUIHand hand)
		{
			if (__instance.constructor.building && __instance.constructor.buildTarget?.GetComponent<ModVehicle>() != null)
			{
				return false;
			}
			return true;
		}
	}
}

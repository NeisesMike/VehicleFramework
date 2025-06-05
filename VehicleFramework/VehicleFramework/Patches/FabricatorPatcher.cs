﻿using HarmonyLib;

// PURPOSE: Ensure onboard fabricators are correctly powered. Ensure the constructor cannot build two MVs at once.
// VALUE: High.

namespace VehicleFramework.Patches
{
	[HarmonyPatch(typeof(GhostCrafter))]
	public static class FabricatorPatcher
	{
		[HarmonyPrefix]
		[HarmonyPatch(nameof(GhostCrafter.HasEnoughPower))]
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
			__result = mv.powerMan.EvaluatePowerStatus() == goodPS;
			return false;
		}
	}

	[HarmonyPatch(typeof(CrafterLogic))]
	public static class CrafterLogicPatcher
	{
		[HarmonyPrefix]
		[HarmonyPatch(nameof(CrafterLogic.ConsumeEnergy))]
		public static bool ConsumeEnergyPrefix(CrafterLogic __instance, ref bool __result, PowerRelay powerRelay, float amount)
		{
			if (!GameModeUtils.RequiresPower())
			{
				// If this is Creative or something, don't even bother
				return true;
			}
			if (powerRelay.powerPreview == null)
			{
				// if powerRelay.powerPreview was null, we must be talking about a ModVehicle
				// (it was never assigned because PowerRelay.Start is skipped for ModVehicles)
				// so let's check for one
				ModVehicle mv = null;
				foreach (ModVehicle tempMV in VehicleManager.VehiclesInPlay)
				{
					if (tempMV.IsUnderCommand)
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
					float WantToSpend = 5f;
					float SpendTolerance = 4.99f;
					float energySpent = mv.powerMan.TrySpendEnergy(WantToSpend);
					__result = SpendTolerance <= energySpent;
					return false;
				}
			}
			// we should never make it here... so let the base game throw an error :shrug:
			return true;
		}
	}

	[HarmonyPatch(typeof(ConstructorInput))]
	public static class ConstructorInputFabricatorPatcher
	{
		[HarmonyPrefix]
		[HarmonyPatch(nameof(ConstructorInput.OnHandClick))]
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

using HarmonyLib;
using UnityEngine;
using VehicleFramework.VehicleTypes;

// PURPOSE: neuter many SubRoot functions
// VALUE: High. It's valuable to have a SubRoot, but I don't want to sort these out or make them work, to be perfectly honest.

namespace VehicleFramework.Patches
{
	[HarmonyPatch(typeof(SubRoot))]
	class SubRootPatcher
	{
		[HarmonyPrefix]
		[HarmonyPatch(nameof(SubRoot.Update))]
		public static bool UpdatePrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch(nameof(SubRoot.GetLeakAmount))]
		public static bool GetLeakAmountPrefix(SubRoot __instance, ref float __result)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				__result = 0;
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch(nameof(SubRoot.OnConsoleCommand_flood))]
		public static bool OnConsoleCommand_floodPrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch(nameof(SubRoot.OnConsoleCommand_crush))]
		public static bool OnConsoleCommand_crushPrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch(nameof(SubRoot.OnConsoleCommand_damagesub))]
		public static bool OnConsoleCommand_damagesubPrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch(nameof(SubRoot.GetOxygenManager))]
		public static bool GetOxygenManagerPrefix(SubRoot __instance, ref OxygenManager? __result)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				__result = null;
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch(nameof(SubRoot.OnKill))]
		public static bool OnKillPrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch(nameof(SubRoot.GetModulesRoot))]
		public static bool GetModulesRootPrefix(SubRoot __instance, ref Transform? __result)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				__result = null;
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch(nameof(SubRoot.GetWorldCenterOfMass))]
		public static bool GetWorldCenterOfMassPrefix(SubRoot __instance, ref Vector3 __result)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				__result = Vector3.zero;
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch(nameof(SubRoot.OnCollisionEnter))]
		public static bool OnCollisionEnterPrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch(nameof(SubRoot.CrushDamageRandomPart))]
		public static bool CrushDamageRandomPartPrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch(nameof(SubRoot.UpdateDamageSettings))]
		public static bool UpdateDamageSettingsPrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch(nameof(SubRoot.ForceLightingState))]
		public static bool ForceLightingStatePrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch(nameof(SubRoot.UpdateLighting))]
		public static bool UpdateLightingPrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch(nameof(SubRoot.GetTemperature))]
		public static bool GetTemperaturePrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch(nameof(SubRoot.UpdateThermalReactorCharge))]
		public static bool UpdateThermalReactorChargePrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch(nameof(SubRoot.FixedUpdate))]
		public static bool FixedUpdatePrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch(nameof(SubRoot.SetCyclopsUpgrades))]
		public static bool SetCyclopsUpgradesPrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch(nameof(SubRoot.SetExtraDepth))]
		public static bool SetExtraDepthPrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch(nameof(SubRoot.UpdatePowerRating))]
		public static bool UpdatePowerRatingPrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch(nameof(SubRoot.GetPowerRating))]
		public static bool GetPowerRatingPrefix(SubRoot __instance, ref float __result)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				__result = 1;
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch(nameof(SubRoot.OnSubModulesChanged))]
		public static bool OnSubModulesChangedPrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch(nameof(SubRoot.UpdateSubModules))]
		public static bool UpdateSubModulesPrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch(nameof(SubRoot.OnPlayerEntered))]
		public static bool OnPlayerEnteredPrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				Logger.DebugLog("skipping enter");
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch(nameof(SubRoot.OnPlayerExited))]
		public static bool OnPlayerExitedPrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				Logger.DebugLog("skipping exit");
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch(nameof(SubRoot.GetSubName))]
		public static bool GetSubNamePrefix(SubRoot __instance, ref string __result)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				__result = "ModVehicle";
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch(nameof(SubRoot.OnProtoSerialize))]
		public static bool OnProtoSerializerefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch(nameof(SubRoot.OnProtoDeserialize))]
		public static bool OnProtoDeserializePrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch(nameof(SubRoot.StartSubShielded))]
		public static bool StartSubShieldedPrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch(nameof(SubRoot.EndSubShielded))]
		public static bool EndSubShieldedPrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch(nameof(SubRoot.PowerDownCyclops))]
		public static bool PowerDownCyclopsPrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch(nameof(SubRoot.DestroyCyclopsSubRoot))]
		public static bool DestroyCyclopsSubRootPrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch(nameof(SubRoot.Awake))]
		public static bool AwakePrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch(nameof(SubRoot.Start))]
		public static bool StartPrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch(nameof(SubRoot.OnTakeDamage))]
		public static bool OnTakeDamagePrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch(nameof(SubRoot.IsLeaking))]
		public static bool IsLeakingPrefix(SubRoot __instance, ref bool __result)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				__result = false;
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch(nameof(SubRoot.IsUnderwater))]
		public static bool IsUnderwaterPrefix(SubRoot __instance, ref bool __result)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				__result = __instance.GetComponent<ModVehicle>().GetIsUnderwater();
				return false;
			}
			return true;
		}
	}
}

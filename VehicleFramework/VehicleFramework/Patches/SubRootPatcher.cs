using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace VehicleFramework.Patches
{
	[HarmonyPatch(typeof(SubRoot))]

	class SubRootPatcher
	{
		[HarmonyPrefix]
		[HarmonyPatch("Update")]
		public static bool UpdatePrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch("GetLeakAmount")]
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
		[HarmonyPatch("OnConsoleCommand_flood")]
		public static bool OnConsoleCommand_floodPrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch("OnConsoleCommand_crush")]
		public static bool OnConsoleCommand_crushPrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch("OnConsoleCommand_damagesub")]
		public static bool OnConsoleCommand_damagesubPrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch("GetOxygenManager")]
		public static bool GetOxygenManagerPrefix(SubRoot __instance, ref OxygenManager __result)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				__result = null;
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch("OnKill")]
		public static bool OnKillPrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch("GetModulesRoot")]
		public static bool GetModulesRootPrefix(SubRoot __instance, ref Transform __result)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				__result = null;
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch("GetWorldCenterOfMass")]
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
		[HarmonyPatch("OnCollisionEnter")]
		public static bool OnCollisionEnterPrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch("CrushDamageRandomPart")]
		public static bool CrushDamageRandomPartPrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch("UpdateDamageSettings")]
		public static bool UpdateDamageSettingsPrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch("ForceLightingState")]
		public static bool ForceLightingStatePrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch("UpdateLighting")]
		public static bool UpdateLightingPrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch("GetTemperature")]
		public static bool GetTemperaturePrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch("UpdateThermalReactorCharge")]
		public static bool UpdateThermalReactorChargePrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch("FixedUpdate")]
		public static bool FixedUpdatePrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch("SetCyclopsUpgrades")]
		public static bool SetCyclopsUpgradesPrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch("SetExtraDepth")]
		public static bool SetExtraDepthPrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch("UpdatePowerRating")]
		public static bool UpdatePowerRatingPrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch("GetPowerRating")]
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
		[HarmonyPatch("OnSubModulesChanged")]
		public static bool OnSubModulesChangedPrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch("UpdateSubModules")]
		public static bool UpdateSubModulesPrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch("OnPlayerEntered")]
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
		[HarmonyPatch("OnPlayerExited")]
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
		[HarmonyPatch("GetSubName")]
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
		[HarmonyPatch("OnProtoSerialize")]
		public static bool OnProtoSerializerefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch("OnProtoDeserialize")]
		public static bool OnProtoDeserializePrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch("StartSubShielded")]
		public static bool StartSubShieldedPrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch("EndSubShielded")]
		public static bool EndSubShieldedPrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch("PowerDownCyclops")]
		public static bool PowerDownCyclopsPrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch("DestroyCyclopsSubRoot")]
		public static bool DestroyCyclopsSubRootPrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch("Awake")]
		public static bool AwakePrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch("Start")]
		public static bool StartPrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch("OnTakeDamage")]
		public static bool OnTakeDamagePrefix(SubRoot __instance)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				return false;
			}
			return true;
		}
		[HarmonyPrefix]
		[HarmonyPatch("IsLeaking")]
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
		[HarmonyPatch("IsUnderwater")]
		public static bool IsUnderwaterPrefix(SubRoot __instance, ref bool __result)
		{
			if (__instance.GetComponent<ModVehicle>())
			{
				__result = true;
				return false;
			}
			return true;
		}
	}
}

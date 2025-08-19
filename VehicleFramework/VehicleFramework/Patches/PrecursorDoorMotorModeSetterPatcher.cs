using HarmonyLib;
using UnityEngine;
using VehicleFramework.VehicleTypes;

// PURPOSE: ModVehicles that go through MoonGates should fall to the ground
// VALUE: High.

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(PrecursorDoorMotorModeSetter))]
    public class PrecursorDoorMotorModeSetterPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(PrecursorDoorMotorModeSetter.OnTriggerEnter))]
        public static void PrecursorDoorMotorModeSetterOnTriggerEnterHarmonyPostfix(PrecursorDoorMotorModeSetter __instance, Collider col)
		{
			if (__instance.setToMotorModeOnEnter == PrecursorDoorMotorMode.None)
			{
				return;
			}
			if (col.gameObject != null && col.gameObject.GetComponentInChildren<IgnoreTrigger>() != null)
			{
				return;
			}
			GameObject gameObject = UWE.Utils.GetEntityRoot(col.gameObject);
			if (gameObject == null)
			{
				if (col.gameObject != null)
                {
                    gameObject = col.gameObject;
                }
				else
				{
					return;
				}
			}
			ModVehicle componentInHierarchy2 = UWE.Utils.GetComponentInHierarchy<ModVehicle>(gameObject);
			if (componentInHierarchy2 != null)
			{
				PrecursorDoorMotorMode precursorDoorMotorMode = __instance.setToMotorModeOnEnter;
				if (precursorDoorMotorMode == PrecursorDoorMotorMode.Auto)
				{
					componentInHierarchy2.precursorOutOfWater = false;
					return;
				}
				if (precursorDoorMotorMode != PrecursorDoorMotorMode.ForceWalk)
				{
					return;
				}
				componentInHierarchy2.precursorOutOfWater = true;
			}
		}
    }
}

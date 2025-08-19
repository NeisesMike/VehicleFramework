using HarmonyLib;
using UnityEngine;

// PURPOSE: Ensure sleeping in a bed in a submarine doesn't cause the vehicle to drift (issues caused by the animation)
// VALUE: high, for the sake of world consistency

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(Bed))]
    public class BedPatcher
    {
        private static Vector3 myRotation = Vector3.zero;
        private static Vector3 myPosition = Vector3.zero;
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Bed.EnterInUseMode))]
        public static void BedEnterInUseModePostfix(Bed __instance)
        {
            VehicleTypes.Submarine? sub = Player.main.GetVehicle() as VehicleTypes.Submarine;
            if (sub != null && __instance.inUseMode == Bed.InUseMode.Sleeping)
            {
                // freeze the sub
                myRotation = sub.transform.eulerAngles;
                myPosition = sub.transform.position;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Bed.Update))]
        public static void UpdatePostfix(Bed __instance)
        {
            VehicleTypes.Submarine? sub = Player.main.GetVehicle() as VehicleTypes.Submarine;
            if (sub != null && __instance.inUseMode == Bed.InUseMode.Sleeping)
            {
                sub.transform.eulerAngles = myRotation;
                sub.transform.position = myPosition;
            }
        }
    }
}

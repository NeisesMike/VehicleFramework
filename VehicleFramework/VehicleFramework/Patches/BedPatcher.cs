using HarmonyLib;
using UnityEngine;

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(Bed))]
    public class BedPatcher
    {
        public static Vector3 myRotation = Vector3.zero;
        public static Vector3 myPosition = Vector3.zero;
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Bed.EnterInUseMode))]
        public static void BedEnterInUseModePostfix(Bed __instance)
        {
            VehicleTypes.Submarine sub = Player.main.GetVehicle() as VehicleTypes.Submarine;
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
            VehicleTypes.Submarine sub = Player.main.GetVehicle() as VehicleTypes.Submarine;
            if (sub != null && __instance.inUseMode == Bed.InUseMode.Sleeping)
            {
                sub.transform.eulerAngles = myRotation;
                sub.transform.position = myPosition;
            }
        }
    }
}

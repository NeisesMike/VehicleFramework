using HarmonyLib;
using UnityEngine;

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(ArmsController))]
    public class ArmsControllerPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(ArmsController.Update))]
        public static void ArmsControllerUpdatePostfix(Bed __instance)
        {
            ModVehicle mv = Player.main.GetVehicle() as ModVehicle;
            if (mv != null)
            {
                mv.HandlePilotingAnimations();
            }
        }
    }
}

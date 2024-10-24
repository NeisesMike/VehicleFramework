using HarmonyLib;
using UnityEngine;

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(ArmsController))]
    public class ArmsControllerPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(ArmsController.Update))]
        public static void ArmsControllerUpdatePostfix()
        {
            ModVehicle mv = Player.main.GetModVehicle();
            if (mv != null)
            {
                mv.HandlePilotingAnimations();
            }
        }
    }
}

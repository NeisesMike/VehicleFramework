using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace VehicleFramework
{
    [HarmonyPatch(typeof(PlayerController))]
    public class PlayerControllerPatcher
    {
        [HarmonyPrefix]
        [HarmonyPatch("HandleControllerState")]
        public static bool HandleControllerStatePrefix(PlayerController __instance)
        {
            ModVehicle mv = Player.main.GetVehicle() as ModVehicle;
            if (mv != null && !mv.IsPlayerPiloting())
            {
                __instance.inVehicle = false;
                __instance.underWater = false;
            }
            return true;
        }
        [HarmonyPrefix]
        [HarmonyPatch("HandleUnderWaterState")]
        public static bool HandleUnderWaterStatePrefix(PlayerController __instance)
        {
            ModVehicle mv = Player.main.GetVehicle() as ModVehicle;
            if (mv != null && !mv.IsPlayerPiloting())
            {
                __instance.inVehicle = true;
                __instance.underWater = false;
                __instance.groundController.SetEnabled(false);
                __instance.underWaterController.SetEnabled(false);
                __instance.activeController = __instance.groundController;
                __instance.desiredControllerHeight = __instance.standheight;
                __instance.activeController.SetControllerHeight(__instance.currentControllerHeight);
                __instance.activeController.SetEnabled(true);
                return false;
            }
            return true;
        }
        /*
        [HarmonyPostfix]
        [HarmonyPatch("HandleUnderWaterState")]
        public static void HandleUnderWaterStatePostfix(PlayerController __instance)
        {
            ModVehicle mv = Player.main.GetVehicle() as ModVehicle;
            if (mv != null && !mv.IsPlayerPiloting())
            {
                __instance.inVehicle = false;
                __instance.underWater = false;
            }
        }
        */
    }
}

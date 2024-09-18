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
        /*
         * This patch ensures the Player behaves as expected inside a ModVehicle.
         * That is, the player should always act as "normally grounded."
         * This patch prevents the player from doing any swim-related behaviors while inside a ModVehicle
         */
        [HarmonyPrefix]
        [HarmonyPatch("HandleUnderWaterState")]
        public static bool HandleUnderWaterStatePrefix(PlayerController __instance)
        {
            VehicleTypes.Submarine mv = Player.main.GetVehicle() as VehicleTypes.Submarine;
            if (mv != null && !mv.IsPlayerControlling())
            {
                __instance.inVehicle = true;
                __instance.underWater = false;
                __instance.groundController.SetEnabled(false);
                __instance.underWaterController.SetEnabled(false);
                __instance.activeController = __instance.groundController;
                __instance.desiredControllerHeight = __instance.standheight;
                __instance.activeController.SetControllerHeight(__instance.currentControllerHeight, __instance.cameraOffset);
                __instance.activeController.SetEnabled(true);
                return false;
            }
            return true;
        }
    }
}

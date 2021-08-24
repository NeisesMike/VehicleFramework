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
        public static bool HandleControllerStatePrefix(ref bool ___inVehicle, ref bool ___underWater)
        {
            ModVehicle mv = Player.main.currentMountedVehicle as ModVehicle;
            if (mv != null && !mv.IsPlayerPiloting())
            {
                ___inVehicle = false;
                ___underWater = false;
            }
            return true;
        }
    }
}

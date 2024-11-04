using HarmonyLib;
using UnityEngine;

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(Bench))]
    public class BenchPatcher
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Bench.ExitSittingMode))]
        public static bool BenchExitSittingModePrefix(Player player, bool skipCinematics)
        {
            if(VehicleTypes.Drone.mountedDrone != null)
            {
                if (skipCinematics || player.isUnderwater.value)
                {
                    if (player.mode == Player.Mode.LockedPiloting)
                    {
                        Player.main.ExitLockedMode();
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Bench.EnterSittingMode))]
        public static void BenchEnterSittingModePostfix()
        {
            if(Player.main.GetModVehicle() != null)
            {
                Player.main.GetModVehicle().controlSheme = Vehicle.ControlSheme.Mech;
            }
        }
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Bench.ExitSittingMode))]
        public static void BenchExitSittingModePostfix()
        {
            if (Player.main.GetModVehicle() != null)
            {
                Player.main.GetModVehicle().controlSheme = (Vehicle.ControlSheme)12;
            }
        }
    }
}

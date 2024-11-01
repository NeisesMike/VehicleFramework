using HarmonyLib;
using UnityEngine;

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(Bench))]
    public class BenchPatcher
    {
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

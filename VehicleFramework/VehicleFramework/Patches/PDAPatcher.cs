using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace VehicleFramework
{
    [HarmonyPatch(typeof(PDA))]
    public class PDAPatcher
    {
        /*
         * This patch ensures our QuickSlots display as expected when inside the ModVehicle but not piloting it.
         * That is, when piloting the ModVehicle, we should see the ModVehicle's modules.
         * When merely standing in the ModVehicle, we should see our own items: knife, flashlight, scanner, etc
         */
        [HarmonyPostfix]
        [HarmonyPatch(nameof(PDA.Close))]
        public static void ClosePostfix()
        {
            VehicleTypes.Submarine mv = Player.main.GetVehicle() as VehicleTypes.Submarine;
            if (mv != null && !mv.IsPlayerPiloting())
            {
                uGUI.main.quickSlots.SetTarget(null);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PDA.Open))]
        public static bool OpenPrefix(PDA __instance)
        {
            return VehicleTypes.Drone.mountedDrone == null;
        }
    }
}

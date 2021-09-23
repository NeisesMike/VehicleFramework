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
        [HarmonyPostfix]
        [HarmonyPatch("Close")]
        public static void ClosePostfix()
        {
            ModVehicle mv = Player.main.GetVehicle() as ModVehicle;
            if (mv != null && !mv.IsPlayerPiloting())
            {
                uGUI.main.quickSlots.SetTarget(null);
            }
        }
    }
}

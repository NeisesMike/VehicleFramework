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
    [HarmonyPatch(typeof(uGUI_PDA))]
    public class uGUI_PDAPatcher
    {
        public static GameObject moduleBuilder;

        [HarmonyPostfix]
        [HarmonyPatch("Awake")]
        public static void AwakePostfix()
        {
            Logger.Log("uGUI_PDA awake!");
            moduleBuilder = new GameObject("ModuleBuilder");
            ModuleBuilder.main = moduleBuilder.AddComponent<ModuleBuilder>();
            Logger.Log("grab components");
            ModuleBuilder.main.grabComponents();

            // We believe this is guaranteed to execute AFTER all the Starts of the initial load,
            // since it executes the first time the player opens the PDA
            VehicleManager.LoadVehicles();

            // TODO maybe destroy self here?

        }
    }
}

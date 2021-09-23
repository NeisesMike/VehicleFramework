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
        [HarmonyPostfix]
        [HarmonyPatch("Awake")]
        public static void AwakePostfix()
        {
            VehicleBuilder.moduleBuilder = new GameObject("ModuleBuilder");
            ModuleBuilder.main = VehicleBuilder.moduleBuilder.AddComponent<ModuleBuilder>();
            ModuleBuilder.main.grabComponents();
            // TODO maybe destroy self here?
        }
    }
}

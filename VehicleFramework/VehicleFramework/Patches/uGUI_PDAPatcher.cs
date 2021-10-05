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
        /*
         * This patch ensures our modules are built at the first available opportunity.
         * That is, the module-build process requires information in the uGUI_PDA.
         * Namely, it borrows some contents related to the Seamoth.
         */
        [HarmonyPostfix]
        [HarmonyPatch("Awake")]
        public static void AwakePostfix()
        {
            VehicleBuilder.moduleBuilder = new GameObject("ModuleBuilder");
            ModuleBuilder.main = VehicleBuilder.moduleBuilder.AddComponent<ModuleBuilder>();
            ModuleBuilder.main.grabComponents();
        }
    }
}

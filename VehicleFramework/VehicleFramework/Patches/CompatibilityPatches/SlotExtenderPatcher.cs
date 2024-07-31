using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace VehicleFramework.Patches.CompatibilityPatches
{
    class SlotExtenderPatcher
    {
        /*
         * This patch is specifically for the Slot Extender mod.
         * It ensures that our ModVehicle upgrades UI is displayed correctly.
         */
        [HarmonyPrefix]
        public static bool PrePrefix(object __instance)
        {
            if (ModuleBuilder.slotExtenderIsPatched)
            {
                return true;
            }
            else if(ModuleBuilder.slotExtenderHasGreenLight)
            {
                ModuleBuilder.slotExtenderIsPatched = true;
                return true;
            }
            return false;
        }

        [HarmonyPrefix]
        public static bool PrePostfix(object __instance)
        {
            if (ModuleBuilder.slotExtenderIsPatched)
            {
                return true;
            }
            else if (ModuleBuilder.slotExtenderHasGreenLight)
            {
                ModuleBuilder.slotExtenderIsPatched = true;
                return true;
            }
            return false;
        }
    }
}

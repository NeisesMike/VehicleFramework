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
        public static bool isPatched = false;
        public static bool hasGreenLight = false;
        /*
         * This patch is specifically for the Slot Extender mod.
         * It ensures that our ModVehicle upgrades UI is displayed correctly.
         */
        [HarmonyPrefix]
        public static bool PrePrefix(object __instance)
        {
            if (isPatched)
            {
                Logger.Log("skipping since already patched");
                return true;
            }
            else if(hasGreenLight)
            {
                Logger.Log("got the green light");
                isPatched = true;
                return true;
            }

            // need to postpone this function until ModuleBuilder is done
            /*
            IEnumerator DoThisAfterSomeTime()
            {
                Logger.Log("wait for it...");
                yield return new WaitForSeconds(5);
                Logger.Log("okay go!");
                var type2 = Type.GetType("SlotExtender.Patches.uGUI_Equipment_Awake_Patch, SlotExtender", false, false);
                var awakeOriginal = AccessTools.Method(type2, "Prefix");
                uGUI_Equipment equipment = uGUI_PDA.main.transform.Find("Content/InventoryTab/Equipment")?.GetComponent<uGUI_Equipment>();
                object dummyInstance = null;
                awakeOriginal.Invoke(dummyInstance, new object[]{equipment});
            }
            ModuleBuilder.main.StartCoroutine(DoThisAfterSomeTime());
            */
            return false;
        }
    }
}

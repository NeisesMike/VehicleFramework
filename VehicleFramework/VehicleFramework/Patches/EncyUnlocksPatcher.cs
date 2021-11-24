using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(KnownTech))]
    public static class KnownTechPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch("Add")]
        public static void UnlockPostfix(TechType techType)
        {
            if(techType == TechType.Constructor)
            {
                // ensure we unlock our encyclopedia pages
                foreach (VehicleEntry ve in VehicleManager.vehicleTypes)
                {
                    PDAEncyclopedia.Add(ve.prefab.name, true);
                }
            }
        }
    }

    [HarmonyPatch(typeof(InventoryConsoleCommands))]
    public static class InventoryConsoleCommandsPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch("OnConsoleCommand_unlock")]
        public static void UnlockPostfix(NotificationCenter.Notification n)
        {
            if (n != null && n.data != null)
            {
                string text = (string)n.data[0];
                foreach (VehicleEntry ve in VehicleManager.vehicleTypes)
                {
                    if (text == ve.prefab.name)
                    {
                        PDAEncyclopedia.Add(ve.prefab.name, true);
                    }
                }

                if(text=="constructor")
                {
                    foreach (VehicleEntry ve in VehicleManager.vehicleTypes)
                    {
                        PDAEncyclopedia.Add(ve.prefab.name, true);
                    }
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace VehicleFramework.Patches
{
    [HarmonyPatch(typeof(CyclopsVehicleStorageTerminalManager))]
    public static class CyclopsVehicleStorageTerminalManagerPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(CyclopsVehicleStorageTerminalManager.StorageButtonClick))]
        public static void StorageButtonClickPostfix(CyclopsVehicleStorageTerminalManager __instance, CyclopsVehicleStorageTerminalManager.VehicleStorageType type, int slotID)
        {
            if (__instance.dockedVehicleType == CyclopsVehicleStorageTerminalManager.DockedVehicleType.Seamoth)
            {
                foreach (StorageInput seamothStorageInput in __instance.currentVehicle.GetAllComponentsInChildren<StorageInput>())
                {
                    if (seamothStorageInput.slotID == slotID)
                    {
                        seamothStorageInput.OpenFromExternal();
                        return;
                    }
                }
                return;
            }
        }
    }
}

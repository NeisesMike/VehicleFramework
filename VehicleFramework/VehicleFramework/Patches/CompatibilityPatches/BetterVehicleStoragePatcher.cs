using System;
using HarmonyLib;

// PURPOSE: add compatibility for better vehicle storage upgrades
// VALUE: High. It's a very cool mod.

namespace VehicleFramework.Patches.CompatibilityPatches
{
    public static class BetterVehicleStoragePatcher
    {
        /*
         * This patch is specifically for the Better Vehicle Storage Mod.
         * It allows the storage containers to be added to ModVehicles.
         */
        [HarmonyPrefix]
        public static bool Prefix(object __instance, Equipment equipment, ref bool __result)
        {
            if (equipment.owner.GetComponent<ModVehicle>() != null)
            {
                __result = true;
                return false;
            }
            return true;
        }

        public static void TryUseBetterVehicleStorage(ModVehicle mv, int slotID, TechType techType)
        {
            var type3 = Type.GetType("BetterVehicleStorage.Managers.StorageModuleMgr, BetterVehicleStorage", false, false);
            if (type3 != null)
            {
                var IsStorageModule = HarmonyLib.AccessTools.Method(type3, "IsStorageModule");
                object[] isStorageModuleParams = new object[] { techType };
                bool isBetterVehicleStorageModule = (bool)IsStorageModule.Invoke(null, isStorageModuleParams);

                if (isBetterVehicleStorageModule)
                {
                    InventoryItem? slotItem = mv.GetSlotItem(slotID);
                    if(slotItem == null)
                    {
                        return;
                    }

                    var GetItemsContainerFromIventoryItem = HarmonyLib.AccessTools.Method(type3, "GetItemsContainerFromIventoryItem");
                    object[] GetItemsParams = new object[] { slotItem, techType };
                    ItemsContainer itemContainer = (ItemsContainer)GetItemsContainerFromIventoryItem.Invoke(null, GetItemsParams);

                    PDA pda = Player.main.GetPDA();
                    Inventory.main.SetUsedStorage(itemContainer, false);
                    pda.Open(PDATab.Inventory, null, null);
                }
            }
        }
    }
}

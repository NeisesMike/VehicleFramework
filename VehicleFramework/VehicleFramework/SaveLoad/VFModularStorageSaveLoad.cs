using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace VehicleFramework.SaveLoad
{
    internal static class VFModularStorageSaveLoad
    {
        const string saveFileNameSuffix = "modularstorage";
        internal static string GetSaveFileName(string thisSlotName)
        {
            return $"{thisSlotName}-{saveFileNameSuffix}";
        }
        internal static string GetNewSaveFileName(string thisSlotName)
        {
            return thisSlotName["Vehicle".Length..];
        }
        internal static void SerializeAllModularStorage(ModVehicle mv)
        {
            foreach(string slotID in mv.slotIDs)
            {
                if (mv.modules.equipment.TryGetValue(slotID, out InventoryItem result))
                {
                    var container = result?.item?.GetComponent<SeamothStorageContainer>();
                    if (container != null && container.container != null)
                    {
                        SaveThisModularStorage(mv, container.container, slotID);
                    }
                }
            }
        }
        private static void SaveThisModularStorage(ModVehicle mv, ItemsContainer container, string slotID)
        {
            List<Tuple<TechType, float, TechType>> result = new();
            foreach (var item in container.ToList())
            {
                TechType thisItemType = item.item.GetTechType();
                float batteryChargeIfApplicable = -1;
                var bat = item.item.GetComponentInChildren<Battery>(true);
                TechType innerBatteryTT = TechType.None;
                if (bat != null)
                {
                    batteryChargeIfApplicable = bat.charge;
                    innerBatteryTT = bat.gameObject.GetComponent<TechTag>().type;
                }
                result.Add(new(thisItemType, batteryChargeIfApplicable, innerBatteryTT));
            }
            JsonInterface.Write(mv, GetNewSaveFileName(slotID), result);
        }
        internal static IEnumerator DeserializeAllModularStorage(ModVehicle mv)
        {
            yield return new WaitUntil(() => Admin.GameStateWatcher.IsWorldLoaded);
            yield return new WaitUntil(() => mv.upgradesInput.equipment != null);
            foreach(var upgradesLoader in mv.GetComponentsInChildren<VFUpgradesIdentifier>())
            {
                yield return new WaitUntil(() => upgradesLoader.isFinished);
            }
            foreach (string slotID in mv.slotIDs)
            {
                if (mv.modules.equipment.TryGetValue(slotID, out InventoryItem result))
                {
                    var container = result?.item?.GetComponent<SeamothStorageContainer>();
                    if (container != null && container.container != null)
                    {
                        Admin.SessionManager.StartCoroutine(LoadThisModularStorage(mv, container.container, slotID));
                    }
                }
            }
            yield break;
        }
        private static IEnumerator LoadThisModularStorage(ModVehicle mv, ItemsContainer container, string slotID)
        {
            var thisStorage = SaveLoad.JsonInterface.Read<List<Tuple<TechType, float, TechType>>>(mv, GetNewSaveFileName(slotID));
            if (thisStorage == default)
            {
                thisStorage = SaveLoad.JsonInterface.Read<List<Tuple<TechType, float, TechType>>>(mv, GetSaveFileName(slotID));
                if (thisStorage == default)
                {
                    yield break;
                }
            }
            TaskResult<GameObject> result = new();
            foreach (var item in thisStorage)
            {
                yield return CraftData.InstantiateFromPrefabAsync(item.Item1, result, false);
                GameObject thisItem = result.Get();

                thisItem.transform.SetParent(mv.StorageRootObject.transform);
                try
                {
                    container.AddItem(thisItem.EnsureComponent<Pickupable>());
                }
                catch (Exception e)
                {
                    Logger.LogException($"Failed to add storage item {thisItem.name} to modular storage in slot {slotID} for {mv.name} : {mv.subName.hullName.text}", e);
                }
                thisItem.SetActive(false);
                if (item.Item2 >= 0)
                {
                    // then we have a battery xor we are a battery
                    try
                    {
                        Admin.SessionManager.StartCoroutine(SaveLoadUtils.ReloadBatteryPower(thisItem, item.Item2, item.Item3));
                    }
                    catch (Exception e)
                    {
                        Logger.LogException($"Failed to load reload battery power for modular storage item {thisItem.name} to modular storage in slot {slotID} for {mv.name} : {mv.subName.hullName.text}", e);
                    }
                }
            }
        }
    }
}

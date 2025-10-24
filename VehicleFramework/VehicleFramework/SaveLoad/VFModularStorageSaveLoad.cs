using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using techTypeString = System.String;

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
            List<Tuple<techTypeString, float, techTypeString>> result = new();
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
                result.Add(new(thisItemType.AsString(), batteryChargeIfApplicable, innerBatteryTT.AsString()));
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
            var thisStorage = SaveLoad.JsonInterface.Read<List<Tuple<techTypeString, float, techTypeString>>>(mv, GetNewSaveFileName(slotID));
            if (thisStorage == default)
            {
                thisStorage = SaveLoad.JsonInterface.Read<List<Tuple<techTypeString, float, techTypeString>>>(mv, GetSaveFileName(slotID));
                if (thisStorage == default)
                {
                    yield break;
                }
            }
            TaskResult<GameObject> result = new();
            foreach (var item in thisStorage)
            {
                TechTypeExtensions.FromString(item.Item1, out TechType techType, true);
                TechTypeExtensions.FromString(item.Item3, out TechType innerTechType, true);
                yield return CraftData.InstantiateFromPrefabAsync(techType, result, false);
                GameObject thisItem = result.Get();

                thisItem.transform.SetParent(mv.StorageRootObject.transform);
                try
                {
                    container.AddItem(thisItem.EnsureComponent<Pickupable>());
                }
                catch (Exception e)
                {
                    Logger.LogException($"Failed to add storage item {thisItem.name} to modular storage in slot {slotID} for {mv.name} : {mv.HullName}", e);
                }
                thisItem.SetActive(false);
                if (item.Item2 >= 0)
                {
                    // then we have a battery xor we are a battery
                    try
                    {
                        Admin.SessionManager.StartCoroutine(SaveLoadUtils.ReloadBatteryPower(thisItem, item.Item2, innerTechType));
                    }
                    catch (Exception e)
                    {
                        Logger.LogException($"Failed to load reload battery power for modular storage item {thisItem.name} to modular storage in slot {slotID} for {mv.name} : {mv.HullName}", e);
                    }
                }
            }
        }
    }
}

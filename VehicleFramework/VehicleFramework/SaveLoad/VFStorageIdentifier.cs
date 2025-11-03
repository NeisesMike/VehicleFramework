using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VehicleFramework.Interfaces;
using VehicleFramework.StorageComponents;
using storageData = System.Collections.Generic.List<System.Tuple<System.String, float, System.String>>;

namespace VehicleFramework.SaveLoad
{
    internal class VFStorageIdentifier : MonoBehaviour, ISaveLoadListener
    {
        internal ModVehicle MV => GetComponentInParent<ModVehicle>();
        bool ISaveLoadListener.IsReady()
        {
            return MV != null && Admin.GameStateWatcher.IsWorldLoaded && MV.upgradesInput.equipment != null;
        }
        string ISaveLoadListener.SaveDataKey => SaveLoadUtils.GetUniqueNameForChild(MV.transform, transform, "storage");
        void ISaveLoadListener.LoadData(JToken? data)
        {
            if (data == null) return;
            if (data is not JArray _)
                throw new Newtonsoft.Json.JsonException("Expected a JSON object for List<Tuple<string,float,string>>.");
            storageData? loadData = data.ToObject<storageData>();
            if (loadData != null)
            {
                if (GetComponent<InnateStorageContainer>() != null)
                {
                    LoadInnateData(loadData);
                }
                if (GetComponent<ModularStorageInput>() != null)
                {
                    LoadModularData(loadData);
                }
            }
        }
        object? ISaveLoadListener.SaveData()
        {
            if (GetComponent<InnateStorageContainer>() != null)
            {
                return SaveInnateData();
            }
            if (GetComponent<ModularStorageInput>() != null)
            {
                return SaveModularData();
            }
            return null;
        }

        storageData? SaveInnateData()
        { 
            return SaveStorageData(GetComponent<InnateStorageContainer>().Container);
        }
        private storageData? SaveModularData()
        {
            ItemsContainer? modStorageContainer = MV.ModGetStorageInSlot(GetComponent<ModularStorageInput>().slotID, TechType.VehicleStorageModule);
            if (modStorageContainer != null)
            {
                return SaveStorageData(modStorageContainer);
            }
            return null;
        }
        private static storageData SaveStorageData(ItemsContainer container)
        {
            storageData result = new();
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
            return result;
        }

        private void LoadInnateData(storageData datum)
        {
            Admin.SessionManager.StartCoroutine(LoadStorage(GetComponent<InnateStorageContainer>().Container, datum));
        }
        private void LoadModularData(storageData datum)
        {
            ItemsContainer? modStorageContainer = MV.ModGetStorageInSlot(GetComponent<ModularStorageInput>().slotID, TechType.VehicleStorageModule);
            if(modStorageContainer != null)
            {
                Admin.SessionManager.StartCoroutine(LoadStorage(modStorageContainer, datum));
            }
        }
        private IEnumerator LoadStorage(ItemsContainer container, storageData datum)
        {
            yield return new WaitUntil(() => MV != null);

            if (datum == default)
            {
                yield break;
            }

            TaskResult<GameObject> result = new();
            foreach (var item in datum)
            {
                TechTypeExtensions.FromString(item.Item1, out TechType techType, true);
                TechTypeExtensions.FromString(item.Item3, out TechType innerTechType, true);
                yield return CraftData.InstantiateFromPrefabAsync(techType, result, false);
                GameObject thisItem = result.Get();

                thisItem.transform.SetParent(MV.StorageRootObject.transform);
                try
                {
                    container.AddItem(thisItem.EnsureComponent<Pickupable>());
                }
                catch (Exception e)
                {
                    Logger.LogException($"Failed to add storage item {thisItem.name} to storage on GameObject {gameObject.name} for {MV.name} : {MV.HullName}", e);
                }
                thisItem.SetActive(false);
                if (item.Item2 >= 0)
                {
                    // then we have a battery xor we are a battery
                    try
                    {
                        Admin.SessionManager.StartCoroutine(SaveLoadUtils.ReloadBatteryPower(thisItem, item.Item2, innerTechType));
                    }
                    catch(Exception e)
                    {
                        Logger.LogException($"Failed to reload battery power for storage item {thisItem.name} in storage on GameObject {gameObject.name} for {MV.name} : {MV.HullName}", e);
                    }
                }
            }
        }
    }
}

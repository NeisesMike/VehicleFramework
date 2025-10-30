using System;
using System.Collections;
using UnityEngine;
using VehicleFramework.Interfaces;
using batteryData = System.Tuple<System.String, float>;
using Newtonsoft.Json.Linq;

namespace VehicleFramework.SaveLoad
{
    internal class VFBatteryIdentifier : MonoBehaviour, ISaveLoadListener
    {
        internal ModVehicle MV => GetComponentInParent<ModVehicle>();
        string ISaveLoadListener.SaveDataKey => SaveLoadUtils.GetUniqueNameForChild(MV.transform, transform, "battery");
        bool ISaveLoadListener.IsReady()
        {
            return MV != null;
        }
        private IEnumerator LoadBattery(object datum)
        {
            yield return new WaitUntil(() => (this as ISaveLoadListener).IsReady());
            batteryData? batteryDatum = datum as batteryData;
            yield return new WaitUntil(() => (this as ISaveLoadListener).IsReady());
            if (batteryDatum == default || string.Equals(batteryDatum.Item1, TechType.None.AsString()))
            {
                yield break;
            }
            TaskResult<GameObject> result = new();
            TechTypeExtensions.FromString(batteryDatum.Item1, out TechType techType, true);
            yield return CraftData.InstantiateFromPrefabAsync(techType, result, false);
            GameObject thisItem = result.Get();
            try
            {
                thisItem.GetComponent<Battery>().charge = batteryDatum.Item2;
                thisItem.transform.SetParent(MV.StorageRootObject.transform);
                GetComponent<EnergyMixin>().battery = thisItem.GetComponent<Battery>();
                GetComponent<EnergyMixin>().batterySlot.AddItem(thisItem.GetComponent<Pickupable>());
                thisItem.SetActive(false);
            }
            catch (Exception e)
            {
                Logger.LogException($"Failed to load battery : {batteryDatum.Item1} for {MV.name} on GameObject {gameObject.name} : {MV.HullName}", e);
            }
        }
        object? ISaveLoadListener.SaveData()
        {
            EnergyMixin thisEM = GetComponent<EnergyMixin>();
            if (thisEM.batterySlot.storedItem == null)
            {
                batteryData emptyBattery = new(TechType.None.AsString(), 0);
                return emptyBattery;
            }
            else
            {
                TechType thisTT = thisEM.batterySlot.storedItem.item.GetTechType();
                float thisEnergy = thisEM.battery.charge;
                batteryData thisBattery = new(thisTT.AsString(), thisEnergy);
                return thisBattery;
            }
        }
        void ISaveLoadListener.LoadData(JToken? data)
        {
            if (data == null) return;
            if (data is not JObject _)
                throw new Newtonsoft.Json.JsonException("Expected a JSON object for Tuple<string,float>.");
            batteryData? loadData = data.ToObject<batteryData>();
            if (loadData != null)
            {
                Admin.SessionManager.StartCoroutine(LoadBattery(loadData));
            }
        }
    }
}

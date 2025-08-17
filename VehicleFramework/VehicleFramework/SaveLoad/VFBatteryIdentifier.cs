using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VehicleFramework.SaveLoad
{
    internal class VFBatteryIdentifier : MonoBehaviour, IProtoTreeEventListener
    {
        internal ModVehicle MV => GetComponentInParent<ModVehicle>();
        const string saveFileNameSuffix = "battery";
        private string SaveFileName => SaveLoadUtils.GetSaveFileName(MV.transform, transform, saveFileNameSuffix);

        void IProtoTreeEventListener.OnProtoSerializeObjectTree(ProtobufSerializer serializer)
        {
            EnergyMixin thisEM = GetComponent<EnergyMixin>();
            if (thisEM.batterySlot.storedItem == null)
            {
                Tuple<TechType, float> emptyBattery = new(0, 0);
                MV.SaveBatteryData(SaveFileName, emptyBattery);
            }
            else
            {
                TechType thisTT = thisEM.batterySlot.storedItem.item.GetTechType();
                float thisEnergy = thisEM.battery.charge;
                Tuple<TechType, float> thisBattery = new(thisTT, thisEnergy);
                MV.SaveBatteryData(SaveFileName, thisBattery);
            }
        }
        void IProtoTreeEventListener.OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
        {
            Admin.Utils.StartCoroutine(LoadBattery());
        }
        private IEnumerator LoadBattery()
        {
            yield return new WaitUntil(() => MV != null);
            var thisBattery = MV.ReadBatteryData(SaveFileName);
            if (thisBattery == default)
            {
                thisBattery = SaveLoad.JsonInterface.Read<Tuple<TechType, float>>(MV, SaveFileName);
            }
            if (thisBattery == default || thisBattery.Item1 == TechType.None)
            {
                yield break;
            }
            TaskResult<GameObject> result = new();
            yield return CraftData.InstantiateFromPrefabAsync(thisBattery.Item1, result, false);
            GameObject thisItem = result.Get();
            try
            {
                thisItem.GetComponent<Battery>().charge = thisBattery.Item2;
                thisItem.transform.SetParent(MV.StorageRootObject.transform);
                GetComponent<EnergyMixin>().battery = thisItem.GetComponent<Battery>();
                GetComponent<EnergyMixin>().batterySlot.AddItem(thisItem.GetComponent<Pickupable>());
                thisItem.SetActive(false);
            }
            catch (Exception e)
            {
                Logger.LogException($"Failed to load battery : {thisBattery.Item1} for {MV.name} on GameObject {gameObject.name} : {MV.subName.hullName.text}", e);
            }
        }
    }
}

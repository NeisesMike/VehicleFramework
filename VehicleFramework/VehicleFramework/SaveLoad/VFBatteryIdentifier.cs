using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using techTypeString = System.String;

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
                Tuple<techTypeString, float> emptyBattery = new(TechType.None.AsString(), 0);
                MV.SaveBatteryData(SaveFileName, emptyBattery);
            }
            else
            {
                TechType thisTT = thisEM.batterySlot.storedItem.item.GetTechType();
                float thisEnergy = thisEM.battery.charge;
                Tuple<techTypeString, float> thisBattery = new(thisTT.AsString(), thisEnergy);
                MV.SaveBatteryData(SaveFileName, thisBattery);
            }
        }
        void IProtoTreeEventListener.OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
        {
            Admin.SessionManager.StartCoroutine(LoadBattery());
        }
        private IEnumerator LoadBattery()
        {
            yield return new WaitUntil(() => MV != null);
            var thisBattery = MV.ReadBatteryData(SaveFileName);
            if (thisBattery == default)
            {
                thisBattery = SaveLoad.JsonInterface.Read<Tuple<techTypeString, float>>(MV, SaveFileName);
            }
            if (thisBattery == default || string.Equals(thisBattery.Item1, TechType.None.AsString()))
            {
                yield break;
            }
            TaskResult<GameObject> result = new();
            TechTypeExtensions.FromString(thisBattery.Item1, out TechType techType, true);
            yield return CraftData.InstantiateFromPrefabAsync(techType, result, false);
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
                Logger.LogException($"Failed to load battery : {thisBattery.Item1} for {MV.name} on GameObject {gameObject.name} : {MV.HullName}", e);
            }
        }
    }
}

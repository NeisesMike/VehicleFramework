using System;
using System.Collections;
using UnityEngine;

namespace VehicleFramework.SaveLoad
{
    internal class VFBatteryIdentifier : MonoBehaviour, IProtoTreeEventListener
    {
        internal ModVehicle mv => GetComponentInParent<ModVehicle>();
        const string saveFileNameSuffix = "battery";
        private string SaveFileName => SaveLoadUtils.GetSaveFileName(mv.transform, transform, saveFileNameSuffix);

        void IProtoTreeEventListener.OnProtoSerializeObjectTree(ProtobufSerializer serializer)
        {
            EnergyMixin thisEM = GetComponent<EnergyMixin>();
            if (thisEM.batterySlot.storedItem == null)
            {
                var emptyBattery = new Tuple<TechType, float>(0, 0);
                SaveLoad.JsonInterface.Write<Tuple<TechType, float>>(mv, SaveFileName, emptyBattery);
            }
            else
            {
                TechType thisTT = thisEM.batterySlot.storedItem.item.GetTechType();
                float thisEnergy = thisEM.battery.charge;
                var thisBattery = new Tuple<TechType, float>(thisTT, thisEnergy);
                SaveLoad.JsonInterface.Write<Tuple<TechType, float>>(mv, SaveFileName, thisBattery);
            }
        }
        void IProtoTreeEventListener.OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
        {
            UWE.CoroutineHost.StartCoroutine(LoadBattery());
        }
        private IEnumerator LoadBattery()
        {
            yield return new WaitUntil(() => mv != null);
            var thisBattery = SaveLoad.JsonInterface.Read<Tuple<TechType, float>>(mv, SaveFileName);
            TaskResult<GameObject> result = new TaskResult<GameObject>();
            yield return CraftData.InstantiateFromPrefabAsync(thisBattery.Item1, result, false);
            GameObject thisItem = result.Get();
            try
            {
                thisItem.GetComponent<Battery>().charge = thisBattery.Item2;
                thisItem.transform.SetParent(mv.StorageRootObject.transform);
                GetComponent<EnergyMixin>().battery = thisItem.GetComponent<Battery>();
                GetComponent<EnergyMixin>().batterySlot.AddItem(thisItem.GetComponent<Pickupable>());
                thisItem.SetActive(false);
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to load battery : {thisBattery.Item1} for {mv.name} on GameObject {gameObject.name} : {mv.subName.hullName.text}");
                Logger.Log(e.Message);
            }
        }
    }
}

﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace VehicleFramework.SaveLoad
{
    internal class VFInnateStorageIdentifier : MonoBehaviour, IProtoTreeEventListener
    {
        internal ModVehicle mv => GetComponentInParent<ModVehicle>();
        const string saveFileNameSuffix = "innatestorage";
        private string SaveFileName => SaveLoadUtils.GetSaveFileName(mv.transform, transform, saveFileNameSuffix);

        void IProtoTreeEventListener.OnProtoSerializeObjectTree(ProtobufSerializer serializer)
        {
            InnateStorageContainer container = GetComponent<InnateStorageContainer>();
            List<Tuple<TechType, float, TechType>> result = new List<Tuple<TechType, float, TechType>>();
            foreach (var item in container.container.ToList())
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
                result.Add(new Tuple<TechType, float, TechType>(thisItemType, batteryChargeIfApplicable, innerBatteryTT));
            }
            JsonInterface.Write(mv, SaveFileName, result);
        }
        void IProtoTreeEventListener.OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
        {
            UWE.CoroutineHost.StartCoroutine(LoadInnateStorage());
        }
        private IEnumerator LoadInnateStorage()
        {
            yield return new WaitUntil(() => mv != null);
            var thisStorage = SaveLoad.JsonInterface.Read<List<Tuple<TechType, float, TechType>>>(mv, SaveFileName);
            if (thisStorage == default)
            {
                yield break;
            }
            TaskResult<GameObject> result = new TaskResult<GameObject>();
            foreach (var item in thisStorage)
            {
                yield return CraftData.InstantiateFromPrefabAsync(item.Item1, result, false);
                GameObject thisItem = result.Get();

                thisItem.transform.SetParent(mv.StorageRootObject.transform);
                try
                {
                    GetComponent<InnateStorageContainer>().container.AddItem(thisItem.GetComponent<Pickupable>());
                }
                catch (Exception e)
                {
                    Logger.Error($"Failed to add storage item {thisItem.name} to innate storage on GameObject {gameObject.name} for {mv.name} : {mv.subName.hullName.text}");
                    Logger.Log(e.Message);
                    Logger.Log(e.StackTrace);
                }
                thisItem.SetActive(false);
                if (item.Item2 >= 0)
                {
                    // then we have a battery xor we are a battery
                    try
                    {
                        UWE.CoroutineHost.StartCoroutine(SaveLoadUtils.ReloadBatteryPower(thisItem, item.Item2, item.Item3));
                    }
                    catch(Exception e)
                    {
                        Logger.Error($"Failed to reload battery power for innate storage item {thisItem.name} in innate storage on GameObject {gameObject.name} for {mv.name} : {mv.subName.hullName.text}");
                        Logger.Log(e.Message);
                        Logger.Log(e.StackTrace);
                    }
                }
            }
        }

        
    }
}

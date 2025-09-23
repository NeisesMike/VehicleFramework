using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using VehicleFramework.StorageComponents;
using techTypeString = System.String;

namespace VehicleFramework.SaveLoad
{
    internal class VFInnateStorageIdentifier : MonoBehaviour, IProtoTreeEventListener
    {
        internal ModVehicle MV => GetComponentInParent<ModVehicle>();
        const string saveFileNameSuffix = "innatestorage";
        private string SaveFileName => SaveLoadUtils.GetSaveFileName(MV.transform, transform, saveFileNameSuffix);

        void IProtoTreeEventListener.OnProtoSerializeObjectTree(ProtobufSerializer serializer)
        {
            InnateStorageContainer container = GetComponent<InnateStorageContainer>();
            List<Tuple<techTypeString, float, techTypeString>> result = new();
            foreach (var item in container.Container.ToList())
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
            MV.SaveInnateStorage(SaveFileName, result);
        }
        void IProtoTreeEventListener.OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
        {
            Admin.SessionManager.StartCoroutine(LoadInnateStorage());
        }
        private IEnumerator LoadInnateStorage()
        {
            yield return new WaitUntil(() => MV != null);

            var thisStorage = MV.ReadInnateStorage(SaveFileName);
            if (thisStorage == default)
            {
                thisStorage = SaveLoad.JsonInterface.Read<List<Tuple<techTypeString, float, techTypeString>>>(MV, SaveFileName);
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

                thisItem.transform.SetParent(MV.StorageRootObject.transform);
                try
                {
                    GetComponent<InnateStorageContainer>().Container.AddItem(thisItem.EnsureComponent<Pickupable>());
                }
                catch (Exception e)
                {
                    Logger.LogException($"Failed to add storage item {thisItem.name} to innate storage on GameObject {gameObject.name} for {MV.name} : {MV.subName.hullName.text}", e);
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
                        Logger.LogException($"Failed to reload battery power for innate storage item {thisItem.name} in innate storage on GameObject {gameObject.name} for {MV.name} : {MV.subName.hullName.text}", e);
                    }
                }
            }
        }

        
    }
}

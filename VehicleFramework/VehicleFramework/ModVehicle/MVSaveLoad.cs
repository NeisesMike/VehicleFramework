using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using VehicleFramework.Engines;
using VehicleFramework.VehicleComponents;
using VehicleFramework.Assets;
using VehicleFramework.Admin;
using VehicleFramework.VehicleBuilding;
using VehicleFramework.LightControllers;
using VehicleFramework.StorageComponents;
using VehicleFramework.Interfaces;
using VehicleFramework.Extensions;
using VehicleFramework.VehicleTypes;
using techTypeString = System.String;
using VehicleFramework.SaveLoad;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Linq;

namespace VehicleFramework
{
    /*
     * ModVehicle is the primary abstract class provided by Vehicle Framework.
     * All VF vehicles inherit from ModVehicle.
     * This file contains the save/load functionality for ModVehicle.
     */
    public abstract partial class ModVehicle : Vehicle, ICraftTarget, IProtoTreeEventListener
    {
        private const string isControlling = "isControlling";
        private const string isInside = "isInside";
        private const string mySubName = "SubName";
        private const string baseColorName = "BaseColor";
        private const string interiorColorName = "InteriorColor";
        private const string stripeColorName = "StripeColor";
        private const string nameColorName = "NameColor";
        private const string defaultColorName = "DefaultColor";
        private const string SimpleDataSaveFileName = "SimpleData";
        internal Dictionary<string, string> SaveSimpleData()
        {
            return new()
            {
                { isControlling, IsPlayerControlling() ? bool.TrueString : bool.FalseString },
                { isInside, IsUnderCommand ? bool.TrueString : bool.FalseString },
                { mySubName, HullName },
                { baseColorName, $"#{ColorUtility.ToHtmlStringRGB(baseColor)}" },
                { interiorColorName, $"#{ColorUtility.ToHtmlStringRGB(interiorColor)}" },
                { stripeColorName, $"#{ColorUtility.ToHtmlStringRGB(stripeColor)}" },
                { nameColorName, $"#{ColorUtility.ToHtmlStringRGB(nameColor)}" },
                { defaultColorName, IsDefaultStyle ? bool.TrueString : bool.FalseString }
            };
        }
        internal IEnumerator LoadSimpleData(Dictionary<string, string> simpleData)
        {
            //var simpleData = SaveLoad.JsonInterface.Read<Dictionary<string, string>>(this, SimpleDataSaveFileName);
            if (simpleData == null || simpleData.Count == 0)
            {
                yield break;
            }
            if (bool.Parse(simpleData[isInside]))
            {
                if(this as Drone == null)
                {
                    PlayerEntry();
                }
            }
            if (bool.Parse(simpleData[isControlling]))
            {
                Drone? drone = this as Drone;
                if (drone == null)
                {
                    BeginPiloting();
                }
                else
                {
                    drone.BeginControlling();
                }
            }
            if (bool.Parse(simpleData[defaultColorName]))
            {
                SetVehicleDefaultStyle(simpleData[mySubName]);
                yield break;
            }
            SetName(simpleData[mySubName]);
            if (ColorUtility.TryParseHtmlString(simpleData[baseColorName], out Color iBaseColor))
            {
                SetBaseColor(iBaseColor);
            }
            if (ColorUtility.TryParseHtmlString(simpleData[nameColorName], out Color iNameColor))
            {
                SetName(simpleData[mySubName], iNameColor);
            }
            if (ColorUtility.TryParseHtmlString(simpleData[interiorColorName], out Color iInteriorColor))
            {
                SetInteriorColor(iInteriorColor);
            }
            if (ColorUtility.TryParseHtmlString(simpleData[stripeColorName], out Color iStripeColor))
            {
                SetStripeColor(iStripeColor);
            }
        }
        void IProtoTreeEventListener.OnProtoSerializeObjectTree(ProtobufSerializer serializer)
        {
            gameObject.EnsureComponent<VFSimpleSaveLoad>();
            try
            {
                SaveEverything();
            }
            catch(Exception e)
            {
                Logger.LogException($"Failed to save simple data for ModVehicle {HullName}", e);
            }
            try
            {
                OnGameSaved();
            }
            catch (Exception e)
            {
                Logger.LogException($"Failed OnGameSaved(); for ModVehicle {HullName}", e);
            }
        }
        void IProtoTreeEventListener.OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
        {
            gameObject.EnsureComponent<VFSimpleSaveLoad>();
            try
            {
                LoadEverything();
            }
            catch(Exception e)
            {
                Logger.LogException($"Failed to load simple data for ModVehicle {HullName}", e);
            }
            try
            {
                OnGameLoaded();
            }
            catch (Exception e)
            {
                Logger.LogException($"Failed OnGameLoaded(); for ModVehicle {HullName}", e);
            }
        }
        protected virtual void OnGameSaved() { }
        protected virtual void OnGameLoaded() { }

        #region SaveLoad
        private const string SaveFileSuffix = "data";
        private void SaveEverything()
        {
            Dictionary<string, object> PendingSaveData = new();
            var saveLoadlisteners = GetComponentsInChildren<ISaveLoadListener>();
            foreach (var saveLoader in saveLoadlisteners)
            {
                object? saveLoaderData = saveLoader.SaveData();
                if (saveLoaderData == null) continue;
                if (JsonInterface.IsJsonSerializable(saveLoaderData) == false)
                {
                    Logger.Warn($"SaveLoadListener on {saveLoader.SaveDataKey} returned data that is not JSON serializable! Data type: {saveLoaderData.GetType()}");
                    continue;
                }
                PendingSaveData.Add(saveLoader.SaveDataKey, saveLoaderData);
            }
            JsonInterface.Write<Dictionary<string, object>>(this, SaveFileSuffix, PendingSaveData);
        }
        private void LoadEverything()
        {
            Admin.SessionManager.StartCoroutine(WaitForListeners());
        }
        private IEnumerator WaitForListeners()
        {
            ReadOnlyDictionary<string, JToken>? PendingLoadData = JsonInterface.Read<ReadOnlyDictionary<string, JToken>>(this, SaveFileSuffix);
            if (PendingLoadData == null)
            {
                Logger.Log("No save data found to load!");
                yield break;
            }
            ModularStorages?.ForEach(x => x.Container.EnsureComponent<VFStorageIdentifier>()); //Normally added by ModularStorageInput, who might not have awaked yet.
            GetComponentsInChildren<ISaveLoadListener>(true).ForEach(x => Admin.SessionManager.StartCoroutine(WaitForListener(PendingLoadData, x)));
        }
        private static IEnumerator WaitForListener(ReadOnlyDictionary<string, JToken> pendingLoadData, ISaveLoadListener listener)
        {
            yield return new WaitUntil(listener.IsReady);
            //Logger.Log($"Loading data for {HullName} : {loadDatum.Key}");
            pendingLoadData.Where(x=> x.Key == listener.SaveDataKey).ForEach(x => listener.LoadData(x.Value));
        }
        #endregion
    }
}

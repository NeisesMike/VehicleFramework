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
        private void SaveSimpleData()
        {
            Dictionary<string, string> simpleData = new()
            {
                { isControlling, IsPlayerControlling() ? bool.TrueString : bool.FalseString },
                { isInside, IsUnderCommand ? bool.TrueString : bool.FalseString },
                { mySubName, subName.hullName.text },
                { baseColorName, $"#{ColorUtility.ToHtmlStringRGB(baseColor)}" },
                { interiorColorName, $"#{ColorUtility.ToHtmlStringRGB(interiorColor)}" },
                { stripeColorName, $"#{ColorUtility.ToHtmlStringRGB(stripeColor)}" },
                { nameColorName, $"#{ColorUtility.ToHtmlStringRGB(nameColor)}" },
                { defaultColorName, this is Submarine sub && sub.IsDefaultTexture ? bool.TrueString : bool.FalseString }
            };
            SaveLoad.JsonInterface.Write(this, SimpleDataSaveFileName, simpleData);
        }
        private IEnumerator LoadSimpleData()
        {
            // Need to handle some things specially here for Submarines
            // Because Submarines had color changing before I knew how to integrate with the Moonpool
            // The new color changing methods are much simpler, but Odyssey and Beluga use the old methods,
            // So I'll still support them.
            yield return new WaitUntil(() => GameStateWatcher.IsWorldLoaded);
            yield return new WaitUntil(() => isInitialized);
            var simpleData = SaveLoad.JsonInterface.Read<Dictionary<string, string>>(this, SimpleDataSaveFileName);
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
            SetName(simpleData[mySubName]);
            Submarine? sub = this as Submarine;
            sub?.PaintVehicleDefaultStyle(simpleData[mySubName]);
            if (bool.Parse(simpleData[defaultColorName]))
            {
                yield break;
            }
            if (ColorUtility.TryParseHtmlString(simpleData[baseColorName], out baseColor))
            {
                subName.SetColor(0, Vector3.zero, baseColor);
                sub?.PaintVehicleName(simpleData[mySubName], Color.black, baseColor);
            }
            if (ColorUtility.TryParseHtmlString(simpleData[nameColorName], out nameColor))
            {
                subName.SetColor(1, Vector3.zero, nameColor);
                sub?.PaintVehicleName(simpleData[mySubName], nameColor, baseColor);
            }
            if (ColorUtility.TryParseHtmlString(simpleData[interiorColorName], out interiorColor))
            {
                subName.SetColor(2, Vector3.zero, interiorColor);
            }
            if (ColorUtility.TryParseHtmlString(simpleData[stripeColorName], out stripeColor))
            {
                subName.SetColor(3, Vector3.zero, stripeColor);
            }
        }
        void IProtoTreeEventListener.OnProtoSerializeObjectTree(ProtobufSerializer serializer)
        {
            try
            {
                SaveSimpleData();
                SaveLoad.VFModularStorageSaveLoad.SerializeAllModularStorage(this);
            }
            catch(Exception e)
            {
                Logger.LogException($"Failed to save simple data for ModVehicle {name}", e);
            }
            OnGameSaved();
        }
        void IProtoTreeEventListener.OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
        {
            SessionManager.StartCoroutine(LoadSimpleData());
            SessionManager.StartCoroutine(SaveLoad.VFModularStorageSaveLoad.DeserializeAllModularStorage(this));
            OnGameLoaded();
        }
        protected virtual void OnGameSaved() { }
        protected virtual void OnGameLoaded() { }

        private const string StorageSaveName = "Storage";
        private Dictionary<string, List<Tuple<techTypeString, float, techTypeString>>>? loadedStorageData = null;
        private readonly Dictionary<string, List<Tuple<techTypeString, float, techTypeString>>> innateStorageSaveData = new();
        internal void SaveInnateStorage(string path, List<Tuple<techTypeString, float, techTypeString>> storageData)
        {
            if(InnateStorages == null)
            {
                return;
            }
            innateStorageSaveData.Add(path, storageData);
            if(innateStorageSaveData.Count == InnateStorages.Count)
            {
                // write it out
                SaveLoad.JsonInterface.Write(this, StorageSaveName, innateStorageSaveData);
                innateStorageSaveData.Clear();
            }
        }
        internal List<Tuple<techTypeString, float, techTypeString>>? ReadInnateStorage(string path)
        {
            loadedStorageData ??= SaveLoad.JsonInterface.Read<Dictionary<string, List<Tuple<techTypeString, float, techTypeString>>>>(this, StorageSaveName);
            if (loadedStorageData == null)
            {
                return default;
            }
            if (loadedStorageData.TryGetValue(path, out List<Tuple<techTypeString, float, techTypeString>>? value))
            {
                return value;
            }
            else
            {
                return default;
            }
        }

        private const string BatterySaveName = "Batteries";
        private Dictionary<string, Tuple<techTypeString, float>>? loadedBatteryData = null;
        private readonly Dictionary<string, Tuple<techTypeString, float>> batterySaveData = new();
        internal void SaveBatteryData(string path, Tuple<techTypeString, float> batteryData)
        {
            int batteryCount = 0;
            if (Batteries != null) batteryCount += Batteries.Count;

            batterySaveData.Add(path, batteryData);
            if (batterySaveData.Count == batteryCount)
            {
                // write it out
                SaveLoad.JsonInterface.Write(this, BatterySaveName, batterySaveData);
                batterySaveData.Clear();
            }
        }
        internal Tuple<techTypeString, float>? ReadBatteryData(string path)
        {
            loadedBatteryData ??= SaveLoad.JsonInterface.Read<Dictionary<string, Tuple<techTypeString, float>>>(this, BatterySaveName);
            if (loadedBatteryData == null)
            {
                return default;
            }
            if (loadedBatteryData.TryGetValue(path, out Tuple<techTypeString, float>? value))
            {
                return value;
            }
            else
            {
                return default;
            }
        }
    }
}

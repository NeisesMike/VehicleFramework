using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VehicleFramework.Interfaces;
using techTypeString = System.String;
using upgradesData = System.Collections.Generic.Dictionary<System.String, System.String>;
using Newtonsoft.Json.Linq;

namespace VehicleFramework.SaveLoad
{
    internal class VFUpgradesIdentifier : MonoBehaviour, ISaveLoadListener
    {
        internal bool isFinished = false;
        internal ModVehicle MV => GetComponentInParent<ModVehicle>();
        bool ISaveLoadListener.IsReady()
        {
            return MV != null;
        }
        private IEnumerator LoadUpgrades(upgradesData datum)
        {
            if (datum == default)
            {
                isFinished = true;
                yield break;
            }
            yield return new WaitUntil(() => MV != null);
            yield return new WaitUntil(() => MV.upgradesInput.equipment != null);
            MV.UnlockDefaultModuleSlots();
            foreach (var upgrade in datum.Where(x => !string.Equals(x.Value, TechType.None.AsString())))
            {
                TaskResult<GameObject> result = new();
                TechTypeExtensions.FromString(upgrade.Value, out TechType techType, true);
                yield return CraftData.InstantiateFromPrefabAsync(techType, result, false);
                try
                {
                    GameObject thisUpgrade = result.Get();
                    thisUpgrade.transform.SetParent(MV.modulesRoot.transform);
                    thisUpgrade.SetActive(false);
                    InventoryItem thisItem = new(thisUpgrade.GetComponent<Pickupable>());
                    MV.modules.AddItem(upgrade.Key, thisItem, true);
                }
                catch (Exception e)
                {
                    Logger.LogException($"Failed to load upgrade {upgrade.Value} in slot {upgrade.Key} for {MV.name} : {MV.HullName}", e);
                    continue;
                }
            }
            isFinished = true;
        }
        string ISaveLoadListener.SaveDataKey => "CoreUpgrades";
        object? ISaveLoadListener.SaveData()
        {
            Dictionary<string, InventoryItem> upgradeList = MV.modules.equipment;
            if (upgradeList == null)
            {
                return null;
            }
            upgradesData result = new();
            upgradeList.ForEach(x => result.Add(x.Key, x.Value?.techType.AsString() ?? TechType.None.AsString()));
            return result;
        }
        void ISaveLoadListener.LoadData(JToken? data)
        {
            if (data == null) return;
            if (data is not JObject _)
                throw new Newtonsoft.Json.JsonException("Expected a JSON object for Dictionary<string,string>.");
            upgradesData? loadData = data.ToObject<upgradesData>();
            if (loadData != null)
            {
                Admin.SessionManager.StartCoroutine(LoadUpgrades(loadData));
            }
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VehicleFramework.SaveLoad
{
    internal class VFUpgradesIdentifier : MonoBehaviour, IProtoTreeEventListener
    {
        internal bool isFinished = false;
        internal ModVehicle MV => GetComponentInParent<ModVehicle>();
        const string saveFileNameSuffix = "upgrades";
        private string SaveFileName => SaveLoadUtils.GetSaveFileName(MV.transform, transform, saveFileNameSuffix);
        private const string NewSaveFileName = "Upgrades";
        void IProtoTreeEventListener.OnProtoSerializeObjectTree(ProtobufSerializer serializer)
        {
            Dictionary<string, InventoryItem> upgradeList = MV.modules.equipment;
            if (upgradeList == null)
            {
                return;
            }
            Dictionary<string, TechType> result = new();
            upgradeList.ForEach(x => result.Add(x.Key, x.Value?.techType ?? TechType.None));
            SaveLoad.JsonInterface.Write<Dictionary<string, TechType>>(MV, NewSaveFileName, result);
        }
        void IProtoTreeEventListener.OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
        {
            Admin.SessionManager.StartCoroutine(LoadUpgrades());
        }
        private IEnumerator LoadUpgrades()
        {
            yield return new WaitUntil(() => MV != null);
            yield return new WaitUntil(() => MV.upgradesInput.equipment != null);
            MV.UnlockDefaultModuleSlots();
            var theseUpgrades = SaveLoad.JsonInterface.Read<Dictionary<string, TechType>>(MV, NewSaveFileName);
            if(theseUpgrades == default)
            {
                theseUpgrades = SaveLoad.JsonInterface.Read<Dictionary<string, TechType>>(MV, SaveFileName);
                if (theseUpgrades == default)
                {
                    isFinished = true;
                    yield break;
                }
            }
            foreach(var upgrade in theseUpgrades.Where(x=>x.Value != TechType.None))
            {
                TaskResult<GameObject> result = new();
                yield return CraftData.InstantiateFromPrefabAsync(upgrade.Value, result, false);
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
                    Logger.LogException($"Failed to load upgrade {upgrade.Value} in slot {upgrade.Key} for {MV.name} : {MV.subName.hullName.text}", e);
                    continue;
                }
            }
            isFinished = true;
        }
    }
}

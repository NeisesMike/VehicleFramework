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
        internal ModVehicle mv => GetComponentInParent<ModVehicle>();
        const string saveFileNameSuffix = "upgrades";
        private string SaveFileName => SaveLoadUtils.GetSaveFileName(mv.transform, transform, saveFileNameSuffix);
        private const string NewSaveFileName = "Upgrades";
        void IProtoTreeEventListener.OnProtoSerializeObjectTree(ProtobufSerializer serializer)
        {
            Dictionary<string, InventoryItem> upgradeList = mv.modules?.equipment;
            if (upgradeList == null)
            {
                return;
            }
            Dictionary<string, TechType> result = new Dictionary<string, TechType>();
            upgradeList.ForEach(x => result.Add(x.Key, x.Value?.techType ?? TechType.None));
            SaveLoad.JsonInterface.Write<Dictionary<string, TechType>>(mv, NewSaveFileName, result);
        }
        void IProtoTreeEventListener.OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
        {
            MainPatcher.Instance.StartCoroutine(LoadUpgrades());
        }
        private IEnumerator LoadUpgrades()
        {
            yield return new WaitUntil(() => mv != null);
            yield return new WaitUntil(() => mv.upgradesInput.equipment != null);
            mv.UnlockDefaultModuleSlots();
            var theseUpgrades = SaveLoad.JsonInterface.Read<Dictionary<string, TechType>>(mv, NewSaveFileName);
            if(theseUpgrades == default)
            {
                theseUpgrades = SaveLoad.JsonInterface.Read<Dictionary<string, TechType>>(mv, SaveFileName);
                if (theseUpgrades == default)
                {
                    isFinished = true;
                    yield break;
                }
            }
            foreach(var upgrade in theseUpgrades.Where(x=>x.Value != TechType.None))
            {
                TaskResult<GameObject> result = new TaskResult<GameObject>();
                yield return CraftData.InstantiateFromPrefabAsync(upgrade.Value, result, false);
                try
                {
                    GameObject thisUpgrade = result.Get();
                    thisUpgrade.transform.SetParent(mv.modulesRoot.transform);
                    thisUpgrade.SetActive(false);
                    InventoryItem thisItem = new InventoryItem(thisUpgrade.GetComponent<Pickupable>());
                    mv.modules.AddItem(upgrade.Key, thisItem, true);
                }
                catch (Exception e)
                {
                    Logger.LogException($"Failed to load upgrade {upgrade.Value} in slot {upgrade.Key} for {mv.name} : {mv.subName.hullName.text}", e);
                    continue;
                }
            }
            isFinished = true;
        }
    }
}

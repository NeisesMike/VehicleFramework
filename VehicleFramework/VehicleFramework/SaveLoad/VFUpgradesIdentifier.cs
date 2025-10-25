using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using techTypeString = System.String;

namespace VehicleFramework.SaveLoad
{
    internal class VFUpgradesIdentifier : MonoBehaviour, IProtoTreeEventListener
    {
        internal bool isFinished = false;
        internal ModVehicle MV => GetComponentInParent<ModVehicle>();
        private const string SaveFileTitle = "Upgrades";
        void IProtoTreeEventListener.OnProtoSerializeObjectTree(ProtobufSerializer serializer)
        {
            Dictionary<string, InventoryItem> upgradeList = MV.modules.equipment;
            if (upgradeList == null)
            {
                return;
            }
            Dictionary<string, techTypeString> result = new();
            upgradeList.ForEach(x => result.Add(x.Key, x.Value?.techType.AsString() ?? TechType.None.AsString()));
            SaveLoad.JsonInterface.Write<Dictionary<string, techTypeString>>(MV, SaveFileTitle, result);
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
            var theseUpgrades = SaveLoad.JsonInterface.Read<Dictionary<string, techTypeString>>(MV, SaveFileTitle);
            if(theseUpgrades == default)
            {
                isFinished = true;
                yield break;
            }
            foreach (var upgrade in theseUpgrades.Where(x => !string.Equals(x.Value, TechType.None.AsString())))
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
    }
}

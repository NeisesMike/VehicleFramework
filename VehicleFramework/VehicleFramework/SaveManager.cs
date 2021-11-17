using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using batteries = System.Collections.Generic.List<System.Tuple<TechType, float>>;

namespace VehicleFramework
{
    public static class SaveManager
    {
        internal static List<Tuple<Vector3, Dictionary<string, TechType>>> SerializeUpgrades()
        {
            List<Tuple<Vector3, Dictionary<string, TechType>>> modVehiclesUpgrades = new List<Tuple<Vector3, Dictionary<string, TechType>>>();
            foreach (ModVehicle mv in VehicleManager.VehiclesInPlay)
            {
                if (mv == null)
                {
                    continue;
                }
                if (!mv.name.Contains("Clone"))
                {
                    // skip the prefabs
                    continue;
                }
                Dictionary<string, TechType> equipmentStrings = new Dictionary<string, TechType>();
                foreach (KeyValuePair<string, InventoryItem> pair in mv.modules.equipment)
                {
                    if (pair.Value != null && pair.Value.item != null && pair.Value.item.name != null)
                    {
                        string thisName = pair.Value.item.name;
                        int cloneIndex = thisName.IndexOf("(Clone)");
                        if (cloneIndex != -1)
                        {
                            thisName = thisName.Remove(cloneIndex, 7);
                        }
                        equipmentStrings.Add(pair.Key, pair.Value.item.GetTechType());
                    }
                }
                Tuple<Vector3, Dictionary<string, TechType>> thisTuple = new Tuple<Vector3, Dictionary<string, TechType>>(mv.transform.position, equipmentStrings);
                modVehiclesUpgrades.Add(thisTuple);
            }
            return modVehiclesUpgrades;
        }
        internal static void DeserializeUpgrades(SaveData data)
        {
            List<Tuple<Vector3, Dictionary<string, TechType>>> modVehiclesUpgrades = data.UpgradeLists;
            foreach (ModVehicle mv in VehicleManager.VehiclesInPlay)
            {
                if (mv==null)
                {
                    continue;
                }
                // try to match against a saved vehicle in our list
                foreach (var tup in modVehiclesUpgrades)
                {
                    if (Vector3.Distance(mv.transform.position, tup.Item1) < 3)
                    {
                        foreach(KeyValuePair<string, TechType> pair in tup.Item2)
                        {
                            GameObject thisUpgrade = GameObject.Instantiate(CraftData.GetPrefabForTechType(pair.Value, true));
                            thisUpgrade.transform.SetParent(mv.modulesRoot.transform);
                            thisUpgrade.SetActive(false);
                            InventoryItem thisItem = new InventoryItem(thisUpgrade.GetComponent<Pickupable>());
                            mv.modules.AddItem(pair.Key, thisItem, true);
                            // try calling OnUpgradeModulesChanged now
                            mv.UpdateModuleSlots();
                        }
                    }
                }
            }
        }
        internal static List<Tuple<Vector3, List<Tuple<int, batteries>>>> SerializeModularStorage()
        {
            List<Tuple<Vector3, List<Tuple<int, batteries>>>> allVehiclesStoragesContents = new List<Tuple<Vector3, List<Tuple<int, batteries>>>>();
            foreach (ModVehicle mv in VehicleManager.VehiclesInPlay)
            {
                if (mv == null)
                {
                    continue;
                }
                if (!mv.name.Contains("Clone"))
                {
                    // skip the prefabs
                    continue;
                }
                List<Tuple<int, batteries>> thisVehiclesStoragesContents = new List<Tuple<int, batteries>>();

                for(int i=0; i<mv.ModularStorages.Count; i++)
                {
                    var thisContainer = mv.GetStorageInSlot(i, TechType.VehicleStorageModule);
                    if (thisContainer != null)
                    {
                        batteries thisContents = new batteries();
                        foreach (var item in thisContainer.ToList())
                        {
                            TechType thisItemType = item.item.GetTechType();
                            float batteryChargeIfApplicable = -1;
                            var bat = item.item.GetComponentInChildren<Battery>(true);
                            if(bat != null)
                            {
                                batteryChargeIfApplicable = bat.charge;
                            }
                            thisContents.Add(new Tuple<TechType, float>(thisItemType, batteryChargeIfApplicable));
                        }
                        thisVehiclesStoragesContents.Add(new Tuple<int, batteries>(i, thisContents));
                    }
                }
                allVehiclesStoragesContents.Add(new Tuple<Vector3, List<Tuple<int, batteries>>>(mv.transform.position, thisVehiclesStoragesContents));
            }
            return allVehiclesStoragesContents;
        }
        internal static void DeserializeModularStorage(SaveData data)
        {
            List<Tuple<Vector3, List<Tuple<int, batteries>>>> allVehiclesStoragesLists = data.ModularStorages;
            foreach (ModVehicle mv in VehicleManager.VehiclesInPlay)
            {
                if (mv == null)
                {
                    continue;
                }
                // try to match against a saved vehicle in our list
                foreach (var vehicle in allVehiclesStoragesLists)
                {
                    if (Vector3.Distance(mv.transform.position, vehicle.Item1) < 3)
                    {
                        // we've matched the vehicle
                        foreach(var container in vehicle.Item2)
                        {
                            var thisContainer = mv.GetStorageInSlot(container.Item1, TechType.VehicleStorageModule);
                            if (thisContainer != null)
                            {
                                foreach(var techtype in container.Item2)
                                {
                                    GameObject thisItem = GameObject.Instantiate(CraftData.GetPrefabForTechType(techtype.Item1, true));
                                    if (techtype.Item2 >= 0)
                                    {
                                        // check whether we *are* a battery xor we *have* a battery
                                        if (thisItem.GetComponent<Battery>() != null)
                                        {
                                            // we are a battery
                                            var bat = thisItem.GetComponentInChildren<Battery>();
                                            bat.charge = techtype.Item2;
                                        }
                                        else
                                        {
                                            // we have a battery (we are a tool)
                                            // Thankfully we have this naming convention
                                            Transform batSlot = thisItem.transform.Find("BatterySlot");
                                            GameObject newBat = GameObject.Instantiate(CraftData.GetPrefabForTechType(TechType.Battery, true));
                                            newBat.GetComponent<Battery>().charge = techtype.Item2;
                                            newBat.transform.SetParent(batSlot);
                                            newBat.SetActive(false);
                                        }
                                    }
                                    thisItem.transform.SetParent(mv.StorageRootObject.transform);
                                    thisContainer.AddItem(thisItem.GetComponent<Pickupable>());
                                    thisItem.SetActive(false);
                                }
                            }
                            else
                            {
                                Logger.Log("Error: tried to deserialize items into a non-existent modular container: " + container.Item1.ToString());
                            }
                        }
                    }
                }
            }
        }
        internal static List<Tuple<Vector3, List<Tuple<Vector3, batteries>>>> SerializeInnateStorage()
        {
            List<Tuple<Vector3, List<Tuple<Vector3, batteries>>>> allVehiclesStoragesContents = new List<Tuple<Vector3, List<Tuple<Vector3, batteries>>>>();
            foreach (ModVehicle mv in VehicleManager.VehiclesInPlay)
            {
                if (mv == null)
                {
                    continue;
                }
                if (!mv.name.Contains("Clone"))
                {
                    // skip the prefabs
                    continue;
                }
                List<Tuple<Vector3, batteries>> thisVehiclesStoragesContents = new List<Tuple<Vector3, batteries>>();
                foreach (InnateStorageContainer vsc in mv.GetComponentsInChildren<InnateStorageContainer>())
                {
                    Vector3 thisLocalPos = vsc.transform.localPosition;
                    batteries thisContents = new batteries();
                    foreach (var item in vsc.container.ToList())
                    {
                        TechType thisItemType = item.item.GetTechType();
                        float batteryChargeIfApplicable = -1;
                        var bat = item.item.GetComponentInChildren<Battery>(true);
                        if (bat != null)
                        {
                            batteryChargeIfApplicable = bat.charge;
                        }
                        thisContents.Add(new Tuple<TechType, float>(thisItemType, batteryChargeIfApplicable));
                    }
                    thisVehiclesStoragesContents.Add(new Tuple<Vector3, batteries>(thisLocalPos, thisContents));
                }
                allVehiclesStoragesContents.Add(new Tuple<Vector3, List<Tuple<Vector3, batteries>>>(mv.transform.position, thisVehiclesStoragesContents));
            }
            return allVehiclesStoragesContents;
        }
        internal static void DeserializeInnateStorage(SaveData data)
        {
            List<Tuple<Vector3, List<Tuple<Vector3, batteries>>>> allVehiclesStoragesLists = data.InnateStorages;
            foreach (ModVehicle mv in VehicleManager.VehiclesInPlay)
            {
                if (mv == null)
                {
                    continue;
                }
                // try to match against a saved vehicle in our list
                foreach (var vehicle in allVehiclesStoragesLists)
                {
                    if (Vector3.Distance(mv.transform.position, vehicle.Item1) < 3)
                    {
                        foreach (var thisStorage in vehicle.Item2)
                        {
                            // load up the storages
                            foreach (var isc in mv.GetComponentsInChildren<InnateStorageContainer>())
                            {
                                if (isc.transform.localPosition == thisStorage.Item1)
                                {
                                    foreach (var techtype in thisStorage.Item2)
                                    {
                                        GameObject thisItem = GameObject.Instantiate(CraftData.GetPrefabForTechType(techtype.Item1, true));
                                        if (techtype.Item2 >= 0)
                                        {
                                            // check whether we *are* a battery xor we *have* a battery
                                            if(thisItem.GetComponent<Battery>() != null)
                                            {
                                                // we are a battery
                                                var bat = thisItem.GetComponentInChildren<Battery>();
                                                bat.charge = techtype.Item2;
                                            }
                                            else
                                            {
                                                // we have a battery (we are a tool)
                                                // Thankfully we have this naming convention
                                                Transform batSlot = thisItem.transform.Find("BatterySlot");
                                                GameObject newBat = GameObject.Instantiate(CraftData.GetPrefabForTechType(TechType.Battery, true));
                                                newBat.GetComponent<Battery>().charge = techtype.Item2;
                                                newBat.transform.SetParent(batSlot);
                                                newBat.SetActive(false);
                                            }
                                        }
                                        thisItem.transform.SetParent(mv.StorageRootObject.transform);
                                        isc.container.AddItem(thisItem.GetComponent<Pickupable>());
                                        thisItem.SetActive(false);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        internal static List<Tuple<Vector3, batteries>> SerializeBatteries()
        {
            List<Tuple<Vector3, batteries>> allVehiclesBatteries = new List<Tuple<Vector3, batteries>>();
            foreach (ModVehicle mv in VehicleManager.VehiclesInPlay)
            {
                if (mv == null)
                {
                    continue;
                }
                if (!mv.name.Contains("Clone"))
                {
                    // skip the prefabs
                    continue;
                }
                List<Tuple<TechType, float>> thisVehiclesBatteries = new List<Tuple<TechType, float>>();
                foreach (EnergyMixin batt in mv.energyInterface.sources)
                {
                    if (batt.battery != null)
                    {
                        thisVehiclesBatteries.Add(new Tuple<TechType,float>(batt.batterySlot.storedItem.item.GetTechType(), batt.battery.charge));
                    }
                }
                allVehiclesBatteries.Add(new Tuple<Vector3,batteries>(mv.transform.position, thisVehiclesBatteries));
            }
            return allVehiclesBatteries;
        }
        internal static void DeserializeBatteries(SaveData data)
        {
            List<Tuple<Vector3, batteries>> allVehiclesBatteries = data.Batteries;
            foreach (ModVehicle mv in VehicleManager.VehiclesInPlay)
            {
                if (mv == null)
                {
                    continue;
                }
                if (!mv.name.Contains("Clone"))
                {
                    // skip the prefabs
                    continue;
                }

                // try to match against a saved vehicle in our list
                foreach (var vehicle in allVehiclesBatteries)
                {
                    if (Vector3.Distance(mv.transform.position, vehicle.Item1) < 3)
                    {
                        foreach (var battery in vehicle.Item2.Select((value, i) => (value, i)))
                        {
                            GameObject thisItem = GameObject.Instantiate(CraftData.GetPrefabForTechType(battery.value.Item1, true));
                            thisItem.GetComponent<Battery>().charge = battery.value.Item2;
                            thisItem.transform.SetParent(mv.StorageRootObject.transform);
                            mv.Batteries[battery.i].BatterySlot.gameObject.GetComponent<EnergyMixin>().battery = thisItem.GetComponent<Battery>();
                            mv.Batteries[battery.i].BatterySlot.gameObject.GetComponent<EnergyMixin>().batterySlot.AddItem(thisItem.GetComponent<Pickupable>());
                            thisItem.SetActive(false);
                        }
                    }
                }
            }
        }
        internal static List<Tuple<Vector3, batteries>> SerializeBackupBatteries()
        {
            List<Tuple<Vector3, batteries>> allVehiclesBatteries = new List<Tuple<Vector3, batteries>>();
            foreach (ModVehicle mv in VehicleManager.VehiclesInPlay)
            {
                if (mv == null)
                {
                    continue;
                }
                if (!mv.name.Contains("Clone"))
                {
                    // skip the prefabs
                    continue;
                }
                List<Tuple<TechType, float>> thisVehiclesBatteries = new List<Tuple<TechType, float>>();
                foreach (EnergyMixin batt in mv.GetComponent<AutoPilot>().aiEI.sources)
                {
                    if (batt.battery != null)
                    {
                        thisVehiclesBatteries.Add(new Tuple<TechType, float>(batt.batterySlot.storedItem.item.GetTechType(), batt.battery.charge));
                    }
                }
                allVehiclesBatteries.Add(new Tuple<Vector3, batteries>(mv.transform.position, thisVehiclesBatteries));
            }
            return allVehiclesBatteries;
        }
        internal static void DeserializeBackupBatteries(SaveData data)
        {
            List<Tuple<Vector3, batteries>> allVehiclesBatteries = data.BackupBatteries;
            foreach (ModVehicle mv in VehicleManager.VehiclesInPlay)
            {
                if (mv == null)
                {
                    continue;
                }
                if (!mv.name.Contains("Clone"))
                {
                    // skip the prefabs
                    continue;
                }
                // try to match against a saved vehicle in our list
                foreach (var vehicle in allVehiclesBatteries)
                {
                    if (Vector3.Distance(mv.transform.position, vehicle.Item1) < 3)
                    {
                        foreach (var battery in vehicle.Item2.Select((value, i) => (value, i)))
                        {
                            GameObject thisItem = GameObject.Instantiate(CraftData.GetPrefabForTechType(battery.value.Item1, true));
                            thisItem.GetComponent<Battery>().charge = battery.value.Item2;
                            thisItem.transform.SetParent(mv.StorageRootObject.transform);
                            mv.BackupBatteries[battery.i].BatterySlot.gameObject.GetComponent<EnergyMixin>().battery = thisItem.GetComponent<Battery>();
                            mv.BackupBatteries[battery.i].BatterySlot.gameObject.GetComponent<EnergyMixin>().batterySlot.AddItem(thisItem.GetComponent<Pickupable>());
                            thisItem.SetActive(false);
                        }
                    }
                }
            }
        }
        internal static List<Tuple<Vector3, bool>> SerializePlayerInside()
        {
            List<Tuple<Vector3, bool>> allVehiclesIsPlayerInside = new List<Tuple<Vector3, bool>>();
            foreach (ModVehicle mv in VehicleManager.VehiclesInPlay)
            {
                if (mv == null)
                {
                    continue;
                }
                if (!mv.name.Contains("Clone"))
                {
                    // skip the prefabs
                    continue;
                }
                allVehiclesIsPlayerInside.Add(new Tuple<Vector3, bool>(mv.transform.position, mv.IsPlayerInside()));
            }
            return allVehiclesIsPlayerInside;
        }
        internal static void DeserializePlayerInside(SaveData data)
        {
            List<Tuple<Vector3, bool>> allVehiclesPlayerInside = data.IsPlayerInside;
            foreach (ModVehicle mv in VehicleManager.VehiclesInPlay)
            {
                if (mv == null)
                {
                    continue;
                }
                if (!mv.name.Contains("Clone"))
                {
                    // skip the prefabs
                    continue;
                }
                foreach(var vehicle in allVehiclesPlayerInside)
                {
                    if(Vector3.Distance(vehicle.Item1, mv.transform.position) < 3 && vehicle.Item2)
                    {
                        mv.PlayerEntry();
                        return;
                    }
                }
            }
        }
    }
}

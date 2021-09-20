using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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
                //ModuleBuilder.main.equipment.Init(mv.modules);
            }
        }
        internal static List<Tuple<Vector3, List<Tuple<int, List<TechType>>>>> SerializeModularStorage()
        {
            List<Tuple<Vector3, List<Tuple<int, List<TechType>>>>> allVehiclesStoragesContents = new List<Tuple<Vector3, List<Tuple<int, List<TechType>>>>>();
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
                List<Tuple<int, List<TechType>>> thisVehiclesStoragesContents = new List<Tuple<int, List<TechType>>>();

                for(int i=0; i<mv.ModularStorages.Count; i++)
                {
                    var thisContainer = mv.GetStorageInSlot(i, TechType.VehicleStorageModule);
                    if (thisContainer != null)
                    {
                        List<TechType> thisContents = new List<TechType>();
                        foreach (var item in thisContainer.ToList())
                        {
                            thisContents.Add(item.item.GetTechType());
                        }
                        thisVehiclesStoragesContents.Add(new Tuple<int, List<TechType>>(i, thisContents));
                    }
                }


                allVehiclesStoragesContents.Add(new Tuple<Vector3, List<Tuple<int, List<TechType>>>>(mv.transform.position, thisVehiclesStoragesContents));
            }
            return allVehiclesStoragesContents;
        }
        internal static void DeserializeModularStorage(SaveData data)
        {
            List<Tuple<Vector3, List<Tuple<int, List<TechType>>>>> allVehiclesStoragesLists = data.ModularStorages;
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
                                    GameObject thisItem = GameObject.Instantiate(CraftData.GetPrefabForTechType(techtype, true));
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
        internal static List<Tuple<Vector3, List<Tuple<Vector3, List<TechType>>>>> SerializeInnateStorage()
        {
            List<Tuple<Vector3, List<Tuple<Vector3, List<TechType>>>>> allVehiclesStoragesContents = new List<Tuple<Vector3, List<Tuple<Vector3, List<TechType>>>>>();
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
                List<Tuple<Vector3, List<TechType>>> thisVehiclesStoragesContents = new List<Tuple<Vector3, List<TechType>>>();
                foreach (InnateStorageContainer vsc in mv.GetComponentsInChildren<InnateStorageContainer>())
                {
                    Vector3 thisLocalPos = vsc.transform.localPosition;
                    List<TechType> thisContents = new List<TechType>();
                    foreach (var item in vsc.container.ToList())
                    {
                        thisContents.Add(item.item.GetTechType());
                    }
                    thisVehiclesStoragesContents.Add(new Tuple<Vector3, List<TechType>>(thisLocalPos, thisContents));
                }
                allVehiclesStoragesContents.Add(new Tuple<Vector3, List<Tuple<Vector3, List<TechType>>>>(mv.transform.position, thisVehiclesStoragesContents));
            }
            return allVehiclesStoragesContents;
        }
        internal static void DeserializeInnateStorage(SaveData data)
        {
            List<Tuple<Vector3, List<Tuple<Vector3, List<TechType>>>>> allVehiclesStoragesLists = data.InnateStorages;
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
                                    foreach (TechType thisTechType in thisStorage.Item2)
                                    {
                                        GameObject thisItem = GameObject.Instantiate(CraftData.GetPrefabForTechType(thisTechType, true));
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
    }
}

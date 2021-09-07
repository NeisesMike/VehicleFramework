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
                    Logger.Log("skipping null ModVehicle");
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
            Logger.Log("At deserialize time, there were " + VehicleManager.VehiclesInPlay.Count.ToString() + " vehicles");

            foreach(ModVehicle mv in VehicleManager.VehiclesInPlay)
            {
                if(mv==null)
                {
                    continue;
                }

                // try to match against a saved vehicle in our list
                foreach(var tup in modVehiclesUpgrades)
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
                            Logger.Log("Added " + thisUpgrade.name + " to " + pair.Key);

                            // try calling OnUpgradeModulesChanged now
                            mv.UpdateModuleSlots();
                        }
                    }
                }
                ModuleBuilder.main.equipment.Init(mv.modules);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using batteries = System.Collections.Generic.List<System.Tuple<TechType, float>>;
using color = System.Tuple<float, float, float, float>;

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
                    Vector3 thisLocalPos = vsc.transform.position;
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
                            bool isStorageMatched = false;
                            // load up the storages
                            foreach (var isc in mv.GetComponentsInChildren<InnateStorageContainer>())
                            {
                                isStorageMatched = false;
                                if (Vector3.Distance(isc.transform.position, thisStorage.Item1) < 0.05f) // this is a weird amount of drift, but I'm afraid to use ==
                                {
                                    isStorageMatched = true;
                                    foreach (var techtype in thisStorage.Item2)
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
                                        isc.container.AddItem(thisItem.GetComponent<Pickupable>());
                                        thisItem.SetActive(false);
                                    }
                                    break;
                                }
                            }
                            if (!isStorageMatched)
                            {
                                // shit out the contents of the missing container, marked with a beacon
                                IEnumerator MarkWithBeacon(Vector3 position)
                                {
                                    GameObject thisBeacon = GameObject.Instantiate(CraftData.GetPrefabForTechType(TechType.Beacon));
                                    thisBeacon.transform.position = position;
                                    yield return null; // let the stray entity be registered by the game
                                    thisBeacon.GetComponentInChildren<BeaconLabel>().SetLabel("Thanks! -Mikjaw");
                                    yield break;
                                }
                                Player.main.StartCoroutine(MarkWithBeacon(mv.transform.position + mv.transform.forward * 5f));

                                GameObject thisFloatingContainer = null;
                                int numContainersSoFar = 0;
                                for (int i = 0; i < thisStorage.Item2.Count; i++)
                                {
                                    if (i % 16 == 0)
                                    {
                                        thisFloatingContainer = GameObject.Instantiate(CraftData.GetPrefabForTechType(TechType.SmallStorage));
                                        Vector3 randomDirection = Vector3.Normalize(new Vector3(UnityEngine.Random.value * 2 - 1, UnityEngine.Random.value * 2 - 1, UnityEngine.Random.value * 2 - 1));
                                        thisFloatingContainer.transform.position = mv.transform.position + mv.transform.forward * 5f + randomDirection * 2f;
                                        thisFloatingContainer.GetComponentInChildren<uGUI_InputField>().text = "Overflow " + numContainersSoFar++.ToString();
                                    }

                                    Tuple<TechType, float> techtype = thisStorage.Item2[i];
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
                                    thisItem.transform.SetParent(thisFloatingContainer.GetComponentInChildren<StorageContainer>().transform);
                                    thisFloatingContainer.GetComponentInChildren<StorageContainer>().container.AddItem(thisItem.GetComponent<Pickupable>());
                                    thisItem.SetActive(false);
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
                foreach (var slot in allVehiclesBatteries)
                {
                    // the following floats we compare should in reality be the same
                    // but anyways there's probably no closer mod vehicle than 1 meter
                    if (Vector3.Distance(mv.transform.position, slot.Item1) < 1)
                    {
                        foreach (var battery in slot.Item2.Select((value, i) => (value, i)))
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
        internal static List<Tuple<Vector3, string, color, color, color, color, bool>> SerializeAesthetics()
        {
            color ExtractFloats(Color col)
            {
                return new Tuple<float, float, float, float>(col.r, col.g, col.b, col.a);
            }
            List<Tuple<Vector3, string, color, color, color, color, bool>> allVehiclesAesthetics = new List<Tuple<Vector3, string, color, color, color, color, bool>>();
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
                allVehiclesAesthetics.Add(new Tuple<Vector3, string, color, color, color, color, bool>(mv.transform.position, mv.NowVehicleName, ExtractFloats(mv.ExteriorMainColor), ExtractFloats(mv.ExteriorPrimaryAccent), ExtractFloats(mv.ExteriorSecondaryAccent), ExtractFloats(mv.ExteriorNameLabel), mv.IsDefaultTexture));
            }
            return allVehiclesAesthetics;
        }
        internal static void DeserializeAesthetics(SaveData data)
        {
            Color SynthesizeColor(color col)
            {
                return new Color(col.Item1, col.Item2, col.Item3, col.Item4);
            }
            List<Tuple<Vector3, string, color, color, color, color, bool>> allVehiclesAesthetics = data.AllVehiclesAesthetics;
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
                foreach (var vehicle in allVehiclesAesthetics)
                {
                    if (Vector3.Distance(vehicle.Item1, mv.transform.position) < 3)
                    {
                        var active = mv.ColorPicker?.transform.Find("EditScreen/Active");
                        if(active is null)
                        {
                            continue;
                        }
                        active.transform.Find("InputField").GetComponent<uGUI_InputField>().text = vehicle.Item2;
                        active.transform.Find("InputField/Text").GetComponent<Text>().text = vehicle.Item2;
                        mv.NowVehicleName = vehicle.Item2;
                        mv.vehicleName = vehicle.Item2;
                        if (vehicle.Item7)
                        {
                            mv.PaintVehicleDefaultStyle(vehicle.Item2);
                            mv.OnNameChangeMaybe(vehicle.Item2);
                        }
                        else
                        {
                            mv.ExteriorMainColor = SynthesizeColor(vehicle.Item3);
                            mv.ExteriorPrimaryAccent = SynthesizeColor(vehicle.Item4);
                            mv.ExteriorSecondaryAccent = SynthesizeColor(vehicle.Item5);
                            mv.ExteriorNameLabel = SynthesizeColor(vehicle.Item6);
                            mv.PaintVehicleSection("ExteriorMainColor", mv.ExteriorMainColor);
                            mv.PaintVehicleSection("ExteriorPrimaryAccent", mv.ExteriorPrimaryAccent);
                            mv.PaintVehicleSection("ExteriorSecondaryAccent", mv.ExteriorSecondaryAccent);
                            mv.PaintVehicleName(vehicle.Item2, mv.ExteriorNameLabel, mv.ExteriorMainColor);

                            mv.IsDefaultTexture = false;

                            //var colorPicker = mv.transform.Find("ColorPicker/EditScreen/Active/ColorPicker").GetComponentInChildren<uGUI_ColorPicker>();
                            //Color.RGBToHSV(mv.ExteriorMainColor, out colorPicker._hue, out colorPicker._saturation, out colorPicker._brightness);

                            active.transform.Find("MainExterior/SelectedColor").GetComponent<Image>().color = mv.ExteriorMainColor;
                            active.transform.Find("PrimaryAccent/SelectedColor").GetComponent<Image>().color = mv.ExteriorPrimaryAccent;
                            active.transform.Find("SecondaryAccent/SelectedColor").GetComponent<Image>().color = mv.ExteriorSecondaryAccent;
                            active.transform.Find("NameLabel/SelectedColor").GetComponent<Image>().color = mv.ExteriorNameLabel;
                        }
                        break;
                    }
                }
            }
        }
    }
}

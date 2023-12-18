using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using batteries = System.Collections.Generic.List<System.Tuple<System.String, float>>;
using color = System.Tuple<float, float, float, float>;
using techtype = System.String;

namespace VehicleFramework
{
    public static class SaveManager
    {
        /* Things what we can serialize
         * List<Tuple<Vector,Vector>>
         * List<Dictionary<Vector3, Vector3>>
         * List<Tuple<Dictionary<Vector3, Vector3>, Vector3>>
         */
        /* Things what we cannot get away with
         * List<Tuple<Dictionary<Vector3, Vector3>, TechType>>
         * List<TechType>
         */
        internal static List<techtype> SerializeTesto()
        {
            List<techtype> ret = new List<techtype>();
            //var dic = new Dictionary<Vector3, Vector3>();
            //var tup = new Tuple<Dictionary<Vector3, Vector3>, TechType>(dic, TechType.Seamoth);
            ret.Add(TechType.Seamoth.AsString());
            Logger.Warn(TechType.Seamoth.AsString());
            return ret;
        }
        internal static List<Tuple<Vector3, Dictionary<string, techtype>>> SerializeUpgrades()
        {
            List<Tuple<Vector3, Dictionary<string, techtype>>> modVehiclesUpgrades = new List<Tuple<Vector3, Dictionary<string, techtype>>>();
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
                Dictionary<string, techtype> equipmentStrings = new Dictionary<string, techtype>();
                foreach (KeyValuePair<string, InventoryItem> pair in mv.modules.equipment)
                {
                    if (pair.Value != null && pair.Value.item != null && pair.Value.item.name != null)
                    {
                        string thisName = pair.Value.item.name;
                        int cloneIndex = thisName.IndexOf("(Clone)");
                        if (cloneIndex != -1)
                        {
                            pair.Value.item.name = thisName.Remove(cloneIndex, 7);
                        }
                        equipmentStrings.Add(pair.Key, pair.Value.item.GetTechType().AsString());
                    }
                }
                Tuple<Vector3, Dictionary<string, techtype>> thisTuple = new Tuple<Vector3, Dictionary<string, techtype>>(mv.transform.position, equipmentStrings);
                // this is the problematic line
                modVehiclesUpgrades.Add(thisTuple);
            }
            return modVehiclesUpgrades;
        }
        internal static IEnumerator DeserializeUpgrades(SaveData data, ModVehicle mv)
        {
            List<Tuple<Vector3, Dictionary<string, techtype>>> modVehiclesUpgrades = data.UpgradeLists;

            // try to match against a saved vehicle in our list
            foreach (var tup in modVehiclesUpgrades)
            {
                if (Vector3.Distance(mv.transform.position, tup.Item1) < 3)
                {
                    foreach (KeyValuePair<string, techtype> pair in tup.Item2)
                    {
                        TaskResult<GameObject> result = new TaskResult<GameObject>();
                        bool resulty = TechTypeExtensions.FromString(pair.Value, out TechType thisTT, true);
                        yield return CraftData.InstantiateFromPrefabAsync(thisTT, result, false);
                        GameObject thisUpgrade = result.Get();
                        thisUpgrade.transform.SetParent(mv.modulesRoot.transform);
                        thisUpgrade.SetActive(false);
                        InventoryItem thisItem = new InventoryItem(thisUpgrade.GetComponent<Pickupable>());
                        mv.modules.AddItem(pair.Key, thisItem, true);
                        // try calling OnUpgradeModulesChanged now
                        mv.UpdateModuleSlots();
                    }
                }
            }
            yield break;
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

                for (int i = 0; i < mv.ModularStorages.Count; i++)
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
                            if (bat != null)
                            {
                                batteryChargeIfApplicable = bat.charge;
                            }
                            thisContents.Add(new Tuple<techtype, float>(thisItemType.AsString(), batteryChargeIfApplicable));
                        }
                        thisVehiclesStoragesContents.Add(new Tuple<int, batteries>(i, thisContents));
                    }
                }
                allVehiclesStoragesContents.Add(new Tuple<Vector3, List<Tuple<int, batteries>>>(mv.transform.position, thisVehiclesStoragesContents));
            }
            return allVehiclesStoragesContents;
        }
        internal static IEnumerator DeserializeModularStorage(SaveData data, ModVehicle mv)
        {
            List<Tuple<Vector3, List<Tuple<int, batteries>>>> allVehiclesStoragesLists = data.ModularStorages;
            // try to match against a saved vehicle in our list
            foreach (var vehicle in allVehiclesStoragesLists)
            {
                if (Vector3.Distance(mv.transform.position, vehicle.Item1) < 3)
                {
                    // we've matched the vehicle
                    foreach (var container in vehicle.Item2)
                    {
                        var thisContainer = mv.ModGetStorageInSlot(container.Item1, TechType.VehicleStorageModule);
                        if (thisContainer != null)
                        {
                            foreach (var techtype in container.Item2)
                            {
                                TaskResult<GameObject> result = new TaskResult<GameObject>();
                                bool resulty = TechTypeExtensions.FromString(techtype.Item1, out TechType thisTT, true);
                                yield return CraftData.InstantiateFromPrefabAsync(thisTT, result, false);
                                GameObject thisItem = result.Get();
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
                                        result = new TaskResult<GameObject>();
                                        yield return CraftData.InstantiateFromPrefabAsync(TechType.Battery, result, false);
                                        GameObject newBat = result.Get();
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
                            Logger.Warn("Tried to deserialize items into a non-existent modular container: " + container.Item1.ToString());
                        }
                    }
                }
            }
            yield break;
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
                        thisContents.Add(new Tuple<techtype, float>(thisItemType.AsString(), batteryChargeIfApplicable));
                    }
                    thisVehiclesStoragesContents.Add(new Tuple<Vector3, batteries>(thisLocalPos, thisContents));
                }
                allVehiclesStoragesContents.Add(new Tuple<Vector3, List<Tuple<Vector3, batteries>>>(mv.transform.position, thisVehiclesStoragesContents));
            }
            return allVehiclesStoragesContents;
        }
        internal static IEnumerator DeserializeInnateStorage(SaveData data, ModVehicle mv)
        {
            List<Tuple<Vector3, List<Tuple<Vector3, batteries>>>> allVehiclesStoragesLists = data.InnateStorages;
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
                                    TaskResult<GameObject> result = new TaskResult<GameObject>();
                                    bool resulty = TechTypeExtensions.FromString(techtype.Item1, out TechType thisTT, true);
                                    yield return CraftData.InstantiateFromPrefabAsync(thisTT, result, false);
                                    GameObject thisItem = result.Get();
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
                                            result = new TaskResult<GameObject>();
                                            yield return CraftData.InstantiateFromPrefabAsync(TechType.Battery, result, false);
                                            GameObject newBat = result.Get();
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
                                TaskResult<GameObject> result = new TaskResult<GameObject>();
                                yield return CraftData.InstantiateFromPrefabAsync(TechType.Beacon, result, false);
                                GameObject thisBeacon = result.Get();
                                thisBeacon.transform.position = position;
                                yield return null; // let the stray entity be registered by the game
                                thisBeacon.GetComponentInChildren<BeaconLabel>().SetLabel("Thanks! -Mikjaw");
                                yield break;
                            }
                            UWE.CoroutineHost.StartCoroutine(MarkWithBeacon(mv.transform.position + mv.transform.forward * 5f));

                            GameObject thisFloatingContainer = null;
                            int numContainersSoFar = 0;
                            for (int i = 0; i < thisStorage.Item2.Count; i++)
                            {
                                if (i % 16 == 0)
                                {
                                    TaskResult<GameObject> resultthisFloatingContainer = new TaskResult<GameObject>();
                                    yield return CraftData.InstantiateFromPrefabAsync(TechType.SmallStorage, resultthisFloatingContainer, false);
                                    thisFloatingContainer = resultthisFloatingContainer.Get();
                                    Vector3 randomDirection = Vector3.Normalize(new Vector3(UnityEngine.Random.value * 2 - 1, UnityEngine.Random.value * 2 - 1, UnityEngine.Random.value * 2 - 1));
                                    thisFloatingContainer.transform.position = mv.transform.position + mv.transform.forward * 5f + randomDirection * 2f;
                                    thisFloatingContainer.GetComponentInChildren<uGUI_InputField>().text = "Overflow " + numContainersSoFar++.ToString();
                                }

                                Tuple<techtype, float> techtype = thisStorage.Item2[i];
                                TaskResult<GameObject> result = new TaskResult<GameObject>();
                                bool resulty = TechTypeExtensions.FromString(techtype.Item1, out TechType thisTT, true);
                                yield return CraftData.InstantiateFromPrefabAsync(thisTT, result, false);
                                GameObject thisItem = result.Get();
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
                                        result = new TaskResult<GameObject>();
                                        yield return CraftData.InstantiateFromPrefabAsync(TechType.Battery, result, false);
                                        GameObject newBat = result.Get();
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
            yield break;
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
                List<Tuple<techtype, float>> thisVehiclesBatteries = new List<Tuple<techtype, float>>();
                foreach (EnergyMixin batt in mv.energyInterface.sources)
                {
                    if (batt.battery != null)
                    {
                        thisVehiclesBatteries.Add(new Tuple<techtype, float>(batt.batterySlot.storedItem.item.GetTechType().AsString(), batt.battery.charge));
                    }
                }
                allVehiclesBatteries.Add(new Tuple<Vector3, batteries>(mv.transform.position, thisVehiclesBatteries));
            }
            return allVehiclesBatteries;
        }
        internal static IEnumerator DeserializeBatteries(SaveData data, ModVehicle mv)
        {
            List<Tuple<Vector3, batteries>> allVehiclesBatteries = data.Batteries;
            // try to match against a saved vehicle in our list
            foreach (var vehicle in allVehiclesBatteries)
            {
                if (Vector3.Distance(mv.transform.position, vehicle.Item1) < 3)
                {
                    foreach (var battery in vehicle.Item2.Select((value, i) => (value, i)))
                    {
                        TaskResult<GameObject> result = new TaskResult<GameObject>();
                        bool resulty = TechTypeExtensions.FromString(battery.value.Item1, out TechType thisTT, true);
                        yield return CraftData.InstantiateFromPrefabAsync(thisTT, result, false);
                        GameObject thisItem = result.Get();
                        thisItem.GetComponent<Battery>().charge = battery.value.Item2;
                        thisItem.transform.SetParent(mv.StorageRootObject.transform);
                        mv.Batteries[battery.i].BatterySlot.gameObject.GetComponent<EnergyMixin>().battery = thisItem.GetComponent<Battery>();
                        mv.Batteries[battery.i].BatterySlot.gameObject.GetComponent<EnergyMixin>().batterySlot.AddItem(thisItem.GetComponent<Pickupable>());
                        thisItem.SetActive(false);
                    }
                }
            }
            yield break;
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
                List<Tuple<techtype, float>> thisVehiclesBatteries = new List<Tuple<techtype, float>>();
                foreach (EnergyMixin batt in mv.GetComponent<AutoPilot>().aiEI.sources)
                {
                    if (batt.battery != null)
                    {
                        thisVehiclesBatteries.Add(new Tuple<techtype, float>(batt.batterySlot.storedItem.item.GetTechType().AsString(), batt.battery.charge));
                    }
                }
                allVehiclesBatteries.Add(new Tuple<Vector3, batteries>(mv.transform.position, thisVehiclesBatteries));
            }
            return allVehiclesBatteries;
        }
        internal static IEnumerator DeserializeBackupBatteries(SaveData data, ModVehicle mv)
        {
            List<Tuple<Vector3, batteries>> allVehiclesBatteries = data.BackupBatteries;
            // try to match against a saved vehicle in our list
            foreach (var slot in allVehiclesBatteries)
            {
                // the following floats we compare should in reality be the same
                // but anyways there's probably no closer mod vehicle than 1 meter
                if (Vector3.Distance(mv.transform.position, slot.Item1) < 1)
                {
                    if(mv.BackupBatteries is null)
                    {
                        continue;
                    }
                    foreach (var battery in slot.Item2.Select((value, i) => (value, i)))
                    {
                        TaskResult<GameObject> result = new TaskResult<GameObject>();
                        bool resulty = TechTypeExtensions.FromString(battery.value.Item1, out TechType thisTT, true);
                        yield return CraftData.InstantiateFromPrefabAsync(thisTT, result, false);
                        GameObject thisItem = result.Get();
                        thisItem.GetComponent<Battery>().charge = battery.value.Item2;
                        thisItem.transform.SetParent(mv.StorageRootObject.transform);
                        mv.BackupBatteries[battery.i].BatterySlot.gameObject.GetComponent<EnergyMixin>().battery = thisItem.GetComponent<Battery>();
                        mv.BackupBatteries[battery.i].BatterySlot.gameObject.GetComponent<EnergyMixin>().batterySlot.AddItem(thisItem.GetComponent<Pickupable>());
                        thisItem.SetActive(false);
                    }
                }
            }
            yield break;
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
        internal static IEnumerator DeserializePlayerInside(SaveData data, ModVehicle mv)
        {
            List<Tuple<Vector3, bool>> allVehiclesPlayerInside = data.IsPlayerInside;
            foreach (var vehicle in allVehiclesPlayerInside)
            {
                if (Vector3.Distance(vehicle.Item1, mv.transform.position) < 3 && vehicle.Item2)
                {
                    mv.PlayerEntry();
                    yield break;
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
        internal static IEnumerator DeserializeAesthetics(SaveData data, ModVehicle mv)
        {
            Color SynthesizeColor(color col)
            {
                return new Color(col.Item1, col.Item2, col.Item3, col.Item4);
            }
            List<Tuple<Vector3, string, color, color, color, color, bool>> allVehiclesAesthetics = data.AllVehiclesAesthetics;
            foreach (var vehicle in allVehiclesAesthetics)
            {
                if (Vector3.Distance(vehicle.Item1, mv.transform.position) < 3)
                {
                    var active = mv.ColorPicker?.transform.Find("EditScreen/Active");
                    if (active is null)
                    {
                        continue;
                    }
                    active.transform.Find("InputField").GetComponent<uGUI_InputField>().text = vehicle.Item2;
                    active.transform.Find("InputField/Text").GetComponent<TMPro.TextMeshProUGUI>().text = vehicle.Item2;
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
            yield break;
        }
    }
}

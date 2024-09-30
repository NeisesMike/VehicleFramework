using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VehicleFramework.VehicleTypes;
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
        internal static bool ValidateMvObject(ModVehicle mv)
        {
            if (mv == null)
            {
                return true;
            }
            if (!mv.name.Contains("Clone"))
            {
                // skip the prefabs
                return true;
            }
            return false;
        }
        internal static bool MatchMv(ModVehicle mv, Vector3 location)
        {
            // the following floats we compare should in reality be the same
            // but anyways there's probably no closer mod vehicle than 1 meter
            return Vector3.Distance(mv.transform.position, location) < 3;
        }
        internal static List<Tuple<Vector3, Dictionary<string, techtype>>> SerializeUpgrades()
        {
            List<Tuple<Vector3, Dictionary<string, techtype>>> modVehiclesUpgrades = new List<Tuple<Vector3, Dictionary<string, techtype>>>();
            foreach (ModVehicle mv in VehicleManager.VehiclesInPlay)
            {
                if (ValidateMvObject(mv))
                {
                    continue;
                }
                try
                {
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
                catch(Exception e)
                {
                    Logger.Error("Failed to serialize upgrades for: " + mv.name + " : " + mv.subName.hullName.text);
                    Logger.Log(e.Message);
                }
            }
            return modVehiclesUpgrades;
        }
        internal static IEnumerator DeserializeUpgrades(SaveData data, ModVehicle mv)
        {
            if (data == null || mv == null || data.UpgradeLists == null)
            {
                yield break;
            }
            // try to match against a saved vehicle in our list
            foreach (Tuple<Vector3, Dictionary<string, techtype>> tup in data.UpgradeLists)
            {
                if (MatchMv(mv, tup.Item1))
                {
                    foreach (KeyValuePair<string, techtype> pair in tup.Item2)
                    {
                        TaskResult<GameObject> result = new TaskResult<GameObject>();
                        bool resulty = TechTypeExtensions.FromString(pair.Value, out TechType thisTT, true);
                        if(!resulty)
                        {
                            continue;
                        }
                        yield return CraftData.InstantiateFromPrefabAsync(thisTT, result, false);
                        try
                        {
                            GameObject thisUpgrade = result.Get();
                            thisUpgrade.transform.SetParent(mv.modulesRoot.transform);
                            thisUpgrade.SetActive(false);
                            InventoryItem thisItem = new InventoryItem(thisUpgrade.GetComponent<Pickupable>());
                            mv.modules.AddItem(pair.Key, thisItem, true);
                            // try calling OnUpgradeModulesChanged now
                            mv.UpdateModuleSlots();
                        }
                        catch (Exception e)
                        {
                            Logger.Error("Failed to load upgrades for " + mv.name + " : " + mv.subName.hullName.text);
                            Logger.Log(e.Message);
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
                if (ValidateMvObject(mv))
                {
                    continue;
                }
                if (mv.ModularStorages == null)
                {
                    return allVehiclesStoragesContents;
                }
                List<Tuple<int, batteries>> thisVehiclesStoragesContents = new List<Tuple<int, batteries>>();

                try
                {
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
                catch (Exception e)
                {
                    Logger.Error("Failed to serialize modular storage for: " + mv.name + " : " + mv.subName.hullName.text);
                    Logger.Log(e.Message);
                }
            }
            return allVehiclesStoragesContents;
        }
        internal static IEnumerator DeserializeModularStorage(SaveData data, ModVehicle mv)
        {
            if (data == null || mv == null || data.ModularStorages == null)
            {
                yield break;
            }
            // try to match against a saved vehicle in our list
            foreach (Tuple<Vector3, List<Tuple<int, batteries>>> vehicle in data.ModularStorages)
            {
                if (MatchMv(mv, vehicle.Item1))
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
                                if (!resulty)
                                {
                                    continue;
                                }
                                yield return CraftData.InstantiateFromPrefabAsync(thisTT, result, false);
                                GameObject thisItem = result.Get();
                                if (techtype.Item2 >= 0)
                                {
                                    // check whether we *are* a battery xor we *have* a battery
                                    if (thisItem.GetComponent<Battery>() != null && thisItem.GetComponentInChildren<Battery>() != null)
                                    {
                                        // we are a battery
                                        thisItem.GetComponentInChildren<Battery>().charge = techtype.Item2;
                                    }
                                    else
                                    {
                                        // we have a battery (we are a tool)
                                        // Thankfully we have this naming convention
                                        Transform batSlot = thisItem.transform.Find("BatterySlot");
                                        if(batSlot == null)
                                        {
                                            Logger.Warn("Failed to load modular storage item : " + thisItem.name + " for " + mv.name + " : " + mv.subName.hullName.text);
                                            continue;
                                        }
                                        result = new TaskResult<GameObject>();
                                        yield return CraftData.InstantiateFromPrefabAsync(TechType.Battery, result, false);
                                        GameObject newBat = result.Get();
                                        if (newBat.GetComponent<Battery>() != null)
                                        {
                                            newBat.GetComponent<Battery>().charge = techtype.Item2;
                                            Logger.Warn("Failed to load modular storage battery : " + thisItem.name + " for " + mv.name + " : " + mv.subName.hullName.text);
                                        }
                                        newBat.transform.SetParent(batSlot);
                                        newBat.SetActive(false);
                                    }
                                }
                                thisItem.transform.SetParent(mv.StorageRootObject.transform);
                                try
                                {
                                    thisContainer.AddItem(thisItem.GetComponent<Pickupable>());
                                }
                                catch(Exception e)
                                {
                                    Logger.Error("Failed to add storage item to modular storage : " + thisItem.name + " for " + mv.name + " : " + mv.subName.hullName.text);
                                    Logger.Log(e.Message);
                                }
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
        }
        internal static List<Tuple<Vector3, List<Tuple<Vector3, batteries>>>> SerializeInnateStorage()
        {
            List<Tuple<Vector3, List<Tuple<Vector3, batteries>>>> allVehiclesStoragesContents = new List<Tuple<Vector3, List<Tuple<Vector3, batteries>>>>();
            foreach (ModVehicle mv in VehicleManager.VehiclesInPlay)
            {
                if (ValidateMvObject(mv))
                {
                    continue;
                }
                List<Tuple<Vector3, batteries>> thisVehiclesStoragesContents = new List<Tuple<Vector3, batteries>>();
                try
                {
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
                catch (Exception e)
                {
                    Logger.Error("Failed to serialize innate storage for: " + mv.name + " : " + mv.subName.hullName.text);
                    Logger.Log(e.Message);
                }
            }
            return allVehiclesStoragesContents;
        }
        internal static IEnumerator DeserializeInnateStorage(SaveData data, ModVehicle mv)
        {
            if (data == null || mv == null || data.InnateStorages == null)
            {
                yield break;
            }
            // try to match against a saved vehicle in our list
            foreach (Tuple<Vector3, List<Tuple<Vector3, batteries>>> vehicle in data.InnateStorages)
            {
                if (MatchMv(mv, vehicle.Item1))
                {
                    foreach (var thisStorage in vehicle.Item2)
                    {
                        bool isStorageMatched = false;
                        if(mv.GetComponentsInChildren<InnateStorageContainer>().Count() == 0)
                        {
                            continue;
                        }
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
                                    if(!resulty)
                                    {
                                        continue;
                                    }
                                    yield return CraftData.InstantiateFromPrefabAsync(thisTT, result, false);
                                    GameObject thisItem = result.Get();
                                    if (techtype.Item2 >= 0)
                                    {
                                        // check whether we *are* a battery xor we *have* a battery
                                        if (thisItem.GetComponent<Battery>() != null && thisItem.GetComponentInChildren<Battery>() != null)
                                        {
                                            // we are a battery
                                            thisItem.GetComponentInChildren<Battery>().charge = techtype.Item2;
                                        }
                                        else
                                        {
                                            // we have a battery (we are a tool)
                                            // Thankfully we have this naming convention
                                            Transform batSlot = thisItem.transform.Find("BatterySlot");
                                            if (batSlot == null)
                                            {
                                                Logger.Warn("Failed to load innate storage item : " + thisItem.name + " for " + mv.name + " : " + mv.subName.hullName.text);
                                                continue;
                                            }
                                            result = new TaskResult<GameObject>();
                                            yield return CraftData.InstantiateFromPrefabAsync(TechType.Battery, result, false);
                                            GameObject newBat = result.Get();
                                            if (newBat.GetComponent<Battery>() != null)
                                            {
                                                newBat.GetComponent<Battery>().charge = techtype.Item2;
                                                Logger.Warn("Failed to load innate storage battery : " + thisItem.name + " for " + mv.name + " : " + mv.subName.hullName.text);
                                            }
                                            newBat.transform.SetParent(batSlot);
                                            newBat.SetActive(false);
                                        }
                                    }
                                    thisItem.transform.SetParent(mv.StorageRootObject.transform);
                                    try
                                    {
                                        isc.container.AddItem(thisItem.GetComponent<Pickupable>());
                                    }
                                    catch (Exception e)
                                    {
                                        Logger.Error("Failed to add storage item to modular storage : " + thisItem.name + " for " + mv.name + " : " + mv.subName.hullName.text);
                                        Logger.Log(e.Message);
                                    }
                                    thisItem.SetActive(false);
                                }
                                break;
                            }
                        }
                        if (!isStorageMatched)
                        {
                            Logger.Error("Failed to restore the contents of the " + mv.name);
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
                if (ValidateMvObject(mv))
                {
                    continue;
                }
                List<Tuple<techtype, float>> thisVehiclesBatteries = new List<Tuple<techtype, float>>();
                try
                {
                    foreach (EnergyMixin batt in mv.energyInterface.sources)
                    {
                        if (batt.battery != null)
                        {
                            thisVehiclesBatteries.Add(new Tuple<techtype, float>(batt.batterySlot.storedItem.item.GetTechType().AsString(), batt.battery.charge));
                        }
                    }
                    allVehiclesBatteries.Add(new Tuple<Vector3, batteries>(mv.transform.position, thisVehiclesBatteries));
                }
                catch (Exception e)
                {
                    Logger.Error("Failed to serialize batteries for: " + mv.name + " : " + mv.subName.hullName.text);
                    Logger.Log(e.Message);
                }
            }
            return allVehiclesBatteries;
        }
        internal static IEnumerator DeserializeBatteries(SaveData data, ModVehicle mv)
        {
            if (data == null || mv == null || data.Batteries == null)
            {
                yield break;
            }
            // try to match against a saved vehicle in our list
            foreach (Tuple<Vector3, batteries> vehicle in data.Batteries)
            {
                if (MatchMv(mv, vehicle.Item1))
                {
                    foreach (var battery in vehicle.Item2.Select((value, i) => (value, i)))
                    {
                        TaskResult<GameObject> result = new TaskResult<GameObject>();
                        bool resulty = TechTypeExtensions.FromString(battery.value.Item1, out TechType thisTT, true);
                        if(!resulty)
                        {
                            continue;
                        }
                        yield return CraftData.InstantiateFromPrefabAsync(thisTT, result, false);
                        GameObject thisItem = result.Get();
                        try
                        {
                            thisItem.GetComponent<Battery>().charge = battery.value.Item2;
                            thisItem.transform.SetParent(mv.StorageRootObject.transform);
                            mv.Batteries[battery.i].BatterySlot.gameObject.GetComponent<EnergyMixin>().battery = thisItem.GetComponent<Battery>();
                            mv.Batteries[battery.i].BatterySlot.gameObject.GetComponent<EnergyMixin>().batterySlot.AddItem(thisItem.GetComponent<Pickupable>());
                            thisItem.SetActive(false);
                        }
                        catch (Exception e)
                        {
                            Logger.Error("Failed to load battery : " + thisItem.name + " for " + mv.name + " : " + mv.subName.hullName.text);
                            Logger.Log(e.Message);
                        }
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
                if (ValidateMvObject(mv))
                {
                    continue;
                }
                if (mv.energyInterface == mv.AIEnergyInterface)
                {
                    // don't back up the same batteries twice
                    continue;
                }
                List<Tuple<techtype, float>> thisVehiclesBatteries = new List<Tuple<techtype, float>>();
                try
                {
                    foreach (EnergyMixin batt in mv.GetComponent<AutoPilot>().aiEI.sources)
                    {
                        if (batt.battery != null)
                        {
                            thisVehiclesBatteries.Add(new Tuple<techtype, float>(batt.batterySlot.storedItem.item.GetTechType().AsString(), batt.battery.charge));
                        }
                    }
                    allVehiclesBatteries.Add(new Tuple<Vector3, batteries>(mv.transform.position, thisVehiclesBatteries));
                }
                catch (Exception e)
                {
                    Logger.Error("Failed to serialize backup batteries for: " + mv.name + " : " + mv.subName.hullName.text);
                    Logger.Log(e.Message);
                }
            }
            return allVehiclesBatteries;
        }
        internal static IEnumerator DeserializeBackupBatteries(SaveData data, Submarine mv)
        {
            if (data == null || mv == null || data.BackupBatteries == null)
            {
                yield break;
            }
            // try to match against a saved vehicle in our list
            foreach (Tuple<Vector3, batteries> slot in data.BackupBatteries)
            {
                if (MatchMv(mv, slot.Item1))
                {
                    if(mv.BackupBatteries is null || mv.BackupBatteries.Count == 0)
                    {
                        continue;
                    }
                    foreach (var battery in slot.Item2.Select((value, i) => (value, i)))
                    {
                        TaskResult<GameObject> result = new TaskResult<GameObject>();
                        bool resulty = TechTypeExtensions.FromString(battery.value.Item1, out TechType thisTT, true);
                        if (!resulty)
                        {
                            continue;
                        }
                        yield return CraftData.InstantiateFromPrefabAsync(thisTT, result, false);
                        GameObject thisItem = result.Get();
                        try
                        {
                            thisItem.GetComponent<Battery>().charge = battery.value.Item2;
                            thisItem.transform.SetParent(mv.StorageRootObject.transform);
                            mv.BackupBatteries[battery.i].BatterySlot.gameObject.GetComponent<EnergyMixin>().battery = thisItem.GetComponent<Battery>();
                            mv.BackupBatteries[battery.i].BatterySlot.gameObject.GetComponent<EnergyMixin>().batterySlot.AddItem(thisItem.GetComponent<Pickupable>());
                            thisItem.SetActive(false);
                        }
                        catch (Exception e)
                        {
                            Logger.Error("Failed to load backup battery : " + thisItem.name + " for " + mv.name + " : " + mv.subName.hullName.text);
                            Logger.Log(e.Message);
                        }
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
                if (ValidateMvObject(mv))
                {
                    continue;
                }
                try
                {
                    allVehiclesIsPlayerInside.Add(new Tuple<Vector3, bool>(mv.transform.position, mv.IsUnderCommand));
                }
                catch (Exception e)
                {
                    Logger.Error("Failed to serialize IsPlayerInside for: " + mv.name + " : " + mv.subName.hullName.text);
                    Logger.Log(e.Message);
                }
        }
            return allVehiclesIsPlayerInside;
        }
        internal static IEnumerator DeserializePlayerInside(SaveData data, ModVehicle mv)
        {
            if (data == null || mv == null || data.IsPlayerInside == null)
            {
                yield break;
            }
            foreach (Tuple<Vector3, bool> vehicle in data.IsPlayerInside)
            {
                if (MatchMv(mv, vehicle.Item1) && vehicle.Item2)
                {
                    try
                    {
                        mv.PlayerEntry();
                    }
                    catch(Exception e)
                    {
                        Logger.Error("Failed to load player into vehicle :" + mv.name + " : " + mv.subName.hullName.text);
                        Logger.Log(e.Message);
                    }
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
                if (ValidateMvObject(mv))
                {
                    continue;
                }
                if (mv as Submarine is null)
                {
                    continue;
                }
                try
                {
                    allVehiclesAesthetics.Add(new Tuple<Vector3, string, color, color, color, color, bool>(mv.transform.position, (mv as Submarine).NowVehicleName, ExtractFloats((mv as Submarine).ExteriorMainColor), ExtractFloats((mv as Submarine).ExteriorPrimaryAccent), ExtractFloats((mv as Submarine).ExteriorSecondaryAccent), ExtractFloats((mv as Submarine).ExteriorNameLabel), (mv as Submarine).IsDefaultTexture));

                }
                catch (Exception e)
                {
                    Logger.Error("Failed to serialize aesthetics for: " + mv.name + " : " + mv.subName.hullName.text);
                    Logger.Log(e.Message);
                }
            }
            return allVehiclesAesthetics;
        }
        internal static IEnumerator DeserializeAesthetics(SaveData data, Submarine mv)
        {
            if (data == null || mv == null || data.AllVehiclesAesthetics == null)
            {
                yield break;
            }
            Color SynthesizeColor(color col)
            {
                return new Color(col.Item1, col.Item2, col.Item3, col.Item4);
            }
            foreach (Tuple<Vector3, string, color, color, color, color, bool> vehicle in data.AllVehiclesAesthetics)
            {
                if (MatchMv(mv, vehicle.Item1))
                {
                    try
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
                    catch(Exception e)
                    {
                        Logger.Error("Failed to load color details for " + mv.name + " : " + mv.subName.hullName.text);
                        Logger.Log(e.Message);
                    }
                }
            }
        }
        internal static List<Tuple<Vector3, bool>> SerializePlayerControlling()
        {
            List<Tuple<Vector3, bool>> allVehiclesIsPlayerControlling = new List<Tuple<Vector3, bool>>();
            foreach (ModVehicle mv in VehicleManager.VehiclesInPlay)
            {
                if (ValidateMvObject(mv))
                {
                    continue;
                }
                try
                {
                    allVehiclesIsPlayerControlling.Add(new Tuple<Vector3, bool>(mv.transform.position, mv.IsPlayerControlling()));
                }
                catch (Exception e)
                {
                    Logger.Error("Failed to serialize IsPlayerControlling for: " + mv.name + " : " + mv.subName.hullName.text);
                    Logger.Log(e.Message);
                }
            }
            return allVehiclesIsPlayerControlling;
        }
        internal static IEnumerator DeserializePlayerControlling(SaveData data, ModVehicle mv)
        {
            if (data == null || mv == null || data.IsPlayerControlling == null)
            {
                yield break;
            }
            foreach (Tuple<Vector3, bool> vehicle in data.IsPlayerControlling)
            {
                if (MatchMv(mv, vehicle.Item1) && vehicle.Item2)
                {
                    try
                    {
                        if(mv as Drone != null)
                        {
                            (mv as Drone).BeginControlling();
                        }
                        else
                        {
                            mv.BeginPiloting();
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error("Failed to load player into vehicle :" + mv.name + " : " + mv.subName.hullName.text);
                        Logger.Log(e.Message);
                    }
                    yield break;
                }
            }
        }
        internal static List<Tuple<Vector3, string>> SerializeSubName()
        {
            List<Tuple<Vector3, string>> allVehiclesSubNames = new List<Tuple<Vector3, string>>();
            foreach (ModVehicle mv in VehicleManager.VehiclesInPlay)
            {
                if (ValidateMvObject(mv))
                {
                    continue;
                }
                try
                {
                    allVehiclesSubNames.Add(new Tuple<Vector3, string>(mv.transform.position, mv.subName.hullName.text));
                }
                catch (Exception e)
                {
                    Logger.Error("Failed to serialize SubName for: " + mv.name + " : " + mv.subName.hullName.text);
                    Logger.Log(e.Message);
                }
            }
            return allVehiclesSubNames;
        }
        internal static IEnumerator DeserializeSubName(SaveData data, ModVehicle mv)
        {
            if (data == null || mv == null || data.SubNames == null)
            {
                yield break;
            }
            foreach (Tuple<Vector3, string> vehicle in data.SubNames)
            {
                if (MatchMv(mv, vehicle.Item1))
                {
                    try
                    {
                        mv.subName.hullName.text = vehicle.Item2;
                    }
                    catch (Exception e)
                    {
                        Logger.Error("Failed to load SubName for vehicle :" + mv.name + " : " + mv.subName.hullName.text);
                        Logger.Log(e.Message);
                    }
                    yield break;
                }
            }
        }
    }
}

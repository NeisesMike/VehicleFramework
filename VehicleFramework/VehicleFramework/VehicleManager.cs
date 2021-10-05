using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using SMLHelper.V2.Json;

namespace VehicleFramework
{
    public struct VehicleMemory
    {
        public Vector3 position;
        public Quaternion rotation;
        public bool headlamp_state;
        public int hp;

        // batteries : power level

        // storage

        // modular storage


        // upgrades
    }
    public static class VehicleManager
    {
        public static List<ModVehicle> VehiclesInPlay = new List<ModVehicle>();
        public static List<PingInstance> mvPings = new List<PingInstance>();
        public static List<VehicleEntry> vehicleTypes = new List<VehicleEntry>();



        public static void PatchCraftables()
        {
            foreach (VehicleEntry ve in vehicleTypes)
            {
                Logger.Log("Patching the " + ve.prefab.name + " Craftable...");
                VehicleCraftable thisCraftable = new VehicleCraftable(ve.prefab.name, ve.prefab.name, ve.description);
                thisCraftable.Patch();
                Logger.Log("Patched the " + ve.prefab.name + " Craftable.");
            }
        }

        public static void RegisterVehicle(ref ModVehicle mv, PingType pt, Atlas.Sprite sprite, int modules, int arms)
        {
            bool isNewEntry = true;
            foreach (VehicleEntry ve in vehicleTypes)
            {
                if (ve.prefab.name == ve.prefab.name)
                {
                    Logger.Log(mv.gameObject.name + " vehicle was already registered.");
                    isNewEntry = false;
                    break;
                }
            }
            if (isNewEntry)
            {
                VehicleBuilder.Prefabricate(ref mv, pt, sprite, modules, arms);
                Logger.Log("Registered the " + mv.gameObject.name);
            }
        }




        // TODO: rename to avoid intuition-collision with the above function of the same name
        public static void RegisterVehicle(ModVehicle mv)
        {
            if (mv.name.Contains("Clone"))
            {
                VehiclesInPlay.Add(mv);
                Logger.Log(mv.name + " : " + mv.GetName() + " : " + mv.subName + " was registered!");
            }
        }
        public static void DeregisterVehicle(ModVehicle mv)
        {
            VehiclesInPlay.Remove(mv);
        }
        public static void SaveVehicles(object sender, JsonFileEventArgs e)
        {
            SaveData data = e.Instance as SaveData;
            data.UpgradeLists = SaveManager.SerializeUpgrades();
            data.InnateStorages = SaveManager.SerializeInnateStorage();
            data.ModularStorages = SaveManager.SerializeModularStorage();
            data.Batteries = SaveManager.SerializeBatteries();
            data.BackupBatteries = SaveManager.SerializeBackupBatteries();
            data.IsPlayerInside = SaveManager.SerializePlayerInside();
        }
        // TODO refactor this to accept a modvehicle as input
        public static void LoadVehicles()
        {
            SaveManager.DeserializeUpgrades(MainPatcher.VehicleSaveData);
            SaveManager.DeserializeInnateStorage(MainPatcher.VehicleSaveData);
            SaveManager.DeserializeModularStorage(MainPatcher.VehicleSaveData);
            SaveManager.DeserializeBatteries(MainPatcher.VehicleSaveData);
            SaveManager.DeserializeBackupBatteries(MainPatcher.VehicleSaveData);
            SaveManager.DeserializePlayerInside(MainPatcher.VehicleSaveData);
        }
        public static void UpdateVehicles()
        {
            foreach(ModVehicle mv in VehiclesInPlay)
            {
                //mv.Update();
            }
        }
    }
}

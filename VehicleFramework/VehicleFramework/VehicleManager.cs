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
        public static List<VehicleHUD> HUDs = new List<VehicleHUD>();
        public static List<ModVehicle> VehiclesInPlay = new List<ModVehicle>();

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
        }
        // TODO refactor this to accept a modvehicle as input
        // TODO can't call this in mv.start... so where do we call it?
        // How about PDA Awake?
        public static void LoadVehicles()
        {
            SaveManager.DeserializeUpgrades(MainPatcher.VehicleSaveData);
            SaveManager.DeserializeInnateStorage(MainPatcher.VehicleSaveData);
            SaveManager.DeserializeModularStorage(MainPatcher.VehicleSaveData);
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

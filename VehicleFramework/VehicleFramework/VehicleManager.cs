using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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
            LoadVehicles();
        }
        public static void DeregisterVehicle(ModVehicle mv)
        {
            VehiclesInPlay.Remove(mv);
        }
        public static void SaveVehicles()
        {

        }
        public static void LoadVehicles()
        {
            SaveManager.DeserializeUpgrades(MainPatcher.VehicleSaveData);
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

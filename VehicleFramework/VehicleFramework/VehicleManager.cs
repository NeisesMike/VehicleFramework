﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Nautilus.Json;
using VehicleFramework.Engines;
using UnityEngine.SceneManagement;

namespace VehicleFramework
{
    public class CoroutineHelper : MonoBehaviour
    {
        public GameObject go { get; set; }
        public static Coroutine Starto(IEnumerator func)
        {
            GameObject gob = new GameObject();
            DontDestroyOnLoad(gob);
            return gob.EnsureComponent<CoroutineHelper>().StartCoroutine(gob.EnsureComponent<CoroutineHelper>().DoAndDie(func));
        }
        public IEnumerator DoAndDie(IEnumerator func)
        {
            go = gameObject;
            yield return StartCoroutine(func);
            Destroy(gameObject);
        }
    }
    public class VehicleMemory
    {
        public ModVehicle mv;
    }

    public static class VehicleManager
    {
        public static List<ModVehicle> VehiclesInPlay = new List<ModVehicle>();
        public static List<PingInstance> mvPings = new List<PingInstance>();
        public static List<VehicleEntry> vehicleTypes = new List<VehicleEntry>();
        public static int VehiclesRegistered = 0;
        public static int VehiclesPrefabricated = 0;

        public static void PatchCraftable(ref VehicleEntry ve)
        {
            VehicleEntry vehicle = ve;
            VehiclePrepper.RegisterVehicle(vehicle);// vehicle.prefab.name, vehicle.prefab.name, vehicle.description, vehicle.recipe, vehicle.encyEntry);
            Logger.Log("Patched the " + vehicle.prefab.name + " Craftable.");
            ve = vehicle;
        }

        private static bool RegistrySemaphore = false;
        public static IEnumerator RegisterVehicle(ModVehicle mv, ModVehicleEngine engine, Dictionary<TechType,int> recipe, PingType pt, Atlas.Sprite sprite, int modules, int arms, int baseCrushDepth, int maxHealth, int mass)
        {
            bool isNewEntry = true;
            foreach (VehicleEntry ve in vehicleTypes)
            {
                if (ve.prefab.name == mv.gameObject.name)
                {
                    Logger.Warn(mv.gameObject.name + " vehicle was already registered.");
                    isNewEntry = false;
                    break;
                }
            }
            if (isNewEntry)
            {
                VehiclesRegistered++;
                VehicleMemory mem = new VehicleMemory
                {
                    mv = mv
                };
                Logger.Log("Prefabricating the " + mem.mv.gameObject.name);
                while(RegistrySemaphore)
                {
                    yield return new WaitForSecondsRealtime(1f);
                }
                RegistrySemaphore = true;
                yield return CoroutineHelper.Starto(VehicleBuilder.Prefabricate(mem, engine, recipe, pt, sprite, modules, arms, baseCrushDepth, maxHealth, mass));
                RegistrySemaphore = false;
                mem.mv.gameObject.SetActive(false);
                Logger.Log("Registered the " + mem.mv.gameObject.name);
            }
            yield break;
        }
        public static void EnrollVehicle(ModVehicle mv)
        {
            if (mv.name.Contains("Clone") && !VehiclesInPlay.Contains(mv))
            {
                VehiclesInPlay.Add(mv);
                Logger.Log("Enrolled the " + mv.name + " : " + mv.GetName() + " : " + mv.subName);
                if(mv.GetComponent<VFXConstructing>().constructed > 3f)
                {
                    CoroutineHelper.Starto(LoadVehicle(mv)); // I wish I knew a good way to optionally NOT do this if this sub is being constructed rn
                }
            }
        }
        public static void DeregisterVehicle(ModVehicle mv)
        {
            VehiclesInPlay.Remove(mv);
        }
        public static void SaveVehicles(object sender, JsonFileEventArgs e)
        {
            SaveData data = e.Instance as SaveData;
            /* TODO
             * All these serializers are bad. For some reason,
             * the system chokes and the game fails to save,
             * but if you left-click, the game will unfreeze,
             * and you can continue playing, having not saved.
             * 
             * It appears to be TechType that is the problem.
             * Fortunately, it appears we can use TechType.AsString
             * and TechTypeExtensions.FromString to get around this.
             */
            data.UpgradeLists = SaveManager.SerializeUpgrades();
            data.InnateStorages = SaveManager.SerializeInnateStorage();
            data.ModularStorages = SaveManager.SerializeModularStorage();
            data.Batteries = SaveManager.SerializeBatteries();
            data.BackupBatteries = SaveManager.SerializeBackupBatteries();
            data.IsPlayerInside = SaveManager.SerializePlayerInside();
            data.AllVehiclesAesthetics = SaveManager.SerializeAesthetics();
        }
        public static IEnumerator LoadVehicle(ModVehicle mv)
        {
            while (!LargeWorldStreamer.main || !LargeWorldStreamer.main.IsReady() || !LargeWorldStreamer.main.IsWorldSettled())
            {
                yield return null;
            }
            while (WaitScreen.IsWaiting)
            {
                yield return null;
            }
            Logger.Log("Loading: " + mv.GetName());
            Coroutine ModuleGetter = mv.StartCoroutine(SaveManager.DeserializeUpgrades(MainPatcher.VehicleSaveData, mv));
            mv.StartCoroutine(SaveManager.DeserializeInnateStorage(MainPatcher.VehicleSaveData, mv));
            mv.StartCoroutine(SaveManager.DeserializeBatteries(MainPatcher.VehicleSaveData, mv));
            mv.StartCoroutine(SaveManager.DeserializeBackupBatteries(MainPatcher.VehicleSaveData, mv));
            mv.StartCoroutine(SaveManager.DeserializePlayerInside(MainPatcher.VehicleSaveData, mv));
            mv.StartCoroutine(SaveManager.DeserializeAesthetics(MainPatcher.VehicleSaveData, mv));
            yield return ModuleGetter; // can't access the modular storage until it's been getted
            mv.StartCoroutine(SaveManager.DeserializeModularStorage(MainPatcher.VehicleSaveData, mv));
        }
    }
}

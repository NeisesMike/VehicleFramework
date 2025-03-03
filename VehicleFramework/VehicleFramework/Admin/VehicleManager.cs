using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using VehicleFramework.VehicleTypes;
using VehicleFramework.SaveLoad;

namespace VehicleFramework
{
    public static class VehicleManager
    {
        public static readonly List<ModVehicle> VehiclesInPlay = new List<ModVehicle>();
        public static readonly List<PingInstance> mvPings = new List<PingInstance>();
        public static readonly List<VehicleEntry> vehicleTypes = new List<VehicleEntry>();
        public static PingType RegisterPingType(PingType pt)
        {
            return RegisterPingType(pt, false);
        }
        public static PingType RegisterPingType(PingType pt, bool verbose)
        {
            PingType ret = pt;
            if ((int)ret < 121)
            {
                VehicleRegistrar.VerboseLog(VehicleRegistrar.LogType.Log, verbose, "PingType " + pt.ToString() + " was too small. Trying 121.");
                ret = (PingType)121;
            }
            while (mvPings.Where(x => x.pingType == ret).Count() > 0)
            {
                VehicleRegistrar.VerboseLog(VehicleRegistrar.LogType.Log, verbose, "PingType " + ret.ToString() + " was taken.");
                ret++;
            }
            VehicleRegistrar.VerboseLog(VehicleRegistrar.LogType.Log, verbose, "Registering PingType " + ret.ToString() + ".");
            return ret;
        }
        public static void EnrollVehicle(ModVehicle mv)
        {
            if (mv.name.Contains("Clone") && !VehiclesInPlay.Contains(mv))
            {
                VehiclesInPlay.Add(mv);
                Logger.Log("Enrolled the " + mv.name + " : " + mv.GetName() + " : " + mv.subName);
                if (mv.GetComponent<VFXConstructing>() == null || mv.GetComponent<VFXConstructing>().constructed > 3f)
                {
                    UWE.CoroutineHost.StartCoroutine(LoadVehicle(mv)); // I wish I knew a good way to optionally NOT do this if this sub is being constructed rn
                }
            }
        }
        public static void DeregisterVehicle(ModVehicle mv)
        {
            VehiclesInPlay.Remove(mv);
        }
        internal static void CreateSaveFileData(object sender, Nautilus.Json.JsonFileEventArgs e)
        {
            SaveData data = e.Instance as SaveData;
            /* TODO
             * This method should only be used for save-file-wide issues
             * Per-vehicle issues should be moved into the vehicle classes
             */
            data.UpgradeLists = SaveManager.SerializeUpgrades();
            data.InnateStorages = SaveManager.SerializeInnateStorage();
            data.ModularStorages = SaveManager.SerializeModularStorage();
            data.Batteries = SaveManager.SerializeBatteries();
            data.BackupBatteries = SaveManager.SerializeBackupBatteries();
            data.IsPlayerInside = SaveManager.SerializePlayerInside();
            data.AllVehiclesAesthetics = SaveManager.SerializeAesthetics();
            data.IsPlayerControlling = SaveManager.SerializePlayerControlling();
            data.SubNames = SaveManager.SerializeSubName();
            JsonInterface.Write<List<string>>(Patches.SaveLoadManagerPatcher.SaveFileSpritesFileName, VehicleManager.vehicleTypes.Select(x => x.techType).Where(x => GameInfoIcon.Has(x)).Select(x => x.AsString()).ToList());
        }
        private static IEnumerator LoadVehicle(ModVehicle mv)
        {
            // TODO
            // this method should be moved into the vehicle classes and made non-static
            // The following methods no longer have serialization complements, and can probably be removed after a long time:
            // DeserializeBatteries
            // DeserializeBackupBatteries
            // DeserializeInnateStorage
            // DeserializeUpgrades
            while (!LargeWorldStreamer.main || !LargeWorldStreamer.main.IsReady() || !LargeWorldStreamer.main.IsWorldSettled())
            {
                yield return null;
            }
            while (WaitScreen.IsWaiting)
            {
                yield return null;
            }
            Logger.Log("Loading: " + mv.GetName());
            Coroutine ModuleGetter = UWE.CoroutineHost.StartCoroutine(SaveManager.DeserializeUpgrades(MainPatcher.SaveFileData, mv));
            Coroutine dis = UWE.CoroutineHost.StartCoroutine(SaveManager.DeserializeInnateStorage(MainPatcher.SaveFileData, mv));
            Coroutine db = UWE.CoroutineHost.StartCoroutine(SaveManager.DeserializeBatteries(MainPatcher.SaveFileData, mv));
            yield return ModuleGetter; // can't access the modular storage until it's been getted
            Coroutine dms = UWE.CoroutineHost.StartCoroutine(SaveManager.DeserializeModularStorage(MainPatcher.SaveFileData, mv));
            Coroutine da = UWE.CoroutineHost.StartCoroutine(SaveManager.DeserializeAesthetics(MainPatcher.SaveFileData, mv as Submarine));
            Coroutine dbb = null;
            if (mv as Submarine != null)
            {
                dbb = UWE.CoroutineHost.StartCoroutine(SaveManager.DeserializeBackupBatteries(MainPatcher.SaveFileData, mv as Submarine));
            }
            Coroutine dpi = UWE.CoroutineHost.StartCoroutine(SaveManager.DeserializePlayerInside(MainPatcher.SaveFileData, mv));
            Coroutine dpc = UWE.CoroutineHost.StartCoroutine(SaveManager.DeserializePlayerControlling(MainPatcher.SaveFileData, mv));
            Coroutine dsn = UWE.CoroutineHost.StartCoroutine(SaveManager.DeserializeSubName(MainPatcher.SaveFileData, mv));
            if (mv.liveMixin.health == 0)
            {
                mv.OnKill();
            }
            yield return dis;
            yield return db;
            yield return dms;
            yield return dpi;
            yield return dpc;
            yield return dsn;
            yield return da;
            if (dbb != null)
            {
                yield return dbb;
            }
            mv.OnFinishedLoading();
        }
    }
}

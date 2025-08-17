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
                    MainPatcher.Instance.StartCoroutine(LoadVehicle(mv)); // I wish I knew a good way to optionally NOT do this if this sub is being constructed rn
                }
            }
        }
        public static void DeregisterVehicle(ModVehicle mv)
        {
            VehiclesInPlay.Remove(mv);
        }
        internal static void CreateSaveFileData(object sender, Nautilus.Json.JsonFileEventArgs e)
        {
            // See SaveData.cs
            SaveData data = e.Instance as SaveData;
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
            // See SaveData.cs
            yield return new WaitUntil(() => LargeWorldStreamer.main != null);
            yield return new WaitUntil(() => LargeWorldStreamer.main.IsReady());
            yield return new WaitUntil(() => LargeWorldStreamer.main.IsWorldSettled());
            yield return new WaitUntil(() => !WaitScreen.IsWaiting);
            Logger.Log($"Loading: {mv.GetName()}");
            Coroutine ModuleGetter = MainPatcher.Instance.StartCoroutine(SaveManager.DeserializeUpgrades(MainPatcher.SaveFileData, mv));
            Coroutine dis = MainPatcher.Instance.StartCoroutine(SaveManager.DeserializeInnateStorage(MainPatcher.SaveFileData, mv));
            Coroutine db = MainPatcher.Instance.StartCoroutine(SaveManager.DeserializeBatteries(MainPatcher.SaveFileData, mv));
            yield return ModuleGetter; // can't access the modular storage until it's been getted
            Coroutine dms = MainPatcher.Instance.StartCoroutine(SaveManager.DeserializeModularStorage(MainPatcher.SaveFileData, mv));
            Coroutine da = MainPatcher.Instance.StartCoroutine(SaveManager.DeserializeAesthetics(MainPatcher.SaveFileData, mv as Submarine));
            Coroutine dbb = null;
            if (mv as Submarine != null)
            {
                dbb = MainPatcher.Instance.StartCoroutine(SaveManager.DeserializeBackupBatteries(MainPatcher.SaveFileData, mv as Submarine));
            }
            Coroutine dpi = MainPatcher.Instance.StartCoroutine(SaveManager.DeserializePlayerInside(MainPatcher.SaveFileData, mv));
            Coroutine dpc = MainPatcher.Instance.StartCoroutine(SaveManager.DeserializePlayerControlling(MainPatcher.SaveFileData, mv));
            Coroutine dsn = MainPatcher.Instance.StartCoroutine(SaveManager.DeserializeSubName(MainPatcher.SaveFileData, mv));
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

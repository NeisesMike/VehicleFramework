using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Nautilus.Json;
using VehicleFramework.Engines;
using UnityEngine.SceneManagement;
using VehicleFramework.VehicleTypes;

namespace VehicleFramework
{
    public static class VehicleManager
    {
        public static List<ModVehicle> VehiclesInPlay = new List<ModVehicle>();
        public static List<PingInstance> mvPings = new List<PingInstance>();
        public static List<VehicleEntry> vehicleTypes = new List<VehicleEntry>();
        public static int VehiclesRegistered = 0;
        public static int VehiclesPrefabricated = 0;
        public static Atlas.Sprite defaultPingSprite = null;
        public static Sprite defaultSaveFileSprite = null;
        public static Dictionary<TechType, int> defaultRecipe = new Dictionary<TechType, int>();
        public static ModVehicleEngine defaultEngine = null;
        public static bool isWorldLoaded = false;

        public static void PatchCraftable(ref VehicleEntry ve, bool verbose)
        {
            TechType techType = VehiclePrepper.RegisterVehicle(ve);
            VehicleRegistrar.VerboseLog(VehicleRegistrar.LogType.Log, verbose, "Patched the " + ve.name + " Craftable");
            VehicleEntry newVE = new VehicleEntry(ve.mv, ve.unique_id, ve.pt, ve.ping_sprite, techType);
            VehicleManager.vehicleTypes.Add(newVE);
        }
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
                if (mv.GetComponent<VFXConstructing>().constructed > 3f)
                {
                    UWE.CoroutineHost.StartCoroutine(LoadVehicle(mv)); // I wish I knew a good way to optionally NOT do this if this sub is being constructed rn
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
            data.IsPlayerControlling = SaveManager.SerializePlayerControlling();
            data.SubNames = SaveManager.SerializeSubName();
            data.HasVehicleTechTypes = SaveManager.SerializeHasVehicleTechTypes();
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
            Coroutine ModuleGetter = UWE.CoroutineHost.StartCoroutine(SaveManager.DeserializeUpgrades(MainPatcher.VehicleSaveData, mv));
            Coroutine dis = UWE.CoroutineHost.StartCoroutine(SaveManager.DeserializeInnateStorage(MainPatcher.VehicleSaveData, mv));
            Coroutine db = UWE.CoroutineHost.StartCoroutine(SaveManager.DeserializeBatteries(MainPatcher.VehicleSaveData, mv));
            yield return ModuleGetter; // can't access the modular storage until it's been getted
            Coroutine dms = UWE.CoroutineHost.StartCoroutine(SaveManager.DeserializeModularStorage(MainPatcher.VehicleSaveData, mv));
            Coroutine da = UWE.CoroutineHost.StartCoroutine(SaveManager.DeserializeAesthetics(MainPatcher.VehicleSaveData, mv as Submarine));
            Coroutine dbb = null;
            if (mv as Submarine != null)
            {
                dbb = UWE.CoroutineHost.StartCoroutine(SaveManager.DeserializeBackupBatteries(MainPatcher.VehicleSaveData, mv as Submarine));
            }
            Coroutine dpi = UWE.CoroutineHost.StartCoroutine(SaveManager.DeserializePlayerInside(MainPatcher.VehicleSaveData, mv));
            Coroutine dpc = UWE.CoroutineHost.StartCoroutine(SaveManager.DeserializePlayerControlling(MainPatcher.VehicleSaveData, mv));
            Coroutine dsn = UWE.CoroutineHost.StartCoroutine(SaveManager.DeserializeSubName(MainPatcher.VehicleSaveData, mv));
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

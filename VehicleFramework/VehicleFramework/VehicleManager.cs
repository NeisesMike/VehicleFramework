using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using SMLHelper.V2.Json;
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
            VehicleCraftable thisCraftable = new VehicleCraftable(vehicle.prefab.name, vehicle.prefab.name, vehicle.description, vehicle.recipe, vehicle.encyEntry);
            thisCraftable.Patch();
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
            if (mv.name.Contains("Clone"))
            {
                VehiclesInPlay.Add(mv);
                Logger.Log("Enrolled the " + mv.name + " : " + mv.GetName() + " : " + mv.subName);
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
        public static IEnumerator LoadVehicles()
        {
            /* Something is the matter. We're not loading in correctly.
             * On first load after boot, the batteries and upgrade models are good, but the color picker is dead and the largeworldstreamer has an error.
             * On subsequent loads, the color picker words and the LWS is okay, but the battery and upgrade models are gone.
             * The batteries and upgrades are still loaded and functional, but the visible models are not preserved.
             * For some reason, getseamothbits does not finish the second time.
             * 
             * Bizarrely, by fixing a bug in CoroutineHelper, I've reversed the problem.
             * Now on first load only the models are gone,
             * and on subsequent loads there is a strange World Streamer error
             * 
             * Okay, now batteries are always good.
             * Modules are good at load 2+
             * And it's actually CellManager.RegisterGlobalEntity that errors at 2+ (not world streamer),
             * ostensibly because this.streamer.globalRoot is null somewhere
             * the LargeWorldStreamer is not assigned anywhere... so
             * globalRoot is set in LargeWorldStreamer.OnGlobalRootLoaded
             * 
             * Yuck, the CellManager error only happens sometimes.
             * Sometimes, the MV Starts too early (apparently)
             * Anyways, when it errors, MV Starts before GetSeamothBits finishes
             * 
             * Wronga. We fixed GetSeamothBits, and now the batteries/modules
             * are correct on every load.
             * However, on load 2+ we are still having the CellManager error.
             * Not sure why.
             * 
             * Well the error goes away with this patch: RegisterGlobalEntityPrefix
             * But the engine sound still plays during load, which indicates the error happens,
             * and the color picker is still not loading,
             * but there are no logged errors.
             * 
             * I think the problem is that something isn't being cleaned up at Quit-time.
             * 
             * I think something about BepInEx plugins means reload is bad.
             * There's a bunch of advice on the modding discord,
             * "don't reload after quitting to menu; quit then reboot the game"
             * but I can't substantiate any claims about it
             * 
             * Maybe it would be okay if I push this, with big disclaimer about reloading
             */

            foreach (var mv in VehiclesInPlay)
            {
                mv.ModVehicleReset();
            }
            IEnumerator WaitForVehiclesToStart()
            {
                bool AreAllVehiclesStarted = false;
                while (!AreAllVehiclesStarted)
                {
                    foreach (ModVehicle mv in VehicleManager.VehiclesInPlay)
                    {
                        if (!mv.isInited)
                        {
                            yield return new WaitForSecondsRealtime(1f);
                        }
                    }
                    AreAllVehiclesStarted = true;
                }
            }
            yield return CoroutineHelper.Starto(WaitForVehiclesToStart());


            // TODO refactor a new LoadVehicles to accept a modvehicle as input
            Coroutine ModuleGetter = CoroutineHelper.Starto(SaveManager.DeserializeUpgrades(MainPatcher.VehicleSaveData));
            CoroutineHelper.Starto(SaveManager.DeserializeInnateStorage(MainPatcher.VehicleSaveData));
            CoroutineHelper.Starto(SaveManager.DeserializeBatteries(MainPatcher.VehicleSaveData));
            CoroutineHelper.Starto(SaveManager.DeserializeBackupBatteries(MainPatcher.VehicleSaveData));
            CoroutineHelper.Starto(SaveManager.DeserializePlayerInside(MainPatcher.VehicleSaveData));
            CoroutineHelper.Starto(SaveManager.DeserializeAesthetics(MainPatcher.VehicleSaveData));
            yield return ModuleGetter; // can't access the modular storage until it's been getted
            CoroutineHelper.Starto(SaveManager.DeserializeModularStorage(MainPatcher.VehicleSaveData));
        }
    }
}

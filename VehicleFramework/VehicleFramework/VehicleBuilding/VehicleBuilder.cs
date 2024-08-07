using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.IO;
using System.Reflection;

using UnityEngine.U2D;

using Nautilus.Assets;
using Nautilus.Crafting;
using Nautilus.Handlers;
using Nautilus.Utility;

using VehicleFramework.Engines;
using VehicleFramework.VehicleTypes;

namespace VehicleFramework
{
    public struct VehicleEntry
    {
        public VehicleEntry(GameObject inputGO, int id, PingType pt_in, Atlas.Sprite sprite, TechType tt)
        {
            mv = null;
            unique_id = id;
            pt = pt_in;
            name = inputGO.name;
            techType = tt;
            ping_sprite = sprite;
        }
        public VehicleEntry(ModVehicle inputMv, int id, PingType pt_in, Atlas.Sprite sprite, TechType tt=(TechType)0)
        {
            mv = inputMv;
            unique_id = id;
            pt = pt_in;
            name = mv.name;
            techType = tt;
            ping_sprite = sprite;
        }
        public ModVehicle mv;
        public string name;
        public int unique_id;
        public PingType pt;
        public Atlas.Sprite ping_sprite;
        public TechType techType;
    }

    public static class SeamothHelper
    {
        internal static TaskResult<GameObject> request = new TaskResult<GameObject>();
        private static Coroutine cor = null;
        public static GameObject Seamoth
        {
            get
            {
                GameObject thisSeamoth = request.Get();
                if (thisSeamoth == null)
                {
                    Logger.Error("Couldn't get Seamoth...");
                    return null;
                }
                UnityEngine.Object.DontDestroyOnLoad(thisSeamoth);
                thisSeamoth.SetActive(false);
                return thisSeamoth;
            }
        }
        public static IEnumerator EnsureSeamoth()
        {
            if (request.Get()) // if we have seamoth
            {
            }
            else if(cor == null) // if we need to get seamoth
            {
                cor = UWE.CoroutineHost.StartCoroutine(CraftData.InstantiateFromPrefabAsync(TechType.Seamoth, request, false));
                yield return cor;
                cor = null;
            }
            else // if someone else is getting seamoth
            {
            }
        }
    }

    public static class VehicleBuilder
    {
        public static GameObject upgradeconsole { get; internal set; }

        private static int numVehicleTypes = 0;
        public static List<ModVehicle> prefabs = new List<ModVehicle>();

        public const EquipmentType ModuleType = (EquipmentType)625;
        public const EquipmentType ArmType = (EquipmentType)626;
        public const TechType InnateStorage = (TechType)0x4100;

        public static IEnumerator Prefabricate(ModVehicle mv, PingType pingType, bool verbose)
        {
            VehicleRegistrar.VerboseLog(VehicleRegistrar.LogType.Log, verbose, "Prefabricating the " + mv.gameObject.name);
            yield return UWE.CoroutineHost.StartCoroutine(SeamothHelper.EnsureSeamoth());
            if(!Instrument(mv, pingType))
            {
                Logger.Error("Failed to instrument the vehicle: " + mv.gameObject.name);
                yield break;
            }
            prefabs.Add(mv);
            VehicleEntry ve = new VehicleEntry(mv, numVehicleTypes, pingType, mv.PingSprite);
            VehicleManager.VehiclesPrefabricated++;
            numVehicleTypes++;
            VehicleManager.PatchCraftable(ref ve, verbose);
        }

        #region setup_funcs
        public static bool SetupObjects(ModVehicle mv)
        {

            // Wow, look at this:
            // This Nautilus line might be super nice if it works for us
            // allow it to be opened as a storage container:
            //PrefabUtils.AddStorageContainer(obj, "StorageRoot", "TallLocker", 3, 8, true);


            int iter = 0;
            try
            {
                if (mv.InnateStorages != null)
                {
                    foreach (VehicleParts.VehicleStorage vs in mv.InnateStorages)
                    {
                        vs.Container.SetActive(false);

                        var cont = vs.Container.EnsureComponent<InnateStorageContainer>();
                        cont.storageRoot = mv.StorageRootObject.GetComponent<ChildObjectIdentifier>();
                        cont.storageLabel = "Vehicle Storage " + iter.ToString();
                        cont.height = vs.Height;
                        cont.width = vs.Width;

                        FMODAsset storageCloseSound = SeamothHelper.Seamoth.transform.Find("Storage/Storage1").GetComponent<SeamothStorageInput>().closeSound;
                        FMODAsset storageOpenSound = SeamothHelper.Seamoth.transform.Find("Storage/Storage1").GetComponent<SeamothStorageInput>().openSound;
                        var inp = vs.Container.EnsureComponent<InnateStorageInput>();
                        inp.mv = mv;
                        inp.slotID = iter;
                        iter++;
                        inp.model = vs.Container;
                        if (vs.Container.GetComponentInChildren<Collider>() is null)
                        {
                            inp.collider = vs.Container.EnsureComponent<BoxCollider>();
                        }
                        inp.openSound = storageOpenSound;
                        inp.closeSound = storageCloseSound;
                        vs.Container.SetActive(true);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error("There was a problem setting up the Innate Storage. Check VehicleStorage.Container and ModVehicle.StorageRootObject");
                Logger.Error(e.ToString());
                return false;
            }
            iter = 0;
            try
            {
                if (mv.ModularStorages != null)
                {
                    foreach (VehicleParts.VehicleStorage vs in mv.ModularStorages)
                    {
                        vs.Container.SetActive(false);

                        var cont = vs.Container.EnsureComponent<SeamothStorageContainer>();
                        cont.storageRoot = mv.StorageRootObject.GetComponent<ChildObjectIdentifier>();
                        cont.storageLabel = "Modular Storage " + iter.ToString();
                        cont.height = vs.Height;
                        cont.width = vs.Width;

                        FMODAsset storageCloseSound = SeamothHelper.Seamoth.transform.Find("Storage/Storage1").GetComponent<SeamothStorageInput>().closeSound;
                        FMODAsset storageOpenSound = SeamothHelper.Seamoth.transform.Find("Storage/Storage1").GetComponent<SeamothStorageInput>().openSound;
                        var inp = vs.Container.EnsureComponent<ModularStorageInput>();
                        inp.mv = mv;
                        inp.slotID = iter;
                        iter++;
                        inp.model = vs.Container;
                        if (vs.Container.GetComponentInChildren<Collider>() is null)
                        {
                            inp.collider = vs.Container.EnsureComponent<BoxCollider>();
                        }
                        inp.openSound = storageOpenSound;
                        inp.closeSound = storageCloseSound;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error("There was a problem setting up the Modular Storage. Check VehicleStorage.Container and ModVehicle.StorageRootObject");
                Logger.Error(e.ToString());
                return false;
            }
            try
            {
                if (mv.Upgrades != null)
                {
                    foreach (VehicleParts.VehicleUpgrades vu in mv.Upgrades)
                    {
                        VehicleUpgradeConsoleInput vuci = vu.Interface.EnsureComponent<VehicleUpgradeConsoleInput>();
                        vuci.flap = vu.Flap.transform;
                        vuci.anglesOpened = vu.AnglesOpened;
                        vuci.anglesClosed = vu.AnglesClosed;
                        vuci.collider = vuci.GetComponentInChildren<Collider>();
                        mv.upgradesInput = vuci;
                        var up = vu.Interface.EnsureComponent<UpgradeProxy>();
                        up.proxies = vu.ModuleProxies;
                    }
                    if(mv.Upgrades.Count() == 0)
                    {
                        VehicleUpgradeConsoleInput vuci = mv.VehicleModel.EnsureComponent<VehicleUpgradeConsoleInput>();
                        vuci.enabled = false;
                        mv.upgradesInput = vuci;
                    }
                }
                else
                {
                    Logger.Warn("The ModVehicle.Upgrades was null.");
                }
            }
            catch (Exception e)
            {
                Logger.Error("There was a problem setting up the Upgrades Interface. Check VehicleUpgrades.Interface and .Flap");
                Logger.Error(e.ToString());
                return false;
            }
            try
            {
                if (mv.BoundingBoxCollider == null)
                {
                    mv.BoundingBoxCollider = mv.BoundingBox.GetComponentInChildren<BoxCollider>(true);
                    mv.BoundingBoxCollider.enabled = false;
                }
            }
            catch (Exception e)
            {
                Logger.Warn("There was a problem setting up the BoundingBoxCollider. If your vehicle uses 'BoundingBox', use 'BoundingBoxCollider' instead.");
                Logger.Warn(e.Message);
                try
                {
                    mv.BoundingBoxCollider = mv.gameObject.AddComponent<BoxCollider>();
                    mv.BoundingBoxCollider.size = new Vector3(6, 8, 12);
                    mv.BoundingBoxCollider.enabled = false;
                    Logger.Warn("The " + mv.name + " has been given a default BoundingBox of size 6x8x12.");
                }
                catch
                {
                    Logger.Error("I couldn't provide a bounding box for this vehicle. See the following error:");
                    Logger.Error(e.ToString());
                    return false;
                }
            }
            return true;
        }
        public static bool SetupObjects(Submarine mv)
        {
            try
            {
                foreach (VehicleParts.VehiclePilotSeat ps in mv.PilotSeats)
                {
                    mv.playerPosition = ps.SitLocation;
                    PilotingTrigger pt = ps.Seat.EnsureComponent<PilotingTrigger>();
                    pt.mv = mv;
                    pt.exit = ps.ExitLocation;
                }
            }
            catch (Exception e)
            {
                Logger.Error("There was a problem setting up the PilotSeats. Check VehiclePilotSeat.Seat");
                Logger.Error(e.ToString());
                return false;
            }
            try
            {
                foreach (VehicleParts.VehicleHatchStruct vhs in mv.Hatches)
                {
                    var hatch = vhs.Hatch.EnsureComponent<VehicleHatch>();
                    hatch.mv = mv;
                    hatch.EntryLocation = vhs.EntryLocation;
                    hatch.ExitLocation = vhs.ExitLocation;
                    hatch.SurfaceExitLocation = vhs.SurfaceExitLocation;
                }
            }
            catch (Exception e)
            {
                Logger.Error("There was a problem setting up the Hatches. Check VehicleHatchStruct.Hatch");
                Logger.Error(e.ToString());
                return false;
            }
            // Configure the Control Panel
            try
            {
                if (mv.ControlPanel)
                {
                    mv.controlPanelLogic = mv.ControlPanel.EnsureComponent<ControlPanel>();
                    mv.controlPanelLogic.mv = mv;
                    mv.ControlPanel.transform.localPosition = mv.transform.Find("Control-Panel-Location").localPosition;
                    mv.ControlPanel.transform.localRotation = mv.transform.Find("Control-Panel-Location").localRotation;
                    GameObject.Destroy(mv.transform.Find("Control-Panel-Location").gameObject);
                }
            }
            catch (Exception e)
            {
                Logger.Error("There was a problem setting up the Control Panel. Check ModVehicle.ControlPanel and ensure \"Control-Panel-Location\" exists at the top level of your model. While you're at it, check that \"Fabricator-Location\" is at the top level of your model too.");
                Logger.Error(e.ToString());
                return false;
            }
            return true;
        }
        public static bool SetupObjects(Submersible mv)
        {
            try
            {
                mv.playerPosition = mv.PilotSeat.SitLocation;
                PilotingTrigger pt = mv.PilotSeat.Seat.EnsureComponent<PilotingTrigger>();
                pt.mv = mv;
                pt.exit = mv.PilotSeat.ExitLocation;
            }
            catch (Exception e)
            {
                Logger.Error("There was a problem setting up the PilotSeats. Check VehiclePilotSeat.Seat");
                Logger.Error(e.ToString());
                return false;
            }
            try
            {
                foreach (VehicleParts.VehicleHatchStruct vhs in mv.Hatches)
                {
                    var hatch = vhs.Hatch.EnsureComponent<VehicleHatch>();
                    hatch.mv = mv;
                    hatch.EntryLocation = vhs.EntryLocation;
                    hatch.ExitLocation = vhs.ExitLocation;
                    hatch.SurfaceExitLocation = vhs.SurfaceExitLocation;
                }
            }
            catch (Exception e)
            {
                Logger.Error("There was a problem setting up the Hatches. Check VehicleHatchStruct.Hatch");
                Logger.Error(e.ToString());
                return false;
            }
            // Configure the Control Panel
            return true;
        }
        public static void SetupEnergyInterface(ModVehicle mv)
        {
            var seamothEnergyMixin = SeamothHelper.Seamoth.GetComponent<EnergyMixin>();
            List<EnergyMixin> energyMixins = new List<EnergyMixin>();
            foreach (VehicleParts.VehicleBattery vb in mv.Batteries)
            {
                // Configure energy mixin for this battery slot
                var energyMixin = vb.BatterySlot.EnsureComponent<EnergyMixin>();
                energyMixin.storageRoot = mv.StorageRootObject.GetComponent<ChildObjectIdentifier>();
                energyMixin.defaultBattery = seamothEnergyMixin.defaultBattery;
                energyMixin.compatibleBatteries = seamothEnergyMixin.compatibleBatteries;
                energyMixin.soundPowerUp = seamothEnergyMixin.soundPowerUp;
                energyMixin.soundPowerDown = seamothEnergyMixin.soundPowerDown;
                energyMixin.soundBatteryAdd = seamothEnergyMixin.soundBatteryAdd;
                energyMixin.soundBatteryRemove = seamothEnergyMixin.soundBatteryRemove;
                energyMixin.batteryModels = seamothEnergyMixin.batteryModels;
                energyMixins.Add(energyMixin);
                var tmp = vb.BatterySlot.EnsureComponent<VehicleBatteryInput>();
                tmp.mixin = energyMixin;
                tmp.tooltip = EnglishString.VehicleBattery;

                var model = vb.BatterySlot.gameObject.EnsureComponent<StorageComponents.BatteryProxy>();
                model.proxy = vb.BatteryProxy;
                model.mixin = energyMixin;
            }
            // Configure energy interface
            var eInterf = mv.gameObject.EnsureComponent<EnergyInterface>();
            eInterf.sources = energyMixins.ToArray();
            mv.energyInterface = eInterf;

            mv.chargingSound = mv.gameObject.AddComponent<FMOD_CustomLoopingEmitter>();
            mv.chargingSound.asset = SeamothHelper.Seamoth.GetComponent<SeaMoth>().chargingSound.asset;
        }
        public static void SetupAIEnergyInterface(ModVehicle mv)
        {
            if (mv.BackupBatteries == null || mv.BackupBatteries.Count == 0)
            {
                mv.AIEnergyInterface = mv.energyInterface;
                return;
            }
            var seamothEnergyMixin = SeamothHelper.Seamoth.GetComponent<EnergyMixin>();
            List<EnergyMixin> energyMixins = new List<EnergyMixin>();
            foreach (VehicleParts.VehicleBattery vb in mv.BackupBatteries)
            {
                // Configure energy mixin for this battery slot
                var em = vb.BatterySlot.EnsureComponent<EnergyMixin>();
                em.storageRoot = mv.StorageRootObject.GetComponent<ChildObjectIdentifier>();
                em.defaultBattery = seamothEnergyMixin.defaultBattery;
                em.compatibleBatteries = new List<TechType>() { TechType.PowerCell, TechType.PrecursorIonPowerCell };
                em.soundPowerUp = seamothEnergyMixin.soundPowerUp;
                em.soundPowerDown = seamothEnergyMixin.soundPowerDown;
                em.soundBatteryAdd = seamothEnergyMixin.soundBatteryAdd;
                em.soundBatteryRemove = seamothEnergyMixin.soundBatteryRemove;
                em.batteryModels = seamothEnergyMixin.batteryModels;

                energyMixins.Add(em);

                var tmp = vb.BatterySlot.EnsureComponent<VehicleBatteryInput>();
                tmp.mixin = em;
                tmp.tooltip = EnglishString.AutoPilotBattery;

                var model = vb.BatterySlot.gameObject.EnsureComponent<StorageComponents.BatteryProxy>();
                model.proxy = vb.BatteryProxy;
                model.mixin = em;
            }
            // Configure energy interface
            mv.AIEnergyInterface = mv.BackupBatteries.First().BatterySlot.EnsureComponent<EnergyInterface>();
            mv.AIEnergyInterface.sources = energyMixins.ToArray();
        }
        public static void SetupLightSounds(ModVehicle mv)
        {
            FMOD_StudioEventEmitter[] fmods = SeamothHelper.Seamoth.GetComponents<FMOD_StudioEventEmitter>();
            foreach (FMOD_StudioEventEmitter fmod in fmods)
            {
                if (fmod.asset.name == "seamoth_light_on")
                {
                    var ce = mv.gameObject.AddComponent<FMOD_CustomEmitter>();
                    ce.asset = fmod.asset;
                    mv.lightsOnSound = ce;
                }
                else if (fmod.asset.name == "seamoth_light_off")
                {
                    var ce = mv.gameObject.AddComponent<FMOD_CustomEmitter>();
                    ce.asset = fmod.asset;
                    mv.lightsOffSound = ce;
                }
            }
        }
        public static void SetupHeadLights(ModVehicle mv)
        {
            GameObject seamothHeadLight = SeamothHelper.Seamoth.transform.Find("lights_parent/light_left").gameObject;
            Transform seamothVL = SeamothHelper.Seamoth.transform.Find("lights_parent/light_left/x_FakeVolumletricLight"); // sic
            MeshFilter seamothVLMF = seamothVL.GetComponent<MeshFilter>();
            MeshRenderer seamothVLMR = seamothVL.GetComponent<MeshRenderer>();
            if (mv.HeadLights != null)
            {
                foreach (VehicleParts.VehicleFloodLight pc in mv.HeadLights)
                {
                    CopyComponent(seamothHeadLight.GetComponent<LightShadowQuality>(), pc.Light);
                    var thisLight = pc.Light.EnsureComponent<Light>();
                    thisLight.type = LightType.Spot;
                    thisLight.spotAngle = pc.Angle;
                    thisLight.innerSpotAngle = pc.Angle * .75f;
                    thisLight.color = pc.Color;
                    thisLight.intensity = pc.Intensity;
                    thisLight.range = pc.Range;
                    thisLight.shadows = LightShadows.Hard;
                    thisLight.gameObject.SetActive(false);

                    GameObject volumetricLight = pc.Light.transform.Find("VolumetricLight").gameObject;
                    volumetricLight.transform.localPosition = Vector3.zero;
                    volumetricLight.transform.localEulerAngles = Vector3.zero;
                    volumetricLight.transform.parent = pc.Light.transform;
                    volumetricLight.transform.localScale = seamothVL.localScale;

                    var lvlMeshFilter = volumetricLight.AddComponent<MeshFilter>();
                    lvlMeshFilter.mesh = seamothVLMF.mesh;
                    lvlMeshFilter.sharedMesh = seamothVLMF.sharedMesh;

                    var lvlMeshRenderer = volumetricLight.AddComponent<MeshRenderer>();
                    lvlMeshRenderer.material = seamothVLMR.material;
                    lvlMeshRenderer.sharedMaterial = seamothVLMR.sharedMaterial;
                    lvlMeshRenderer.shadowCastingMode = seamothVLMR.shadowCastingMode;
                    lvlMeshRenderer.renderingLayerMask = seamothVLMR.renderingLayerMask;

                    var leftVFX = CopyComponent(seamothHeadLight.GetComponent<VFXVolumetricLight>(), pc.Light);
                    leftVFX.lightSource = thisLight;
                    leftVFX.color = pc.Color;
                    leftVFX.volumGO = volumetricLight;
                    leftVFX.volumRenderer = lvlMeshRenderer;
                    leftVFX.volumMeshFilter = lvlMeshFilter;
                    leftVFX.angle = (int)pc.Angle;
                    leftVFX.range = pc.Range;
                    mv.lights.Add(pc.Light);
                    mv.volumetricLights.Add(volumetricLight);

                    var RLS = mv.gameObject.AddComponent<RegistredLightSource>();
                    RLS.hostLight = thisLight;
                }
            }
        }
        public static void SetupFloodLights(Submarine mv)
        {
            GameObject seamothHeadLight = SeamothHelper.Seamoth.transform.Find("lights_parent/light_left").gameObject;
            Transform seamothVL = SeamothHelper.Seamoth.transform.Find("lights_parent/light_left/x_FakeVolumletricLight"); // sic
            MeshFilter seamothVLMF = seamothVL.GetComponent<MeshFilter>();
            MeshRenderer seamothVLMR = seamothVL.GetComponent<MeshRenderer>();
            if (mv.FloodLights != null)
            {
                foreach (VehicleParts.VehicleFloodLight pc in mv.FloodLights)
                {
                    CopyComponent(seamothHeadLight.GetComponent<LightShadowQuality>(), pc.Light);
                    var thisLight = pc.Light.EnsureComponent<Light>();
                    thisLight.type = LightType.Spot;
                    thisLight.spotAngle = pc.Angle;
                    thisLight.innerSpotAngle = pc.Angle * .75f;
                    thisLight.color = pc.Color;
                    thisLight.intensity = pc.Intensity;
                    thisLight.range = pc.Range;
                    thisLight.shadows = LightShadows.Hard;
                    pc.Light.SetActive(false);

                    var RLS = mv.gameObject.AddComponent<RegistredLightSource>();
                    RLS.hostLight = thisLight;
                }
            }
        }
        public static void SetupLiveMixin(ModVehicle mv)
        {
            var liveMixin = mv.gameObject.EnsureComponent<LiveMixin>();
            var lmData = ScriptableObject.CreateInstance<LiveMixinData>();
            lmData.canResurrect = true;
            lmData.broadcastKillOnDeath = true;
            lmData.destroyOnDeath = false;
            // NEWNEW
            // What's going to happen when a vdehicle dies now?
            //lmData.explodeOnDestroy = true;
            lmData.invincibleInCreative = true;
            lmData.weldable = true;
            lmData.minDamageForSound = 20f;
            /*
             * Other Max Health Values
             * Seamoth: 200
             * Prawn: 600
             * Odyssey: 667
             * Atrama: 1000
             * Abyss: 1250
             * Cyclops: 1500
             */
            lmData.maxHealth = mv.MaxHealth;
            liveMixin.health = mv.MaxHealth;
            liveMixin.data = lmData;
            mv.liveMixin = liveMixin;
        }
        public static void SetupRigidbody(ModVehicle mv)
        {
            var rb = mv.gameObject.EnsureComponent<Rigidbody>();
            /* 
             * For reference,
             * Cyclop: 12000
             * Abyss: 5000
             * Atrama: 4250
             * Odyssey: 3500
             * Prawn: 1250
             * Seamoth: 800
             */
            rb.mass = mv.Mass;
            rb.drag = 10f;
            rb.angularDrag = 10f;
            rb.useGravity = false;
            mv.useRigidbody = rb;
        }
        public static void SetupEngine(Submarine mv)
        {
            if(mv.Engine == null)
            {
                mv.Engine = mv.gameObject.AddComponent<OdysseyEngine>();
            }
            // Add the engine (physics control)
            mv.Engine.mv = mv;
            mv.Engine.rb = mv.useRigidbody;
        }
        public static void SetupEngine(Submersible mv)
        {
            if (mv.Engine == null)
            {
                mv.Engine = mv.gameObject.AddComponent<CricketEngine>();
            }
            // Add the engine (physics control)
            mv.Engine.mv = mv;
            mv.Engine.rb = mv.useRigidbody;
        }
        public static void SetupEngine(Drone mv)
        {
            if (mv.Engine == null)
            {
                mv.Engine = mv.gameObject.AddComponent<CricketEngine>();
            }
            // Add the engine (physics control)
            mv.Engine.mv = mv;
            mv.Engine.rb = mv.useRigidbody;
        }
        public static void SetupWorldForces(ModVehicle mv)
        {
            mv.worldForces = CopyComponent<WorldForces>(SeamothHelper.Seamoth.GetComponent<SeaMoth>().worldForces, mv.gameObject);
            mv.worldForces.useRigidbody = mv.useRigidbody;
            mv.worldForces.underwaterGravity = 0f;
            mv.worldForces.aboveWaterGravity = 9.8f;
            mv.worldForces.waterDepth = 0f;
        }
        public static void SetupLargeWorldEntity(ModVehicle mv)
        {
            // Ensure vehicle remains in the world always
            mv.gameObject.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Global;
        }
        public static void SetupHudPing(ModVehicle mv, PingType pingType)
        {
            mv.pingInstance = mv.gameObject.EnsureComponent<PingInstance>();
            mv.pingInstance.origin = mv.transform;
            mv.pingInstance.pingType = pingType;
            mv.pingInstance.SetLabel("Vehicle");
            VehicleManager.mvPings.Add(mv.pingInstance);
        }
        public static void SetupVehicleConfig(ModVehicle mv)
        {
            // add various vehicle things
            mv.stabilizeRoll = true;
            mv.controlSheme = (Vehicle.ControlSheme)12;
            mv.mainAnimator = mv.gameObject.EnsureComponent<Animator>();
            mv.ambienceSound = CopyComponent<FMOD_StudioEventEmitter>(SeamothHelper.Seamoth.GetComponent<SeaMoth>().ambienceSound, mv.gameObject);
            mv.splashSound = SeamothHelper.Seamoth.GetComponent<SeaMoth>().splashSound;
            // TODO
            //atrama.vehicle.bubbles = CopyComponent<ParticleSystem>(seamoth.GetComponent<SeaMoth>().bubbles, atrama.vehicle.gameObject);
        }
        public static void SetupCrushDamage(ModVehicle mv)
        {
            var ce = mv.gameObject.AddComponent<FMOD_CustomEmitter>();
            ce.restartOnPlay = true;
            foreach (var thisCE in SeamothHelper.Seamoth.GetComponentsInChildren<FMOD_CustomEmitter>())
            {
                if (thisCE.name == "crushDamageSound")
                {
                    ce.asset = thisCE.asset;
                }
            }
            /* For reference,
             * Prawn dies from max health in 3:00 minutes.
             * Seamoth in 0:30
             * Cyclops in 3:45
             * So ModVehicles can die in 3:00 as well
             */
            mv.crushDamage = mv.gameObject.EnsureComponent<CrushDamage>();
            mv.crushDamage.soundOnDamage = ce;
            mv.crushDamage.kBaseCrushDepth = mv.BaseCrushDepth;
            mv.crushDamage.damagePerCrush = mv.CrushDamage;
            mv.crushDamage.crushPeriod = mv.CrushPeriod;
            mv.crushDamage.vehicle = mv;
            mv.crushDamage.liveMixin = mv.liveMixin;
            // TODO: this is of type VoiceNotification
            mv.crushDamage.crushDepthUpdate = null;
        }
        public static void SetupWaterClipping(ModVehicle mv)
        {
            if (mv.WaterClipProxies != null)
            {
                // Enable water clipping for proper interaction with the surface of the ocean
                WaterClipProxy seamothWCP = SeamothHelper.Seamoth.GetComponentInChildren<WaterClipProxy>();
                foreach (GameObject proxy in mv.WaterClipProxies)
                {
                    WaterClipProxy waterClip = proxy.AddComponent<WaterClipProxy>();
                    waterClip.shape = WaterClipProxy.Shape.Box;
                    //"""Apply the seamoth's clip material. No idea what shader it uses or what settings it actually has, so this is an easier option. Reuse the game's assets.""" -Lee23
                    waterClip.clipMaterial = seamothWCP.clipMaterial;
                    //"""You need to do this. By default the layer is 0. This makes it displace everything in the default rendering layer. We only want to displace water.""" -Lee23
                    waterClip.gameObject.layer = seamothWCP.gameObject.layer;
                }
            }
        }
        public static void SetupSubName(ModVehicle mv)
        {
            var subname = mv.gameObject.EnsureComponent<SubName>();
            subname.pingInstance = mv.pingInstance;
            subname.colorsInitialized = 0;
            subname.hullName = mv.StorageRootObject.AddComponent<TMPro.TextMeshProUGUI>(); // DO NOT push a TMPro.TextMeshProUGUI on the root vehicle object!!!
            subname.hullName.text = mv.vehicleName;
            mv.subName = subname;
        }
        public static void SetupCollisionSound(ModVehicle mv)
        {
            var colsound = mv.gameObject.EnsureComponent<CollisionSound>();
            var seamothColSound = SeamothHelper.Seamoth.GetComponent<CollisionSound>();
            colsound.hitSoundSmall = seamothColSound.hitSoundSmall;
            colsound.hitSoundSlow = seamothColSound.hitSoundSlow;
            colsound.hitSoundMedium = seamothColSound.hitSoundMedium;
            colsound.hitSoundFast = seamothColSound.hitSoundFast;
        }
        public static void SetupOutOfBoundsWarp(ModVehicle mv)
        {
            mv.gameObject.EnsureComponent<OutOfBoundsWarp>();
        }
        public static void SetupConstructionObstacle(ModVehicle mv)
        {
            var co = mv.gameObject.EnsureComponent<ConstructionObstacle>();
            co.reason = mv.name + " is in the way.";
        }
        public static void SetupSoundOnDamage(ModVehicle mv)
        {
            // TODO: we could have unique sounds for each damage type
            // TODO: this might not work, might need to put it in a VehicleStatusListener
            var sod = mv.gameObject.EnsureComponent<SoundOnDamage>();
            sod.damageType = DamageType.Normal;
            sod.sound = SeamothHelper.Seamoth.GetComponent<SoundOnDamage>().sound;
        }
        public static void SetupDealDamageOnImpact(ModVehicle mv)
        {
            var ddoi = mv.gameObject.EnsureComponent<DealDamageOnImpact>();
            // NEWNEW
            // ddoi.damageTerrain = true;
            ddoi.speedMinimumForSelfDamage = 4;
            ddoi.speedMinimumForDamage = 2;
            ddoi.affectsEcosystem = true;
            ddoi.minimumMassForDamage = 5;
            ddoi.mirroredSelfDamage = true;
            ddoi.mirroredSelfDamageFraction = 0.5f;
            ddoi.capMirrorDamage = -1;
            ddoi.minDamageInterval = 0;
            ddoi.timeLastDamage = 0;
            ddoi.timeLastDamagedSelf = 0;
            ddoi.prevPosition = Vector3.zero;
            ddoi.prevPosition = Vector3.zero;
            ddoi.allowDamageToPlayer = false;
        }
        public static void SetupDamageComponents(ModVehicle mv)
        {
            // add vfxvehicledamages... or not

            // add temperaturedamage
            var tempdamg = mv.gameObject.EnsureComponent<TemperatureDamage>();
            tempdamg.lavaDatabase = SeamothHelper.Seamoth.GetComponent<TemperatureDamage>().lavaDatabase;
            tempdamg.liveMixin = mv.liveMixin;
            tempdamg.baseDamagePerSecond = 2.0f; // 10 times what the seamoth takes, since the Atrama 
            // the following configurations are the same values the seamoth takes
            tempdamg.minDamageTemperature = 70f;
            tempdamg.onlyLavaDamage = false;
            tempdamg.timeDamageStarted = -1000;
            tempdamg.timeLastDamage = 0;
            tempdamg.player = null;

            // add ecotarget
            var et = mv.gameObject.EnsureComponent<EcoTarget>();
            et.type = EcoTargetType.Shark; // same as seamoth (lol)
            et.nextUpdateTime = 0f;

            // add creatureutils
            var cr = mv.gameObject.EnsureComponent<CreatureUtils>();
            cr.setupEcoTarget = true;
            cr.setupEcoBehaviours = false;
            cr.addedComponents = new Component[1];
            cr.addedComponents.Append(et as Component);

        }
        public static void SetupRespawnPoint(Submarine mv)
        {
            if (mv.TetherSources.Count > 0)
            {
                var subroot = mv.gameObject.EnsureComponent<SubRoot>();
                subroot.rb = mv.useRigidbody;
                subroot.worldForces = mv.worldForces;
                var tmp = mv.TetherSources.First();
                tmp.EnsureComponent<RespawnPoint>();
            }
        }
        public static void SetupDroneObjects(Drone drone)
        {
        }

        #endregion
        public static bool Instrument(ModVehicle mv, PingType pingType)
        {
            mv.StorageRootObject.EnsureComponent<ChildObjectIdentifier>();
            mv.modulesRoot = mv.ModulesRootObject.EnsureComponent<ChildObjectIdentifier>();
            
            if(!SetupObjects(mv as ModVehicle))
            {
                Logger.Error("Failed to SetupObjects for ModVehicle.");
                return false;
            }
            if ((mv as Submarine != null) && !SetupObjects(mv as Submarine))
            {
                Logger.Error("Failed to SetupObjects for Submarine.");
                return false;
            }
            if ((mv as Submersible != null) && !SetupObjects(mv as Submersible))
            {
                Logger.Error("Failed to SetupObjects for Submersible.");
                return false;
            }
            mv.enabled = false;
            SetupEnergyInterface(mv);
            SetupAIEnergyInterface(mv);
            mv.enabled = true;
            SetupHeadLights(mv);
            SetupLightSounds(mv);
            SetupLiveMixin(mv);
            SetupRigidbody(mv);
            SetupWorldForces(mv);
            SetupLargeWorldEntity(mv);
            SetupHudPing(mv, pingType);
            SetupVehicleConfig(mv);
            SetupCrushDamage(mv);
            SetupWaterClipping(mv);
            SetupSubName(mv);
            SetupCollisionSound(mv);
            SetupOutOfBoundsWarp(mv);
            SetupConstructionObstacle(mv);
            SetupSoundOnDamage(mv);
            SetupDealDamageOnImpact(mv);
            SetupDamageComponents(mv);
            mv.collisionModel = mv.CollisionModel;

            if (mv as Submarine != null)
            {
                SetupEngine(mv as Submarine);
                SetupFloodLights(mv as Submarine);
                SetupRespawnPoint(mv as Submarine); // depends on SetupWorldForces
                mv.gameObject.AddComponent<PowerRelay>(); // See PowerRelayPatcher. Allows Submarines to recharge batteries.
            }
            if (mv as Submersible != null)
            {
                SetupEngine(mv as Submersible);
            }
            if (mv as Drone != null)
            {
                SetupEngine(mv as Drone);
                SetupDroneObjects(mv as Drone);
            }
            ApplySkyAppliers(mv);

            // ApplyShaders should happen last
            Shader shader = Shader.Find("MarmosetUBER");
            ApplyShaders(mv, shader);

            return true;
        }
        public static void ApplyGlassMaterial(ModVehicle mv)
        {
            // Add the [marmoset] shader to all renderers
            foreach (var renderer in mv.gameObject.GetComponentsInChildren<Renderer>(true))
            {
                if (mv.CanopyWindows != null && mv.CanopyWindows.Contains(renderer.gameObject))
                {
                    var seamothGlassMaterial = SeamothHelper.Seamoth.transform.Find("Model/Submersible_SeaMoth/Submersible_seaMoth_geo/Submersible_SeaMoth_glass_interior_geo").GetComponent<SkinnedMeshRenderer>().material;
                    renderer.material = seamothGlassMaterial;
                    renderer.material = seamothGlassMaterial; // this is the right line
                    continue;
                }
            }
        }
        public static void ApplyShaders(ModVehicle mv, Shader shader)
        {
            if (mv.AutoApplyShaders)
            {
                ForceApplyShaders(mv, shader);
                ApplyGlassMaterial(mv);
            }
        }
        public static void ForceApplyShaders(ModVehicle mv, Shader shader)
        {
            if(shader == null)
            {
                Logger.Error("Tried to apply a null Shader.");
                return;
            }
            // Add the [marmoset] shader to all renderers
            foreach (var renderer in mv.gameObject.GetComponentsInChildren<Renderer>(true))
            {
                // skip some materials
                if (renderer.gameObject.name.ToLower().Contains("light"))
                {
                    continue;
                }
                if(mv.CanopyWindows != null && mv.CanopyWindows.Contains(renderer.gameObject))
                {
                    continue;
                }
                foreach (Material mat in renderer.materials)
                {
                    // give it the marmo shader, no matter what
                    mat.shader = shader;
                }
            }
        }
        public static void ApplySkyAppliers(ModVehicle mv)
        {
            var ska = mv.gameObject.EnsureComponent<SkyApplier>();
            ska.anchorSky = Skies.Auto;
            ska.customSkyPrefab = null;
            ska.dynamic = true;
            ska.emissiveFromPower = false;
            ska.environmentSky = null;

            var rends = mv.gameObject.GetComponentsInChildren<Renderer>();
            ska.renderers = new Renderer[rends.Count()];
            foreach(var rend in rends)
            {
                ska.renderers.Append(rend);
            }

        }
        public static T CopyComponent<T>(T original, GameObject destination) where T : Component
        {
            System.Type type = original.GetType();
            Component copy = destination.EnsureComponent(type);
            System.Reflection.FieldInfo[] fields = type.GetFields();
            foreach (System.Reflection.FieldInfo field in fields)
            {
                field.SetValue(copy, field.GetValue(original));
            }
            return copy as T;
        }

        public static string GetPingTypeString(CachedEnumString<PingType> cache, PingType inputType)
        {
            foreach(VehicleEntry ve in VehicleManager.vehicleTypes)
            {
                if (ve.pt == inputType)
                {
                    return ve.name;
                }
            }
            return PingManager.sCachedPingTypeStrings.Get(inputType);
        }
        public static Atlas.Sprite GetPingTypeSprite(SpriteManager.Group group, string name)
        {
            foreach (VehicleEntry ve in VehicleManager.vehicleTypes)
            {
                if (ve.name == name)
                {
                    return ve.ping_sprite;
                }
            }
            return SpriteManager.Get(SpriteManager.Group.Pings, name);
        }

        /*
        //https://github.com/Metious/MetiousSubnauticaMods/blob/master/CustomDataboxes/API/Databox.cs
        public static void VehicleDataboxPatch(CustomDataboxes.API.Databox databox)
        {
            string result = "";

            if (string.IsNullOrEmpty(databox.DataboxID))
                result += "Missing required Info 'DataboxID'\n";
            if (string.IsNullOrEmpty(databox.PrimaryDescription))
                result += "Missing required Info 'PrimaryDescription'\n";
            if (!string.IsNullOrEmpty(result))
            {
                string msg = "Unable to patch\n" + result;
                Logger.Log(msg);
                throw new InvalidOperationException(msg);
            }

            var dataBox = new CustomDataboxes.Databoxes.CustomDatabox(DataboxID)
            {
                PrimaryDescription = this.PrimaryDescription,
                SecondaryDescription = this.SecondaryDescription,
                TechTypeToUnlock = this.TechTypeToUnlock,
                BiomesToSpawn = BiomesToSpawnIn,
                coordinatedSpawns = CoordinatedSpawns,
                ModifyGameObject = this.ModifyGameObject
            };
            dataBox.Patch();

            TechType = dataBox.TechType;
        }
        public static void ScatterDataBoxes(List<VehicleCraftable> craftables)
        {
            List<Spawnable.SpawnLocation> spawnLocations = new List<Spawnable.SpawnLocation>
            {
                new Spawnable.SpawnLocation(Vector3.zero, Vector3.zero),
                new Spawnable.SpawnLocation(new Vector3(50,0,0), Vector3.zero),
                new Spawnable.SpawnLocation(new Vector3(100,0,0), Vector3.zero),
                new Spawnable.SpawnLocation(new Vector3(200,0,0), Vector3.zero),
                new Spawnable.SpawnLocation(new Vector3(400,0,0), Vector3.zero),
            };

            foreach (var craftable in craftables)
            {
                CustomDataboxes.API.Databox myDatabox = new CustomDataboxes.API.Databox()
                {
                    DataboxID = craftable.ClassID + "_databox",
                    PrimaryDescription = craftable.FriendlyName + "_databox",
                    SecondaryDescription = "wow so cool",
                    CoordinatedSpawns = spawnLocations,
                    TechTypeToUnlock = craftable.TechType
                };
                myDatabox.Patch();
            }
        }
        */
    }
}

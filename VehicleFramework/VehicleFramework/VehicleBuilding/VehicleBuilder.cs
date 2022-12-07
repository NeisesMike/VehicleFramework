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

using SMLHelper;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using VehicleFramework.Engines;

namespace VehicleFramework
{
    public struct VehicleEntry
    {
        public VehicleEntry(GameObject prefabObj, ModVehicleEngine in_engine, Dictionary<TechType, int> in_recipe, int id, string desc, string in_encyEntry, PingType pt_in, Atlas.Sprite sprite, int modules_in, int arms_in)
        {
            prefab = prefabObj;
            engine = in_engine;
            recipe = in_recipe;
            unique_id = id;
            description = desc;
            encyEntry = in_encyEntry;
            pt = pt_in;
            ping_sprite = sprite;
            modules = modules_in;
            arms = arms_in;
        }
        public GameObject prefab;
        public ModVehicleEngine engine;
        public Dictionary<TechType, int> recipe;
        public int unique_id;
        public string description;
        public string encyEntry;
        public PingType pt;
        public Atlas.Sprite ping_sprite;
        public int modules;
        public int arms;
    }

    public static class VehicleBuilder
    {
        public static GameObject moduleBuilder;

        private static int numVehicleTypes = 0;
        public static List<ModVehicle> prefabs = new List<ModVehicle>();
        public static GameObject seamoth = CraftData.GetPrefabForTechType(TechType.Seamoth, true);
        public static GameObject upgradeconsole = CraftData.GetPrefabForTechType(TechType.BaseUpgradeConsole, true);
        public static GameObject coroutineHelper;
        public static GameObject powercell = GameObject.CreatePrimitive(PrimitiveType.Capsule);//CraftData.GetPrefabForTechType(TechType.PowerCell, true); // TODO is this right?

        public const EquipmentType ModuleType = (EquipmentType)625;
        public const EquipmentType ArmType = (EquipmentType)626;
        public const TechType InnateStorage = (TechType)0x4100;

        public static void Prefabricate(ref ModVehicle mv, ModVehicleEngine engine, Dictionary<TechType, int> recipe, PingType pingType, Atlas.Sprite sprite, int modules, int arms, int baseCrushDepth, int maxHealth)
        {
            mv.numVehicleModules = modules;
            mv.hasArms = arms > 0;

            Instrument(ref mv, engine, pingType, baseCrushDepth, maxHealth);
            prefabs.Add(mv);
            VehicleEntry ve = new VehicleEntry(mv.gameObject, engine, recipe, numVehicleTypes, mv.GetDescription(), mv.GetEncyEntry(), pingType, sprite, modules, arms);
            VehicleManager.vehicleTypes.Add(ve);
            numVehicleTypes++;
        }

        #region setup_funcs
        public static void SetupPrefabObjects(ref ModVehicle mv)
        {
            int iter = 0;
            foreach (VehicleParts.VehiclePilotSeat ps in mv.PilotSeats)
            {
                mv.playerPosition = ps.SitLocation;
                PilotingTrigger pt = ps.Seat.EnsureComponent<PilotingTrigger>();
                pt.mv = mv;
                pt.exit = ps.ExitLocation;
            }
            foreach (VehicleParts.VehicleHatchStruct vhs in mv.Hatches)
            {
                var hatch = vhs.Hatch.EnsureComponent<VehicleHatch>();
                hatch.mv = mv;
                hatch.EntryLocation = vhs.EntryLocation;
                hatch.ExitLocation = vhs.ExitLocation;
                hatch.SurfaceExitLocation = vhs.SurfaceExitLocation;
            }
            foreach (VehicleParts.VehicleStorage vs in mv.InnateStorages)
            {
                vs.Container.SetActive(false);

                var cont = vs.Container.EnsureComponent<InnateStorageContainer>();
                cont.storageRoot = mv.StorageRootObject.GetComponent<ChildObjectIdentifier>();
                cont.storageLabel = "Vehicle Storage " + iter.ToString();
                cont.height = vs.Height;
                cont.width = vs.Width;

                FMODAsset storageCloseSound = seamoth.transform.Find("Storage/Storage1").GetComponent<SeamothStorageInput>().closeSound;
                FMODAsset storageOpenSound = seamoth.transform.Find("Storage/Storage1").GetComponent<SeamothStorageInput>().openSound;
                var inp = vs.Container.EnsureComponent<InnateStorageInput>();
                inp.mv = mv;
                inp.slotID = iter;
                iter++;
                inp.model = vs.Container;
                inp.collider = vs.Container.EnsureComponent<BoxCollider>();
                inp.openSound = storageOpenSound;
                inp.closeSound = storageCloseSound;
                vs.Container.SetActive(true);
            }
            iter = 0;
            foreach (VehicleParts.VehicleStorage vs in mv.ModularStorages)
            {
                vs.Container.SetActive(false);

                /*
                var cont = vs.Container.EnsureComponent<SeamothStorageContainer>();
                cont.storageRoot = mv.StorageRootObject.GetComponent<ChildObjectIdentifier>();
                cont.storageLabel = "Modular Storage " + iter.ToString();
                cont.height = vs.Height;
                cont.width = vs.Width;
                */

                FMODAsset storageCloseSound = seamoth.transform.Find("Storage/Storage1").GetComponent<SeamothStorageInput>().closeSound;
                FMODAsset storageOpenSound = seamoth.transform.Find("Storage/Storage1").GetComponent<SeamothStorageInput>().openSound;
                var inp = vs.Container.EnsureComponent<ModularStorageInput>();
                inp.mv = mv;
                inp.slotID = iter;
                iter++;
                inp.model = vs.Container;
                inp.collider = vs.Container.EnsureComponent<BoxCollider>();
                inp.openSound = storageOpenSound;
                inp.closeSound = storageCloseSound;
            }
            foreach (VehicleParts.VehicleUpgrades vu in mv.Upgrades)
            {
                VehicleUpgradeConsoleInput vuci = vu.Interface.EnsureComponent<VehicleUpgradeConsoleInput>();
                vuci.flap = vu.Flap.transform;
                vuci.anglesOpened = vu.AnglesOpened;
                vuci.anglesClosed = vu.AnglesClosed;
                mv.upgradesInput = vuci;
                var up = vu.Interface.EnsureComponent<UpgradeProxy>();
                up.proxies = vu.ModuleProxies;
            }
            // Configure the Control Panel
            if (mv.ControlPanel)
            {
                mv.controlPanelLogic = mv.ControlPanel.EnsureComponent<ControlPanel>();
                mv.controlPanelLogic.mv = mv;
                mv.ControlPanel.transform.localPosition = mv.transform.Find("Control-Panel-Location").localPosition;
                mv.ControlPanel.transform.localRotation = mv.transform.Find("Control-Panel-Location").localRotation;
                GameObject.Destroy(mv.transform.Find("Control-Panel-Location").gameObject);
            }
        }
        public static void SetupEnergyInterface(ref ModVehicle mv)
        {
            var seamothEnergyMixin = seamoth.GetComponent<EnergyMixin>();
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
                tmp.tooltip = "Vehicle Battery";

                var model = vb.BatterySlot.gameObject.EnsureComponent<StorageComponents.BatteryProxy>();
                model.proxy = vb.BatteryProxy;
                model.mixin = energyMixin;
            }
            // Configure energy interface
            var eInterf = mv.gameObject.EnsureComponent<EnergyInterface>();
            eInterf.sources = energyMixins.ToArray();
            mv.energyInterface = eInterf;
        }
        public static void SetupAIEnergyInterface(ref ModVehicle mv)
        {
            if (mv.BackupBatteries == null)
            {
                Logger.Log("ERROR: Could not find AI battery gameobject(s) for vehicle: " + mv.name);
                return;
            }
            var seamothEnergyMixin = seamoth.GetComponent<EnergyMixin>();
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
                tmp.tooltip = "Autopilot Battery";

                var model = vb.BatterySlot.gameObject.EnsureComponent<StorageComponents.BatteryProxy>();
                model.proxy = vb.BatteryProxy;
                model.mixin = em;
            }
            // Configure energy interface
            mv.AIEnergyInterface = mv.BackupBatteries.First().BatterySlot.EnsureComponent<EnergyInterface>();
            mv.AIEnergyInterface.sources = energyMixins.ToArray();
        }
        public static void SetupLightSounds(ref ModVehicle mv)
        {
            FMOD_StudioEventEmitter[] fmods = seamoth.GetComponents<FMOD_StudioEventEmitter>();
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
        public static void SetupLights(ref ModVehicle mv)
        {
            GameObject seamothHeadLight = seamoth.transform.Find("lights_parent/light_left").gameObject;
            Transform seamothVL = seamoth.transform.Find("lights_parent/light_left/x_FakeVolumletricLight"); // sic
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
        public static void SetupLiveMixin(ref ModVehicle mv, int maxHealth)
        {
            var liveMixin = mv.gameObject.EnsureComponent<LiveMixin>();
            var lmData = ScriptableObject.CreateInstance<LiveMixinData>();
            lmData.canResurrect = true;
            lmData.broadcastKillOnDeath = true;
            lmData.destroyOnDeath = true;
            lmData.explodeOnDestroy = true;
            lmData.invincibleInCreative = true;
            lmData.weldable = true;
            lmData.minDamageForSound = 20f;
            /*
             * Other Max Health Values
             * Seamoth: 200
             * Prawn: 600
             * Odyssey: 667
             * Atrama: 1000
             * Cyclops: 1500
             */
            lmData.maxHealth = maxHealth;
            liveMixin.health = maxHealth;
            liveMixin.data = lmData;
            mv.liveMixin = liveMixin;
        }
        public static void SetupRigidbody(ref ModVehicle mv, int mass)
        {
            var rb = mv.gameObject.EnsureComponent<Rigidbody>();
            /* 
             * For reference,
             * Cyclop: 12000
             * Prawn: 1250
             * Seamoth: 800
             */
            rb.mass = mass;
            rb.drag = 10f;
            rb.angularDrag = 10f;
            rb.useGravity = false;
            mv.useRigidbody = rb;
        }
        public static void SetupEngine(ref ModVehicle mv, ModVehicleEngine engineType)
        {
            // Add the engine (physics control)
            mv.engine = mv.gameObject.AddComponent(engineType.GetType()) as ModVehicleEngine;
            mv.engine.mv = mv;
            mv.engine.rb = mv.useRigidbody;
        }
        public static void SetupWorldForces(ref ModVehicle mv)
        {
            mv.worldForces = CopyComponent<WorldForces>(seamoth.GetComponent<SeaMoth>().worldForces, mv.gameObject);
            mv.worldForces.useRigidbody = mv.useRigidbody;
            mv.worldForces.underwaterGravity = 0f;
            mv.worldForces.aboveWaterGravity = 9.8f;
            mv.worldForces.waterDepth = 0f;
        }
        public static void SetupLargeWorldEntity(ref ModVehicle mv)
        {
            // Ensure vehicle remains in the world always
            mv.gameObject.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Global;
        }
        public static void SetupHudPing(ref ModVehicle mv, PingType pingType)
        {
            mv.pingInstance = mv.gameObject.EnsureComponent<PingInstance>();
            mv.pingInstance.origin = mv.transform;
            mv.pingInstance.pingType = pingType;
            mv.pingInstance.SetLabel("Vehicle");
            VehicleManager.mvPings.Add(mv.pingInstance);
        }
        public static void SetupVehicleConfig(ref ModVehicle mv)
        {
            // add various vehicle things
            mv.stabilizeRoll = true;
            mv.controlSheme = (Vehicle.ControlSheme)12;
            mv.mainAnimator = mv.gameObject.EnsureComponent<Animator>();
            mv.ambienceSound = CopyComponent<FMOD_StudioEventEmitter>(seamoth.GetComponent<SeaMoth>().ambienceSound, mv.gameObject);
            mv.splashSound = seamoth.GetComponent<SeaMoth>().splashSound;
            // TODO
            //atrama.vehicle.bubbles = CopyComponent<ParticleSystem>(seamoth.GetComponent<SeaMoth>().bubbles, atrama.vehicle.gameObject);
        }
        public static void SetupCrushDamage(ref ModVehicle mv, int baseCrushDepth, int maxHealth, int secondsCanLiveAtCrushDepth, int crushPeriod)
        {
            var ce = mv.gameObject.AddComponent<FMOD_CustomEmitter>();
            ce.restartOnPlay = true;
            foreach (var thisCE in seamoth.GetComponentsInChildren<FMOD_CustomEmitter>())
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
            mv.crushDamage.kBaseCrushDepth = baseCrushDepth;
            mv.crushDamage.damagePerCrush = ((float)maxHealth)/(secondsCanLiveAtCrushDepth/ crushPeriod);
            mv.crushDamage.crushPeriod = crushPeriod;
            mv.crushDamage.vehicle = mv;
            mv.crushDamage.liveMixin = mv.liveMixin;
            // TODO: this is of type VoiceNotification
            mv.crushDamage.crushDepthUpdate = null;
        }
        public static void SetupWaterClipping(ref ModVehicle mv)
        {
            // Enable water clipping for proper interaction with the surface of the ocean
            WaterClipProxy seamothWCP = seamoth.GetComponentInChildren<WaterClipProxy>();
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
        public static void SetupSubName(ref ModVehicle mv)
        {
            // TODO
            var subname = mv.gameObject.EnsureComponent<SubName>();
            subname.pingInstance = mv.pingInstance;
            subname.colorsInitialized = 0;
            subname.hullName = mv.NameDecals[0].GetComponent<UnityEngine.UI.Text>();
            //mv.subName = subname;
        }
        public static void SetupCollisionSound(ref ModVehicle mv)
        {
            var colsound = mv.gameObject.EnsureComponent<CollisionSound>();
            var seamothColSound = seamoth.GetComponent<CollisionSound>();
            colsound.hitSoundSmall = seamothColSound.hitSoundSmall;
            colsound.hitSoundSlow = seamothColSound.hitSoundSlow;
            colsound.hitSoundMedium = seamothColSound.hitSoundMedium;
            colsound.hitSoundFast = seamothColSound.hitSoundFast;
        }
        public static void SetupOutOfBoundsWarp(ref ModVehicle mv)
        {
            mv.gameObject.EnsureComponent<OutOfBoundsWarp>();
        }
        public static void SetupConstructionObstacle(ref ModVehicle mv)
        {
            var co = mv.gameObject.EnsureComponent<ConstructionObstacle>();
            co.reason = mv.name + " is in the way.";
        }
        public static void SetupSoundOnDamage(ref ModVehicle mv)
        {
            // TODO: we could have unique sounds for each damage type
            // TODO: this might not work, might need to put it in a VehicleStatusListener
            var sod = mv.gameObject.EnsureComponent<SoundOnDamage>();
            sod.damageType = DamageType.Normal;
            sod.sound = seamoth.GetComponent<SoundOnDamage>().sound;
        }
        public static void SetupDealDamageOnImpact(ref ModVehicle mv)
        {
            var ddoi = mv.gameObject.EnsureComponent<DealDamageOnImpact>();
            ddoi.damageTerrain = true;
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
        }
        public static void SetupDamageComponents(ref ModVehicle mv)
        {
            // add vfxvehicledamages... or not

            // add temperaturedamage
            var tempdamg = mv.gameObject.EnsureComponent<TemperatureDamage>();
            tempdamg.lavaDatabase = seamoth.GetComponent<TemperatureDamage>().lavaDatabase;
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
        public static void SetupRespawnPoint(ref ModVehicle mv)
        {
            if (mv.TetherSources.Count > 0)
            {
                mv.gameObject.EnsureComponent<SubRoot>();
                var tmp = mv.TetherSources.First();
                tmp.EnsureComponent<RespawnPoint>();
            }
        }

        #endregion
        public static void Instrument(ref ModVehicle mv, ModVehicleEngine engine, PingType pingType, int baseCrushDepth, int maxHealth)
        {
            mv.StorageRootObject.EnsureComponent<ChildObjectIdentifier>();
            mv.modulesRoot = mv.ModulesRootObject.EnsureComponent<ChildObjectIdentifier>();
            
            SetupPrefabObjects(ref mv);
            mv.enabled = false;
            SetupEnergyInterface(ref mv);
            SetupAIEnergyInterface(ref mv);
            mv.enabled = true;
            SetupLights(ref mv);
            SetupLightSounds(ref mv);
            SetupLiveMixin(ref mv, maxHealth);
            SetupRigidbody(ref mv, 500);
            SetupEngine(ref mv, engine);
            SetupWorldForces(ref mv);
            SetupLargeWorldEntity(ref mv);
            SetupHudPing(ref mv, pingType);
            SetupVehicleConfig(ref mv);
            SetupCrushDamage(ref mv, baseCrushDepth, maxHealth, 15, 1);
            SetupWaterClipping(ref mv);
            SetupCollisionSound(ref mv);
            SetupOutOfBoundsWarp(ref mv);
            SetupConstructionObstacle(ref mv);
            SetupSoundOnDamage(ref mv);
            SetupDealDamageOnImpact(ref mv);
            SetupDamageComponents(ref mv);
            SetupRespawnPoint(ref mv);
            mv.collisionModel = mv.CollisionModel;

            ApplySkyAppliers(ref mv);

            // ApplyShaders should happen last
            ApplyShaders(ref mv);

            #region todo
            /*
            //Allows power to connect to here.
            var powerRelay = prefab.AddComponent<PowerRelay>();

            //Sky appliers to make it look nicer. Not sure if it even makes a difference, but I'm sticking with it.
            var skyApplierInterior = interiorModels.gameObject.AddComponent<SkyApplier>();
            skyApplierInterior.renderers = interiorModels.GetComponentsInChildren<Renderer>();
            skyApplierInterior.anchorSky = Skies.BaseInterior;
            skyApplierInterior.SetSky(Skies.BaseInterior);
            */
            #endregion
        }

        public static void ApplyShaders(ref ModVehicle mv)
        {
            // Add the marmoset shader to all renderers
            Shader marmosetShader = Shader.Find("MarmosetUBER");
            foreach (var renderer in mv.gameObject.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (renderer.gameObject.name.Contains("Canopy"))
                {
                    // TODO: find a way to add transparency
                    // ZWrite set to 1 (a boolean value) makes the canopy opaque.
                    var seamothGlassMaterial = seamoth.transform.Find("Model/Submersible_SeaMoth/Submersible_seaMoth_geo/Submersible_SeaMoth_glass_interior_geo").GetComponent<SkinnedMeshRenderer>().material;
                    var seamothGlassShader = seamothGlassMaterial.shader;
                    renderer.material = seamothGlassMaterial;
                    // TODO decide which line is right:
                    renderer.material = seamothGlassMaterial; // this is the right line
                    //renderer.material.shader = seamothGlassShader;
                    //renderer.material.shader = marmosetShader;
                    //renderer.material.SetFloat("_ZWrite", 1f);
                    //renderer.material.SetFloat("_MyCullVariable", 1f);
                    continue;
                }
                foreach (Material mat in renderer.materials)
                {
                    mat.shader = marmosetShader;
                    // skip some materials
                    if (renderer.gameObject.name.ToLower().Contains("light"))
                    {
                        continue;
                    }
                    else
                    {
                        // give it the marmo shader, no matter what
                        mat.shader = marmosetShader;

                        // if this is a piece of interior geometry, enable the lightmap
                        Transform itInterior = renderer.gameObject.transform;
                        bool isthisinterior = itInterior.name == "Interior_Main";
                        while (itInterior.parent != null)
                        {
                            if (itInterior.parent.name == "Interior_Main")
                            {
                                isthisinterior = true;
                                break;
                            }
                            itInterior = itInterior.parent;
                        }
                        if (isthisinterior)
                        {
                            mat.EnableKeyword("MARMO_SPECMAP");
                            mat.EnableKeyword("UWE_LIGHTMAP");
                            mat.SetFloat("_LightmapStrength", 7.5f);
                        }
                    }
                }
            }
        }
        public static void ApplySkyAppliers(ref ModVehicle mv)
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
                    return ve.prefab.name;
                }
            }
            return PingManager.sCachedPingTypeStrings.Get(inputType);
        }
        public static Atlas.Sprite GetPingTypeSprite(SpriteManager.Group group, string name)
        {
            foreach (VehicleEntry ve in VehicleManager.vehicleTypes)
            {
                if (ve.prefab.name == name)
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

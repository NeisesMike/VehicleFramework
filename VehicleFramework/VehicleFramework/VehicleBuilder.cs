using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;
using System.Reflection;

using UnityEngine.U2D;

using SMLHelper;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

namespace VehicleFramework
{
    public struct VehicleEntry
    {
        public VehicleEntry(GameObject prefabObj, int id, string desc, PingType pt_in, Atlas.Sprite sprite, int modules_in, int arms_in)
        {
            prefab = prefabObj;
            unique_id = id;
            description = desc;
            pt = pt_in;
            ping_sprite = sprite;
            modules = modules_in;
            arms = arms_in;
        }
        public GameObject prefab;
        public int unique_id;
        public string description;
        public PingType pt;
        public Atlas.Sprite ping_sprite;
        public int modules;
        public int arms;
    }

    public static class VehicleBuilder
    {
        private static int numVehicleTypes = 0;
        public static List<VehicleEntry> vehicleTypes = new List<VehicleEntry>();
        public static List<ModVehicle> prefabs = new List<ModVehicle>();
        public static GameObject seamoth = CraftData.GetPrefabForTechType(TechType.Seamoth, true);
        public static GameObject coroutineHelper;

        public static readonly EquipmentType ModuleType = (EquipmentType)625;
        public static readonly EquipmentType ArmType = (EquipmentType)626;

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
            foreach(VehicleEntry ve in vehicleTypes)
            {
                if(ve.prefab.name == ve.prefab.name)
                {
                    Logger.Log(mv.gameObject.name + " vehicle was already registered.");
                    isNewEntry = false;
                    break;
                }
            }
            if (isNewEntry)
            {
                Instrument(ref mv, pt);
                VehicleEntry ve = new VehicleEntry(mv.gameObject, numVehicleTypes, mv.GetDescription(), pt, sprite, modules, arms);
                vehicleTypes.Add(ve);
                numVehicleTypes++;
                Logger.Log("Registered the " + mv.gameObject.name);
            }
        }

        public static void Instrument(ref ModVehicle mv, PingType pingType)
        {
            mv.StorageRootObject.EnsureComponent<ChildObjectIdentifier>();
            mv.modulesRoot = mv.ModulesRootObject.EnsureComponent<ChildObjectIdentifier>();

            foreach (VehicleParts.VehiclePilotSeat ps in mv.PilotSeats)
            {
                mv.playerPosition = ps.SitLocation;
                PilotingTrigger pt = ps.Seat.EnsureComponent<PilotingTrigger>();
                pt.mv = mv;
            }
            foreach (VehicleParts.VehicleHatchStruct vhs in mv.Hatches)
            {
                var hatch = vhs.Hatch.EnsureComponent<VehicleHatch>();
                hatch.mv = mv;
                hatch.EntryLocation = vhs.EntryLocation;
                hatch.ExitLocation = vhs.ExitLocation;
            }
            int iter = 0;
            foreach (VehicleParts.VehicleStorage vs in mv.Storages)
            {
                vs.Container.SetActive(false);

                var cont = vs.Container.EnsureComponent<VehicleStorageContainer>();
                cont.storageRoot = mv.StorageRootObject.GetComponent<ChildObjectIdentifier>();
                cont.storageLabel = "Vehicle Storage";
                cont.height = vs.Height;
                cont.width = vs.Width;

                FMODAsset storageCloseSound = seamoth.transform.Find("Storage/Storage1").GetComponent<SeamothStorageInput>().closeSound;
                FMODAsset storageOpenSound = seamoth.transform.Find("Storage/Storage1").GetComponent<SeamothStorageInput>().openSound;
                var inp = vs.Container.EnsureComponent<VehicleStorageInput>();
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

                var cont = vs.Container.EnsureComponent<VehicleStorageContainer>();
                cont.storageRoot = mv.StorageRootObject.GetComponent<ChildObjectIdentifier>();
                cont.storageLabel = "Vehicle Storage";
                cont.height = vs.Height;
                cont.width = vs.Width;

                FMODAsset storageCloseSound = seamoth.transform.Find("Storage/Storage1").GetComponent<SeamothStorageInput>().closeSound;
                FMODAsset storageOpenSound = seamoth.transform.Find("Storage/Storage1").GetComponent<SeamothStorageInput>().openSound;
                var inp = vs.Container.EnsureComponent<VehicleStorageInput>();
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
                vuci.flap = vu.Interface.transform.Find("flap");
                vuci.anglesOpened = new Vector3(80, 0, 0);
                mv.upgradesInput = vuci;
            }

            mv.enabled = false;
            var seamothEnergyMixin = seamoth.GetComponent<EnergyMixin>();
            List<EnergyMixin> energyMixins = new List<EnergyMixin>();
            foreach (VehicleParts.VehicleBattery vb in mv.Batteries)
            {
                // Configure energy mixin for this battery slot
                var em = vb.BatterySlot.EnsureComponent<EnergyMixin>();
                em.storageRoot = mv.StorageRootObject.GetComponent<ChildObjectIdentifier>();
                em.defaultBattery = seamothEnergyMixin.defaultBattery;
                em.compatibleBatteries = seamothEnergyMixin.compatibleBatteries;
                em.soundPowerUp = seamothEnergyMixin.soundPowerUp;
                em.soundPowerDown = seamothEnergyMixin.soundPowerDown;
                em.soundBatteryAdd = seamothEnergyMixin.soundBatteryAdd;
                em.soundBatteryRemove = seamothEnergyMixin.soundBatteryRemove;
                em.batteryModels = seamothEnergyMixin.batteryModels;
                //atramaEnergyMixin.capacity = 500; //TODO
                //atramaEnergyMixin.batterySlot = 

                energyMixins.Add(em);

                vb.BatterySlot.EnsureComponent<VehicleBatteryInput>().mixin = em;
            }
            // Configure energy interface
            var eInterf = mv.gameObject.EnsureComponent<EnergyInterface>();
            eInterf.sources = energyMixins.ToArray();
            mv.enabled = true;

            // Configure Lights
            FMOD_StudioEventEmitter[] fmods = seamoth.GetComponents<FMOD_StudioEventEmitter>();
            foreach (FMOD_StudioEventEmitter fmod in fmods)
            {
                if (fmod.asset.name == "seamoth_light_on")
                {
                    mv.lightsOnSound = CopyComponent(fmod, mv.gameObject);
                }
                else if (fmod.asset.name == "seamoth_light_off")
                {
                    mv.lightsOffSound = CopyComponent(fmod, mv.gameObject);
                }
            }
            GameObject seamothHeadLight = seamoth.transform.Find("lights_parent/light_left").gameObject;
            Transform seamothVL = seamoth.transform.Find("lights_parent/light_left/x_FakeVolumletricLight"); // sic
            MeshFilter seamothVLMF = seamothVL.GetComponent<MeshFilter>();
            MeshRenderer seamothVLMR = seamothVL.GetComponent<MeshRenderer>();
            foreach (VehicleParts.VehicleLight pc in mv.Lights)
            {
                CopyComponent(seamothHeadLight.GetComponent<LightShadowQuality>(), pc.Light);
                var leftLight = pc.Light.EnsureComponent<Light>();
                leftLight.type = LightType.Spot;
                leftLight.spotAngle = pc.Angle;
                leftLight.innerSpotAngle = pc.Angle * .75f;
                leftLight.color = pc.Color;
                leftLight.intensity = pc.Strength/60f;
                leftLight.range = pc.Strength;
                leftLight.shadows = LightShadows.Hard;

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
                leftVFX.lightSource = leftLight;
                leftVFX.color = pc.Color;
                leftVFX.volumGO = volumetricLight;
                leftVFX.volumRenderer = lvlMeshRenderer;
                leftVFX.volumMeshFilter = lvlMeshFilter;

                mv.lights.Add(pc.Light);
                mv.volumetricLights.Add(volumetricLight);
            }

            // Ensure Vehicle can die
            // TODO read this lol
            var liveMixin = mv.gameObject.EnsureComponent<LiveMixin>();
            var lmData = ScriptableObject.CreateInstance<LiveMixinData>();
            lmData.canResurrect = true;
            lmData.broadcastKillOnDeath = false;
            lmData.destroyOnDeath = false;
            lmData.explodeOnDestroy = false;
            lmData.invincibleInCreative = true;
            lmData.weldable = false;
            lmData.minDamageForSound = 20f;
            lmData.maxHealth = 1000;
            liveMixin.data = lmData;
            mv.liveMixin = liveMixin;

            // Ensure vehicle is a physics object
            var rb = mv.gameObject.EnsureComponent<Rigidbody>();
            rb.mass = 4000f;
            rb.drag = 10f;
            rb.angularDrag = 10f;
            rb.useGravity = false;
            mv.useRigidbody = rb;
            // Add the engine (physics control)
            var ve = mv.gameObject.EnsureComponent<VehicleEngine>();
            ve.mv = mv;
            ve.rb = rb;

            // Ensure vehicle remains in the world always
            mv.gameObject.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Global;

            // Add the hud ping instance
            mv.pingInstance = mv.gameObject.EnsureComponent<PingInstance>();
            mv.pingInstance.origin = mv.transform;
            mv.pingInstance.pingType = pingType;
            mv.pingInstance.SetLabel("Vehicle");

            // add various vehicle things
            mv.stabilizeRoll = true;
            mv.controlSheme = Vehicle.ControlSheme.Submersible;
            mv.mainAnimator = mv.gameObject.EnsureComponent<Animator>();

            // borrow some things from the seamoth
            mv.crushDamage = CopyComponent<CrushDamage>(seamoth.GetComponent<CrushDamage>(), mv.gameObject);
            mv.crushDamage.kBaseCrushDepth = 300;
            mv.crushDamage.damagePerCrush = 3;
            mv.crushDamage.crushPeriod = 1;
            //atrama.vehicle.bubbles = CopyComponent<ParticleSystem>(seamoth.GetComponent<SeaMoth>().bubbles, atrama.vehicle.gameObject);
            mv.ambienceSound = CopyComponent<FMOD_StudioEventEmitter>(seamoth.GetComponent<SeaMoth>().ambienceSound, mv.gameObject);
            //mv.toggleLights = CopyComponent<ToggleLights>(seamoth.GetComponent<SeaMoth>().toggleLights, mv.gameObject);
            mv.worldForces = CopyComponent<WorldForces>(seamoth.GetComponent<SeaMoth>().worldForces, mv.gameObject);


            // Add the marmoset shader to all renderers
            Shader marmosetShader = Shader.Find("MarmosetUBER");
            foreach (var renderer in mv.gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                foreach (Material mat in renderer.materials)
                {
                    // skip some materials
                    if (renderer.gameObject.name.Contains("Light"))
                    {
                        continue;
                    }

                    mat.shader = marmosetShader;

                    /*
                    // add emission to certain materials
                    // in order to light the interior
                    if (
                        (renderer.gameObject.name == "Main-Body" && mat.name.Contains("Material"))
                        || renderer.gameObject.name == "Mechanical-Panel"
                        || renderer.gameObject.name == "AtramaPilotChair"
                        || renderer.gameObject.name == "Hatch"
                        )
                    {
                        mv.interiorRenderers.Add(renderer);

                        // TODO move this to OnPowered and OnUnpowered
                        mat.EnableKeyword("MARMO_EMISSION");
                        mat.SetFloat("_EmissionLM", 0.25f);
                        mat.SetFloat("_EmissionLMNight", 0.25f);
                        mat.SetFloat("_GlowStrength", 0f);
                        mat.SetFloat("_GlowStrengthNight", 0f);
                    }
                    */

                }
            }



            #region todo
            /*
            //Basically an extension to Unity rigidbodys. Necessary for buoyancy.
            var worldForces = prefab.AddComponent<WorldForces>();
            worldForces.useRigidbody = rigidbody;
            worldForces.underwaterGravity = -20f; //Despite it being negative, which would apply downward force, this actually makes it go UP on the y axis.
            worldForces.aboveWaterGravity = 20f; //Counteract the strong upward force
            worldForces.waterDepth = -5f;

            //Determines the places the little build bots point their laser beams at.
            var buildBots = prefab.AddComponent<BuildBotBeamPoints>();

            Transform beamPointsParent = Helpers.FindChild(prefab, "BuildBotPoints").transform;

            //These are arbitrarily placed.
            buildBots.beamPoints = new Transform[beamPointsParent.childCount];
            for (int i = 0; i < beamPointsParent.childCount; i++)
            {
                buildBots.beamPoints[i] = beamPointsParent.GetChild(i);
            }

            //The path the build bots take to get to the ship to construct it.
            Transform pathsParent = Helpers.FindChild(prefab, "BuildBotPaths").transform;

            //4 paths, one for each build bot to take.
            CreateBuildBotPath(prefab, pathsParent.GetChild(0));
            CreateBuildBotPath(prefab, pathsParent.GetChild(1));
            CreateBuildBotPath(prefab, pathsParent.GetChild(2));
            CreateBuildBotPath(prefab, pathsParent.GetChild(3));

            //The effects for the constructor.
            var vfxConstructing = prefab.AddComponent<VFXConstructing>();
            var rocketPlatformVfx = rocketPlatformReference.GetComponentInChildren<VFXConstructing>();
            vfxConstructing.ghostMaterial = rocketPlatformVfx.ghostMaterial;
            vfxConstructing.surfaceSplashSound = rocketPlatformVfx.surfaceSplashSound;
            vfxConstructing.surfaceSplashFX = rocketPlatformVfx.surfaceSplashFX;
            vfxConstructing.Regenerate();

            //Some components might need this. I don't WANT it to take damage though, so I will just give it a LOT of health.
            var liveMixin = prefab.AddComponent<LiveMixin>();
            var lmData = ScriptableObject.CreateInstance<LiveMixinData>();
            lmData.canResurrect = true;
            lmData.broadcastKillOnDeath = false;
            lmData.destroyOnDeath = false;
            lmData.explodeOnDestroy = false;
            lmData.invincibleInCreative = true;
            lmData.weldable = false;
            lmData.minDamageForSound = 20f;
            lmData.maxHealth = float.MaxValue;
            liveMixin.data = lmData;

            //I don't know if this does anything at all as ships float above the surface, but I'm keeping it.
            var oxygenManager = prefab.AddComponent<OxygenManager>();

            //I don't understand why I'm doing this, but I will anyway. The power cell is nowhere to be seen. To avoid learning how the EnergyMixin code works, I just added an external solar panel that stores all the power anyway.
            var energyMixin = prefab.AddComponent<EnergyMixin>();
            energyMixin.compatibleBatteries = new List<TechType>() { TechType.PowerCell, TechType.PrecursorIonPowerCell };
            energyMixin.defaultBattery = TechType.PowerCell;

            //Allows power to connect to here.
            var powerRelay = prefab.AddComponent<PowerRelay>();

            //Sky appliers to make it look nicer. Not sure if it even makes a difference, but I'm sticking with it.
            var skyApplierInterior = interiorModels.gameObject.AddComponent<SkyApplier>();
            skyApplierInterior.renderers = interiorModels.GetComponentsInChildren<Renderer>();
            skyApplierInterior.anchorSky = Skies.BaseInterior;
            skyApplierInterior.SetSky(Skies.BaseInterior);

            //Spawn a seamoth for reference.
            var seamothRef = CraftData.GetPrefabForTechType(TechType.Seamoth);
            //Get the seamoth's water clip proxy component. This is what displaces the water.
            var seamothProxy = seamothRef.GetComponentInChildren<WaterClipProxy>();
            //Find the parent of all the ship's clip proxys.
            Transform proxyParent = Helpers.FindChild(prefab, "ClipProxys").transform;
            //Loop through them all
            foreach (Transform child in proxyParent)
            {
                var waterClip = child.gameObject.AddComponent<WaterClipProxy>();
                waterClip.shape = WaterClipProxy.Shape.Box;
                //Apply the seamoth's clip material. No idea what shader it uses or what settings it actually has, so this is an easier option. Reuse the game's assets.
                waterClip.clipMaterial = seamothProxy.clipMaterial;
                //You need to do this. By default the layer is 0. This makes it displace everything in the default rendering layer. We only want to displace water.
                waterClip.gameObject.layer = seamothProxy.gameObject.layer;
            }
            //Unload the prefab to save on resources.
            Resources.UnloadAsset(seamothRef);

            //Arbitrary number. The ship doesn't have batteries anyway.
            energyMixin.maxEnergy = 1200f;

            //Add this component. It inherits from the same component that both the cyclops submarine and seabases use.
            var shipBehaviour = prefab.AddComponent<SeaVoyager>();

            //A ping so you can see it from far away
            var ping = prefab.AddComponent<PingInstance>();
            ping.pingType = QPatch.shipPingType;
            ping.origin = Helpers.FindChild(prefab, "PingOrigin").transform;
            */
            #endregion




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
            foreach(VehicleEntry ve in vehicleTypes)
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
            foreach (VehicleEntry ve in vehicleTypes)
            {
                if (ve.prefab.name == name)
                {
                    return ve.ping_sprite;
                }
            }
            return SpriteManager.Get(SpriteManager.Group.Pings, name);
        }

    }
}

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
        public static GameObject moduleBuilder;

        private static int numVehicleTypes = 0;
        public static List<VehicleEntry> vehicleTypes = new List<VehicleEntry>();
        public static List<ModVehicle> prefabs = new List<ModVehicle>();
        public static GameObject seamoth = CraftData.GetPrefabForTechType(TechType.Seamoth, true);
        public static GameObject coroutineHelper;

        public const EquipmentType ModuleType = (EquipmentType)625;
        public const EquipmentType ArmType = (EquipmentType)626;
        public const TechType InnateStorage = (TechType)0x4100;

        public static void PatchCraftables()
        {
            seamoth.SetActive(false);
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

            #region setup_prefab_objects
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
                vuci.flap = vu.Interface.transform.Find("flap");
                vuci.anglesOpened = new Vector3(80, 0, 0);
                mv.upgradesInput = vuci;
            }            
            // Configure the Control Panel
            mv.controlPanelLogic = mv.ControlPanel.EnsureComponent<ControlPanel>();
            mv.controlPanelLogic.mv = mv;
            mv.ControlPanel.transform.localPosition = mv.transform.Find("Control-Panel-Location").localPosition;
            mv.ControlPanel.transform.localRotation = mv.transform.Find("Control-Panel-Location").localRotation;
            GameObject.Destroy(mv.transform.Find("Control-Panel-Location").gameObject);
            #endregion
            mv.enabled = false;
            #region energy_interface
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
            #endregion
            mv.enabled = true;
            #region configure_lights
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
            #endregion
            #region live_mixin
            // Ensure Vehicle can die
            var liveMixin = mv.gameObject.EnsureComponent<LiveMixin>();
            var lmData = ScriptableObject.CreateInstance<LiveMixinData>();
            lmData.canResurrect = true;
            lmData.broadcastKillOnDeath = true;
            lmData.destroyOnDeath = true;
            lmData.explodeOnDestroy = true;
            lmData.invincibleInCreative = true;
            lmData.weldable = false;
            lmData.minDamageForSound = 20f;
            lmData.maxHealth = 1000;
            liveMixin.data = lmData;
            mv.liveMixin = liveMixin;
            #endregion
            #region rigidbody
            // Ensure vehicle is a physics object
            var rb = mv.gameObject.EnsureComponent<Rigidbody>();
            rb.mass = 4000f;
            rb.drag = 10f;
            rb.angularDrag = 10f;
            rb.useGravity = false;
            mv.useRigidbody = rb;
            #endregion
            #region engine
            // Add the engine (physics control)
            var ve = mv.gameObject.EnsureComponent<VehicleEngine>();
            ve.mv = mv;
            ve.rb = rb;
            #endregion
            #region world_forces
            mv.worldForces = CopyComponent<WorldForces>(seamoth.GetComponent<SeaMoth>().worldForces, mv.gameObject);
            mv.worldForces.useRigidbody = mv.useRigidbody;
            mv.worldForces.underwaterGravity = 0f;
            mv.worldForces.aboveWaterGravity = 9.8f;
            mv.worldForces.waterDepth = 0f;
            #endregion
            #region large_world_entity
            // Ensure vehicle remains in the world always
            mv.gameObject.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Global;
            #endregion
            #region hud_ping
            // Add the hud ping instance
            mv.pingInstance = mv.gameObject.EnsureComponent<PingInstance>();
            mv.pingInstance.origin = mv.transform;
            mv.pingInstance.pingType = pingType;
            mv.pingInstance.SetLabel("Vehicle");
            #endregion
            #region vehicle_config
            // add various vehicle things
            mv.stabilizeRoll = true;
            mv.controlSheme = Vehicle.ControlSheme.Submersible;
            mv.mainAnimator = mv.gameObject.EnsureComponent<Animator>();
            #endregion
            #region crush_damage
            mv.crushDamage = CopyComponent<CrushDamage>(seamoth.GetComponent<CrushDamage>(), mv.gameObject);
            mv.crushDamage.kBaseCrushDepth = 300;
            mv.crushDamage.damagePerCrush = 3;
            mv.crushDamage.crushPeriod = 1;
            #endregion

            //atrama.vehicle.bubbles = CopyComponent<ParticleSystem>(seamoth.GetComponent<SeaMoth>().bubbles, atrama.vehicle.gameObject);
            mv.ambienceSound = CopyComponent<FMOD_StudioEventEmitter>(seamoth.GetComponent<SeaMoth>().ambienceSound, mv.gameObject);
            //mv.toggleLights = CopyComponent<ToggleLights>(seamoth.GetComponent<SeaMoth>().toggleLights, mv.gameObject);




            #region water_clipping
            // Enable water clipping for proper interaction with the surface of the ocean
            WaterClipProxy seamothWCP = seamoth.GetComponentInChildren<WaterClipProxy>();
            foreach (GameObject proxy in mv.WaterClipProxies)
            {
                WaterClipProxy waterClip = proxy.AddComponent<WaterClipProxy>();
                waterClip.shape = WaterClipProxy.Shape.Box;
                //Apply the seamoth's clip material. No idea what shader it uses or what settings it actually has, so this is an easier option. Reuse the game's assets.
                waterClip.clipMaterial = seamothWCP.clipMaterial;
                //You need to do this. By default the layer is 0. This makes it displace everything in the default rendering layer. We only want to displace water.
                waterClip.gameObject.layer = seamothWCP.gameObject.layer;
            }



            /*
            Logger.Log("bing");
            var wcp = mv.gameObject.EnsureComponent<WaterClipProxy>();
            wcp.shape = WaterClipProxy.Shape.DistanceField;
            wcp.immovable = false;
            Logger.Log("bing");
            wcp.clipMaterial = seamothWCP.clipMaterial;
            wcp.distanceField = seamothWCP.distanceField;
            wcp.distanceFieldTexture = seamothWCP.distanceFieldTexture;
            wcp.waterSurface = seamothWCP.waterSurface;
            // TODO: these two Vector3 values will *only* work for the Atrama
            // Find a way to generalize them
            wcp.distanceFieldMin = new Vector3(-0.65f, 0.27f, -0.58f);
            wcp.distanceFieldMax = new Vector3(0.65f, 1.57f, 4.24f);
            wcp.clipMaterial = seamothWCP.clipMaterial;
            wcp.gameObject.layer = seamothWCP.gameObject.layer;
            */


            /*
            var wcomr = wco.EnsureComponent<MeshRenderer>();
            var seamothWCMR = seamoth.transform.Find("DistanceFieldProxy").GetComponent<MeshRenderer>();
            wcomr.material = seamothWCMR.material;
            wcomr.sharedMaterial = seamothWCMR.sharedMaterial;
            Logger.Log("bing");
            wcomr.shadowCastingMode = seamothWCMR.shadowCastingMode;
            wcomr.renderingLayerMask = seamothWCMR.renderingLayerMask;
            var wcomf = wco.EnsureComponent<MeshFilter>();
            
            */



            #endregion



            #region todo
            /*
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

            //I don't know if this does anything at all as ships float above the surface, but I'm keeping it.
            var oxygenManager = prefab.AddComponent<OxygenManager>();

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

            */
            #endregion



            // Add the marmoset shader to all renderers
            Shader marmosetShader = Shader.Find("MarmosetUBER");
            foreach (var renderer in mv.gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                if (renderer.gameObject.name.Contains("Canopy"))
                {
                    var seamothGlassMaterial = seamoth.transform.Find("Model/Submersible_SeaMoth/Submersible_seaMoth_geo/Submersible_SeaMoth_glass_interior_geo").GetComponent<SkinnedMeshRenderer>().material;
                    var seamothGlassShader = seamothGlassMaterial.shader;
                    renderer.material = seamothGlassMaterial;
                    renderer.material.shader = seamothGlassShader;
                    renderer.material.SetFloat("_ZWrite", 1f);

                }
                foreach (Material mat in renderer.materials)
                {
                    // skip some materials
                    if (renderer.gameObject.name.Contains("Light"))
                    {
                        continue;
                    }
                    else if (renderer.gameObject.name.Contains("Canopy"))
                    {
                        continue;
                    }
                    else
                    {
                        mat.shader = marmosetShader;
                    }
                }
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

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

namespace AtramaVehicle
{
    public static class AtramaPreparer
    {
        public static Dictionary<string, GameObject> scene2prefab;

        public static GameObject atramaPrefab = null;
        public static Atrama atrama = null;

        public static TechType atramaTechType;
        public static string atramaID;

        public static GameObject seamoth = CraftData.GetPrefabForTechType(TechType.Seamoth, true);

        public static void getAtramaPrefab()
        {
            // load the asset bundle
            string modPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var myLoadedAssetBundle = AssetBundle.LoadFromFile(Path.Combine(modPath, "assets/atrama"));
            if (myLoadedAssetBundle == null)
            {
                Logger.Log("Failed to load AssetBundle!");
                return;
            }

            System.Object[] arr = myLoadedAssetBundle.LoadAllAssets();
            foreach(System.Object obj in arr)
            {
                Logger.Log(obj.ToString());
                if(obj.ToString().Contains("PingSpriteAtlas"))
                {
                    SpriteAtlas thisAtlas = (SpriteAtlas) obj;
                    Sprite ping = thisAtlas.GetSprite("AtramaHudPing");
                    AtramaManager.atramaPingSprite = new Atlas.Sprite(ping);
                }
                else if (obj.ToString().Contains("subnautica-atrama"))
                {
                    atramaPrefab = (GameObject)obj;
                }
            }
        }
        public static void buildAtramaPrefab()
        {
            // grab the vehicle model
            Logger.Log("Building Atrama Prefab");
            
            getAtramaPrefab();
            atramaPrefab.name = "Atrama";

            addAtramaSubsystem();
            addVehicleSubsystem();
            addHealthSubsystem();
            addPowerSubsystem();
            addStorageSubsystem();
            addLightsSubsystem();
            addUpgradeSubsystem();
            addStorageModules();
            applyMarmosetShader();


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
            Component copy = destination.AddComponent(type);
            System.Reflection.FieldInfo[] fields = type.GetFields();
            foreach (System.Reflection.FieldInfo field in fields)
            {
                field.SetValue(copy, field.GetValue(original));
            }
            /* experimental
            System.Reflection.PropertyInfo[] props = type.GetProperties();
            foreach (System.Reflection.PropertyInfo prop in props)
            {
                if (prop.SetMethod == null)
                {
                    continue;
                }
                prop.SetValue(copy, prop.GetValue(original));
            }
            */
            return copy as T;
        }
        public static void addAtramaSubsystem()
        {
            Logger.Log("Add Atrama Subsystem");

            // Make the model an Atrama
            atrama = atramaPrefab.EnsureComponent<Atrama>();
            atrama.enabled = false;

            // Add the engine (physics control)
            atramaPrefab.EnsureComponent<AtramaEngine>();

            // Add the hatch
            atrama.hatch = atramaPrefab.transform.Find("Hatch").gameObject.EnsureComponent<AtramaHatch>();

            // add storage handles
            atrama.leftStorage = atramaPrefab.transform.Find("LeftStorage").gameObject;
            atrama.rightStorage = atramaPrefab.transform.Find("RightStorage").gameObject;

            // Add the hud ping instance
            atrama.pingInstance = atramaPrefab.EnsureComponent<PingInstance>();
            atrama.pingInstance.origin = atramaPrefab.transform;
            atrama.pingInstance.pingType = AtramaManager.atramaPingType;
            atrama.pingInstance.SetLabel("Atrama");

            // Ensure cargo storage has a root object
            GameObject storageRootObj = new GameObject("StorageRootObject");
            storageRootObj.transform.parent = atramaPrefab.transform;
            atrama.storageRoot = storageRootObj.EnsureComponent<ChildObjectIdentifier>();


            atrama.enabled = true;
        }
        public static void addStorageSubsystem()
        {
            Logger.Log("Add Storage Subsystem");
            // Add the storage modules to the construction pods

            atrama.leftStorage.SetActive(false);
            atrama.rightStorage.SetActive(false);

            var lStor = atrama.leftStorage.EnsureComponent<AtramaStorageContainer>();
            lStor.storageRoot = atrama.storageRoot;
            lStor.storageLabel = "Left Pod Storage";
            lStor.height = 6;
            lStor.width = 8;
            var rStor = atrama.rightStorage.EnsureComponent<AtramaStorageContainer>();
            rStor.storageRoot = atrama.storageRoot;
            rStor.storageLabel = "Right Pod Storage";
            rStor.height = 6;
            rStor.width = 8;

            atrama.leftStorage.EnsureComponent<AtramaStorageInput>();
            atrama.rightStorage.EnsureComponent<AtramaStorageInput>();

            atrama.leftStorage.SetActive(true);
            atrama.rightStorage.SetActive(true);
        }
        public static void addPowerSubsystem()
        {
            Logger.Log("Add Power Subsystem");
            atrama.vehicle.enabled = false;

            var seamothEnergyMixin = seamoth.GetComponent<EnergyMixin>();

            List<EnergyMixin> atramaEnergyMixins = new List<EnergyMixin>();

            // Setup battery inputs
            List<GameObject> batterySlots = new List<GameObject>();
            batterySlots.Add(atramaPrefab.transform.Find("Mechanical-Panel/BatteryInputs/1").gameObject);
            batterySlots.Add(atramaPrefab.transform.Find("Mechanical-Panel/BatteryInputs/2").gameObject);
            batterySlots.Add(atramaPrefab.transform.Find("Mechanical-Panel/BatteryInputs/3").gameObject);
            batterySlots.Add(atramaPrefab.transform.Find("Mechanical-Panel/BatteryInputs/4").gameObject);
            foreach (GameObject slot in batterySlots)
            {
                // Configure energy mixin
                var em = slot.EnsureComponent<EnergyMixin>();
                em.storageRoot = atrama.storageRoot;
                em.defaultBattery = seamothEnergyMixin.defaultBattery;
                em.compatibleBatteries = seamothEnergyMixin.compatibleBatteries;
                em.soundPowerUp = seamothEnergyMixin.soundPowerUp;
                em.soundPowerDown = seamothEnergyMixin.soundPowerDown;
                em.soundBatteryAdd = seamothEnergyMixin.soundBatteryAdd;
                em.soundBatteryRemove = seamothEnergyMixin.soundBatteryRemove;
                em.batteryModels = seamothEnergyMixin.batteryModels;
                //atramaEnergyMixin.capacity = 500; //TODO
                //atramaEnergyMixin.batterySlot = 

                atramaEnergyMixins.Add(em);

                slot.EnsureComponent<AtramaBatteryInput>().mixin = em;
            }

            // Configure energy interface
            atrama.energyInterface = atrama.vehicle.gameObject.EnsureComponent<EnergyInterface>();
            atrama.energyInterface.sources = atramaEnergyMixins.ToArray();

            atrama.vehicle.enabled = true;
        }
        public static void addUpgradeSubsystem()
        {
            Logger.Log("Add Upgrade Subsystem");
            // Add the upgrade console code
            GameObject upgradePanel = atramaPrefab.transform.Find("Mechanical-Panel/Upgrades-Panel").gameObject;
            VehicleUpgradeConsoleInput vuci = upgradePanel.EnsureComponent<VehicleUpgradeConsoleInput>();
            vuci.flap = upgradePanel.transform.Find("flap");
            vuci.anglesOpened = new Vector3(80, 0, 0);
            atrama.vehicle.upgradesInput = vuci;
        }
        public static void addHealthSubsystem()
        {
            Logger.Log("Add Health Subsystem");
            // Ensure Atrama can die
            // TODO read this lol
            var liveMixin = atrama.vehicle.gameObject.EnsureComponent<LiveMixin>();
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
            atrama.vehicle.liveMixin = liveMixin;
        }
        public static void addLightsSubsystem()
        {
            Logger.Log("Add Lights Subsystem");
            // get seamoth flood lamp
            GameObject seamothHeadLight = seamoth.transform.Find("lights_parent/light_left").gameObject;
            Transform seamothVL = seamoth.transform.Find("lights_parent/light_left/x_FakeVolumletricLight"); // sic
            MeshFilter seamothVLMF = seamothVL.GetComponent<MeshFilter>();
            MeshRenderer seamothVLMR = seamothVL.GetComponent<MeshRenderer>();

            // create left and right atrama flood lamps
            GameObject atramaLeftHeadLight = atrama.transform.Find("LightsParent/LeftLight").gameObject;
            CopyComponent(seamothHeadLight.GetComponent<LightShadowQuality>(), atramaLeftHeadLight);
            var leftLight = atramaLeftHeadLight.EnsureComponent<Light>();
            leftLight.type = LightType.Spot;
            leftLight.spotAngle = 60;
            leftLight.innerSpotAngle = 45;
            leftLight.color = Color.white;
            leftLight.intensity = 2;
            leftLight.range = 120;
            leftLight.shadows = LightShadows.Hard;

            GameObject leftVolumetricLight = new GameObject("LeftVolumetricLight");
            leftVolumetricLight.transform.localEulerAngles = Vector3.zero;

            leftVolumetricLight.transform.parent = atramaLeftHeadLight.transform;
            leftVolumetricLight.transform.localScale = seamothVL.localScale;
            leftVolumetricLight.transform.localPosition = Vector3.zero;

            var lvlMeshFilter = leftVolumetricLight.AddComponent<MeshFilter>();
            lvlMeshFilter.mesh = seamothVLMF.mesh;
            lvlMeshFilter.sharedMesh = seamothVLMF.sharedMesh;

            var lvlMeshRenderer = leftVolumetricLight.AddComponent<MeshRenderer>();
            lvlMeshRenderer.material = seamothVLMR.material;
            lvlMeshRenderer.sharedMaterial = seamothVLMR.sharedMaterial;
            lvlMeshRenderer.shadowCastingMode = seamothVLMR.shadowCastingMode;
            lvlMeshRenderer.renderingLayerMask = seamothVLMR.renderingLayerMask;

            var leftVFX = CopyComponent(seamothHeadLight.GetComponent<VFXVolumetricLight>(), atramaLeftHeadLight);
            leftVFX.lightSource = leftLight;
            leftVFX.color = Color.white;
            leftVFX.volumGO = leftVolumetricLight;
            leftVFX.volumRenderer = lvlMeshRenderer;
            leftVFX.volumMeshFilter = lvlMeshFilter;

            GameObject atramaRightHeadLight = atrama.transform.Find("LightsParent/RightLight").gameObject;
            CopyComponent(seamothHeadLight.GetComponent<LightShadowQuality>(), atramaRightHeadLight);
            var rightLight = atramaRightHeadLight.EnsureComponent<Light>();
            rightLight.type = LightType.Spot;
            rightLight.spotAngle = 60;
            rightLight.innerSpotAngle = 45;
            rightLight.color = Color.white;
            rightLight.intensity = 2;
            rightLight.range = 120;
            rightLight.shadows = LightShadows.Hard;

            GameObject rightVolumetricLight = new GameObject("RightVolumetricLight");

            rightVolumetricLight.transform.parent = atramaRightHeadLight.transform;
            rightVolumetricLight.transform.localScale = seamothVL.localScale;
            rightVolumetricLight.transform.localPosition = Vector3.zero;

            var rvlMeshFilter = rightVolumetricLight.AddComponent<MeshFilter>();
            rvlMeshFilter.mesh = seamothVLMF.mesh;
            rvlMeshFilter.sharedMesh = seamothVLMF.sharedMesh;

            var rvlMeshRenderer = rightVolumetricLight.AddComponent<MeshRenderer>();
            rvlMeshRenderer.material = seamothVLMR.material;
            rvlMeshRenderer.sharedMaterial = seamothVLMR.sharedMaterial;
            rvlMeshRenderer.shadowCastingMode = seamothVLMR.shadowCastingMode;
            rvlMeshRenderer.renderingLayerMask = seamothVLMR.renderingLayerMask;

            var rightVFX = CopyComponent(seamothHeadLight.GetComponent<VFXVolumetricLight>(), atramaRightHeadLight);
            rightVFX.lightSource = rightLight;
            rightVFX.color = Color.white;
            rightVFX.volumGO = rightVolumetricLight;
            rightVFX.volumRenderer = rvlMeshRenderer;
            rightVFX.volumMeshFilter = rvlMeshFilter;

            atrama.lights.Add(atramaLeftHeadLight);
            atrama.lights.Add(atramaRightHeadLight);
            atrama.volumetricLights.Add(leftVolumetricLight);
            atrama.volumetricLights.Add(rightVolumetricLight);

            atramaLeftHeadLight.transform.localEulerAngles = new Vector3(0, 350, 0);
            atramaRightHeadLight.transform.localEulerAngles = new Vector3(0, 10, 0);

            FMOD_StudioEventEmitter[] fmods = seamoth.GetComponents<FMOD_StudioEventEmitter>();
            foreach (FMOD_StudioEventEmitter fmod in fmods)
            {
                if (fmod.asset.name == "seamoth_light_on")
                {
                    atrama.lightsOnSound = CopyComponent(fmod, atrama.vehicle.gameObject);
                }
                else if (fmod.asset.name == "seamoth_light_off")
                {
                    atrama.lightsOffSound = CopyComponent(fmod, atrama.vehicle.gameObject);
                }
            }
        }
        public static void addStorageModules()
        {
            Logger.Log("Add Storage Modules");
            atrama.modularStorage = atramaPrefab.transform.Find("ModularStorage").gameObject;
            atrama.modularStorage.transform.parent = atramaPrefab.transform;

            FMODAsset storageCloseSound = seamoth.transform.Find("Storage/Storage1").GetComponent<SeamothStorageInput>().closeSound;
            FMODAsset storageOpenSound = seamoth.transform.Find("Storage/Storage1").GetComponent<SeamothStorageInput>().openSound;

            GameObject atramaStorageModule1 = atrama.modularStorage.transform.Find("StorageModule1").gameObject;
            var stor1 = atramaStorageModule1.EnsureComponent<AtramaStorageContainer>();
            stor1.storageRoot = atrama.storageRoot;
            stor1.storageLabel = "Storage Module 1";
            stor1.height = 4;
            stor1.width = 4;
            var input1 = atramaStorageModule1.EnsureComponent<AtramaStorageInput>();
            input1.atrama = atrama.vehicle;
            input1.model = atramaStorageModule1;
            input1.collider = atramaStorageModule1.EnsureComponent<BoxCollider>();
            input1.openSound = storageOpenSound;
            input1.closeSound = storageCloseSound;

            GameObject atramaStorageModule2 = atrama.modularStorage.transform.Find("StorageModule2").gameObject;
            var stor2 = atramaStorageModule2.EnsureComponent<AtramaStorageContainer>();
            stor2.storageRoot = atrama.storageRoot;
            stor2.storageLabel = "Storage Module 2";
            stor2.height = 4;
            stor2.width = 4;
            var input2 = atramaStorageModule2.EnsureComponent<AtramaStorageInput>();
            input2.atrama = atrama.vehicle;
            input2.model = atramaStorageModule2;
            input2.collider = atramaStorageModule2.EnsureComponent<BoxCollider>();
            input2.openSound = storageOpenSound;
            input2.closeSound = storageCloseSound;

            GameObject atramaStorageModule3 = atrama.modularStorage.transform.Find("StorageModule3").gameObject;
            var stor3 = atramaStorageModule3.EnsureComponent<AtramaStorageContainer>();
            stor3.storageRoot = atrama.storageRoot;
            stor3.storageLabel = "Storage Module 3";
            stor3.height = 4;
            stor3.width = 4;
            var input3 = atramaStorageModule3.EnsureComponent<AtramaStorageInput>();
            input3.atrama = atrama.vehicle;
            input3.model = atramaStorageModule3;
            input3.collider = atramaStorageModule3.EnsureComponent<BoxCollider>();
            input3.openSound = storageOpenSound;
            input3.closeSound = storageCloseSound;

            GameObject atramaStorageModule4 = atrama.modularStorage.transform.Find("StorageModule4").gameObject;
            var stor4 = atramaStorageModule4.EnsureComponent<AtramaStorageContainer>();
            stor4.storageRoot = atrama.storageRoot;
            stor4.storageLabel = "Storage Module 4";
            stor4.height = 4;
            stor4.width = 4;
            var input4 = atramaStorageModule4.EnsureComponent<AtramaStorageInput>();
            input4.atrama = atrama.vehicle;
            input4.model = atramaStorageModule4;
            input4.collider = atramaStorageModule4.EnsureComponent<BoxCollider>();
            input4.openSound = storageOpenSound;
            input4.closeSound = storageCloseSound;

            GameObject atramaStorageModule5 = atrama.modularStorage.transform.Find("StorageModule5").gameObject;
            var stor5 = atramaStorageModule5.EnsureComponent<AtramaStorageContainer>();
            stor5.storageRoot = atrama.storageRoot;
            stor5.storageLabel = "Storage Module 5";
            stor5.height = 4;
            stor5.width = 4;
            var input5 = atramaStorageModule5.EnsureComponent<AtramaStorageInput>();
            input5.atrama = atrama.vehicle;
            input5.model = atramaStorageModule5;
            input5.collider = atramaStorageModule5.EnsureComponent<BoxCollider>();
            input5.openSound = storageOpenSound;
            input5.closeSound = storageCloseSound;

            GameObject atramaStorageModule6 = atrama.modularStorage.transform.Find("StorageModule6").gameObject;
            var stor6 = atramaStorageModule6.EnsureComponent<AtramaStorageContainer>();
            stor6.storageRoot = atrama.storageRoot;
            stor6.storageLabel = "Storage Module 6";
            stor6.height = 4;
            stor6.width = 4;
            var input6 = atramaStorageModule6.EnsureComponent<AtramaStorageInput>();
            input6.atrama = atrama.vehicle;
            input6.model = atramaStorageModule6;
            input6.collider = atramaStorageModule6.EnsureComponent<BoxCollider>();
            input6.openSound = storageOpenSound;
            input6.closeSound = storageCloseSound;



            atrama.vehicle.storageInputs = new AtramaStorageInput[6] { input1, input2, input3, input4, input5, input6 };

            var lStor = atrama.leftStorage.GetComponent<AtramaStorageInput>();
            lStor.openSound = storageOpenSound;
            lStor.closeSound = storageCloseSound;
            var rStor = atrama.rightStorage.GetComponent<AtramaStorageInput>();
            rStor.openSound = storageOpenSound;
            rStor.closeSound = storageCloseSound;
        }
        public static void applyMarmosetShader()
        { 
            Logger.Log("Apply Marmoset Shader");
            // Add the marmoset shader to all renderers
            Shader marmosetShader = Shader.Find("MarmosetUBER");
            foreach (var renderer in atramaPrefab.GetComponentsInChildren<MeshRenderer>())
            {
                foreach (Material mat in renderer.materials)
                {
                    // skip some materials
                    if (renderer.gameObject.name.Contains("Light"))
                    {
                        continue;
                    }

                    mat.shader = marmosetShader;

                    // add emission to certain materials
                    // in order to light the interior
                    if (
                        (renderer.gameObject.name == "Main-Body" && mat.name.Contains("Material"))
                        || renderer.gameObject.name == "Mechanical-Panel"
                        || renderer.gameObject.name == "AtramaPilotChair"
                        || renderer.gameObject.name == "Hatch"
                        )
                    {
                        atrama.interiorRenderers.Add(renderer);

                        // TODO move this to OnPowered and OnUnpowered
                        mat.EnableKeyword("MARMO_EMISSION");
                        mat.SetFloat("_EmissionLM", 0.25f);
                        mat.SetFloat("_EmissionLMNight", 0.25f);
                        mat.SetFloat("_GlowStrength", 0f);
                        mat.SetFloat("_GlowStrengthNight", 0f);
                    }

                }
            }
        }
        public static void addVehicleSubsystem()
        {
            Logger.Log("Add Vehicle Subsystem");

            // Make the chair a Vehicle
            GameObject pilotChair = atramaPrefab.transform.Find("Chair").gameObject;
            pilotChair.name = "AtramaPilotChair";
            atrama.vehicle = pilotChair.EnsureComponent<AtramaVehicle>();
            atrama.vehicle.playerPosition = pilotChair;
            atrama.vehicle.stabilizeRoll = true;
            atrama.vehicle.controlSheme = Vehicle.ControlSheme.Submersible;

            atrama.vehicle.mainAnimator = atramaPrefab.EnsureComponent<Animator>();

            // Ensure vehicle remains in the world always
            atramaPrefab.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Global;

            // Ensure vehicle is a physics object
            var rb = atramaPrefab.EnsureComponent<Rigidbody>();
            rb.mass = 4000f;
            rb.drag = 10f;
            rb.angularDrag = 10f;
            rb.useGravity = false;
            atrama.vehicle.useRigidbody = rb;

            // Ensure module storage has a root object
            GameObject modulesRootObj = new GameObject("ModulesRootObject");
            modulesRootObj.transform.parent = atramaPrefab.transform;
            atrama.vehicle.modulesRoot = modulesRootObj.EnsureComponent<ChildObjectIdentifier>();

            // borrow some things from the seamoth
            atrama.vehicle.crushDamage = CopyComponent<CrushDamage>(seamoth.GetComponent<CrushDamage>(), atrama.vehicle.gameObject);
            atrama.vehicle.crushDamage.kBaseCrushDepth = 300;
            atrama.vehicle.crushDamage.damagePerCrush = 5;
            atrama.vehicle.crushDamage.crushPeriod = 3;
            //atrama.vehicle.bubbles = CopyComponent<ParticleSystem>(seamoth.GetComponent<SeaMoth>().bubbles, atrama.vehicle.gameObject);
            atrama.vehicle.ambienceSound = CopyComponent<FMOD_StudioEventEmitter>(seamoth.GetComponent<SeaMoth>().ambienceSound, atrama.vehicle.gameObject);
            atrama.vehicle.toggleLights = CopyComponent<ToggleLights>(seamoth.GetComponent<SeaMoth>().toggleLights, atrama.vehicle.gameObject);
            atrama.vehicle.worldForces = CopyComponent<WorldForces>(seamoth.GetComponent<SeaMoth>().worldForces, atrama.vehicle.gameObject);
        }



        public static void addInteriorLightsSubsystem()
        {

        }


        public static void addCyclopsComponents()
        {
            Logger.Log("Add cyclops components");
            //atramaPrefab.EnsureComponent<SubRoot>();
            /*
            GameObject cyclopsPrefab;
            AtramaManager.scene2prefab.TryGetValue("cyclops", out cyclopsPrefab);

            Logger.Log("Configuring chair... please do away with this...");
            Transform chairTransform = gameObject.transform.Find("Chair");
            chairTransform.forward = -chairTransform.up;
            gameObject.GetComponent<AtramaVehicle>().playerPosition = chairTransform.gameObject;

            Logger.Log("Grabbing steering column...");
            GameObject steeringColumnPrefab = cyclopsPrefab.transform.Find("CyclopsMeshAnimated/Submarine_Steering_Console").gameObject;
            Logger.Log("Instantiating steering column");
            GameObject steeringColumn = GameObject.Instantiate(steeringColumnPrefab, chairTransform);
            steeringColumn.transform.localPosition = Vector3.zero;
            */

            /*
            Logger.Log("Listing cyclops children...");
            foreach (Transform child in cyclopsPrefab.transform)
            {
                Logger.Log(child.gameObject.name);
            }
            */



            // Add the hatch

        }

        public static bool isPlayerNotInAtrama()
        {
            return Player.main.currentMountedVehicle.gameObject.GetComponent<Atrama>() == null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;
using System.Reflection;

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

        public static GameObject getAtramaPrefab()
        {
            // load the asset bundle
            string modPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var myLoadedAssetBundle = AssetBundle.LoadFromFile(Path.Combine(modPath, "assets/atrama"));
            if (myLoadedAssetBundle == null)
            {
                Logger.Log("Failed to load AssetBundle!");
                return null;
            }
            return (myLoadedAssetBundle.LoadAsset<GameObject>("subnautica-atrama.prefab"));
        }
        public static void buildAtramaPrefab()
        {
            // grab the vehicle model
            Logger.Log("Creating initial Atrama...");
            atramaPrefab = getAtramaPrefab();
            atramaPrefab.name = "Atrama";

            // Disable the prefab so none of the Awakes fire before the whole ship is setup
            /*
            Logger.Log("Disabling Atrama...");
            thisAtramaObject.SetActive(false);
            */

            // Make the model an Atrama
            Logger.Log("Ensuring Atrama...");
            atrama = atramaPrefab.EnsureComponent<Atrama>();


            addCustomComponents();
            addStorageModules();
            addVehicleComponents();
            addSeamothComponents();
            addCyclopsComponents();

            // enable the AtramaVehicle
            atrama.vehicle.gameObject.SetActive(true);

            // Add the marmoset shader to all renderers
            Logger.Log("Adding marmoset shader...");
            Shader marmosetShader = Shader.Find("MarmosetUBER");
            foreach (var renderer in atramaPrefab.GetComponentsInChildren<MeshRenderer>())
            {
                Logger.Log("Adding marmoset to: " + renderer.gameObject.name);
                foreach (Material mat in renderer.materials)
                {
                    Logger.Log("This material is: " + mat.name);

                    // skip some materials
                    if(renderer.gameObject.name.Contains("Light"))
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
                        Logger.Log("Adding emission!");
                        mat.EnableKeyword("MARMO_EMISSION");
                        mat.SetFloat("_EmissionLM", 0.25f);
                        mat.SetFloat("_EmissionLMNight", 0.25f);
                        mat.SetFloat("_GlowStrength", 0f);
                        mat.SetFloat("_GlowStrengthNight", 0f);
                    }

                }
                /*
                renderer.sharedMaterial.shader = shader;
                renderer.material.shader = shader;
                */
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
            Component copy = destination.AddComponent(type);
            System.Reflection.FieldInfo[] fields = type.GetFields();
            foreach (System.Reflection.FieldInfo field in fields)
            {
                field.SetValue(copy, field.GetValue(original));
            }
            return copy as T;
        }
        public static void addCustomComponents()
        {
            // Make the chair an AtramaVehicle
            Logger.Log("Add custom components");

            // Make the chair a Vehicle (disabled for now)
            GameObject pilotChair = atramaPrefab.transform.Find("Chair").gameObject;
            pilotChair.name = "AtramaPilotChair";
            pilotChair.SetActive(false);
            atrama.vehicle = pilotChair.EnsureComponent<AtramaVehicle>();
            atrama.vehicle.playerPosition = pilotChair;
            atrama.vehicle.stabilizeRoll = true;

            // Call the Vehicle a seamoth... sorta
            atrama.vehicle.controlSheme = Vehicle.ControlSheme.Submersible;

            // Add the engine (physics control)
            atramaPrefab.EnsureComponent<AtramaEngine>();

            // Add the upgrade console code
            var upInput = atramaPrefab.transform.Find("Mechanical-Panel/Control-Panel").gameObject.EnsureComponent<VehicleUpgradeConsoleInput>();
            atrama.vehicle.upgradesInput = upInput;

            // Add the hatch
            atrama.hatch = atramaPrefab.transform.Find("Hatch").gameObject.EnsureComponent<AtramaHatch>();

        }
        public static void addStorageModules()
        { 
            // Add the storage modules to the construction pods

            // Ensure cargo storage has a root object
            GameObject storageRootObj = new GameObject("StorageRootObject");
            storageRootObj.transform.parent = atramaPrefab.transform;
            atrama.storageRoot = storageRootObj.EnsureComponent<ChildObjectIdentifier>();

            atrama.leftStorage  = atramaPrefab.transform.Find("LeftStorage").gameObject;
            atrama.rightStorage = atramaPrefab.transform.Find("RightStorage").gameObject;

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
        public static void addVehicleComponents()
        {

            Logger.Log("Add vehicle components");
            #region add_vehicle_components

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

            // Ensure module storage has a root object
            GameObject modulesRootObj = new GameObject("ModulesRootObject");
            modulesRootObj.transform.parent = atramaPrefab.transform;
            atrama.vehicle.modulesRoot = modulesRootObj.EnsureComponent<ChildObjectIdentifier>();




            #endregion
        }
        public static void addSeamothComponents()
        {
            Logger.Log("Add seamoth components");
            GameObject seamoth = CraftData.GetPrefabForTechType(TechType.Seamoth, true);

            atrama.vehicle.crushDamage = CopyComponent<CrushDamage>(seamoth.GetComponent<CrushDamage>(), atrama.vehicle.gameObject);
            //atrama.vehicle.bubbles = CopyComponent<ParticleSystem>(seamoth.GetComponent<SeaMoth>().bubbles, atrama.vehicle.gameObject);
            atrama.vehicle.ambienceSound = CopyComponent<FMOD_StudioEventEmitter>(seamoth.GetComponent<SeaMoth>().ambienceSound, atrama.vehicle.gameObject);
            atrama.vehicle.toggleLights = CopyComponent<ToggleLights>(seamoth.GetComponent<SeaMoth>().toggleLights, atrama.vehicle.gameObject);
            atrama.vehicle.worldForces = CopyComponent<WorldForces>(seamoth.GetComponent<SeaMoth>().worldForces, atrama.vehicle.gameObject);


            Logger.Log("Configure Atrama Power Systems");
            GenericHandTarget seamothBatteryInput = seamoth.transform.Find("BatteryInput").GetComponent<GenericHandTarget>();
            var seamothEnergyMixin = seamoth.GetComponent<EnergyMixin>();

            List<EnergyMixin> atramaEnergyMixins = new List<EnergyMixin>();

            // Setup battery inputs
            List<GameObject> batterySlots = new List<GameObject>();
            batterySlots.Add(atramaPrefab.transform.Find("Mechanical-Panel/BatteryInputs/1").gameObject);
            batterySlots.Add(atramaPrefab.transform.Find("Mechanical-Panel/BatteryInputs/2").gameObject);
            batterySlots.Add(atramaPrefab.transform.Find("Mechanical-Panel/BatteryInputs/3").gameObject);
            batterySlots.Add(atramaPrefab.transform.Find("Mechanical-Panel/BatteryInputs/4").gameObject);
            foreach(GameObject slot in batterySlots)
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





            // get seamoth flood lamp
            Logger.Log("get seamoth lamp and volumetric light");
            GameObject seamothHeadLight = seamoth.transform.Find("lights_parent/light_left").gameObject;
            Transform seamothVL = seamoth.transform.Find("lights_parent/light_left/x_FakeVolumletricLight"); // sic
            MeshFilter seamothVLMF = seamothVL.GetComponent<MeshFilter>();
            MeshRenderer seamothVLMR = seamothVL.GetComponent<MeshRenderer>();

            // create left and right atrama flood lamps
            Logger.Log("create left atrama lamp");
            GameObject atramaLeftHeadLight  = atrama.transform.Find("LightsParent/LeftLight").gameObject;
            CopyComponent(seamothHeadLight.GetComponent<LightShadowQuality>(), atramaLeftHeadLight);
            var leftLight = atramaLeftHeadLight.EnsureComponent<Light>();
            leftLight.type = LightType.Spot;
            leftLight.spotAngle = 60;
            leftLight.innerSpotAngle = 45;
            leftLight.color = Color.white;
            leftLight.intensity = 2;
            leftLight.range = 120;
            leftLight.shadows = LightShadows.Hard;

            Logger.Log("create left atrama volumetric light");
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

            Logger.Log("create right atrama lamp");
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

            Logger.Log("create right atrama volumetric light");
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

            Logger.Log("Collecting Atrama lights");
            atrama.lights.Add(atramaLeftHeadLight);
            atrama.lights.Add(atramaRightHeadLight);
            atrama.volumetricLights.Add(leftVolumetricLight);
            atrama.volumetricLights.Add(rightVolumetricLight);

            Logger.Log("Adjust lamp angles");
            atramaLeftHeadLight.transform.localEulerAngles = new Vector3(0, 350, 0);
            atramaRightHeadLight.transform.localEulerAngles = new Vector3(0, 10, 0);

            Logger.Log("Add light toggle sounds");
            FMOD_StudioEventEmitter[] fmods = seamoth.GetComponents<FMOD_StudioEventEmitter>();
            foreach(FMOD_StudioEventEmitter fmod in fmods)
            {
                if(fmod.asset.name == "seamoth_light_on")
                {
                    atrama.lightsOnSound = CopyComponent(fmod, atrama.vehicle.gameObject);
                }
                else if(fmod.asset.name == "seamoth_light_off")
                {
                    atrama.lightsOffSound = CopyComponent(fmod, atrama.vehicle.gameObject);
                }
            }


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

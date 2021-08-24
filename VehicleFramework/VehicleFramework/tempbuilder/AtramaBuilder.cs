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
    public static partial class AtramaBuilder
    {
        public static Dictionary<string, GameObject> scene2prefab;
        public static GameObject seamoth = CraftData.GetPrefabForTechType(TechType.Seamoth, true);
        public static GameObject atramaModel = null;
        public static GameObject atramaPrefab = null;
        public static GameObject coroutineHelper;

        public static void Init()
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
            foreach (System.Object obj in arr)
            {
                if (obj.ToString().Contains("PingSpriteAtlas"))
                {
                    SpriteAtlas thisAtlas = (SpriteAtlas)obj;
                    Sprite ping = thisAtlas.GetSprite("AtramaHudPing");
                    AtramaManager.atramaPingSprite = new Atlas.Sprite(ping);
                }
                else if (obj.ToString().Contains("subnautica-atrama"))
                {
                    atramaModel = (GameObject)obj;
                }
            }
        }

        public static void buildAtrama(Atrama atrama)
        {
            atrama.gameObject.name = "Atrama";

            collectHandles(atrama);

            addVehicleSubsystem(atrama);
            addHealthSubsystem(atrama);
            addPowerSubsystem(atrama);
            addStorageSubsystem(atrama);
            addLightsSubsystem(atrama);
            addUpgradeSubsystem(atrama);
            addStorageModules(atrama);
            applyMarmosetShader(atrama);

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

            if(atramaPrefab != null)
            {
                GameObject.Destroy(atramaPrefab);
            }
            atramaPrefab = GameObject.Instantiate(atrama.gameObject);
            atramaPrefab.SetActive(false);

        }

        public static void collectHandles(Atrama atrama)
        {
            Transform chair = atrama.transform.Find("Chair");
            if (chair == null)
            {
                chair = atrama.transform.Find("AtramaPilotChair");
            }
            GameObject pilotChair = chair.gameObject;
            pilotChair.name = "AtramaPilotChair";
            atrama.vehicle = pilotChair.EnsureComponent<AtramaVehicle>();
            atrama.vehicle.playerPosition = pilotChair;

            if (atrama.storageRoot == null)
            {
                // Ensure cargo storage has a root object
                GameObject storageRootObj = new GameObject("StorageRootObject");
                storageRootObj.transform.parent = atrama.transform;
                atrama.storageRoot = storageRootObj.EnsureComponent<ChildObjectIdentifier>();
            }

            if (atrama.vehicle.modulesRoot == null)
            {
                // Ensure module storage has a root object
                GameObject modulesRootObj = new GameObject("ModulesRootObject");
                modulesRootObj.transform.parent = atrama.transform;
                atrama.vehicle.modulesRoot = modulesRootObj.EnsureComponent<ChildObjectIdentifier>();
            }

            // Add the hatch
            atrama.hatch = atrama.transform.Find("Hatch").gameObject.EnsureComponent<AtramaHatch>();

            // add storage handles
            atrama.leftStorage = atrama.transform.Find("LeftStorage").gameObject;
            atrama.rightStorage = atrama.transform.Find("RightStorage").gameObject;
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


        /*
         * =========================================================================================================================================
         * =========================================================================================================================================
         * =========================================================================================================================================
         */


        public static void addPilotHUD()
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

    }
}

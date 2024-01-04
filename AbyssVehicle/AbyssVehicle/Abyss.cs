using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VehicleFramework;
using System.IO;
using System.Reflection;

using UnityEngine.U2D;
using VehicleFramework.VehicleParts;

namespace AbyssVehicle
{
    public class Abyss : ModVehicle
    {
        public static GameObject model = null;
        public static RuntimeAnimatorController animatorController = null;
        public static GameObject controlPanel = null;
        public static Atlas.Sprite pingSprite = null;
        public static GameObject cameraGUI = null;

        public static void GetAssets()
        {
            // load the asset bundle
            string modPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var myLoadedAssetBundle = AssetBundle.LoadFromFile(Path.Combine(modPath, "assets/abyss"));
            if (myLoadedAssetBundle == null)
            {
                Logger.Log("ERROR: Failed to load AssetBundle.");
                return;
            }

            System.Object[] arr = myLoadedAssetBundle.LoadAllAssets();
            foreach (System.Object obj in arr)
            {
                if (obj.ToString().Contains("PingSpriteAtlas"))
                {
                    SpriteAtlas thisAtlas = (SpriteAtlas)obj;
                    Sprite ping = thisAtlas.GetSprite("AbyssPingSprite");
                    pingSprite = new Atlas.Sprite(ping);
                }
                else if (obj.ToString().Contains("Vehicle"))
                {
                    model = (GameObject)obj;
                }
                else if (obj.ToString().Contains("Control-Panel"))
                {
                    controlPanel = (GameObject)obj;
                }
                else if (obj.ToString().Contains("CameraGUI"))
                {
                    cameraGUI = (GameObject)obj;
                }
                else
                {
                    //Logger.Log(obj.ToString());
                }
            }
        }
        public static Dictionary<TechType, int> GetRecipe()
        {
            Dictionary<TechType, int> recipe = new Dictionary<TechType, int>();
            recipe.Add(TechType.PlasteelIngot, 2);
            recipe.Add(TechType.Lubricant, 1);
            recipe.Add(TechType.ComputerChip, 1);
            recipe.Add(TechType.AdvancedWiringKit, 1);
            recipe.Add(TechType.Lead, 2);
            recipe.Add(TechType.EnameledGlass, 2);
            return recipe;
        }
        public static IEnumerator Register()
        {
            GetAssets();
            ModVehicle abyss = model.EnsureComponent<Abyss>() as ModVehicle;
            abyss.name = "Abyss";
            yield return UWE.CoroutineHost.StartCoroutine(VehicleManager.RegisterVehicle(abyss, new AbyssEngine(), GetRecipe(), (PingType)123, pingSprite, 8, 0, 600, 1500, 5000));
        }

        public override string vehicleDefaultName
        {
            get
            {
                Language main = Language.main;
                if (!(main != null))
                {
                    return "ABYSS";
                }
                return main.Get("AbyssDefaultName");
            }
        }
        public override string GetDescription()
        {
            return "A sturdy submarine with plenty of floorspace. With a wide flat top that can be tread upon, it's like a tiny island.";
        }

        public override string GetEncyEntry()
        {
            /*
             * The Formula:
             * 2 or 3 sentence blurb
             * Features
             * Advice
             * Ratings
             * Kek
             */
            string ency = "The Abyss is a submarine built to last. ";
            ency += "It is meant to be large enough to build small constructions inside. \n";
            ency += "It is quite sturdy among submersibles. \n";
            ency += "\nIt features:\n";
            ency += "- A system of cameras that can be used by the pilot. \n";
            ency += "- Great external storage capacity, that cannot be further expanded with upgrades. \n";
            ency += "- Standard headlights augmented with forward facing floodlamps. \n";
            ency += "- A signature autopilot which can automatically level out the vessel. \n";
            ency += "\nRatings:\n";
            ency += "- Top Speed (each axis): 10.0m/s \n";
            ency += "- Acceleration (each axis): 3.3m/s/s \n";
            ency += "- Distance per Power Cell: 7km \n";
            ency += "- Crush Depth: 600 \n";
            ency += "- Upgrade Slots: 8 \n";
            ency += "- Dimensions: 3.7m x 5m x 10.6m \n";
            ency += "- Persons: 1-3\n";
            return ency;
        }

        public override GameObject VehicleModel
        {
            get
            {
                return model;
            }
        }


        public override GameObject StorageRootObject
        {
            get
            {
                return transform.Find("StorageRoot").gameObject;
            }
        }

        public override GameObject ModulesRootObject
        {
            get
            {
                return transform.Find("ModulesRoot").gameObject;
            }
        }

        public override List<VehiclePilotSeat> PilotSeats
        {
            get
            {
                var list = new List<VehicleFramework.VehicleParts.VehiclePilotSeat>();
                VehicleFramework.VehicleParts.VehiclePilotSeat vps = new VehicleFramework.VehicleParts.VehiclePilotSeat();
                Transform mainSeat = transform.Find("PilotSeat");
                vps.Seat = mainSeat.gameObject;
                vps.SitLocation = mainSeat.Find("SitLocation").gameObject;
                vps.LeftHandLocation = mainSeat;
                vps.RightHandLocation = mainSeat;
                vps.ExitLocation = mainSeat.Find("ExitLocation");
                // TODO exit location
                list.Add(vps);
                return list;
            }
        }

        public override List<VehicleHatchStruct> Hatches
        {
            get
            {
                var list = new List<VehicleFramework.VehicleParts.VehicleHatchStruct>();

                VehicleFramework.VehicleParts.VehicleHatchStruct interior_vhs = new VehicleFramework.VehicleParts.VehicleHatchStruct();
                Transform intHatch = transform.Find("Hatches/TopHatch/InsideHatch");
                interior_vhs.Hatch = intHatch.gameObject;
                interior_vhs.EntryLocation = intHatch.Find("Entry");
                interior_vhs.ExitLocation = intHatch.Find("Exit");
                interior_vhs.SurfaceExitLocation = intHatch.Find("SurfaceExit");

                VehicleFramework.VehicleParts.VehicleHatchStruct exterior_vhs = new VehicleFramework.VehicleParts.VehicleHatchStruct();
                Transform extHatch = transform.Find("Hatches/TopHatch/OutsideHatch");
                exterior_vhs.Hatch = extHatch.gameObject;
                exterior_vhs.EntryLocation = interior_vhs.EntryLocation;
                exterior_vhs.ExitLocation = interior_vhs.ExitLocation;
                exterior_vhs.SurfaceExitLocation = interior_vhs.SurfaceExitLocation;

                list.Add(interior_vhs);
                list.Add(exterior_vhs);


                VehicleFramework.VehicleParts.VehicleHatchStruct interior_vhs2 = new VehicleFramework.VehicleParts.VehicleHatchStruct();
                Transform intHatch2 = transform.Find("Hatches/BottomHatch/InsideHatch");
                interior_vhs2.Hatch = intHatch2.gameObject;
                interior_vhs2.EntryLocation = intHatch2.Find("Entry");
                interior_vhs2.ExitLocation = intHatch2.Find("Exit");
                interior_vhs2.SurfaceExitLocation = intHatch2.Find("SurfaceExit");

                VehicleFramework.VehicleParts.VehicleHatchStruct exterior_vhs2 = new VehicleFramework.VehicleParts.VehicleHatchStruct();
                Transform extHatch2 = transform.Find("Hatches/BottomHatch/OutsideHatch");
                exterior_vhs2.Hatch = extHatch2.gameObject;
                exterior_vhs2.EntryLocation = interior_vhs2.EntryLocation;
                exterior_vhs2.ExitLocation = interior_vhs2.ExitLocation;
                exterior_vhs2.SurfaceExitLocation = interior_vhs2.SurfaceExitLocation;

                list.Add(interior_vhs2);
                list.Add(exterior_vhs2);

                return list;
            }
        }

        public override List<VehicleStorage> InnateStorages
        {
            get
            {
                var list = new List<VehicleFramework.VehicleParts.VehicleStorage>();

                Transform innate1 = transform.Find("InnateStorages/Storage1");
                Transform innate2 = transform.Find("InnateStorages/Storage2");

                VehicleFramework.VehicleParts.VehicleStorage IS1 = new VehicleFramework.VehicleParts.VehicleStorage();
                IS1.Container = innate1.gameObject;
                IS1.Height = 10;
                IS1.Width = 8;
                list.Add(IS1);
                VehicleFramework.VehicleParts.VehicleStorage IS2 = new VehicleFramework.VehicleParts.VehicleStorage();
                IS2.Container = innate2.gameObject;
                IS2.Height = 10;
                IS2.Width = 8;
                list.Add(IS2);

                return list;
            }
        }

        public override List<VehicleStorage> ModularStorages
        {
            get
            {
                var list = new List<VehicleFramework.VehicleParts.VehicleStorage>();
                return list;
            }
        }

        public override List<VehicleUpgrades> Upgrades
        {
            get
            {
                var list = new List<VehicleFramework.VehicleParts.VehicleUpgrades>();
                VehicleFramework.VehicleParts.VehicleUpgrades vu = new VehicleFramework.VehicleParts.VehicleUpgrades();
                vu.Interface = transform.Find("UpgradesInterface").gameObject;
                vu.Flap = vu.Interface;
                vu.AnglesClosed = Vector3.zero;
                vu.AnglesOpened = new Vector3(0, 30, 0);

                vu.ModuleProxies = null;

                list.Add(vu);
                return list;
            }
        }

        public override List<VehicleBattery> Batteries
        {
            get
            {
                var list = new List<VehicleFramework.VehicleParts.VehicleBattery>();

                VehicleFramework.VehicleParts.VehicleBattery vb1 = new VehicleFramework.VehicleParts.VehicleBattery();
                vb1.BatterySlot = transform.Find("Batteries/Battery1").gameObject;
                vb1.BatteryProxy = null;
                list.Add(vb1);

                VehicleFramework.VehicleParts.VehicleBattery vb2 = new VehicleFramework.VehicleParts.VehicleBattery();
                vb2.BatterySlot = transform.Find("Batteries/Battery2").gameObject;
                vb2.BatteryProxy = null;
                list.Add(vb2);

                return list;
            }
        }

        public override List<VehicleBattery> BackupBatteries
        {
            get
            {
                var list = new List<VehicleFramework.VehicleParts.VehicleBattery>();
                return null;
            }
        }

        public override List<VehicleFloodLight> HeadLights
        {
            get
            {
                var list = new List<VehicleFramework.VehicleParts.VehicleFloodLight>();

                list.Add(new VehicleFramework.VehicleParts.VehicleFloodLight
                {
                    Light = transform.Find("lights_parent/headlights/headlight1").gameObject,
                    Angle = 70,
                    Color = Color.white,
                    Intensity = 1.3f,
                    Range = 90f
                });
                list.Add(new VehicleFramework.VehicleParts.VehicleFloodLight
                {
                    Light = transform.Find("lights_parent/headlights/headlight2").gameObject,
                    Angle = 70,
                    Color = Color.white,
                    Intensity = 1.3f,
                    Range = 90f
                });
                list.Add(new VehicleFramework.VehicleParts.VehicleFloodLight
                {
                    Light = transform.Find("lights_parent/headlights/headlight3").gameObject,
                    Angle = 70,
                    Color = Color.white,
                    Intensity = 1.3f,
                    Range = 90f
                });
                list.Add(new VehicleFramework.VehicleParts.VehicleFloodLight
                {
                    Light = transform.Find("lights_parent/headlights/headlight4").gameObject,
                    Angle = 70,
                    Color = Color.white,
                    Intensity = 1.3f,
                    Range = 90f
                });
                list.Add(new VehicleFramework.VehicleParts.VehicleFloodLight
                {
                    Light = transform.Find("lights_parent/headlights/headlight5").gameObject,
                    Angle = 70,
                    Color = Color.white,
                    Intensity = 1.3f,
                    Range = 90f
                });

                return list;
            }
        }

        public override List<VehicleFloodLight> FloodLights
        {
            get
            {
                var list = new List<VehicleFramework.VehicleParts.VehicleFloodLight>();
                list.Add(new VehicleFramework.VehicleParts.VehicleFloodLight
                {
                    Light = transform.Find("lights_parent/floodlights/floodlight1").gameObject,
                    Angle = 90,
                    Color = Color.white,
                    Intensity = 1,
                    Range = 120f
                });
                list.Add(new VehicleFramework.VehicleParts.VehicleFloodLight
                {
                    Light = transform.Find("lights_parent/floodlights/floodlight2").gameObject,
                    Angle = 90,
                    Color = Color.white,
                    Intensity = 1,
                    Range = 120f
                });
                list.Add(new VehicleFramework.VehicleParts.VehicleFloodLight
                {
                    Light = transform.Find("lights_parent/floodlights/floodlight3").gameObject,
                    Angle = 90,
                    Color = Color.white,
                    Intensity = 1,
                    Range = 120f
                });
                list.Add(new VehicleFramework.VehicleParts.VehicleFloodLight
                {
                    Light = transform.Find("lights_parent/floodlights/floodlight4").gameObject,
                    Angle = 90,
                    Color = Color.white,
                    Intensity = 1,
                    Range = 120f
                });
                list.Add(new VehicleFramework.VehicleParts.VehicleFloodLight
                {
                    Light = transform.Find("lights_parent/floodlights/floodlight5").gameObject,
                    Angle = 90,
                    Color = Color.white,
                    Intensity = 1,
                    Range = 120f
                });

                return list;
            }
        }

        public override List<GameObject> NavigationPortLights
        {
            get
            {
                return null;
            }
        }

        public override List<GameObject> NavigationStarboardLights
        {
            get
            {
                return null;
            }
        }

        public override List<GameObject> NavigationPositionLights
        {
            get
            {
                return null;
            }
        }

        public override List<GameObject> NavigationWhiteStrobeLights
        {
            get
            {
                return null;
            }
        }

        public override List<GameObject> NavigationRedStrobeLights
        {
            get
            {
                return null;
            }
        }

        public override List<GameObject> WaterClipProxies
        {
            get
            {
                var list = new List<GameObject>();
                foreach (Transform child in transform.Find("WaterClipProxies"))
                {
                    list.Add(child.gameObject);
                }
                return list;
            }
        }

        public override List<GameObject> CanopyWindows
        {
            get
            {
                var list = new List<GameObject>();
                //                list.Add(transform.Find("model/glass2").gameObject);
                transform.Find("model/CanopyOutside").gameObject.SetActive(false);
                list.Add(transform.Find("model/CanopyInside").gameObject);
                return list;
            }
        }

        public override List<GameObject> TetherSources
        {
            get
            {
                var list = new List<GameObject>();
                foreach (Transform child in transform.Find("TetherSources"))
                {
                    list.Add(child.gameObject);
                }
                return list;
            }
        }
        public override GameObject ColorPicker
        {
            get
            {
                return null;
            }
        }
        public override GameObject Fabricator
        {
            get
            {
                return transform.Find("Fabricator-Location").gameObject;
            }
        }

        public override GameObject BoundingBox
        {
            get
            {
                return transform.Find("BoundingBox").gameObject;
            }
        }

        public override GameObject ControlPanel
        {
            get
            {
                controlPanel.transform.SetParent(transform);
                return controlPanel;
            }
        }

        public override GameObject CollisionModel
        {
            get
            {
                //return transform.Find("CollisionModel").gameObject;
                return null;
            }
        }

        public override GameObject SteeringWheelLeftHandTarget
        {
            get
            {
                //return transform.Find("Geometry/Interior_Main/SteeringConsole/SteeringConsoleArmature/SteeringRoot 1/SteeringStem1/SteeringStem2/SteeringWheel 1/LeftHandPlug").gameObject;
                return null;
            }
        }
        public override GameObject SteeringWheelRightHandTarget
        {
            get
            {
                //return transform.Find("Geometry/Interior_Main/SteeringConsole/SteeringConsoleArmature/SteeringRoot 1/SteeringStem1/SteeringStem2/SteeringWheel 1/RightHandPlug").gameObject;
                return null;
            }
        }

        public override void Awake()
        {
            // Give the Odyssey a new name and make sure we track it well.
            OGVehicleName = "ABY-" + Mathf.RoundToInt(UnityEngine.Random.value * 10000).ToString();
            vehicleName = OGVehicleName;
            NowVehicleName = OGVehicleName;

            // ModVehicle.Awake
            base.Awake();
        }
        public override void Start()
        {
            base.Start();
            SetupMotorWheels();
            SetupCameras();
            SetupCameraGUI();
        }

        public override void Update()
        {
            base.Update();
            MaybeControlCameras();

            guiCanvases.ForEach(x => x.Value.transform.Find("RawImage").localScale = Vector3.one * MainPatcher.config.guiSize);
            guiCanvases.ForEach(x => x.Value.transform.Find("RawImage").localPosition = new Vector3(MainPatcher.config.guiXPosition, MainPatcher.config.guiYPosition, x.Value.transform.Find("RawImage").localPosition.z));
        }

        public override void StopPiloting()
        {
            base.StopPiloting();
            DisableAllCameras();
            EnablePlayerCamera();
            currentCameraState = CameraState.player;
        }

        List<Camera> abyssCameras = new List<Camera>();
        private enum CameraState
        {
            bottomHatch,
            port,
            player,
            forward,
            starboard,
            rear,
            widelens
        }
        private CameraState currentCameraState = CameraState.player;
        private void SetupCameras()
        {
            List<string> cameraNames = new List<string>();
            cameraNames.Add("RoundCamera1");
            cameraNames.Add("RoundCamera2");
            cameraNames.Add("RoundCamera3");
            cameraNames.Add("RoundCamera4");
            cameraNames.Add("RoundCamera5");
            cameraNames.Add("RoundCamera6");
            foreach (string str in cameraNames)
            {
                Transform cameraObject = transform.Find(str + "/Inner_corpuse/Lens_camera/Camera");
                abyssCameras.Add(cameraObject.GetComponent<Camera>());
            }
            abyssCameras.ForEach(x => x.cullingMask = MainCamera.camera.cullingMask);
            foreach (GameObject cam in abyssCameras.Select(x => x.gameObject))
            {
                cam.EnsureComponent<WaterscapeVolumeOnCamera>().settings = MainCamera.camera.gameObject.GetComponent<WaterscapeVolumeOnCamera>().settings;
                cam.EnsureComponent<WaterSurfaceOnCamera>().waterSurface = MainCamera.camera.gameObject.GetComponent<WaterSurfaceOnCamera>().waterSurface;
                cam.EnsureComponent<WaterSunShaftsOnCamera>().surface = MainCamera.camera.gameObject.GetComponent<WaterSunShaftsOnCamera>().surface;
                cam.EnsureComponent<UnityEngine.PostProcessing.PostProcessingBehaviour>().profile = MainCamera.camera.gameObject.GetComponent<UnityEngine.PostProcessing.PostProcessingBehaviour>().profile;
                cam.EnsureComponent<UnityEngine.PostProcessing.PostProcessingBehaviour>().jitteredMatrixFunc = MainCamera.camera.gameObject.GetComponent<UnityEngine.PostProcessing.PostProcessingBehaviour>().jitteredMatrixFunc;
                cam.EnsureComponent<ColorCorrection>().shader = MainCamera.camera.gameObject.GetComponent<ColorCorrection>().shader;
                cam.EnsureComponent<ColorCorrection>().adjustWithDepth = false;
            }
            DisableAllCameras();
        }
        private void MaybeControlCameras()
        {
            if (IsPlayerPiloting())
            {
                ControlCameraState();
                ControlCameras();
            }
            else
            {
                DisableAllCameras();
            }
        }
        private void DisableAllCameras()
        {
            abyssCameras.ForEach(x => x.enabled = false);
            guiCanvases.ForEach(x => x.Value.SetActive(false));
        }
        private void ControlCameraState()
        {
            if (Input.GetKeyDown(MainPatcher.config.nextCamera))
            {
                currentCameraState++;
                if (currentCameraState > CameraState.widelens)
                {
                    currentCameraState = 0;
                }
            }
            else if (Input.GetKeyDown(MainPatcher.config.previousCamera))
            {
                currentCameraState--;
                if (currentCameraState < 0)
                {
                    currentCameraState = CameraState.widelens;
                }
            }
            else if (Input.GetKeyDown(MainPatcher.config.exitCamera))
            {
                currentCameraState = CameraState.player;
            }
        }
        private void ControlCameras()
        {
            switch (currentCameraState)
            {
                case CameraState.bottomHatch:
                    DisablePlayerCamera();
                    abyssCameras.Where(x => x.transform.parent.parent.parent.name.Contains("1")).ForEach(x => x.enabled = true);
                    abyssCameras.Where(x => !x.transform.parent.parent.parent.name.Contains("1")).ForEach(x => x.enabled = false);
                    break;
                case CameraState.port:
                    DisablePlayerCamera();
                    abyssCameras.Where(x => x.transform.parent.parent.parent.name.Contains("4")).ForEach(x => x.enabled = true);
                    abyssCameras.Where(x => !x.transform.parent.parent.parent.name.Contains("4")).ForEach(x => x.enabled = false);
                    break;
                case CameraState.player:
                    DisableAllCameras();
                    EnablePlayerCamera();
                    break;
                case CameraState.forward:
                    DisablePlayerCamera();
                    abyssCameras.Where(x => x.transform.parent.parent.parent.name.Contains("6")).ForEach(x => x.enabled = true);
                    abyssCameras.Where(x => !x.transform.parent.parent.parent.name.Contains("6")).ForEach(x => x.enabled = false);
                    break;
                case CameraState.starboard:
                    DisablePlayerCamera();
                    abyssCameras.Where(x => x.transform.parent.parent.parent.name.Contains("5")).ForEach(x => x.enabled = true);
                    abyssCameras.Where(x => !x.transform.parent.parent.parent.name.Contains("5")).ForEach(x => x.enabled = false);
                    break;
                case CameraState.rear:
                    DisablePlayerCamera();
                    abyssCameras.Where(x => x.transform.parent.parent.parent.name.Contains("3")).ForEach(x => x.enabled = true);
                    abyssCameras.Where(x => !x.transform.parent.parent.parent.name.Contains("3")).ForEach(x => x.enabled = false);
                    break;
                case CameraState.widelens:
                    DisablePlayerCamera();
                    abyssCameras.Where(x => x.transform.parent.parent.parent.name.Contains("2")).ForEach(x => x.enabled = true);
                    abyssCameras.Where(x => !x.transform.parent.parent.parent.name.Contains("2")).ForEach(x => x.enabled = false);
                    break;
                default:
                    DisableAllCameras();
                    EnablePlayerCamera();
                    break;
            }
            guiCanvases.Where(x => x.Key == currentCameraState).ForEach(x => x.Value.SetActive(true));
            guiCanvases.Where(x => x.Key != currentCameraState).ForEach(x => x.Value.SetActive(false));
        }
        private void DisablePlayerCamera()
        {
            MainCameraControl.main.GetComponentInChildren<Camera>().enabled = false;
        }
        private void EnablePlayerCamera()
        {
            MainCameraControl.main.GetComponentInChildren<Camera>().enabled = true;
        }

        Dictionary<CameraState, GameObject> guiCanvases = new Dictionary<CameraState, GameObject>();
        private void SetupCameraGUI()
        {
            GameObject thisCameraGUI = Instantiate(cameraGUI);
            thisCameraGUI.transform.SetParent(transform);
            guiCanvases.Add(CameraState.player, thisCameraGUI.transform.Find("CanvasNone").gameObject);
            guiCanvases.Add(CameraState.forward, thisCameraGUI.transform.Find("CanvasForward").gameObject);
            guiCanvases.Add(CameraState.port, thisCameraGUI.transform.Find("CanvasPort").gameObject);
            guiCanvases.Add(CameraState.starboard, thisCameraGUI.transform.Find("CanvasStarboard").gameObject);
            guiCanvases.Add(CameraState.rear, thisCameraGUI.transform.Find("CanvasAstern").gameObject);
            guiCanvases.Add(CameraState.widelens, thisCameraGUI.transform.Find("CanvasDescent").gameObject);
            guiCanvases.Add(CameraState.bottomHatch, thisCameraGUI.transform.Find("CanvasHatch").gameObject);
            DisableAllCameras();
        }


        public override void ModVehicleReset()
        {
            base.ModVehicleReset();
        }
        public void SetupMotorWheels()
        {
            var wheelone = transform.Find("model/WheelOne").gameObject.EnsureComponent<MotorWheel>();
            var wheeltwo = transform.Find("model/WheelTwo").gameObject.EnsureComponent<MotorWheel>();
            var wheelthree = transform.Find("model/WheelThree").gameObject.EnsureComponent<MotorWheel>();
            wheelone.mwt = MotorWheelType.backforth;
            wheeltwo.mwt = MotorWheelType.leftright;
            wheelthree.mwt = MotorWheelType.downup;
            wheelone.mv = this;
            wheeltwo.mv = this;
            wheelthree.mv = this;
            gameObject.GetComponent<AbyssEngine>().backforth = wheelone;
            gameObject.GetComponent<AbyssEngine>().leftright = wheeltwo;
            gameObject.GetComponent<AbyssEngine>().downup = wheelthree;
        }
    }
}

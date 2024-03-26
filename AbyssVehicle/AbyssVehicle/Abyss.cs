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
using VehicleFramework.VehicleTypes;
using VehicleFramework.Engines;

namespace AbyssVehicle
{
    public partial class Abyss : Submarine
    {
        public static GameObject model = null;
        public static RuntimeAnimatorController animatorController = null;
        public static GameObject controlPanel = null;
        public static Atlas.Sprite pingSprite = null;
        public static Atlas.Sprite crafterSprite = null;

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
                if (obj.ToString().Contains("SpriteAtlas"))
                {
                    SpriteAtlas thisAtlas = (SpriteAtlas)obj;
                    Sprite ping = thisAtlas.GetSprite("PingSprite"); 
                    pingSprite = new Atlas.Sprite(ping);
                    Sprite ping2 = thisAtlas.GetSprite("CrafterSprite");
                    crafterSprite = new Atlas.Sprite(ping2);
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
        public override Dictionary<TechType, int> Recipe
        {
            get
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
        }
        public static IEnumerator Register()
        {
            GetAssets();
            Submarine abyss = model.EnsureComponent<Abyss>() as Submarine;
            abyss.name = "Abyss";
            yield return UWE.CoroutineHost.StartCoroutine(VehicleRegistrar.RegisterVehicle(abyss));
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
        public override string Description
        {
            get
            {
                return "A sturdy submarine with plenty of floorspace. With a wide flat top that can be tread upon, it's like a tiny island.";
            }
        }

        public override string EncyclopediaEntry
        {
            get
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
                return transform.Find("CollisionModel").gameObject;
            }
        }

        public override GameObject LeviathanGrabPoint
        {
            get
            {
                return PilotSeats.First().SitLocation;
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


        public override ModVehicleEngine Engine
        {
            get
            {
                return gameObject.EnsureComponent<AbyssEngine>();
            }
        }

        public override Atlas.Sprite PingSprite
        {
            get
            {
                return pingSprite;
            }
        }

        public override int BaseCrushDepth
        {
            get
            {
                return 600;
            }
        }

        public override int MaxHealth
        {
            get
            {
                return 1500;
            }
        }

        public override int Mass
        {
            get
            {
                return 5000;
            }
        }

        public override int NumModules
        {
            get
            {
                return 8;
            }
        }

        public override bool HasArms
        {
            get
            {
                return false;
            }
        }

        public override Atlas.Sprite CraftingSprite
        {
            get
            {
                return crafterSprite;
            }
        }
    }
}

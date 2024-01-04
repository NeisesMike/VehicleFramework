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

namespace Atrama
{
    public class Atrama : ModVehicle
    {
        public static GameObject model = null;
        public static GameObject controlPanel = null;
        public static Atlas.Sprite pingSprite = null;
        public static void GetAssets()
        {
            // load the asset bundle
            string modPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var myLoadedAssetBundle = AssetBundle.LoadFromFile(Path.Combine(modPath, "assets/atrama"));
            if (myLoadedAssetBundle == null)
            {
                //Logger.Log("Failed to load AssetBundle!");
                return;
            }

            System.Object[] arr = myLoadedAssetBundle.LoadAllAssets();
            foreach (System.Object obj in arr)
            {
                if (obj.ToString().Contains("PingSpriteAtlas"))
                {
                    SpriteAtlas thisAtlas = (SpriteAtlas)obj;
                    Sprite ping = thisAtlas.GetSprite("AtramaHudPing");
                    pingSprite = new Atlas.Sprite(ping);
                }
                else if (obj.ToString().Contains("Atrama"))
                {
                    model = (GameObject)obj;
                }
                else if (obj.ToString().Contains("Control-Panel"))
                {
                    controlPanel = (GameObject)obj;
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
            recipe.Add(TechType.TitaniumIngot, 1);
            recipe.Add(TechType.PlasteelIngot, 1);
            recipe.Add(TechType.Lubricant, 1);
            recipe.Add(TechType.AdvancedWiringKit, 1);
            recipe.Add(TechType.Lead, 2);
            recipe.Add(TechType.EnameledGlass, 2);
            return recipe;
        }
        public static IEnumerator Register()
        {
            GetAssets();
            ModVehicle atrama = model.EnsureComponent<Atrama>() as ModVehicle;
            yield return UWE.CoroutineHost.StartCoroutine(VehicleManager.RegisterVehicle(atrama, new VehicleFramework.Engines.AtramaEngine(), GetRecipe(), (PingType)121, pingSprite, 6, 2, 900, 1000, 4250));
        }

        public override string vehicleDefaultName
        {
            get
            {
                Language main = Language.main;
                if (!(main != null))
                {
                    return "ATRAMA";
                }
                return main.Get("AtramaDefaultName");
            }
        }
        public override string GetDescription()
        {
            return "A submarine built for construction. It is quite sluggish, but has an enormous storage capacity.";
        }
        public override void Awake()
        {
            // Give the Odyssey a new name and make sure we track it well.
            OGVehicleName = "ATR-" + Mathf.RoundToInt(UnityEngine.Random.value * 10000).ToString();
            vehicleName = OGVehicleName;
            NowVehicleName = OGVehicleName;
            base.Awake();
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
            string ency = "The Atrama is a submarine purpose built for Construction. ";
            ency += "Its signature arms (in development) are what earned it its Lithuanian name. \n";
            ency += "\nIt features:\n";
            ency += "- Two arms which have several different attachments (in development). \n";
            ency += "- Ample storage capacity, which can be further expanded by upgrades. \n";
            ency += "- A signature autopilot which can automatically level out the vessel. \n";
            ency += "\nRatings:\n";
            ency += "- Top Speed: 15m/s \n";
            ency += "- Acceleration: 3m/s/s \n";
            ency += "- Distance per Power Cell: 7.5km \n";
            ency += "- Crush Depth: 900 \n";
            ency += "- Upgrade Slots: 6 \n";
            ency += "- Dimensions: 7.5m x 4m x 14.5m \n";
            ency += "- Persons: 1-2 \n";
            ency += "\n\"Pass on the drama- just build the Atrama.\" ";
            return ency;
        }
        public override List<VehicleFramework.VehicleParts.VehicleBattery> Batteries
        {
            get
            {
                var list = new List<VehicleFramework.VehicleParts.VehicleBattery>();

                VehicleFramework.VehicleParts.VehicleBattery vb1 = new VehicleFramework.VehicleParts.VehicleBattery();
                vb1.BatterySlot = transform.Find("model/Mechanical-Panel/BatteryInputs/1").gameObject;
                list.Add(vb1);

                VehicleFramework.VehicleParts.VehicleBattery vb2 = new VehicleFramework.VehicleParts.VehicleBattery();
                vb2.BatterySlot = transform.Find("model/Mechanical-Panel/BatteryInputs/2").gameObject;
                list.Add(vb2);

                VehicleFramework.VehicleParts.VehicleBattery vb3 = new VehicleFramework.VehicleParts.VehicleBattery();
                vb3.BatterySlot = transform.Find("model/Mechanical-Panel/BatteryInputs/3").gameObject;
                list.Add(vb3);

                VehicleFramework.VehicleParts.VehicleBattery vb4 = new VehicleFramework.VehicleParts.VehicleBattery();
                vb4.BatterySlot = transform.Find("model/Mechanical-Panel/BatteryInputs/4").gameObject;
                list.Add(vb4);

                return list;
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
                return transform.Find("StorageRootObject").gameObject;
            }
        }
        public override GameObject ModulesRootObject
        {
            get
            {
                return transform.Find("ModulesRootObject").gameObject;
            }
        }
        public override List<VehicleFramework.VehicleParts.VehicleHatchStruct> Hatches
        {
            get
            {
                var list = new List<VehicleFramework.VehicleParts.VehicleHatchStruct>();
                VehicleFramework.VehicleParts.VehicleHatchStruct vhs = new VehicleFramework.VehicleParts.VehicleHatchStruct();
                Transform mainHatch = transform.Find("model/Hatch");
                vhs.Hatch = mainHatch.gameObject;
                vhs.EntryLocation = mainHatch.Find("Entry");
                vhs.ExitLocation = mainHatch.Find("Exit");
                vhs.SurfaceExitLocation = mainHatch.Find("SurfaceExit");
                list.Add(vhs);
                return list;
            }
        }
        public override List<VehicleFramework.VehicleParts.VehicleFloodLight> HeadLights
        {
            get
            {
                var list = new List<VehicleFramework.VehicleParts.VehicleFloodLight>();

                VehicleFramework.VehicleParts.VehicleFloodLight leftLight = new VehicleFramework.VehicleParts.VehicleFloodLight
                {
                    Light = transform.Find("lights_parent/HeadLights/LeftLight").gameObject,
                    Angle = 60,
                    Color = Color.white,
                    Intensity = 1.5f,
                    Range = 120f
                };
                list.Add(leftLight);

                VehicleFramework.VehicleParts.VehicleFloodLight rightLight = new VehicleFramework.VehicleParts.VehicleFloodLight
                {
                    Light = transform.Find("lights_parent/HeadLights/RightLight").gameObject,
                    Angle = 60,
                    Color = Color.white,
                    Intensity = 1.5f,
                    Range = 120f
                };
                list.Add(rightLight);

                return list;
            }
        }
        public override List<VehicleFramework.VehicleParts.VehicleFloodLight> FloodLights
        {
            get
            {
                var list = new List<VehicleFramework.VehicleParts.VehicleFloodLight>();

                VehicleFramework.VehicleParts.VehicleFloodLight mainFlood = new VehicleFramework.VehicleParts.VehicleFloodLight
                {
                    Light = transform.Find("lights_parent/FloodLights/main").gameObject,
                    Angle = 120,
                    Color = Color.white,
                    Intensity = 1f,
                    Range = 100f
                };
                list.Add(mainFlood);


                VehicleFramework.VehicleParts.VehicleFloodLight portFlood = new VehicleFramework.VehicleParts.VehicleFloodLight
                {
                    Light = transform.Find("lights_parent/FloodLights/port").gameObject,
                    Angle = 90,
                    Color = Color.white,
                    Intensity = 1,
                    Range = 100f
                };
                list.Add(portFlood);


                VehicleFramework.VehicleParts.VehicleFloodLight starboardFlood = new VehicleFramework.VehicleParts.VehicleFloodLight
                {
                    Light = transform.Find("lights_parent/FloodLights/starboard").gameObject,
                    Angle = 90,
                    Color = Color.white,
                    Intensity = 1f,
                    Range = 100f
                };
                list.Add(starboardFlood);

                return list;
            }
        }
        public override List<VehicleFramework.VehicleParts.VehiclePilotSeat> PilotSeats
        {
            get
            {
                var list = new List<VehicleFramework.VehicleParts.VehiclePilotSeat>();
                VehicleFramework.VehicleParts.VehiclePilotSeat vps = new VehicleFramework.VehicleParts.VehiclePilotSeat();
                Transform mainSeat = transform.Find("model/PilotSeat");
                vps.Seat = mainSeat.gameObject;
                vps.SitLocation = mainSeat.Find("SitLocation").gameObject;
                vps.ExitLocation = mainSeat.Find("ExitLocation");
                vps.LeftHandLocation = mainSeat;
                vps.RightHandLocation = mainSeat;
                list.Add(vps);
                return list;
            }
        }
        public override List<VehicleFramework.VehicleParts.VehicleStorage> InnateStorages
        {
            get
            {
                var list = new List<VehicleFramework.VehicleParts.VehicleStorage>();

                Transform left = transform.Find("model/InnateStorage/LeftStorage");
                Transform right = transform.Find("model/InnateStorage/RightStorage");

                VehicleFramework.VehicleParts.VehicleStorage leftVS = new VehicleFramework.VehicleParts.VehicleStorage();
                leftVS.Container = left.gameObject;
                leftVS.Height = 8;
                leftVS.Width = 6;
                list.Add(leftVS);

                VehicleFramework.VehicleParts.VehicleStorage rightVS = new VehicleFramework.VehicleParts.VehicleStorage();
                rightVS.Container = right.gameObject;
                rightVS.Height = 8;
                rightVS.Width = 6;
                list.Add(rightVS);

                return list;
            }
        }
        public override List<VehicleFramework.VehicleParts.VehicleStorage> ModularStorages
        {
            get
            {
                var list = new List<VehicleFramework.VehicleParts.VehicleStorage>();
                for(int i=1; i<7; i++)
                {
                    VehicleFramework.VehicleParts.VehicleStorage thisVS = new VehicleFramework.VehicleParts.VehicleStorage();
                    Transform thisStorage = transform.Find("model/ModularStorage/StorageModule" + i.ToString());
                    thisVS.Container = thisStorage.gameObject;
                    thisVS.Height = 4;
                    thisVS.Width = 4;
                    list.Add(thisVS);
                }
                return list;
            }
        }
        public override List<VehicleFramework.VehicleParts.VehicleUpgrades> Upgrades
        {
            get
            {
                var list = new List<VehicleFramework.VehicleParts.VehicleUpgrades>();
                VehicleFramework.VehicleParts.VehicleUpgrades vu = new VehicleFramework.VehicleParts.VehicleUpgrades();
                vu.Interface = transform.Find("model/Mechanical-Panel/Upgrades-Panel").gameObject;
                vu.Flap = vu.Interface;
                vu.AnglesClosed = Vector3.zero;
                vu.AnglesOpened = Vector3.zero;
                list.Add(vu);
                return list;
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
                return transform.Find("model/BoundingBox").gameObject;
            }
        }
        public override List<GameObject> TetherSources
        {
            get
            {
                var list = new List<GameObject>();
                foreach (Transform child in transform.Find("model/TetherSources"))
                {
                    list.Add(child.gameObject);
                }
                return list;
            }
        }
        public override List<GameObject> WaterClipProxies
        {
            get
            {
                var list = new List<GameObject>();
                foreach(Transform child in transform.Find("model/WaterClipProxies"))
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
                list.Add(transform.Find("model/Canopy").gameObject);
                return list;
            }
        }
        public override List<GameObject> NavigationPortLights
        {
            get
            {
                var list = new List<GameObject>();
                foreach (Transform child in transform.Find("lights_parent/NavigationLights/PortLights"))
                {
                    list.Add(child.gameObject);
                }
                return list;
            }
        }
        public override List<GameObject> NavigationStarboardLights
        {
            get
            {
                var list = new List<GameObject>();
                foreach (Transform child in transform.Find("lights_parent/NavigationLights/StarboardLights"))
                {
                    list.Add(child.gameObject);
                }
                return list;
            }
        }
        public override List<GameObject> NavigationPositionLights
        {
            get
            {
                var list = new List<GameObject>();
                foreach (Transform child in transform.Find("lights_parent/NavigationLights/PositionLights"))
                {
                    list.Add(child.gameObject);
                }
                return list;
            }
        }
        public override List<GameObject> NavigationWhiteStrobeLights
        {
            get
            {
                var list = new List<GameObject>();
                foreach (Transform child in transform.Find("lights_parent/NavigationLights/WhiteStrobes"))
                {
                    list.Add(child.gameObject);
                }
                return list;
            }
        }
        public override List<GameObject> NavigationRedStrobeLights
        {
            get
            {
                var list = new List<GameObject>();
                foreach (Transform child in transform.Find("lights_parent/NavigationLights/RedStrobes"))
                {
                    list.Add(child.gameObject);
                }
                return list;
            }
        }
        public override List<VehicleBattery> BackupBatteries
        {
            get
            {
                /*
                var list = new List<VehicleFramework.VehicleParts.VehicleBattery>();
                VehicleFramework.VehicleParts.VehicleBattery vb1 = new VehicleFramework.VehicleParts.VehicleBattery();
                vb1.BatterySlot = transform.Find("Main-Body/BackupBattery").gameObject;
                list.Add(vb1);
                */
                return new List<VehicleFramework.VehicleParts.VehicleBattery>();
            }
        }
        public override GameObject CollisionModel
        {
            get
            {
                return transform.Find("model/CollisionModel").gameObject;
            }
        }
    }
}

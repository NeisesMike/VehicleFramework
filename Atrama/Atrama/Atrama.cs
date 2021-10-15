using System;
using System.Collections.Generic;
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
                    pingSprite = new Atlas.Sprite(ping);
                }
                else if (obj.ToString().Contains("atrama"))
                {
                    model = (GameObject)obj;
                }
                else if (obj.ToString().Contains("Control-Panel"))
                {
                    controlPanel = (GameObject)obj;
                }
                else
                {
                    Logger.Log(obj.ToString());
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
        public static void Register()
        {
            GetAssets();
            ModVehicle atrama = model.EnsureComponent<Atrama>() as ModVehicle;
            VehicleManager.RegisterVehicle(ref atrama, new VehicleFramework.Engines.AtramaEngine(), GetRecipe(), (PingType)121, pingSprite, 6, 2);
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

        public override List<VehicleFramework.VehicleParts.VehicleBattery> Batteries
        {
            get
            {
                var list = new List<VehicleFramework.VehicleParts.VehicleBattery>();

                VehicleFramework.VehicleParts.VehicleBattery vb1 = new VehicleFramework.VehicleParts.VehicleBattery();
                vb1.BatterySlot = transform.Find("Mechanical-Panel/BatteryInputs/1").gameObject;
                list.Add(vb1);

                VehicleFramework.VehicleParts.VehicleBattery vb2 = new VehicleFramework.VehicleParts.VehicleBattery();
                vb2.BatterySlot = transform.Find("Mechanical-Panel/BatteryInputs/2").gameObject;
                list.Add(vb2);

                VehicleFramework.VehicleParts.VehicleBattery vb3 = new VehicleFramework.VehicleParts.VehicleBattery();
                vb3.BatterySlot = transform.Find("Mechanical-Panel/BatteryInputs/3").gameObject;
                list.Add(vb3);

                VehicleFramework.VehicleParts.VehicleBattery vb4 = new VehicleFramework.VehicleParts.VehicleBattery();
                vb4.BatterySlot = transform.Find("Mechanical-Panel/BatteryInputs/4").gameObject;
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
                Transform mainHatch = transform.Find("Hatch");
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

                VehicleFramework.VehicleParts.VehicleFloodLight leftLight = new VehicleFramework.VehicleParts.VehicleFloodLight();
                leftLight.Light = transform.Find("HeadLights/LeftLight").gameObject;
                leftLight.Angle = 60;
                leftLight.Color = Color.white;
                leftLight.Intensity = 1.5f;
                leftLight.Range = 120f;
                list.Add(leftLight);

                VehicleFramework.VehicleParts.VehicleFloodLight rightLight = new VehicleFramework.VehicleParts.VehicleFloodLight();
                rightLight.Light = transform.Find("HeadLights/RightLight").gameObject;
                rightLight.Angle = 60;
                rightLight.Color = Color.white;
                rightLight.Intensity = 1.5f;
                rightLight.Range = 120f;
                list.Add(rightLight);

                return list;
            }
        }
        public override List<VehicleFramework.VehicleParts.VehicleFloodLight> FloodLights
        {
            get
            {
                var list = new List<VehicleFramework.VehicleParts.VehicleFloodLight>();

                VehicleFramework.VehicleParts.VehicleFloodLight mainFlood = new VehicleFramework.VehicleParts.VehicleFloodLight();
                mainFlood.Light = transform.Find("FloodLights/main").gameObject;
                mainFlood.Angle = 120;
                mainFlood.Color = Color.white;
                mainFlood.Intensity = 1f;
                mainFlood.Range = 100f;
                list.Add(mainFlood);


                VehicleFramework.VehicleParts.VehicleFloodLight portFlood = new VehicleFramework.VehicleParts.VehicleFloodLight();
                portFlood.Light = transform.Find("FloodLights/port").gameObject;
                portFlood.Angle = 90;
                portFlood.Color = Color.white;
                portFlood.Intensity = 1;
                portFlood.Range = 100f;
                list.Add(portFlood);


                VehicleFramework.VehicleParts.VehicleFloodLight starboardFlood = new VehicleFramework.VehicleParts.VehicleFloodLight();
                starboardFlood.Light = transform.Find("FloodLights/starboard").gameObject;
                starboardFlood.Angle = 90;
                starboardFlood.Color = Color.white;
                starboardFlood.Intensity = 1f;
                starboardFlood.Range = 100f;
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
                Transform mainSeat = transform.Find("Chair");
                vps.Seat = mainSeat.gameObject;
                vps.SitLocation = mainSeat.Find("SitLocation").gameObject;
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

                Transform left = transform.Find("InnateStorage/LeftStorage");
                Transform right = transform.Find("InnateStorage/RightStorage");

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
                    Transform thisStorage = transform.Find("ModularStorage/StorageModule" + i.ToString());
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
                vu.Interface = transform.Find("Mechanical-Panel/Upgrades-Panel").gameObject;
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
        public override GameObject BoundingBox
        {
            get
            {
                return transform.Find("BoundingBox").gameObject;
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
        public override List<GameObject> WaterClipProxies
        {
            get
            {
                var list = new List<GameObject>();
                foreach(Transform child in transform.Find("WaterClipProxies"))
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
                list.Add(transform.Find("Canopy").gameObject);
                return list;
            }
        }
        public override List<GameObject> NameDecals
        {
            get
            {
                var list = new List<GameObject>();
                list.Add(transform.Find("NameDecals/Left").gameObject);
                list.Add(transform.Find("NameDecals/Right").gameObject);
                return list;
            }
        }
        public override List<GameObject> NavigationPortLights
        {
            get
            {
                var list = new List<GameObject>();
                foreach (Transform child in transform.Find("NavigationLights/PortLights"))
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
                foreach (Transform child in transform.Find("NavigationLights/StarboardLights"))
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
                foreach (Transform child in transform.Find("NavigationLights/PositionLights"))
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
                foreach (Transform child in transform.Find("NavigationLights/WhiteStrobes"))
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
                foreach (Transform child in transform.Find("NavigationLights/RedStrobes"))
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
                var list = new List<VehicleFramework.VehicleParts.VehicleBattery>();
                VehicleFramework.VehicleParts.VehicleBattery vb1 = new VehicleFramework.VehicleParts.VehicleBattery();
                vb1.BatterySlot = transform.Find("Main-Body/BackupBattery").gameObject;
                list.Add(vb1);
                return list;
            }
        }
        public override GameObject CollisionModel
        {
            get
            {
                return transform.Find("CollisionModel").gameObject;
            }
        }
    }
}

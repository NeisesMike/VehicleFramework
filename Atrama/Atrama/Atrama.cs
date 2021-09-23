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
        public static void Register()
        {
            GetAssets();
            ModVehicle atrama = model.EnsureComponent<Atrama>() as ModVehicle;
            VehicleBuilder.RegisterVehicle(ref atrama, (PingType)121, pingSprite, 6, 2);
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
        public override List<GameObject> WalkableInteriors => new List<GameObject>() { transform.Find("Main-Body/InteriorTrigger").gameObject };
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
        public override List<VehicleFramework.VehicleParts.VehicleLight> Lights
        {
            get
            {
                var list = new List<VehicleFramework.VehicleParts.VehicleLight>();

                VehicleFramework.VehicleParts.VehicleLight leftLight = new VehicleFramework.VehicleParts.VehicleLight();
                leftLight.Light = transform.Find("LightsParent/LeftLight").gameObject;
                leftLight.Angle = 45;
                leftLight.Color = Color.white;
                leftLight.Strength = 60;
                list.Add(leftLight);

                VehicleFramework.VehicleParts.VehicleLight rightLight = new VehicleFramework.VehicleParts.VehicleLight();
                rightLight.Light = transform.Find("LightsParent/RightLight").gameObject;
                rightLight.Angle = 45;
                rightLight.Color = Color.white;
                rightLight.Strength = 60;
                list.Add(rightLight);

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
                vps.SitLocation = mainSeat.gameObject;
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
    }
}

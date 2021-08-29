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
                vb1.BatterySlot = model.transform.Find("Mechanical-Panel/BatteryInputs/1").gameObject;
                list.Add(vb1);

                VehicleFramework.VehicleParts.VehicleBattery vb2 = new VehicleFramework.VehicleParts.VehicleBattery();
                vb2.BatterySlot = model.transform.Find("Mechanical-Panel/BatteryInputs/2").gameObject;
                list.Add(vb2);

                VehicleFramework.VehicleParts.VehicleBattery vb3 = new VehicleFramework.VehicleParts.VehicleBattery();
                vb3.BatterySlot = model.transform.Find("Mechanical-Panel/BatteryInputs/3").gameObject;
                list.Add(vb3);

                VehicleFramework.VehicleParts.VehicleBattery vb4 = new VehicleFramework.VehicleParts.VehicleBattery();
                vb4.BatterySlot = model.transform.Find("Mechanical-Panel/BatteryInputs/4").gameObject;
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
                return model.transform.Find("StorageRootObject").gameObject;
            }
        }
        public override GameObject ModulesRootObject
        {
            get
            {
                return model.transform.Find("ModulesRootObject").gameObject;
            }
        }
        public override List<GameObject> WalkableInteriors => new List<GameObject>() { model.transform.Find("Main-Body/InteriorTrigger").gameObject };

        public override List<VehicleFramework.VehicleParts.VehicleHatchStruct> Hatches
        {
            get
            {
                var list = new List<VehicleFramework.VehicleParts.VehicleHatchStruct>();
                VehicleFramework.VehicleParts.VehicleHatchStruct vhs = new VehicleFramework.VehicleParts.VehicleHatchStruct();
                Transform mainHatch = model.transform.Find("Hatch");
                vhs.Hatch = mainHatch.gameObject;
                vhs.EntryLocation = mainHatch.Find("Entry");
                vhs.ExitLocation = mainHatch.Find("Exit");
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
                leftLight.Light = model.transform.Find("LightsParent/LeftLight").gameObject;
                leftLight.Angle = 45;
                leftLight.Color = Color.white;
                leftLight.Strength = 150;
                list.Add(leftLight);

                VehicleFramework.VehicleParts.VehicleLight rightLight = new VehicleFramework.VehicleParts.VehicleLight();
                rightLight.Light = model.transform.Find("LightsParent/RightLight").gameObject;
                rightLight.Angle = 45;
                rightLight.Color = Color.white;
                rightLight.Strength = 150;
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
                Transform mainSeat = model.transform.Find("Chair");
                vps.Seat = mainSeat.gameObject;
                vps.SitLocation = mainSeat.gameObject;
                vps.LeftHandLocation = mainSeat;
                vps.RightHandLocation = mainSeat;
                list.Add(vps);
                return list;
            }
        }

        public override List<VehicleFramework.VehicleParts.VehicleStorage> Storages
{
            get
            {
                var list = new List<VehicleFramework.VehicleParts.VehicleStorage>();

                Transform left = model.transform.Find("LeftStorage");
                Transform right = model.transform.Find("RightStorage");

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

        public override List<VehicleFramework.VehicleParts.VehicleUpgrades> Upgrades
        {
            get
            {
                var list = new List<VehicleFramework.VehicleParts.VehicleUpgrades>();
                VehicleFramework.VehicleParts.VehicleUpgrades vu = new VehicleFramework.VehicleParts.VehicleUpgrades();
                vu.Interface = model.transform.Find("Mechanical-Panel/Upgrades-Panel").gameObject;
                list.Add(vu);
                return list;
            }
        }
    }
}

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

namespace CrushDrone
{
    public class Crush : Drone
    {
        public override Transform CameraLocation
        {
            get
            {
                return transform.Find("CameraLocation");
            }
        }
        public override List<GameObject> PairingButtons
        {
            get
            {

                var list = new List<GameObject>();
                list.Add(transform.Find("FrontButton").gameObject);
                list.Add(transform.Find("RearButton").gameObject);
                return list;
            }
        }
        public override string vehicleDefaultName
        {
            get
            {
                return "Crush";
            }
        }

        public override GameObject VehicleModel
        {
            get
            {
                return model;
            }
        }

        public override GameObject CollisionModel
        {
            get
            {
                return transform.Find("CollisionModel").gameObject;
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

        public override List<VehicleStorage> InnateStorages
        {
            get
            {
                var list = new List<VehicleFramework.VehicleParts.VehicleStorage>();

                VehicleFramework.VehicleParts.VehicleStorage thisVS = new VehicleFramework.VehicleParts.VehicleStorage();
                Transform thisStorage = transform.Find("ChassiTop");
                thisVS.Container = thisStorage.gameObject;
                thisVS.Height = 5;
                thisVS.Width = 4;
                list.Add(thisVS);

                return list;
            }
        }

        public override List<VehicleStorage> ModularStorages
        {
            get
            {
                return null;
            }
        }

        public override List<VehicleUpgrades> Upgrades
        {
            get
            {
                var list = new List<VehicleFramework.VehicleParts.VehicleUpgrades>();
                VehicleFramework.VehicleParts.VehicleUpgrades vu = new VehicleFramework.VehicleParts.VehicleUpgrades();
                vu.Interface = transform.Find("BackLeft").gameObject;
                vu.Flap = vu.Interface;
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
                vb1.BatterySlot = transform.Find("BackRight").gameObject;
                list.Add(vb1);

                return list;
            }
        }

        public override List<VehicleFloodLight> HeadLights
        {
            get
            {
                var list = new List<VehicleFramework.VehicleParts.VehicleFloodLight>();

                VehicleFramework.VehicleParts.VehicleFloodLight leftLight = new VehicleFramework.VehicleParts.VehicleFloodLight
                {
                    Light = transform.Find("lights_parent/headlights/left").gameObject,
                    Angle = 100,
                    Color = Color.white,
                    Intensity = 0.65f,
                    Range = 30f
                };
                list.Add(leftLight);

                VehicleFramework.VehicleParts.VehicleFloodLight rightLight = new VehicleFramework.VehicleParts.VehicleFloodLight
                {
                    Light = transform.Find("lights_parent/headlights/right").gameObject,
                    Angle = 100,
                    Color = Color.white,
                    Intensity = 0.65f,
                    Range = 30f
                };
                list.Add(rightLight);


                return list;
            }
        }

        public override List<GameObject> WaterClipProxies => null;

        public override List<GameObject> CanopyWindows => null;

        public override GameObject BoundingBox
        {
            get
            {
                return transform.Find("BoundingBox").gameObject;
            }
        }

        public override Dictionary<TechType, int> Recipe
        {
            get
            {
                Dictionary<TechType, int> recipe = new Dictionary<TechType, int>();
                recipe.Add(TechType.Titanium, 4);
                recipe.Add(TechType.PowerCell, 1);
                recipe.Add(TechType.Glass, 1);
                recipe.Add(TechType.Lubricant, 1);
                recipe.Add(TechType.Lead, 1);
                recipe.Add(TechType.ComputerChip, 1);
                return recipe;
            }
        }

        public override Atlas.Sprite PingSprite
        {
            get
            {
                return pingSprite;
            }
        }

        public override string Description
        {
            get
            {
                return "A small drone with powerful claws capable of collecting resources.";
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
                string ency = "The Crush is a remotely controlled drone designed for resource collection.";
                ency += "Its powerful claws are what earned it its name. \n";
                ency += "\nIt features:\n";
                ency += "- Remote Connectivity \n";
                ency += "- Powerful claws for collecting resources \n";
                ency += "- One power cell capacity. \n";



                ency += "\nRatings:\n";
                ency += "- Top Speed: 12m/s \n";
                ency += "- Acceleration: 6m/s/s \n";
                ency += "- Distance per Power Cell: 7km \n";
                ency += "- Crush Depth: 300 \n";
                ency += "- Max Crush Depth (upgrade required): 1100 \n";
                ency += "- Upgrade Slots: 4 \n";
                ency += "- Dimensions: 3.5m x 3.5m x 3.1m \n";



                ency += "- Persons: 0\n";
                ency += "\n\"You can count on your Crush.\" ";
                return ency;
            }
        }

        public override int BaseCrushDepth => 300;

        public override int CrushDepthUpgrade1 => 200;

        public override int CrushDepthUpgrade2 => 400;

        public override int CrushDepthUpgrade3 => 800;

        public override int MaxHealth => 250;

        public override int Mass => 500;

        public override int NumModules => 2;

        public override bool HasArms => false;
        public override Atlas.Sprite CraftingSprite
        {
            get
            {
                return crafterSprite;
            }
        }


        public static GameObject model = null;
        public static Atlas.Sprite pingSprite = null;
        public static Atlas.Sprite crafterSprite = null;

        public static void GetAssets()
        {
            // load the asset bundle
            string modPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var myLoadedAssetBundle = AssetBundle.LoadFromFile(Path.Combine(modPath, "crush"));
            if (myLoadedAssetBundle == null)
            {
                Logger.Log("CrushDrone Failed to load AssetBundle!");
                return;
            }
            System.Object[] arr = myLoadedAssetBundle.LoadAllAssets();
            foreach (System.Object obj in arr)
            {
                if (obj.ToString().Contains("SpriteAtlas"))
                {
                    SpriteAtlas thisAtlas = (SpriteAtlas)obj;

                    Sprite ping = thisAtlas.GetSprite("DronePing");
                    pingSprite = new Atlas.Sprite(ping);

                    //Sprite ping4 = thisAtlas.GetSprite("DroneCrafterSprite");
                    //crafterSprite = new Atlas.Sprite(ping4);
                }
                else if (obj.ToString().Contains("Crush"))
                {
                    model = (GameObject)obj;
                }
                else
                {
                    Logger.Log(obj.ToString());
                }
            }
        }

        public static IEnumerator Register()
        {
            Drone crush = model.EnsureComponent<Crush>() as Drone;
            yield return UWE.CoroutineHost.StartCoroutine(VehicleRegistrar.RegisterVehicle(crush));
        }
    }
}

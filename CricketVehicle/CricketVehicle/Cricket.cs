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

namespace CricketVehicle
{
    public partial class Cricket : Submersible
    {
        public static GameObject model = null;
        public static GameObject controlPanel = null;
        public static Atlas.Sprite pingSprite = null;
        public static Atlas.Sprite cratePingSprite = null;
        public static Atlas.Sprite crafterSprite = null;
        public static Atlas.Sprite boxCrafterSprite = null;
        public static GameObject storageContainer = null;
        
        public static void GetAssets()
        {
            // load the asset bundle
            string modPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var myLoadedAssetBundle = AssetBundle.LoadFromFile(Path.Combine(modPath, "assets/cricket"));
            if (myLoadedAssetBundle == null)
            {
                Logger.Log("Failed to load AssetBundle!");
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

                    Sprite ping2 = thisAtlas.GetSprite("BoxSprite");
                    cratePingSprite = new Atlas.Sprite(ping2);

                    Sprite ping3 = thisAtlas.GetSprite("CrafterSprite");
                    crafterSprite = new Atlas.Sprite(ping3);

                    Sprite ping4 = thisAtlas.GetSprite("BoxCrafterSprite");
                    boxCrafterSprite = new Atlas.Sprite(ping4);
                }
                else if (obj.ToString().Contains("Cricket"))
                {
                    model = (GameObject)obj;
                }
                else if (obj.ToString().Contains("SFCrate"))
                {
                    storageContainer = (GameObject)obj;
                    var cc = Cricket.storageContainer.EnsureComponent<CricketContainer>();
                    cc.SetupGameObjectPregame();
                }
                else
                {
                    Logger.Log(obj.ToString());
                }
            }
        }

        public override Dictionary<TechType, int> Recipe
        {
            get
            {
                Dictionary<TechType, int> recipe = new Dictionary<TechType, int>();
                recipe.Add(TechType.TitaniumIngot, 1);
                recipe.Add(TechType.PowerCell, 1);
                recipe.Add(TechType.EnameledGlass, 2);
                recipe.Add(TechType.Lubricant, 1);
                recipe.Add(TechType.Lead, 1);
                recipe.Add(TechType.WiringKit, 1);
                return recipe;
            }
        }

        public static IEnumerator Register()
        {
            Submersible cricket = model.EnsureComponent<Cricket>() as Submersible;
            yield return UWE.CoroutineHost.StartCoroutine(VehicleRegistrar.RegisterVehicle(cricket));
        }

        public override string vehicleDefaultName
        {
            get
            {
                Language main = Language.main;
                if (!(main != null))
                {
                    return "CRICKET";
                }
                return main.Get("CricketDefaultName");
            }
        }

        public override string Description
        {
            get
            {
                return "A small spherical submersible built for rapid forward movement of personal and cargo.";
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
                string ency = "The Cricket is a submersible designed for rapid movement of personal and cargo through tight spaces.";
                ency += "Its speed and size are what earned it the name. \n";
                ency += "\nIt features:\n";
                ency += "- A mount for one Cricket Container (built separately). \n";
                ency += "- Rapid acceleration in all directions, but only a high top speed moving forward. \n";
                ency += "- One power cell in each of the two small thrusters. \n";
                ency += "\nRatings:\n";
                ency += "- Top Speed: 12m/s \n";
                ency += "- Acceleration: 6m/s/s \n";
                ency += "- Distance per Power Cell: 7km \n";
                ency += "- Crush Depth: 250 \n";
                ency += "- Max Crush Depth (upgrade required): 1100 \n";
                ency += "- Upgrade Slots: 4 \n";
                ency += "- Dimensions: 3.5m x 3.5m x 3.1m \n";
                ency += "- Persons: 1\n";
                ency += "\n\"If you get scared, jump on outta there.\" ";
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

        public override VehiclePilotSeat PilotSeat
        {
            get
            {
                VehicleFramework.VehicleParts.VehiclePilotSeat vps = new VehicleFramework.VehicleParts.VehiclePilotSeat();
                Transform mainSeat = transform.Find("Chair");
                vps.Seat = mainSeat.gameObject;
                vps.SitLocation = mainSeat.Find("SitPosition").gameObject;
                vps.LeftHandLocation = mainSeat;
                vps.RightHandLocation = mainSeat;
                return vps;
            }
        }

        public override List<VehicleHatchStruct> Hatches
        {
            get
            {
                var list = new List<VehicleFramework.VehicleParts.VehicleHatchStruct>();

                VehicleFramework.VehicleParts.VehicleHatchStruct interior_vhs = new VehicleFramework.VehicleParts.VehicleHatchStruct();
                Transform intHatch = transform.Find("Hatch");
                interior_vhs.Hatch = intHatch.gameObject;
                interior_vhs.ExitLocation = intHatch.Find("ExitPosition");
                interior_vhs.SurfaceExitLocation = intHatch.Find("ExitPosition");
                list.Add(interior_vhs);


                VehicleFramework.VehicleParts.VehicleHatchStruct interior_vhs2 = new VehicleFramework.VehicleParts.VehicleHatchStruct();
                interior_vhs2.Hatch = transform.Find("CollisionModel/Sphere").gameObject;
                interior_vhs2.ExitLocation = interior_vhs.ExitLocation;
                interior_vhs2.SurfaceExitLocation = interior_vhs.ExitLocation;
                list.Add(interior_vhs2);
                
                return list;
            }
        }

        public override List<VehicleStorage> InnateStorages
        {
            get
            {
                var list = new List<VehicleFramework.VehicleParts.VehicleStorage>();
                return list;
            }
        }

        public override List<VehicleStorage> ModularStorages
        {
            get
            {
                var list = new List<VehicleFramework.VehicleParts.VehicleStorage>();

                VehicleFramework.VehicleParts.VehicleStorage thisVS = new VehicleFramework.VehicleParts.VehicleStorage();
                Transform thisStorage = transform.Find("SFCrate");
                thisVS.Container = thisStorage.gameObject;
                thisVS.Height = 6;
                thisVS.Width = 5;
                list.Add(thisVS);

                return list;
            }
        }

        public override List<VehicleUpgrades> Upgrades
        {
            get
            {
                var list = new List<VehicleFramework.VehicleParts.VehicleUpgrades>();
                VehicleFramework.VehicleParts.VehicleUpgrades vu = new VehicleFramework.VehicleParts.VehicleUpgrades();
                vu.Interface = transform.Find("CollisionModel/MainProp").gameObject;
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
                vb1.BatterySlot = transform.Find("CollisionModel/LeftProp").gameObject;
                list.Add(vb1);

                VehicleFramework.VehicleParts.VehicleBattery vb2 = new VehicleFramework.VehicleParts.VehicleBattery();
                vb2.BatterySlot = transform.Find("CollisionModel/RightProp").gameObject;
                list.Add(vb2);

                return list;
            }
        }

        public override List<VehicleBattery> BackupBatteries
        {
            get
            {
                return null;
            }
        }

        public override List<VehicleFloodLight> HeadLights
        {
            get
            {
                var list = new List<VehicleFramework.VehicleParts.VehicleFloodLight>();

                VehicleFramework.VehicleParts.VehicleFloodLight mainLight = new VehicleFramework.VehicleParts.VehicleFloodLight
                {
                    Light = transform.Find("lights_parent/HeadLights/Main").gameObject,
                    Angle = 70,
                    Color = Color.white,
                    Intensity = 1.3f,
                    Range = 90f
                };
                list.Add(mainLight);

                return list;
            }
        }

        public override List<GameObject> WaterClipProxies
        {
            get
            {
                var list = new List<GameObject>();
                list.Add(transform.Find("CollisionModel/Sphere").gameObject);
                /*
                foreach (Transform child in transform.Find("WaterClipProxies"))
                {
                    list.Add(child.gameObject);
                }
                */
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

        public override GameObject BoundingBox
        {
            get
            {
                return transform.Find("BoundingBox").gameObject;
            }
        }

        public override GameObject CollisionModel
        {
            get
            {
                return transform.Find("CollisionModel").gameObject;
            }
        }
        
        public override VehicleFramework.Engines.ModVehicleEngine Engine
        {
            get
            {
                return gameObject.EnsureComponent<VehicleFramework.Engines.CricketEngine>();
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
                return 250;
                // Degasi 1 @ 250
                // seamoth at 200 now
            }
        }

        public override int CrushDepthUpgrade1
        {
            get
            {
                return 150; // 400
                // seamoth at 300 now
            }
        }

        public override int CrushDepthUpgrade2
        {
            get
            {
                return 250; // 650
                // Degasi 2 @ 500
                // seamoth at 500 now
            }
        }

        public override int CrushDepthUpgrade3
        {
            get
            {
                return 450; // 1100, Lost River
                // 1700 end game
                // seamoth at 900 now
            }
        }

        public override int MaxHealth
        {
            get
            {
                return 420;
            }
        }

        public override int Mass
        {
            get
            {
                return 500;
            }
        }

        public override int NumModules
        {
            get
            {
                return 4;
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

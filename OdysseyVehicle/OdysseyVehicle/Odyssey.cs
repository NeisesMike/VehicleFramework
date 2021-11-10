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

namespace OdysseyVehicle
{
    public class Odyssey : ModVehicle
    {
        public static GameObject model = null;
        public static GameObject controlPanel = null;
        public static Atlas.Sprite pingSprite = null;
        public static void GetAssets()
        {
            // load the asset bundle
            string modPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var myLoadedAssetBundle = AssetBundle.LoadFromFile(Path.Combine(modPath, "assets/odyssey"));
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
                    Sprite ping = thisAtlas.GetSprite("OdysseyPingSprite");
                    pingSprite = new Atlas.Sprite(ping);
                }
                else if (obj.ToString().Contains("Odyssey"))
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
            ModVehicle odyssey = model.EnsureComponent<Odyssey>() as ModVehicle;
            VehicleManager.RegisterVehicle(ref odyssey, new VehicleFramework.Engines.OdysseyEngine(), GetRecipe(), (PingType)122, pingSprite, 8, 0, 700, 667);
        }

        public override string vehicleDefaultName
        {
            get
            {
                Language main = Language.main;
                if (!(main != null))
                {
                    return "ODYSSEY";
                }
                return main.Get("OdysseyDefaultName");
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

        public override List<VehiclePilotSeat> PilotSeats
        {
            get
            {
                var list = new List<VehicleFramework.VehicleParts.VehiclePilotSeat>();
                VehicleFramework.VehicleParts.VehiclePilotSeat vps = new VehicleFramework.VehicleParts.VehiclePilotSeat();
                Transform mainSeat = transform.Find("Seat");
                vps.Seat = mainSeat.gameObject;
                vps.SitLocation = mainSeat.Find("SitLocation").gameObject;
                vps.LeftHandLocation = mainSeat;
                vps.RightHandLocation = mainSeat;
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
                Transform intHatch = transform.Find("Hatches/InteriorHatch");
                interior_vhs.Hatch = intHatch.gameObject;
                interior_vhs.EntryLocation = intHatch.Find("Entry");
                interior_vhs.ExitLocation = intHatch.Find("Exit");
                interior_vhs.SurfaceExitLocation = intHatch.Find("SurfaceExit");

                VehicleFramework.VehicleParts.VehicleHatchStruct exterior_vhs = new VehicleFramework.VehicleParts.VehicleHatchStruct();
                Transform extHatch = transform.Find("Hatches/ExteriorHatch");
                exterior_vhs.Hatch = extHatch.gameObject;
                exterior_vhs.EntryLocation = extHatch.Find("Entry");
                exterior_vhs.ExitLocation = extHatch.Find("Exit");
                exterior_vhs.SurfaceExitLocation = extHatch.Find("SurfaceExit");

                list.Add(interior_vhs);
                list.Add(exterior_vhs);
                return list;
            }
        }

        public override List<VehicleStorage> InnateStorages
        {
            get
            {
                var list = new List<VehicleFramework.VehicleParts.VehicleStorage>();

                Transform innate1 = transform.Find("InnateStorage/1");
                Transform innate2 = transform.Find("InnateStorage/2");
                Transform innate3 = transform.Find("InnateStorage/3");
                Transform innate4 = transform.Find("InnateStorage/4");
                Transform innate5 = transform.Find("InnateStorage/5");

                VehicleFramework.VehicleParts.VehicleStorage IS1 = new VehicleFramework.VehicleParts.VehicleStorage();
                IS1.Container = innate1.gameObject;
                IS1.Height = 6;
                IS1.Width = 5;
                list.Add(IS1);
                VehicleFramework.VehicleParts.VehicleStorage IS2 = new VehicleFramework.VehicleParts.VehicleStorage();
                IS2.Container = innate2.gameObject;
                IS2.Height = 6;
                IS2.Width = 5;
                list.Add(IS2);
                VehicleFramework.VehicleParts.VehicleStorage IS3 = new VehicleFramework.VehicleParts.VehicleStorage();
                IS3.Container = innate3.gameObject;
                IS3.Height = 6;
                IS3.Width = 5;
                list.Add(IS3);
                VehicleFramework.VehicleParts.VehicleStorage IS4 = new VehicleFramework.VehicleParts.VehicleStorage();
                IS4.Container = innate4.gameObject;
                IS4.Height = 6;
                IS4.Width = 5;
                list.Add(IS4);
                VehicleFramework.VehicleParts.VehicleStorage IS5 = new VehicleFramework.VehicleParts.VehicleStorage();
                IS5.Container = innate5.gameObject;
                IS5.Height = 6;
                IS5.Width = 5;
                list.Add(IS5);
                
                return list;
            }
        }

        public override List<VehicleStorage> ModularStorages
        {
            get
            {
                var list = new List<VehicleFramework.VehicleParts.VehicleStorage>();
                for (int i = 1; i <= 8; i++)
                {
                    VehicleFramework.VehicleParts.VehicleStorage thisVS = new VehicleFramework.VehicleParts.VehicleStorage();
                    Transform thisStorage = transform.Find("ModularStorages/" + i.ToString());
                    thisVS.Container = thisStorage.gameObject;
                    thisVS.Height = 4;
                    thisVS.Width = 4;
                    list.Add(thisVS);
                }
                return list;
            }
        }

        public override List<VehicleUpgrades> Upgrades
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

        public override List<VehicleBattery> Batteries
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

        public override List<VehicleBattery> BackupBatteries
        {
            get
            {
                var list = new List<VehicleFramework.VehicleParts.VehicleBattery>();
                VehicleFramework.VehicleParts.VehicleBattery vb1 = new VehicleFramework.VehicleParts.VehicleBattery();
                vb1.BatterySlot = transform.Find("Mechanical-Panel/BatteryInputs/BackupBattery").gameObject;
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
                    Light = transform.Find("LightsParent/HeadLights/Left").gameObject,
                    Angle = 70,
                    Color = Color.white,
                    Intensity = 1.3f,
                    Range = 90f
                };
                list.Add(leftLight);

                VehicleFramework.VehicleParts.VehicleFloodLight rightLight = new VehicleFramework.VehicleParts.VehicleFloodLight
                {
                    Light = transform.Find("LightsParent/HeadLights/Right").gameObject,
                    Angle = 70,
                    Color = Color.white,
                    Intensity = 1.3f,
                    Range = 90f
                };
                list.Add(rightLight);

                return list;
            }
        }

        public override List<VehicleFloodLight> FloodLights
        {
            get
            {
                var list = new List<VehicleFramework.VehicleParts.VehicleFloodLight>();

                VehicleFramework.VehicleParts.VehicleFloodLight mainFlood = new VehicleFramework.VehicleParts.VehicleFloodLight
                {
                    Light = transform.Find("LightsParent/FloodLights/FrontCenter").gameObject,
                    Angle = 120,
                    Color = Color.white,
                    Intensity = 1f,
                    Range = 100f
                };
                list.Add(mainFlood);

                foreach (Transform floodlight in transform.Find("LightsParent/FloodLights/LateralLights"))
                {
                    VehicleFramework.VehicleParts.VehicleFloodLight thisFloodLight = new VehicleFramework.VehicleParts.VehicleFloodLight
                    {
                        Light = floodlight.gameObject,
                        Angle = 90,
                        Color = Color.white,
                        Intensity = 1,
                        Range = 120f
                    };
                    list.Add(thisFloodLight);
                }

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
                list.Add(transform.Find("Canopy").gameObject);
                return list;
            }
        }

        public override List<GameObject> NameDecals
        {
            get
            {
                return new List<GameObject>();
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
    }
}

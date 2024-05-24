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

namespace OdysseyVehicle
{
    public class Odyssey : Submarine, ILightsStatusListener
    {
        public static GameObject model = null;
        public static RuntimeAnimatorController animatorController = null;
        public static GameObject controlPanel = null;
        public static Atlas.Sprite pingSprite = null;
        public static Atlas.Sprite crafterSprite = null;

        public const int textureRadius = 2048;

        public static Texture2D hull_default = null;

        public static Texture2D hull_details = null;
        public static Texture2D hull_height = null;
        public static Texture2D hull_metal = null;
        public static Texture2D hull_normal = null;

        public static GameObject name_label_generator = null;
        public static Texture2D name_details = null; 

        private static GameObject generator = null;
        private static Queue<Tuple<ModVehicle, string, Color, Color>> innerNameLabelsToGenerate = null;
        public static Queue<Tuple<ModVehicle, string, Color, Color>> NameLabelsToGenerate
        {
            get
            {
                if (innerNameLabelsToGenerate is null)
                {
                    innerNameLabelsToGenerate = new Queue<Tuple<ModVehicle, string, Color, Color>>();
                }
                return innerNameLabelsToGenerate;
            }
        }
        public static IEnumerator ManageLabelQueue()
        {
            while (true)
            {
                if (NameLabelsToGenerate.Count == 0)
                {
                    yield return new WaitForSeconds(1f);
                    continue;
                }
                var thisNameLabel = NameLabelsToGenerate.Dequeue();
                yield return PaintVehicleNameHelper(thisNameLabel.Item1, thisNameLabel.Item2, thisNameLabel.Item3, thisNameLabel.Item4);
            }
        }
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
                Logger.Log(obj.ToString());
                if (obj.ToString().Contains("SpriteAtlas"))
                {
                    SpriteAtlas thisAtlas = (SpriteAtlas)obj;

                    Sprite ping = thisAtlas.GetSprite("PingSprite");
                    pingSprite = new Atlas.Sprite(ping);

                    Sprite ping3 = thisAtlas.GetSprite("CrafterSprite");
                    crafterSprite = new Atlas.Sprite(ping3);
                }
                else if (obj.ToString().Contains("Odyssey"))
                {
                    model = (GameObject)obj;
                }
                else if (obj.ToString().Contains("AnimController"))
                {
                    animatorController = (RuntimeAnimatorController)obj;
                }
                else if (obj.ToString().Contains("Control-Panel"))
                {
                    controlPanel = (GameObject)obj;
                }
                else if (obj.ToString().Contains("hull_height"))
                {
                    hull_height = (Texture2D)obj;
                }
                else if (obj.ToString().Contains("hull_metal"))
                {
                    hull_metal = (Texture2D)obj;
                }
                else if (obj.ToString().Contains("hull_normal"))
                {
                    hull_normal = (Texture2D)obj;
                }
                else if (obj.ToString().Contains("hull_default"))
                {
                    hull_default = (Texture2D)obj;
                }
                else if (obj.ToString().Contains("NameLabelGenerator"))
                {
                    name_label_generator = (GameObject)obj;
                }
                else
                {
                    //Logger.Log(obj.ToString());
                }
            }

            // get the hull detailsas Texture2Ds
            // if we get these textures from an asset bundle,
            // we cannot later read the bytes of that texture.
            // Reading those bytes is critical, so we have to load the images like this.
            // It works out ok because then users can draw whatever they want on those images
            hull_details = new Texture2D(textureRadius, textureRadius, TextureFormat.ARGB32, false);
            byte[] hullStripeBytes = System.IO.File.ReadAllBytes(Path.Combine(modPath, "assets/hull_details.png"));
            hull_details.LoadImage(hullStripeBytes);

            name_details = new Texture2D(1024, 256, TextureFormat.ARGB32, false);
            byte[] nameDetailsBytes = System.IO.File.ReadAllBytes(Path.Combine(modPath, "assets/name_details.png"));
            name_details.LoadImage(nameDetailsBytes);
        }
        public override Dictionary<TechType, int> Recipe
        {
            get
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
        }
        public static IEnumerator Register()
        {
            GetAssets();
            Submarine odyssey = model.EnsureComponent<Odyssey>() as Submarine;
            odyssey.gameObject.GetComponent<Animator>().runtimeAnimatorController = animatorController;
            yield return UWE.CoroutineHost.StartCoroutine(VehicleRegistrar.RegisterVehicle(odyssey));
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
        public override string Description
        {
            get
            {
                return "A submarine built for exploration. It is nimble for its size, it fits into small corridors, and its floodlights are extremely powerful.";
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
                string ency = "The Odyssey is a submarine purpose-built for exploration. ";
                ency += "Its manueverability and illumination capabilities are what earned it the name. \n";
                ency += "\nIt features:\n";
                ency += "- Modest storage capacity, which can be further expanded with upgrades. \n";
                ency += "- Extremely high power flood lights. \n";
                ency += "- A signature autopilot which can automatically level out the vessel. \n";
                ency += "\nRatings:\n";
                ency += "- Top Speed: 12.5m/s \n";
                ency += "- Acceleration: 5m/s/s \n";
                ency += "- Distance per Power Cell: 7km \n";
                ency += "- Crush Depth: 600 \n";
                ency += "- Upgrade Slots: 8 \n";
                ency += "- Dimensions: 3.7m x 5m x 10.6m \n";
                ency += "- Persons: 1-2\n";
                ency += "\n\"Don't like it? That's odd; I see.\" ";
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
                Transform mainSeat = transform.Find("Geometry/Interior_Main_light/SteeringConsole/Seat");
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
                Transform intHatch = transform.Find("Geometry/Interior_Main_light/Hatch_light/InteriorHatch");
                interior_vhs.Hatch = intHatch.gameObject;
                interior_vhs.EntryLocation = intHatch.Find("Entry");
                interior_vhs.ExitLocation = intHatch.Find("Exit");
                interior_vhs.SurfaceExitLocation = intHatch.Find("SurfaceExit");

                VehicleFramework.VehicleParts.VehicleHatchStruct exterior_vhs = new VehicleFramework.VehicleParts.VehicleHatchStruct();
                Transform extHatch = transform.Find("Geometry/Interior_Main_light/Hatch_light/ExteriorHatch");
                exterior_vhs.Hatch = extHatch.gameObject;
                //exterior_vhs.EntryLocation = extHatch.Find("Entry");
                //exterior_vhs.ExitLocation = extHatch.Find("Exit");
                exterior_vhs.EntryLocation = interior_vhs.EntryLocation;
                exterior_vhs.ExitLocation = interior_vhs.ExitLocation;
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

                Transform innate1 = transform.Find("Geometry/Interior_Main_light/InnateStorage/DoorModule/InateStorageBaseRooms.001/InateStorageRoot.001/Rail1.001/Rail2.001/Rail3.001/InateStorageDoor.001_light");
                Transform innate2 = transform.Find("Geometry/Interior_Main_light/InnateStorage/DoorModule.001/InateStorageBaseRooms.002/InateStorageRoot.002/Rail1.002/Rail2.002/Rail3.002/InateStorageDoor.002_light");
                Transform innate3 = transform.Find("Geometry/Interior_Main_light/InnateStorage/DoorModule.002/InateStorageBaseRooms.003/InateStorageRoot.003/Rail1.003/Rail2.003/Rail3.003/InateStorageDoor.003_light");
                Transform innate4 = transform.Find("Geometry/Interior_Main_light/InnateStorage/DoorModule.003/InateStorageBaseRooms.004/InateStorageRoot.004/Rail1.004/Rail2.004/Rail3.004/InateStorageDoor.004_light");

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
                    Transform thisStorage = transform.Find("Geometry/ExternalStorage/ExternalStorage" + i.ToString());
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
                vu.Interface = transform.Find("Geometry/Exterior Panels/Panel Left/PanelInsertLeft").gameObject;
                vu.Flap = transform.Find("Geometry/Exterior Panels/Panel Left/DoorLeftTopHinge").gameObject;
                vu.AnglesClosed = Vector3.zero;
                vu.AnglesOpened = new Vector3(0, 90, 0);

                List<Transform> proxies = new List<Transform>();
                proxies.Add(transform.Find("Proxies/UpgradeModuleProxy_1"));
                proxies.Add(transform.Find("Proxies/UpgradeModuleProxy_2"));
                proxies.Add(transform.Find("Proxies/UpgradeModuleProxy_3"));
                proxies.Add(transform.Find("Proxies/UpgradeModuleProxy_4"));
                proxies.Add(transform.Find("Proxies/UpgradeModuleProxy_5"));
                proxies.Add(transform.Find("Proxies/UpgradeModuleProxy_6"));
                proxies.Add(transform.Find("Proxies/UpgradeModuleProxy_7"));
                proxies.Add(transform.Find("Proxies/UpgradeModuleProxy_8"));
                vu.ModuleProxies = proxies;

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
                vb1.BatterySlot = transform.Find("Geometry/Interior_Main_light/MainPower/PowerCellSlot.002").gameObject;
                vb1.BatteryProxy = transform.Find("Proxies/Battery_1_Proxy");
                list.Add(vb1);

                VehicleFramework.VehicleParts.VehicleBattery vb2 = new VehicleFramework.VehicleParts.VehicleBattery();
                vb2.BatterySlot = transform.Find("Geometry/Interior_Main_light/MainPower/PowerCellSlot.003").gameObject;
                vb2.BatteryProxy = transform.Find("Proxies/Battery_2_Proxy");
                list.Add(vb2);

                VehicleFramework.VehicleParts.VehicleBattery vb3 = new VehicleFramework.VehicleParts.VehicleBattery();
                vb3.BatterySlot = transform.Find("Geometry/Interior_Main_light/MainPower/PowerCellSlot").gameObject;
                vb3.BatteryProxy = transform.Find("Proxies/Battery_3_Proxy");
                list.Add(vb3);

                VehicleFramework.VehicleParts.VehicleBattery vb4 = new VehicleFramework.VehicleParts.VehicleBattery();
                vb4.BatterySlot = transform.Find("Geometry/Interior_Main_light/MainPower/PowerCellSlot.001").gameObject;
                vb4.BatteryProxy = transform.Find("Proxies/Battery_4_Proxy");
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
                vb1.BatterySlot = transform.Find("Geometry/Exterior Panels/Panel Right/PanelInsertRight").gameObject;
                vb1.BatteryProxy = transform.Find("Proxies/BackupBattery_1_Proxy");
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
                    Light = transform.Find("lights_parent/HeadLights/Left").gameObject,
                    Angle = 70,
                    Color = Color.white,
                    Intensity = 1.3f,
                    Range = 90f
                };
                list.Add(leftLight);

                VehicleFramework.VehicleParts.VehicleFloodLight rightLight = new VehicleFramework.VehicleParts.VehicleFloodLight
                {
                    Light = transform.Find("lights_parent/HeadLights/Right").gameObject,
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
                    Light = transform.Find("lights_parent/FloodLights/FrontCenter").gameObject,
                    Angle = 120,
                    Color = Color.white,
                    Intensity = 1f,
                    Range = 100f
                };
                list.Add(mainFlood);

                foreach (Transform floodlight in transform.Find("lights_parent/FloodLights/LateralLights"))
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
                foreach (Transform child in transform.Find("Things/WaterClipProxies"))
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
                list.Add(transform.Find("Geometry/Canopy_Inner").gameObject);
                list.Add(transform.Find("Geometry/Canopy_Outer").gameObject);
                list.Add(transform.Find("Geometry/camera/Camera_Glass_LP1").gameObject);
                list.Add(transform.Find("Geometry/camera2/Camera_Glass_LP1.001").gameObject);
                return list;
            }
        }

        public override List<GameObject> TetherSources
        {
            get
            {
                var list = new List<GameObject>();
                foreach (Transform child in transform.Find("Things/TetherSources"))
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
                return transform.Find("ColorPicker").gameObject;
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
                return transform.Find("Things/BoundingBox").gameObject;
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
                return transform.Find("Things/CollisionModel").gameObject;
            }
        }

        public override GameObject SteeringWheelLeftHandTarget
        {
            get
            {
                //return transform.Find("Geometry/Interior_Main_light/SteeringConsole/SteeringConsoleArmature/SteeringRoot 1/SteeringStem1/SteeringStem2/SteeringWheel 1/LeftHandPlug").gameObject;
                return null;
            }
        }
        public override GameObject SteeringWheelRightHandTarget
        {
            get
            {
                //return transform.Find("Geometry/Interior_Main_light/SteeringConsole/SteeringConsoleArmature/SteeringRoot 1/SteeringStem1/SteeringStem2/SteeringWheel 1/RightHandPlug").gameObject;
                return null;
            }
        }

        public override ModVehicleEngine Engine
        {
            get
            {
                return gameObject.EnsureComponent<OdysseyEngine>();
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
                return 667;
            }
        }

        public override int Mass
        {
            get
            {
                return 3500;
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

        public override List<Light> InteriorLights
        {
            get
            {
                List<Light> lights = new List<Light>
                {
                    transform.Find("wall_light1/light").GetComponent<Light>(),
                    transform.Find("wall_light2/light").GetComponent<Light>(),
                    transform.Find("wall_light3/light").GetComponent<Light>(),
                    transform.Find("wall_light4/light").GetComponent<Light>()
                };
                return lights;
            }
        }

        public override void SubConstructionBeginning()
        {
            base.SubConstructionBeginning();
            transform.Find("Geometry/Interior_Main_light").gameObject.SetActive(false);
        }
        public override void SubConstructionComplete()
        {
            base.SubConstructionComplete();
            transform.Find("Geometry/Interior_Main_light").gameObject.SetActive(true);
        }

        public override void PaintVehicleSection(string materialName, Color col)
        {
            IsDefaultTexture = false;
            base.PaintVehicleSection(materialName, col);
            StartCoroutine(PaintVehicleSectionHelper(materialName, col));
        }
        public IEnumerator PaintVehicleSectionHelper(string materialName, Color col)
        {
            Color[] detailPixels = hull_details.GetPixels();
            yield return null;
            // prepare the color texture
            int yieldBoundary = detailPixels.Length / 100;
            for (int i = 0; i < detailPixels.Length; i++)
            {
                if (i % yieldBoundary == 0)
                {
                    yield return null;
                }
                Color thisPixel = detailPixels[i];
                if (thisPixel.a == 0)
                {
                    detailPixels[i] = col;
                }
                else
                {
                    detailPixels[i] = Color.Lerp(col, thisPixel, thisPixel.a);
                    detailPixels[i].a = 1;
                }
            }
            yield return null;
            Texture2D newlyColoredTexture = new Texture2D(textureRadius, textureRadius);
            yield return null;
            newlyColoredTexture.SetPixels(detailPixels);
            newlyColoredTexture.Apply();
            yield return null;
            foreach (Renderer thisRend in GetComponentsInChildren<Renderer>())
            {
                for (int j = 0; j < thisRend.materials.Length; j++)
                {
                    Material thisMat = thisRend.materials[j];
                    if (thisMat.name.Contains(materialName))
                    {
                        Material[] deseMats = thisRend.materials;
                        deseMats[j].SetTexture("_MainTex", newlyColoredTexture);
                        deseMats[j].SetTexture("_BumpMap", hull_normal);
                        deseMats[j].SetTexture("_ParallaxMap", hull_height);
                        deseMats[j].SetTexture("_MetallicGlossMap", hull_metal);
                        thisRend.materials = deseMats;
                    }
                    yield return null;
                }
            }
        }

        public override void PaintVehicleName(string name, Color nameColor, Color hullColor)
        {
            base.PaintVehicleName(name, nameColor, hullColor);
            NameLabelsToGenerate.Enqueue(new Tuple<ModVehicle, string, Color, Color>(this, name, nameColor, hullColor));
        }
        public static IEnumerator PaintVehicleNameHelper(ModVehicle mv, string name, Color nameColor, Color mainHullColor)
        {
            Texture2D toTexture2D(RenderTexture rTex)
            {
                Texture2D tex = new Texture2D(1024, 256, TextureFormat.ARGB32, false);
                // ReadPixels looks at the active RenderTexture.
                RenderTexture.active = rTex;
                tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
                tex.Apply();
                return tex;
            }
            // prepare the name texture
            // 1. create a 3d text with "name"
            // 2. create a planar background for that text (of different color than the text)
            // 3. create a camera that looks at that text
            // 4. read pixels from that camera into a render texture
            // 5. save that render texture to a texture 2d
            generator = GameObject.Instantiate(name_label_generator);
            TextMesh text = generator.GetComponentInChildren<TextMesh>();
            Texture2D label;
            text.text = name;
            yield return null;
            label = toTexture2D(generator.transform.Find("Render").GetComponent<Renderer>().material.mainTexture as RenderTexture);
            text.text = "";

            yield return null;
            // 6. re-color the name and background
            Color[] labelPixels = label.GetPixels();
            yield return null;
            // prepare the color texture
            int yieldBoundary = labelPixels.Length / 100;
            for (int i = 0; i < labelPixels.Length; i++)
            {
                if (i % yieldBoundary == 0)
                {
                    //yield return null;
                }
                labelPixels[i] = Color.Lerp(mainHullColor, nameColor, labelPixels[i].r);
            }


            // apply the name texture to the name material
            Texture2D newlyColoredTexture = new Texture2D(1024, 256);
            newlyColoredTexture.SetPixels(labelPixels);
            yield return null;
            newlyColoredTexture.Apply();
            yield return null;

            var thisRend = mv.transform.Find("Geometry/hull_geo.001").GetComponent<Renderer>();
            Material[] deseMats = thisRend.materials;
            for (int j = 0; j < deseMats.Length; j++)
            {
                if (deseMats[j] is null)
                {
                    continue;
                }
                if (deseMats[j].name.Contains("ExteriorNameLabel"))
                {
                    deseMats[j].SetTexture("_MainTex", newlyColoredTexture);
                }
            }
            thisRend.materials = deseMats;
            GameObject.Destroy(generator);
            yield break;
        }

        public override void PaintNameDefaultStyle(string name)
        {
            base.PaintNameDefaultStyle(name);
            PaintVehicleName(name, Color.black, name_details.GetPixel(0, 0));
        }
        public override void PaintVehicleDefaultStyle(string name)
        {
            foreach (Renderer thisRend in GetComponentsInChildren<Renderer>())
            {
                for (int j = 0; j < thisRend.materials.Length; j++)
                {
                    Material thisMat = thisRend.materials[j];
                    foreach (string matName in new List<string> { "ExteriorMainColor", "ExteriorPrimaryAccent", "ExteriorSecondaryAccent" })
                    {
                        if (thisMat.name.Contains(matName))
                        {
                            Material[] deseMats = thisRend.materials;
                            deseMats[j].SetTexture("_MainTex", hull_default);
                            deseMats[j].SetTexture("_BumpMap", hull_normal);
                            deseMats[j].SetTexture("_ParallaxMap", hull_height);
                            deseMats[j].SetTexture("_MetallicGlossMap", hull_metal);
                            thisRend.materials = deseMats;
                        }
                    }
                }
            }
            base.PaintVehicleDefaultStyle(name);
        }

        public override void Awake()
        {
            // Give the Odyssey a new name and make sure we track it well.
            OGVehicleName = "ODY-" + Mathf.RoundToInt(UnityEngine.Random.value * 10000).ToString();
            vehicleName = OGVehicleName;
            NowVehicleName = OGVehicleName;
            
            Player.main.StartCoroutine(ManageLabelQueue());

            // ModVehicle.Awake
            base.Awake();
        }
        public override void Start()
        {
            base.Start();
            ApplySkyAppliers();
        }

        public Sequence aiBatterySequence = new Sequence();
        public override void Update()
        {
            base.Update();
            aiBatterySequence.Update();
            if (aiBatterySequence.active)
            {
                Quaternion anglesClosed = Quaternion.Euler(0, 0, 0);
                Quaternion anglesOpenedTop = Quaternion.Euler(0, -90, 0);
                Quaternion anglesOpenedBottom = Quaternion.Euler(0, 90, 0);
                transform.Find("Geometry/Exterior Panels/Panel Right/DoorRightTopHinge").localRotation = Quaternion.Lerp(anglesClosed, anglesOpenedTop, aiBatterySequence.t);
                transform.Find("Geometry/Exterior Panels/Panel Right/DoorRightBottomHinge").localRotation = Quaternion.Lerp(anglesClosed, anglesOpenedBottom, aiBatterySequence.t);
            }
        }
        public override void OnAIBatteryReload()
        {
            //aiBatterySequence.Set(0.5f, true, new SequenceCallback(this.OpenPDA));
            aiBatterySequence.Set(0.5f, true, null);
        }
        public Sequence storageSequence = new Sequence();
        public override float OnStorageOpen(string storageName, bool open)
        {
            this.mainAnimator.runtimeAnimatorController = animatorController;
            switch (storageName)
            {
                case "InateStorageDoor.001_light":
                    this.mainAnimator.SetBool("OD_inat_S1", open);
                    return 1f;
                case "InateStorageDoor.002_light":
                    this.mainAnimator.SetBool("OD_inat_S2", open);
                    return 1f;
                case "InateStorageDoor.003_light":
                    this.mainAnimator.SetBool("OD_inat_S3", open);
                    return 1f;
                case "InateStorageDoor.004_light":
                    this.mainAnimator.SetBool("OD_inat_S4", open);
                    return 1f;
                case "ExternalStorage1":
                    this.mainAnimator.SetBool("OD_ext_S1", open);
                    return 0.5f;
                case "ExternalStorage2":
                    this.mainAnimator.SetBool("OD_ext_S2", open);
                    return 0.5f;
                case "ExternalStorage3":
                    this.mainAnimator.SetBool("OD_ext_S3", open);
                    return 0.5f;
                case "ExternalStorage4":
                    this.mainAnimator.SetBool("OD_ext_S4", open);
                    return 0.5f;
                case "ExternalStorage5":
                    this.mainAnimator.SetBool("OD_ext_S5", open);
                    return 0.5f;
                case "ExternalStorage6":
                    this.mainAnimator.SetBool("OD_ext_S6", open);
                    return 0.5f;
                case "ExternalStorage7":
                    this.mainAnimator.SetBool("OD_ext_S7", open);
                    return 0.5f;
                case "ExternalStorage8":
                    this.mainAnimator.SetBool("OD_ext_S8", open);
                    return 0.5f;
            }
            // should never get here, return zero anyway :shrug:
            return 0;
        }

        public override void ModVehicleReset()
        {
            base.ModVehicleReset();
            innerNameLabelsToGenerate = null;
        }

        void ILightsStatusListener.OnHeadLightsOn()
        {
        }

        void ILightsStatusListener.OnHeadLightsOff()
        {
        }

        void ILightsStatusListener.OnInteriorLightsOn()
        {
            InteriorLights.ForEach(x =>
            {
                x.transform.parent.Find("WallLightGlassCover").gameObject.GetComponent<Renderer>().material.SetColor("_EmissionColor", new Color(1f, 1f, 1f, 1f));
            });
        }

        void ILightsStatusListener.OnInteriorLightsOff()
        {
            InteriorLights.ForEach(x =>
            {
                x.transform.parent.Find("WallLightGlassCover").gameObject.GetComponent<Renderer>().material.SetColor("_EmissionColor", new Color(0.285f, 0, 0.047f, 0f));
            });
        }

        void ILightsStatusListener.OnNavLightsOn()
        {
        }

        void ILightsStatusListener.OnNavLightsOff()
        {
        }

        void ILightsStatusListener.OnFloodLightsOn()
        {
        }

        void ILightsStatusListener.OnFloodLightsOff()
        {
        }

        public override Atlas.Sprite CraftingSprite
        {
            get
            {
                return crafterSprite;
            }
        }

        public void ApplySkyAppliers()
        {
            var ska = transform.Find("Geometry/Interior_Main_light").gameObject.EnsureComponent<SkyApplier>();
            ska.anchorSky = Skies.Auto;
            ska.customSkyPrefab = null;
            ska.dynamic = true;
            ska.emissiveFromPower = false;
            ska.environmentSky = null;

            var rends = transform.Find("Geometry/Interior_Main_light").gameObject.GetComponentsInChildren<Renderer>();
            ska.renderers = new Renderer[rends.Count()];
            foreach (var rend in rends)
            {
                ska.renderers.Append(rend);
            }
        }
        public override void OnVehicleDocked()
        {
            base.OnVehicleDocked();
            upgradesInput.collider.enabled = true;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using Nautilus.Crafting;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Utility;
using UnityEngine;
using Nautilus.Assets.PrefabTemplates;
using Ingredient = CraftData.Ingredient;
using VehicleFramework.VehicleTypes;
using VehicleFramework.Assets;
using System.IO;
using System.Reflection;

namespace VehicleFramework
{
    public static class BuildableDroneStation
    {
        public const string classID = "DroneStation";
        public const string displayName = "Drone Station";
        public const string description = "A terminal from which to control drones remotely";

        public static TechType RegisterConsole(GameObject droneStation, Atlas.Sprite crafter, Sprite unlock)
        {
            PrefabInfo Info = PrefabInfo.WithTechType(classID, displayName, description)
                .WithIcon(crafter);
            CustomPrefab prefab = new CustomPrefab(Info);
            ConstructableFlags constructableFlags = ConstructableFlags.Inside | ConstructableFlags.Wall | ConstructableFlags.Submarine;
            droneStation.AddComponent<DroneStation>();
            droneStation.GetComponentsInChildren<MeshRenderer>(true).ForEach(x => x.materials.ForEach(y => y.shader = Shader.Find("MarmosetUBER")));
            PrefabUtils.AddBasicComponents(droneStation, classID, Info.TechType, LargeWorldEntity.CellLevel.Medium);
            PrefabUtils.AddConstructable(droneStation, Info.TechType, constructableFlags, droneStation.transform.Find("model").gameObject);
            prefab.SetGameObject(droneStation);
            prefab.SetPdaGroupCategory(TechGroup.InteriorModules, TechCategory.InteriorModule);
            prefab.SetRecipe(new RecipeData(new Ingredient(TechType.ComputerChip, 1), new Ingredient(TechType.Glass, 1), new Ingredient(TechType.Titanium, 1), new Ingredient(TechType.Silver, 1)));
            prefab.SetUnlock(TechType.Fragment)
                .WithAnalysisTech(unlock, unlockMessage: "Drone Required");
            prefab.Register();
            return Info.TechType;
        }

        public static void Register()
        {
            string directoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string bundlePath = Path.Combine(directoryPath, "dronestation");
            AssetBundleInterface abi = new AssetBundleInterface(bundlePath);
            GameObject fragment = abi.GetGameObject("Fragment");
            GameObject droneStation = abi.GetGameObject("DroneStation");
            Atlas.Sprite sprite = abi.GetSprite("DSSpriteAtlas", "DSCrafterSprite");
            Sprite rawSprite = abi.GetRawSprite("DSSpriteAtlas", "DSUnlockSprite");
            TechType consoleTT = RegisterConsole(droneStation, sprite, rawSprite);
            AbstractBiomeData abd = new AbstractBiomeData()
                .WithBiome(AbstractBiomeType.SafeShallows)
                .WithBiome(AbstractBiomeType.KelpForest)
                .WithBiome(AbstractBiomeType.GrassyPlateus);
            FragmentManager.RegisterFragment(fragment, consoleTT, 3, classID + "Fragment", displayName + " Fragment", description + " ...fragment", rawSprite, abd.Get());
        }
    }

    public class DroneStation : HandTarget, IHandTarget
    {
        public static DroneStation BroadcastingStation = null;
        private Drone _pairedDrone = null;
        public Drone pairedDrone
        {
            get
            {
                return _pairedDrone;
            }
            private set
            {
                _pairedDrone = value;
            }
        }
        public void Unpair()
        {
            pairedDrone = null;
        }
        public override void Awake()
        {
            base.Awake();
            Admin.GameObjectManager<DroneStation>.Register(this);
        }
        public void Start()
        {
            IEnumerator WaitThenAct()
            {
                while(!Admin.GameStateWatcher.IsPlayerStarted)
                {
                    yield return null;
                }
                DroneStation.FastenConnection(this, FindNearestUnpairedDrone());
                if (GetComponent<Rigidbody>())
                {
                    Component.Destroy(GetComponent<Rigidbody>());
                }
            }
            StartCoroutine(WaitThenAct());
        }
        Drone FindNearestUnpairedDrone()
        {
            return Admin.GameObjectManager<Drone>.FindNearestSuch(transform.position, (x => x.pairedStation is null));
        }
        public static void FastenConnection(DroneStation station, Drone drone)
        {
            if(drone == null || station == null)
            {
                return;
            }
            if(station.pairedDrone != null)
            {
                // if we have a paired drone already, we need to tell it we're finished
                station.pairedDrone.pairedStation = null;
            }
            station.pairedDrone = drone;

            if(drone.pairedStation != null)
            {
                // if our newly paired drone already had a paired station, we need to tell that station its pairing is history
                drone.pairedStation.pairedDrone = null;
            }
            drone.pairedStation = station;
        }
        void IHandTarget.OnHandClick(GUIHand hand)
        {
        }
        void IHandTarget.OnHandHover(GUIHand hand)
        {
            HandReticle.main.SetIcon(HandReticle.IconType.Interact, 1f);
            Targeting.GetTarget(Player.main.gameObject, 6f, out GameObject target, out float _);
            OnScreenHover();
        }
        public Drone SelectDrone(List<Drone> list, bool next)
        {
            int index = list.FindIndex(x => x == pairedDrone);
            if (list.Count() == 0)
            {
                return null;
            }
            else
            {
                if (index == -1)
                {
                    return list.First();
                }
                else
                {
                    if(next)
                    {
                        index++;
                    }
                    else
                    {
                        index--;
                    }
                    if(index < 0)
                    {
                        index = list.Count() - 1;
                    }
                    if((list.Count - 1) < index)
                    {
                        index = 0;
                    }
                    return list[index];
                }
            }
        }
        public void OnScreenHover()
        {
            var list = Admin.GameObjectManager<Drone>.Where(x => !x.isScuttled && x.liveMixin.IsAlive() && x.energyInterface.hasCharge);
            if (pairedDrone == null && list.Count() > 0)
            {
                FastenConnection(this, list.First());
            }
            HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, BuildScreenText());
            if (GameInput.GetButtonDown(GameInput.Button.LeftHand) && pairedDrone != null)
            {
                pairedDrone.BeginControlling();
            }
            if (list.Count() > 0)
            {
                Drone selected = null;
                if (GameInput.GetButtonDown(GameInput.Button.CycleNext))
                {
                    selected = SelectDrone(list, true);
                }
                else if (GameInput.GetButtonDown(GameInput.Button.CyclePrev))
                {
                    selected = SelectDrone(list, false);
                }
                if (selected != null)
                {
                    FastenConnection(this, selected);
                }
            }
        }
        public string BuildScreenText()
        {
            string ret = "Current Drone: " + ((pairedDrone != null) ? pairedDrone.subName.hullName.text : "[empty]") + "\n";
            ret += HandReticle.main.GetText("Connect ", false, GameInput.Button.LeftHand) + "\n";
            ret += HandReticle.main.GetText("Next Drone: ", false, GameInput.Button.CycleNext) + "\n";
            ret += HandReticle.main.GetText("Previous Drone: ", false, GameInput.Button.CyclePrev) + "\n";
            return ret;
        }
    }
}

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

namespace VehicleFramework
{
    public static class BuildableDroneStation
    {
        public static PrefabInfo Info { get; } = PrefabInfo.WithTechType("DroneStation", "Drone Station", "A terminal from which to control drones remotely.")
            // set the icon to that of the vanilla locker:
            .WithIcon(SpriteManager.Get(TechType.PictureFrame));
        public static void Register()
        {
            CustomPrefab prefab = new CustomPrefab(Info);
            CloneTemplate cloneTemplate = new CloneTemplate(Info, TechType.PictureFrame);
            cloneTemplate.ModifyPrefab += obj =>
            {
                GameObject model = obj.transform.Find("mesh/submarine_Picture_Frame").gameObject;
                ConstructableFlags constructableFlags = ConstructableFlags.Inside | ConstructableFlags.Wall | ConstructableFlags.Submarine;
                obj.AddComponent<DroneStation>();
                PrefabUtils.AddConstructable(obj, Info.TechType, constructableFlags, model);
            };
            prefab.SetGameObject(cloneTemplate);
            prefab.SetPdaGroupCategory(TechGroup.InteriorModules, TechCategory.InteriorModule);
            prefab.SetRecipe(new RecipeData(new Ingredient(TechType.ComputerChip, 1), new Ingredient(TechType.Glass, 1), new Ingredient(TechType.Titanium, 1), new Ingredient(TechType.Silver, 1)));
            prefab.Register();
        }
    }

    public class DroneStation : HandTarget, IHandTarget
    {
        public static DroneStation BroadcastingStation = null;
        public Drone pairedDrone;
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
                GetComponent<PictureFrame>().enabled = false;
                transform.Find("Trigger").gameObject.SetActive(false);
                transform.Find("mesh/submarine_Picture_Frame/submarine_Picture_Frame_button").gameObject.AddComponent<BoxCollider>();
                gameObject.AddComponent<BoxCollider>();
                DroneStation.FastenConnection(this, FindNearestUnpairedDrone());
                Component.Destroy(GetComponent<Rigidbody>());
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
            HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
            Targeting.GetTarget(Player.main.gameObject, 6f, out GameObject target, out float _);
            if (target.name.Contains("DroneStation"))
            {
                OnScreenHover();
            }
            else if(target.name.Contains("submarine_Picture_Frame_button"))
            {
                OnButtonHover();
            }
        }
        public void OnScreenHover()
        {
            HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, BuildScreenText());
            if (GameInput.GetButtonDown(GameInput.Button.LeftHand))
            {
                pairedDrone.BeginControlling();
            }
            else if (GameInput.GetButtonDown(GameInput.Button.RightHand))
            {
                pairedDrone.Rename();
            }
            else if (GameInput.GetButtonDown(GameInput.Button.CycleNext))
            {
                int index = Admin.GameObjectManager<Drone>.AllSuchObjects.FindIndex(x => x == pairedDrone);
                index++;
                if (index == Admin.GameObjectManager<Drone>.AllSuchObjects.Count())
                {
                    index = 0;
                }
                FastenConnection(this, Admin.GameObjectManager<Drone>.AllSuchObjects[index]);
            }
            else if (GameInput.GetButtonDown(GameInput.Button.CyclePrev))
            {
                int index = Admin.GameObjectManager<Drone>.AllSuchObjects.FindIndex(x => x == pairedDrone);
                index--;
                if (index == -1)
                {
                    index = Admin.GameObjectManager<Drone>.AllSuchObjects.Count() - 1;
                }
                FastenConnection(this, Admin.GameObjectManager<Drone>.AllSuchObjects[index]);
            }
        }
        public void OnButtonHover()
        {
            HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, "Connect to Last Drone");
            if (GameInput.GetButtonDown(GameInput.Button.LeftHand))
            {
                pairedDrone.BeginControlling();
            }
        }
        public string BuildScreenText()
        {
            string ret = "Current Drone: " + ((pairedDrone != null) ? pairedDrone.subName.hullName.text : "[empty]") + "\n";
            ret += HandReticle.main.GetText("Connect ", false, GameInput.Button.LeftHand) + "\n";
            ret += HandReticle.main.GetText("Rename ", false, GameInput.Button.RightHand) + "\n";
            ret += HandReticle.main.GetText("Next Drone: ", false, GameInput.Button.CycleNext) + "\n";
            ret += HandReticle.main.GetText("Previous Drone: ", false, GameInput.Button.CyclePrev) + "\n";
            return ret;
        }
    }
}

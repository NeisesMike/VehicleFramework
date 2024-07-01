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

namespace CrushDrone
{
    public class Crush : Drone
    {
        /* Set UnlockedWith to an impossible value
         * So that the Crush can be unlocked,
         * but not by scanning anything in the base game.
         * We add our own scanning logic in CrushFragment.
         */
        public override TechType UnlockedWith => TechType.Fragment;
        public override Sprite UnlockedSprite => MainPatcher.assets.unlock;
        public override string UnlockedMessage => "Drone Station Required";
        public override int FragmentsToScan => 3;
        public override Transform CameraLocation => transform.Find("CameraLocation");
        public override string vehicleDefaultName => "Crush";
        public override GameObject VehicleModel => MainPatcher.assets.model;
        public override GameObject CollisionModel => transform.Find("CollisionModel").gameObject;
        public override GameObject StorageRootObject => transform.Find("StorageRoot").gameObject;
        public override GameObject ModulesRootObject => transform.Find("ModulesRoot").gameObject;
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
        public override List<VehicleStorage> ModularStorages => null;
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
        public override GameObject BoundingBox => transform.Find("BoundingBox").gameObject;
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
        public override Atlas.Sprite PingSprite => MainPatcher.assets.ping;
        public override Atlas.Sprite CraftingSprite => MainPatcher.assets.crafter;
        public override string Description =>  "A small drone with powerful claws capable of collecting resources.";
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
                string ency = "The Crush is a remotely controlled drone designed for resource collection. ";
                ency += "Its powerful claws are what earned it its name. \n";
                ency += "\nIt features:\n";
                ency += "- Remote Connectivity \n";
                ency += "- Powerful claws for collecting resources \n";
                ency += "- One power cell capacity. \n";

                ency += "\nRatings:\n";
                ency += "- Top Speed: 11m/s \n";
                ency += "- Acceleration: 6m/s/s \n";
                ency += "- Distance per Power Cell: 7km \n";
                ency += "- Crush Depth: 300 \n";
                ency += "- Max Crush Depth (upgrade required): 1100 \n";
                ency += "- Upgrade Slots: 4 \n";
                ency += "- Dimensions: 3.5m x 3.5m x 3.1m \n";

                ency += "- Persons: 0\n";
                ency += "\n\"You can count on Crush.\" ";
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
        public override ModVehicleEngine Engine => gameObject.EnsureComponent<CrushEngine>();
    }
}

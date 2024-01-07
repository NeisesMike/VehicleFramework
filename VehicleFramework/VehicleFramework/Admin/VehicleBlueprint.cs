using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleFramework.Engines;
using UnityEngine;

namespace VehicleFramework
{
    public class VehicleBlueprint
    {
        public GameObject model { get; set; }
        public GameObject control_panel { get; set; }
        public Atlas.Sprite ping_sprite { get; set; }
        public ModVehicleEngine engine { get; set; }
        public Dictionary<TechType, int> recipe { get; set; }
        public PingType pt { get; set; }
        public int modules { get; set; }
        public int arms { get; set; }
        public int baseCrushDepth { get; set; }
        public int maxHealth { get; set; }
        public int mass { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string encyclopedia_entry { get; set; }


        public VehicleBlueprint withModel(GameObject inputModel)
        {
            model = inputModel;
            return this;
        }
        public VehicleBlueprint withControlPanel(GameObject inputModel)
        {
            control_panel = inputModel;
            return this;
        }
        public VehicleBlueprint withPingSprite(Atlas.Sprite sprite)
        {
            ping_sprite = sprite;
            return this;
        }
        public VehicleBlueprint withEngine(ModVehicleEngine thisEngine)
        {
            engine = thisEngine;
            return this;
        }
        public VehicleBlueprint withRecipe(Dictionary<TechType, int> thisRecipe)
        {
            recipe = thisRecipe;
            return this;
        }
        public VehicleBlueprint withPingType(PingType thisPt)
        {
            pt = thisPt;
            return this;
        }
        public VehicleBlueprint withNumUpgradeModules(int thisNum)
        {
            modules = thisNum;
            return this;
        }
        public VehicleBlueprint withNumArms(int thisNum)
        {
            arms = thisNum;
            return this;
        }
        public VehicleBlueprint withBaseCrushDepth(int thisNum)
        {
            baseCrushDepth = thisNum;
            return this;
        }
        public VehicleBlueprint withMaxHealth(int thisNum)
        {
            maxHealth = thisNum;
            return this;
        }
        public VehicleBlueprint withMass(int thisNum)
        {
            mass = thisNum;
            return this;
        }

        public VehicleBlueprint withName(string str)
        {
            name = str;
            return this;
        }
        public VehicleBlueprint withDescription(string str)
        {
            description = str;
            return this;
        }
        public VehicleBlueprint withEncyclopediaEntry(string str)
        {
            encyclopedia_entry = str;
            return this;
        }



    }
}

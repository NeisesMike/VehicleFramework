using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework.VehicleComponents
{
    public struct SubSystemStruct
    {
        public List<Collider> repairColliders;
        public int currentDamage;
        public int maxDamage;
        public Action<ModVehicle, int> ManageDamageState;
        public Func<float, DamageType, float> ModifyDamageCalculation;
        internal List<LiveMixin> livemixins;
    }
    public class VehicleDamageTracker : MonoBehaviour
    {
        public ModVehicle mv = null;
        public Dictionary<string, SubSystemStruct> subsystems = new Dictionary<string, SubSystemStruct>();
        public static Transform FindTransformRecursive(Transform parent, string name)
        {
            // If the current transform's name matches, return it
            if (parent.name == name)
            {
                return parent;
            }

            // Recursively search in each child
            foreach (Transform child in parent)
            {
                Transform result = FindTransformRecursive(child, name);
                if (result != null)
                {
                    return result;
                }
            }

            // If not found, return null
            return null;
        }
        private List<Collider> hull_colliders
        {
            get
            {
                Transform master = FindTransformRecursive(mv.transform, "MVDAMAGE_HULL");
                return master.GetComponentsInChildren<Collider>().ToList();
            }
        }
        private List<Collider> engine_colliders
        {
            get
            {
                Transform master = FindTransformRecursive(mv.transform, "MVDAMAGE_ENGINE");
                return master.GetComponentsInChildren<Collider>().ToList();
            }
        }
        private List<Collider> light_colliders
        {
            get
            {
                Transform master = FindTransformRecursive(mv.transform, "MVDAMAGE_LIGHTS");
                return master.GetComponentsInChildren<Collider>().ToList();
            }
        }
        private List<Collider> battery_colliders
        {
            get
            {
                Transform master = FindTransformRecursive(mv.transform, "MVDAMAGE_BATTERIES");
                return master.GetComponentsInChildren<Collider>().ToList();
            }
        }
        private List<Collider> upgrade_colliders
        {
            get
            {
                Transform master = FindTransformRecursive(mv.transform, "MVDAMAGE_UPGRADES");
                return master.GetComponentsInChildren<Collider>().ToList();
            }
        }
        public void Start()
        {
            mv = GetComponentInParent<ModVehicle>();
            if(!ValidateDefaultDamageTracker(mv))
            {
                enabled = false;
                Component.Destroy(this);
                return;
            }
            subsystems.Add("hull", new SubSystemStruct { currentDamage = 0, maxDamage = 3, ManageDamageState = SimpleManageHullDamageState, ModifyDamageCalculation = SimpleDamageModification });
            subsystems.Add("engines", new SubSystemStruct { currentDamage = 0, maxDamage = 3, ManageDamageState = SimpleManageEngineDamageState, ModifyDamageCalculation = SimpleDamageModification });
            subsystems.Add("lights", new SubSystemStruct { currentDamage = 0, maxDamage = 1, ManageDamageState = SimpleManageLightsDamageState, ModifyDamageCalculation = SimpleDamageModification });
            if (mv.Batteries != null && mv.Batteries.Count() > 0)
            {
                subsystems.Add("batteries", new SubSystemStruct { currentDamage = 0, maxDamage = 4, ManageDamageState = SimpleManageBatteriesDamageState, ModifyDamageCalculation = SimpleDamageModification });
            }
            if (mv.Upgrades != null && mv.Upgrades.Count() > 0)
            {
                subsystems.Add("upgrades", new SubSystemStruct { currentDamage = 0, maxDamage = 3, ManageDamageState = SimpleManageUpgradesDamageState, ModifyDamageCalculation = SimpleDamageModification });
            }
        }
        public void TakeDamagePostfix(float damage, Vector3 position, DamageType type, GameObject dealer)
        {
            var random = new System.Random();
            SubSystemStruct chosen = subsystems.ElementAt(random.Next(0, subsystems.Count)).Value;
            if (chosen.repairColliders != null)
            {
                HandleDamage(chosen, damage, position, type, dealer);
            }
        }
        public void HandleDamage(SubSystemStruct subsystem, float damageTaken, Vector3 position, DamageType type, GameObject dealer)
        {
            if (ShouldTakeDamage(subsystem, damageTaken, position, type, dealer))
            {
                ManageDamage(subsystem, true);
            }
        }
        public void ManageDamage(SubSystemStruct subsys, bool isHurtingMe)
        {
            subsys.currentDamage = isHurtingMe ? subsys.currentDamage + 1 : subsys.currentDamage - 1;
            subsys.currentDamage = Math.Min(subsys.maxDamage, Math.Max(0, subsys.currentDamage));
            subsys.ManageDamageState?.Invoke(GetComponent<ModVehicle>(), subsys.currentDamage);
        }
        public static bool ShouldTakeDamage(SubSystemStruct subsystem, float damageTaken, Vector3 position, DamageType type, GameObject dealer)
        {
            // 50 (exosuit) => 100%
            // 40 damage => 81% chance of failure
            // 30 => 46%
            // Knife (25) => 32%
            // 20 => 20%
            // 10 => 5%
            // 9 => 0%
            float chanceOfFailure;
            if (damageTaken < 10f)
            {
                chanceOfFailure = 0f;
            }
            else if (damageTaken < 40)
            {
                chanceOfFailure = Mathf.Pow(Mathf.Min(damageTaken, 40f) / 40f * 0.9f, 2);
            }
            else
            {
                chanceOfFailure = 1;
            }
            if (subsystem.ModifyDamageCalculation != null)
            {
                chanceOfFailure = subsystem.ModifyDamageCalculation(chanceOfFailure, type);
            }
            return UnityEngine.Random.value < chanceOfFailure;
        }
        public static float SimpleDamageModification(float chance, DamageType type)
        {
            if (type == DamageType.Electrical
                || type == DamageType.Acid
                || type == DamageType.Explosive
                || type == DamageType.Puncture)
            {
                chance = Mathf.Pow(chance, 0.9f);
            }
            else if (type == DamageType.Cold
                    || type == DamageType.Fire
                    || type == DamageType.Heat
                    || type == DamageType.Radiation
                    || type == DamageType.Poison)
            {
                chance = Mathf.Pow(chance, 1.3f);
            }
            return chance;
        }

        // TODO, the following five methods need also to manage the repair tool
        // They should add repair colliders to the sub systems that are damaged
        public static void SimpleManageEngineDamageState(ModVehicle mv, int damage)
        {
            if (damage == 0)
            {
                mv.GetComponent<Engines.ModVehicleEngine>().damageModifier = 1;
            }
            else if (damage == 1)
            {
                mv.GetComponent<Engines.ModVehicleEngine>().damageModifier = 0.8f;
            }
            else if (damage == 2)
            {
                mv.GetComponent<Engines.ModVehicleEngine>().damageModifier = 0.4f;
            }
            else if (damage == 3)
            {
                mv.GetComponent<Engines.ModVehicleEngine>().damageModifier = 0.1f;
            }
            else
            {
                mv.GetComponent<Engines.ModVehicleEngine>().damageModifier = 1;
            }
        }
        public static void SimpleManageLightsDamageState(ModVehicle mv, int damage)
        {
            if (damage == 0)
            {
                mv.GetComponent<HeadLightsController>().isDamaged = false;
            }
            else if (damage == 1)
            {
                mv.GetComponent<HeadLightsController>().DisableHeadlights();
                mv.GetComponent<HeadLightsController>().isDamaged = true;
            }
            else
            {
                mv.GetComponent<HeadLightsController>().isDamaged = false;
            }
        }
        public static void SimpleManageUpgradesDamageState(ModVehicle mv, int damage)
        {
            if (damage == 0)
            {
                mv.upgradesInput.collider.enabled = true;
            }
            else if (damage == 1)
            {
                mv.upgradesInput.collider.enabled = false;
            }
            else
            {
                mv.upgradesInput.collider.enabled = true;
            }
        }
        public static void SimpleManageBatteriesDamageState(ModVehicle mv, int damage)
        {
            var cols = mv.Batteries.Select(x => x.BatterySlot.GetComponent<Collider>()).ToList();
            while (cols.Where(x => !x.enabled).Count() < damage)
            {
                cols[(new System.Random()).Next(0, cols.Count())].enabled = false;
            }
            while (cols.Where(x => !x.enabled).Count() > damage)
            {
                cols[(new System.Random()).Next(0, cols.Count())].enabled = true;
            }
        }
        public static void SimpleManageHullDamageState(ModVehicle mv, int damage)
        {
            void EnsureNumGashes(int num)
            {
                /*
                var cols = mv.Batteries.Select(x => x.BatterySlot.GetComponent<Collider>()).ToList();
                while (cols.Where(x => !x.enabled).Count() < num)
                {
                    cols[(new System.Random()).Next(0, cols.Count())].enabled = false;
                }
                while (cols.Where(x => !x.enabled).Count() > num)
                {
                    cols[(new System.Random()).Next(0, cols.Count())].enabled = true;
                }
                */
            }
            // diminish crush depth
            mv.crushDamage.kBaseCrushDepth = mv.BaseCrushDepth - damage * 50;
        }
        public static void dothing(Collider col, int damage)
        {
            // give the collider object a livemixin so that it can interact with the repair tool (welder)?
        }

        public static bool ValidateDefaultDamageTracker(ModVehicle mv)
        {
            return false;
        }

    }

    /*
    
    A simple accounting of things that deal damage in Subnautica.
    The form of each entry is
    DamageType : Name of Damage Source : Damage Dealt

    Creatures
	    Normal : Leech Suck : 5
	    Normal : Floater Attach : 3-7
	    [per creature] : Projectile : 10
	    [per creature] : Range Attacker : 10
	    Poison : Gas pod : 10
	    Normal : Warper Warp : 10
	    Electrical : Shocker Melee Else : 15
	    Normal : Reaper Bite Seamoth Per Second : 15
	    Normal : Seadragon Crush Exosuit per Second : 15
	    Normal : Seadragon Swat : 20
	    Collider : Sandworm : 30
	    Normal : Crabsnake Bite : 30
	    Normal : Seadragon Shove : 40
	    Normal : Seatreader Step : 40
	    Normal : Crabsnake Kill : 40
	    Electrical : Shocker Melee SubControl : 50
	    Normal : Juvenile Emperor Melee : 60
	    Normal : Bite Damage : [per creature]

    Player Deals
	    Electrical : Electrical Defense : 3
	    Drill : Exo : 4
	    Electrical : Cyclops Defense Against Attacker : 20	
	    Normal : Knife : 25
	    Fire : Knife : 25
	    Normal : Exo : 50

    Environmental
	    Normal : Drowning : 2
	    Heat : Burning Chunk : 5
	    Fire : Fire Interval : 5
	    Pressure : Base Crush : 10
	    Acid : Acidic Brine : 10
	    Normal : Lava : 10
	    Heat : Lava Meteor : 10 
	    Normal : Magma Blob : 20
	    Fire : Geyser : 20
	    Pressure : Vehicle Crush : 20
	    Collide : Fall Damage : infinity
    */
}

using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace VehicleFramework.VehicleComponents
{
	// damage from anywhere can wreck certain subsystems
	// then they must be repaired on-site.
	public static class VehicleDamageTrackerExtensions
	{
		public static VehicleDamageTracker WithSubsystem(this VehicleDamageTracker vdt, Subsystem sys)
		{
			if (sys != null)
			{
				vdt.subsystems.Add(sys);
			}
			return vdt;
		}
	}
    public class VehicleDamageTracker : MonoBehaviour, IOnTakeDamage
    {
		internal readonly List<Subsystem> subsystems = new List<Subsystem>();
        void IOnTakeDamage.OnTakeDamage(DamageInfo damageInfo)
		{
            if (subsystems.Any())
			{
				System.Random rnd = new System.Random();
				int randIndex = rnd.Next(subsystems.Count);
				subsystems[randIndex].OnTakeDamage?.Invoke(damageInfo);
			}
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

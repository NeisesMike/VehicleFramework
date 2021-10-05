using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework
{
    /*
     * Tether Sources are meant to be placed throughout the ModVehicle.
     * They will interact with a player's ModVehicleTether.
     * A player will "leash" to them when close enough,
     * which ensures the player's entry is recognized no matter what (warp in).
     */
    public class TetherSource : MonoBehaviour
    {
        private bool isTetherEstablished = false;

        public void Start()
        {
            StartCoroutine(TryEstablishTether());
        }

        public IEnumerator TryEstablishTether()
        {
            while(true)
            {
                if (isTetherEstablished)
                {
                    yield return null;
                }
                else
                {
                    var mv = GetComponent<ModVehicle>();
                    foreach (var tethersrc in mv.TetherSources)
                    {
                        // TODO: make this constant depend on the vehicle model somehow
                        var tmp = Vector3.Distance(Player.main.transform.position, tethersrc.transform.position);
                        if (tmp < 0.75f)
                        {
                            mv.PlayerEntry();
                            isTetherEstablished = true;
                            Player.main.GetComponent<ModVehicleTether>().CatchTether(mv);
                            break;
                        }
                    }
                    yield return new WaitForSeconds(0.5f);
                }
            }
        }

        public void BreakTether()
        {
            isTetherEstablished = false;
        }

    }
}

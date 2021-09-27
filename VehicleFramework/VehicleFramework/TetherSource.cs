using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework
{
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

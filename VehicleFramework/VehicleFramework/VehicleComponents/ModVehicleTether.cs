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
     * The player is given a ModVehicleTether.
     * It is what will interact with the TetherSources on the ModVehicle.
     * A player will "leash" to the TetherSources when close enough,
     * which ensures the player's entry is recognized no matter what (warp in).
     */
    public class ModVehicleTether : MonoBehaviour
    {
        private ModVehicle currentMV = null;

        public void CatchTether(ModVehicle mv)
        {
            currentMV = mv;
            StartCoroutine(CheckTether());
        }

        public IEnumerator CheckTether()
        {
            while (true)
            {
                if (currentMV != null)
                {
                    bool shouldDropLeash = true;
                    foreach (var tethersrc in currentMV.TetherSources)
                    {
                        // TODO make this constant depend on the vehicle somehow
                        if (Vector3.Distance(Player.main.transform.position, tethersrc.transform.position) < 5f)
                        {
                            shouldDropLeash = false;
                            break;
                        }
                    }
                    if (shouldDropLeash)
                    {
                        if (currentMV.IsPlayerInside())
                        {
                            currentMV.PlayerExit();
                        }
                        currentMV.GetComponent<TetherSource>().BreakTether();
                        yield break;
                    }
                }
                yield return new WaitForSeconds(0.25f);
            }
        }

    }
}

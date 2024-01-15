using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VehicleFramework.VehicleTypes;

namespace VehicleFramework
{
    /*
     * Tether Sources are meant to be placed throughout the Submarine.
     * They will interact with a player's ModVehicleTether.
     * A player will "leash" to them when close enough,
     * which ensures the player's entry is recognized no matter what (warp in).
     */
    public class TetherSource : MonoBehaviour
    {
        private bool isTetherEstablished = false;
        public Submarine mv = null;

        public void Start()
        {
            Player.main.StartCoroutine(ManagerTether());
        }

        public void TryToDropLeash()
        {
            isTetherEstablished = Vector3.Distance(Player.main.transform.position, transform.position) < 5f;
            if(!isTetherEstablished)
            {
                if(mv.TetherSources.Where(x=>x.GetComponent<TetherSource>().isTetherEstablished).Count() > 0)
                {
                    // some tether is still hanging on
                }
                else
                {
                    if (mv.IsPlayerInside())
                    {
                        mv.PlayerExit();
                    }
                }
            }
        }

        public void TryToEstablishLeash()
        {
            // TODO: make this constant depend on the vehicle model somehow
            isTetherEstablished = Vector3.Distance(Player.main.transform.position, transform.position) < 0.75f;
            if (isTetherEstablished)
            {
                if (!mv.IsPlayerInside())
                {
                    mv.PlayerEntry();
                }
            }
        }

        public IEnumerator ManagerTether()
        {
            yield return new WaitForSeconds(3f);
            while (true)
            {
                if(mv == null)
                {
                    yield break;
                }
                if (isTetherEstablished)
                {
                    TryToDropLeash();
                }
                else
                {
                    TryToEstablishLeash();
                }
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}

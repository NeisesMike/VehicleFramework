using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework
{
    public class PlayerTether : MonoBehaviour, IVehicleStatusListener
    {
        private bool isTetherEstablished = false;

        public float DebugLastTetherDistance = 0f;

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
                    foreach (var tethersrc in gameObject.GetComponent<ModVehicle>().TetherSources)
                    {
                        // TODO: make this constant depend on the vehicle model somehow
                        var tmp = Vector3.Distance(Player.main.transform.position, tethersrc.transform.position);
                        if (tmp < 0.75f)
                        {
                            gameObject.GetComponent<ModVehicle>().PlayerEntry();
                            isTetherEstablished = true;
                            StartCoroutine(CheckTether());
                            break;
                        }
                    }
                    yield return new WaitForSeconds(1f);
                }
            }
        }

        public IEnumerator CheckTether()
        {
            while (true)
            {
                bool shouldDropLeash = false;
                foreach (var tethersrc in gameObject.GetComponent<ModVehicle>().TetherSources)
                {
                    // TODO make this constant depend on the vehicle somehow
                    if (5f < Vector3.Distance(Player.main.transform.position, tethersrc.transform.position))
                    {
                        shouldDropLeash = true;
                        break;
                    }
                }
                if (shouldDropLeash)
                {
                    GetComponent<ModVehicle>().PlayerExit();
                    isTetherEstablished = false;
                    yield break;
                }
                yield return new WaitForSeconds(0.25f);
            }
        }

        void IVehicleStatusListener.OnPlayerEntry()
        {
        }

        void IVehicleStatusListener.OnPlayerExit()
        {
        }

        void IVehicleStatusListener.OnPilotBegin()
        {
        }

        void IVehicleStatusListener.OnPilotEnd()
        {
        }

        void IVehicleStatusListener.OnPowerUp()
        {
        }

        void IVehicleStatusListener.OnPowerDown()
        {
        }

        void IVehicleStatusListener.OnExteriorLightsOn()
        {
        }

        void IVehicleStatusListener.OnExteriorLightsOff()
        {
        }

        void IVehicleStatusListener.OnInteriorLightsOn()
        {
        }

        void IVehicleStatusListener.OnInteriorLightsOff()
        {
        }

        void IVehicleStatusListener.OnTakeDamage()
        {
        }

        void IVehicleStatusListener.OnAutoLevel()
        {
        }

        void IVehicleStatusListener.OnAutoPilotBegin()
        {
        }

        void IVehicleStatusListener.OnAutoPilotEnd()
        {
        }
    }
}

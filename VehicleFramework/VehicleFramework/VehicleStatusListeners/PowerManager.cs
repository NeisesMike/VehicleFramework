using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework
{
    /*
     * We would like very much do consolidate all our power drains here
     * This is trivial for lights,
     * but a bit more difficult for driving
     */
    public class PowerManager : MonoBehaviour, IVehicleStatusListener
    {
        private PowerStatus currentPowerStatus = PowerStatus.OnBatterySafe;
        private bool isHeadlightsOn = false;
        private bool isFloodlightsOn = false;
        private bool isNavLightsOn = false;
        private bool isInteriorLightsOn = false;
        private ModVehicle mv = null;

        public void Start()
        {
            mv = GetComponent<ModVehicle>();
            currentPowerStatus = EvaluatePowerStatus();
            mv.NotifyStatus(currentPowerStatus);
        }
        public void Update()
        {
            /*
             * research suggests engines should be between 10 and 100x more draining than the lights
             * engine takes [0,3], so we're justified for either [0,0.3] or [0,0.03]
             * we chose [0,0.1]
             */
            if (isHeadlightsOn)
            {
                mv.GetComponent<EnergyInterface>().ConsumeEnergy(0.01f * Time.deltaTime);
            }
            if (isFloodlightsOn)
            {
                mv.GetComponent<EnergyInterface>().ConsumeEnergy(0.1f * Time.deltaTime);
            }
            if(isNavLightsOn)
            {
                mv.GetComponent<EnergyInterface>().ConsumeEnergy(0.001f * Time.deltaTime);
            }
            if(isInteriorLightsOn)
            {
                mv.GetComponent<EnergyInterface>().ConsumeEnergy(0.001f * Time.deltaTime);
            }

            // check battery thresholds, and make notifications as appropriate
            PowerStatus thisPS = EvaluatePowerStatus();
            if(thisPS != currentPowerStatus)
            {
                currentPowerStatus = thisPS;
                // TODO
                //mv.NotifyStatus()
            }
        }
        private PowerStatus EvaluatePowerStatus()
        {
            float charge;
            float capacity;
            mv.energyInterface.GetValues(out charge, out capacity);

            if (charge < 1)
            {
                return PowerStatus.OnBatteryDepleted;
            }
            else if (charge < 8)
            {
                return PowerStatus.OnBatteryNearlyEmpty;
            }
            else if(charge < 80)
            {
                return PowerStatus.OnBatteryLow;
            }
            else
            {
                return PowerStatus.OnBatterySafe;
            }
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

        void IVehicleStatusListener.OnFloodLightsOff()
        {
            isFloodlightsOn = false;
        }

        void IVehicleStatusListener.OnFloodLightsOn()
        {
            isFloodlightsOn = true;
        }

        void IVehicleStatusListener.OnHeadLightsOff()
        {
            isHeadlightsOn = false;
        }

        void IVehicleStatusListener.OnHeadLightsOn()
        {
            isHeadlightsOn = true;
        }

        void IVehicleStatusListener.OnInteriorLightsOff()
        {
            isInteriorLightsOn = false;
        }

        void IVehicleStatusListener.OnInteriorLightsOn()
        {
            isInteriorLightsOn = true;
        }

        void IVehicleStatusListener.OnNavLightsOff()
        {
            isNavLightsOn = false;
        }

        void IVehicleStatusListener.OnNavLightsOn()
        {
            isNavLightsOn = true;
        }

        void IVehicleStatusListener.OnTakeDamage()
        {
        }
    }
}

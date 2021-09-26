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
     * 
     * 
     */
    public class PowerManager : MonoBehaviour, IVehicleStatusListener
    {
        private bool isHeadlightsOn = false;
        private bool isFloodlightsOn = false;
        private bool isNavLightsOn = false;
        private bool isInteriorLightsOn = false;
        private ModVehicle mv = null;

        public void Awake()
        {
            mv = GetComponent<ModVehicle>();
        }
        public void Update()
        {
            /*
             * research suggests engines should be between 10 and 100x more draining than the lights
             * 
             */
            if (isHeadlightsOn)
            {
                mv.GetComponent<EnergyInterface>().ConsumeEnergy(0.005f * Time.deltaTime);
            }
            if (isFloodlightsOn)
            {
                mv.GetComponent<EnergyInterface>().ConsumeEnergy(0.005f * Time.deltaTime);
            }
            if(isNavLightsOn)
            {
                mv.GetComponent<EnergyInterface>().ConsumeEnergy(0.005f * Time.deltaTime);
            }
            if(isInteriorLightsOn)
            {
                mv.GetComponent<EnergyInterface>().ConsumeEnergy(0.005f * Time.deltaTime);
            }
        }
        void IVehicleStatusListener.OnAutoLevel()
        {
            throw new NotImplementedException();
        }

        void IVehicleStatusListener.OnAutoPilotBegin()
        {
            throw new NotImplementedException();
        }

        void IVehicleStatusListener.OnAutoPilotEnd()
        {
            throw new NotImplementedException();
        }

        void IVehicleStatusListener.OnBatteryDepletion()
        {
            throw new NotImplementedException();
        }

        void IVehicleStatusListener.OnBatteryLow()
        {
            throw new NotImplementedException();
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

        void IVehicleStatusListener.OnPilotBegin()
        {
            throw new NotImplementedException();
        }

        void IVehicleStatusListener.OnPilotEnd()
        {
            throw new NotImplementedException();
        }

        void IVehicleStatusListener.OnPlayerEntry()
        {
            throw new NotImplementedException();
        }

        void IVehicleStatusListener.OnPlayerExit()
        {
            throw new NotImplementedException();
        }

        void IVehicleStatusListener.OnPowerDown()
        {
            throw new NotImplementedException();
        }

        void IVehicleStatusListener.OnPowerUp()
        {
            throw new NotImplementedException();
        }

        void IVehicleStatusListener.OnTakeDamage()
        {
            throw new NotImplementedException();
        }
    }
}

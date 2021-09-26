using System;
using UnityEngine;

namespace VehicleFramework
{
    public class FloodLightsController : MonoBehaviour, IVehicleStatusListener
	{
		private ModVehicle mv;
        private bool isFloodLightsOn = false;

        public void Awake()
        {
            mv = GetComponent<ModVehicle>();
        }

        public virtual void Update()
        {
            if(isFloodLightsOn)
            {
            }
        }
        public void EnableFloodLights()
        {
            if (!isFloodLightsOn)
            {
                SetFloodLampsActive(true);
                mv.lightsOnSound.Stop();
                mv.lightsOnSound.Play();
                isFloodLightsOn = !isFloodLightsOn;
            }
        }
        public void DisableFloodLights()
        {
            if (isFloodLightsOn)
            {
                SetFloodLampsActive(false);
                mv.lightsOffSound.Stop();
                mv.lightsOffSound.Play();
                isFloodLightsOn = !isFloodLightsOn;
            }
        }
        public void ToggleFloodLights()
        {
            if (mv.IsPowered())
            {
                if (isFloodLightsOn)
                {
                    DisableFloodLights();
                }
                else
                {
                    EnableFloodLights();
                }
            }
            else
            {
                isFloodLightsOn = false;
            }
        }
        public void SetFloodLampsActive(bool enabled)
        {
            foreach (var light in mv.FloodLights)
            {
                light.Light.SetActive(enabled && mv.IsPowered());
            }
            if (enabled)
            {
                mv.NotifyStatus(VehicleStatus.OnFloodLightsOn);
            }
            else
            {
                mv.NotifyStatus(VehicleStatus.OnFloodLightsOff);
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
            DisableFloodLights();
        }

        void IVehicleStatusListener.OnTakeDamage()
        {
            throw new NotImplementedException();
        }

        void IVehicleStatusListener.OnAutoLevel()
        {
        }

        void IVehicleStatusListener.OnAutoPilotBegin()
        {
            throw new NotImplementedException();
        }

        void IVehicleStatusListener.OnAutoPilotEnd()
        {
            throw new NotImplementedException();
        }

        void IVehicleStatusListener.OnHeadLightsOn()
        {
        }

        void IVehicleStatusListener.OnHeadLightsOff()
        {
        }

        void IVehicleStatusListener.OnInteriorLightsOn()
        {
        }

        void IVehicleStatusListener.OnInteriorLightsOff()
        {
        }

        void IVehicleStatusListener.OnBatteryLow()
        {
            throw new NotImplementedException();
        }

        void IVehicleStatusListener.OnBatteryDepletion()
        {
            throw new NotImplementedException();
        }

        void IVehicleStatusListener.OnFloodLightsOn()
        {
        }

        void IVehicleStatusListener.OnFloodLightsOff()
        {
        }

        void IVehicleStatusListener.OnNavLightsOn()
        {
        }

        void IVehicleStatusListener.OnNavLightsOff()
        {
        }
    }
}

using System;
using UnityEngine;

namespace VehicleFramework
{
    public class FloodLightsController : MonoBehaviour, IPowerListener
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

        void IPowerListener.OnPowerUp()
        {
        }

        void IPowerListener.OnPowerDown()
        {
            DisableFloodLights();
        }

        void IPowerListener.OnBatterySafe()
        {
        }

        void IPowerListener.OnBatteryLow()
        {
        }

        void IPowerListener.OnBatteryNearlyEmpty()
        {
        }

        void IPowerListener.OnBatteryDepleted()
        {
        }
    }
}

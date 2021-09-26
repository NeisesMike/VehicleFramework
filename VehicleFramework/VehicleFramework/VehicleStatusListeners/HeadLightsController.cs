using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework
{
    public class HeadLightsController : MonoBehaviour, IVehicleStatusListener
	{
        private ModVehicle mv;
        private bool isHeadlightsOn = true;

        public void Awake()
        {
            mv = GetComponent<ModVehicle>();
        }

        public virtual void Update()
        {
            if (mv.IsPlayerPiloting() && Player.main.GetRightHandDown() && !Player.main.GetPDA().isInUse)
            {
                ToggleHeadlights();
            }
            if(isHeadlightsOn)
            {
                mv.GetComponent<EnergyInterface>().ConsumeEnergy(0.001f * Time.deltaTime);
            }
        }
        public void EnableHeadlights()
        {
            if (!isHeadlightsOn)
            {
                SetHeadLightsActive(true);
                mv.lightsOnSound.Stop();
                mv.lightsOnSound.Play();
                isHeadlightsOn = !isHeadlightsOn;
            }
        }
        public void DisableHeadlights()
        {
            if (isHeadlightsOn)
            {
                SetHeadLightsActive(false);
                mv.lightsOffSound.Stop();
                mv.lightsOffSound.Play();
                isHeadlightsOn = !isHeadlightsOn;
            }
        }
        public void ToggleHeadlights()
        {
            if (mv.IsPowered())
            {
                if (isHeadlightsOn)
                {
                    DisableHeadlights();
                }
                else
                {
                    EnableHeadlights();
                }
            }
            else
            {
                isHeadlightsOn = false;
            }
        }
        public void SetVolumetricLightsActive(bool enabled)
        {
            foreach (GameObject light in mv.volumetricLights)
            {
                light.SetActive(!mv.IsPlayerInside() && enabled && mv.IsPowered());
            }
        }
        public void SetHeadLightsActive(bool enabled)
        {
            foreach (GameObject light in mv.lights)
            {
                light.SetActive(enabled && mv.IsPowered());
            }
            SetVolumetricLightsActive(enabled);
            if (enabled)
            {
                mv.NotifyStatus(VehicleStatus.OnHeadLightsOn);
            }
            else
            {
                mv.NotifyStatus(VehicleStatus.OnHeadLightsOff);
            }
        }

        void IVehicleStatusListener.OnPlayerEntry()
        {
            SetVolumetricLightsActive(false);
        }

        void IVehicleStatusListener.OnPlayerExit()
        {
            SetVolumetricLightsActive(true);
        }

        void IVehicleStatusListener.OnPilotBegin()
        {
        }

        void IVehicleStatusListener.OnPilotEnd()
        {
        }

        void IVehicleStatusListener.OnPowerUp()
        {
            EnableHeadlights();
        }

        void IVehicleStatusListener.OnPowerDown()
        {
            DisableHeadlights();
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

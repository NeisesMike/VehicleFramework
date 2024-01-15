using System;
using UnityEngine;

namespace VehicleFramework
{
    public class FloodLightsController : MonoBehaviour, IPowerListener, IVehicleStatusListener
    {
		private VehicleTypes.Submarine mv;
        private bool isFloodLightsOn = false;

        public void Awake()
        {
            mv = GetComponent<VehicleTypes.Submarine>();
        }

        public void EnableFloodLights()
        {
            SetFloodLampsActive(true);
            if (VehicleManager.isWorldLoaded)
            {
                mv.lightsOnSound.Stop();
                mv.lightsOnSound.Play();
            }
            isFloodLightsOn = !isFloodLightsOn;
        }
        public void DisableFloodLights()
        {
            SetFloodLampsActive(false);
            if (VehicleManager.isWorldLoaded)
            {
                mv.lightsOffSound.Stop();
                mv.lightsOffSound.Play();
            }
            isFloodLightsOn = !isFloodLightsOn;
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
                EnableFloodLampEmission();
                mv.NotifyStatus(LightsStatus.OnFloodLightsOn);
            }
            else
            {
                DisableFloodLampEmission();
                mv.NotifyStatus(LightsStatus.OnFloodLightsOff);
            }
        }
        public void EnableFloodLampEmission()
        {
            foreach (var vlight in mv.FloodLights)
            {
                if (vlight.Light.GetComponent<MeshRenderer>() != null)
                {
                    foreach (Material mat in vlight.Light.GetComponent<MeshRenderer>().materials)
                    {
                        mat.EnableKeyword("MARMO_EMISSION");
                        mat.SetFloat("_EmissionLM", 10f);
                        mat.SetFloat("_EmissionLMNight", 10f);
                        mat.SetFloat("_GlowStrength", 0f);
                        mat.SetFloat("_GlowStrengthNight", 0f);
                    }
                }
            }
        }

        public void DisableFloodLampEmission()
        {
            foreach (var vlight in mv.FloodLights)
            {
                if (vlight.Light.GetComponent<MeshRenderer>() != null)
                {
                    foreach (Material mat in vlight.Light.GetComponent<MeshRenderer>().materials)
                    {
                        mat.DisableKeyword("MARMO_EMISSION");
                    }
                }
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
            DisableFloodLights();
        }

        void IPowerListener.OnBatteryNearlyEmpty()
        {
            DisableFloodLights();
        }

        void IPowerListener.OnBatteryDepleted()
        {
            DisableFloodLights();
        }

        void IPowerListener.OnBatteryDead()
        {
            DisableFloodLights();
        }

        void IPowerListener.OnBatteryRevive()
        {
        }
        void IVehicleStatusListener.OnNearbyLeviathan()
        {
            if (isFloodLightsOn)
            {
                ToggleFloodLights();
            }
        }

        void IVehicleStatusListener.OnTakeDamage()
        {
            return;
        }
    }
}

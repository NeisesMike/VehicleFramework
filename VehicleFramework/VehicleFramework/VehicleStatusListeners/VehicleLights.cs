using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework
{
    public class VehicleLights : MonoBehaviour, IVehicleStatusListener
	{
		public ModVehicle mv;
        private bool isLightsOn = true;
        private bool isInteriorLightsOn = true;
        private bool wasPowered = false;
        
        public VehicleLights(ModVehicle inputMV)
        {
            mv = inputMV;
        }

        public virtual void Update()
        {
            if (mv.IsPlayerPiloting() && Player.main.GetRightHandDown() && !Player.main.GetPDA().isInUse)
            {
                ToggleExteriorLighting();
            }
            if(isLightsOn)
            {
                mv.GetComponent<EnergyInterface>().ConsumeEnergy(0.001f * Time.deltaTime);
            }
        }
        public void EnableExteriorLighting()
        {
            SetFloodLampsActive(true);
            Utils.PlayEnvSound(mv.lightsOnSound, mv.lightsOnSound.gameObject.transform.position, 20f);
        }
        public void DisableExteriorLighting()
        {
            SetFloodLampsActive(false);
            Utils.PlayEnvSound(mv.lightsOffSound, mv.lightsOffSound.gameObject.transform.position, 20f);
        }

        public void ToggleExteriorLighting()
        {
            if (mv.IsPowered())
            {
                isLightsOn = !isLightsOn;
                if (isLightsOn)
                {
                    EnableExteriorLighting();
                }
                else
                {
                    DisableExteriorLighting();
                }
            }
        }
        public void ToggleInteriorLighting()
        {
            if (mv.IsPowered())
            {
                isInteriorLightsOn = !isInteriorLightsOn;
                if (isInteriorLightsOn)
                {
                    DisableInteriorLighting();
                }
                else
                {
                    EnableInteriorLighting();
                }
            }
        }
        public void CheckPower()
        {
            if (mv.IsPowered())
            {
                // if newly powered
                if (!wasPowered)
                {
                    EnableExteriorLighting();
                    EnableInteriorLighting();
                }
                wasPowered = true;
            }
            else
            {
                // if newly unpowered
                if (wasPowered)
                {
                    DisableExteriorLighting();
                    DisableInteriorLighting();
                }
                wasPowered = false;
            }
        }

        public void SetVolumetricLightsActive(bool enabled)
        {
            foreach (GameObject light in mv.volumetricLights)
            {
                light.SetActive(!mv.IsPlayerInside() && enabled && mv.IsPowered());
            }
        }
        public void SetFloodLampsActive(bool enabled)
        {
            foreach (GameObject light in mv.lights)
            {
                light.SetActive(enabled && mv.IsPowered());
            }
            SetVolumetricLightsActive(enabled);
            if (enabled)
            {
                mv.NotifyStatus(VehicleStatus.OnExteriorLightsOn);
            }
            else
            {
                mv.NotifyStatus(VehicleStatus.OnExteriorLightsOff);
            }
        }
        public void EnableInteriorLighting()
        {
            foreach (var renderer in mv.GetComponentsInChildren<Renderer>())
            {
                foreach (Material mat in renderer.materials)
                {
                    // add emission to certain materials
                    if (mat.name.Contains("InteriorIlluminatedMaterial"))
                    {
                        mat.EnableKeyword("MARMO_EMISSION");
                        mat.SetFloat("_EmissionLM", 0.25f);
                        mat.SetFloat("_EmissionLMNight", 0.25f);
                        mat.SetFloat("_GlowStrength", 0f);
                        mat.SetFloat("_GlowStrengthNight", 0f);
                    }
                }
            }
            mv.NotifyStatus(VehicleStatus.OnInteriorLightsOn);
        }
        public void DisableInteriorLighting()
        {
            foreach (var renderer in mv.GetComponentsInChildren<Renderer>())
            {
                foreach (Material mat in renderer.materials)
                {
                    // add emission to certain materials
                    if (mat.name.Contains("InteriorIlluminatedMaterial"))
                    {
                        mat.DisableKeyword("MARMO_EMISSION");
                    }

                }
            }
            mv.NotifyStatus(VehicleStatus.OnInteriorLightsOff);
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
            EnableExteriorLighting();
            EnableInteriorLighting();
        }

        void IVehicleStatusListener.OnPowerDown()
        {
            DisableExteriorLighting();
            DisableInteriorLighting();
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
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework
{
    public class VehicleLights : MonoBehaviour, VehicleComponent
	{
		public ModVehicle mv;
        private bool isLightsOn = true;
        private bool wasPowered = false;
        
        public VehicleLights(ModVehicle inputMV)
        {
            mv = inputMV;
        }

        public virtual void Update()
        {
            if (mv.IsPlayerPiloting() && Player.main.GetRightHandDown() && !Player.main.GetPDA().isInUse)
            {
                toggleLights();
            }
            if(isLightsOn)
            {
                mv.GetComponent<EnergyInterface>().ConsumeEnergy(0.01f * Time.deltaTime);
            }
        }

        public void toggleLights()
        {
            isLightsOn = !isLightsOn;
            if (isLightsOn)
            {
                setFloodLampsActive(true);
                if (mv.IsPowered())
                {
                    Utils.PlayEnvSound(mv.lightsOnSound, mv.lightsOnSound.gameObject.transform.position, 20f);
                }
            }
            else
            {
                setFloodLampsActive(false);
                if (mv.IsPowered())
                {
                    Utils.PlayEnvSound(mv.lightsOffSound, mv.lightsOffSound.gameObject.transform.position, 20f);
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
                    setFloodLampsActive(true);
                    EnableInteriorLighting();
                }
                wasPowered = true;
            }
            else
            {
                // if newly unpowered
                if (wasPowered)
                {
                    setFloodLampsActive(false);
                    DisableInteriorLighting();
                }
                wasPowered = false;
            }
        }

        public void setVolumetricLightsActive(bool enabled)
        {
            foreach (GameObject light in mv.volumetricLights)
            {
                light.SetActive(!mv.IsPlayerInside() && enabled && mv.IsPowered());
            }
        }
        public void setFloodLampsActive(bool enabled)
        {
            foreach (GameObject light in mv.lights)
            {
                light.SetActive(enabled && mv.IsPowered());
            }
            setVolumetricLightsActive(enabled);
            /* Beware of infinite loop
            if (enabled)
            {
                foreach (var component in GetComponentsInChildren<VehicleComponent>())
                {
                    component.OnPowerUp();
                }
            }
            else
            {
                foreach (var component in GetComponentsInChildren<VehicleComponent>())
                {
                    component.OnPowerDown();
                }
            }
            */
        }
        public void EnableInteriorLighting()
        {
            foreach (var renderer in mv.InteriorRenderers)
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
        }
        public void DisableInteriorLighting()
        {
            foreach (var renderer in mv.InteriorRenderers)
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
        }

        void VehicleComponent.OnPlayerEntry()
        {
            setVolumetricLightsActive(false);
        }

        void VehicleComponent.OnPlayerExit()
        {
            setVolumetricLightsActive(true);
        }

        void VehicleComponent.OnPilotBegin()
        {
        }

        void VehicleComponent.OnPilotEnd()
        {
        }

        void VehicleComponent.OnPowerUp()
        {
            setFloodLampsActive(true);
            EnableInteriorLighting();
        }

        void VehicleComponent.OnPowerDown()
        {
            setFloodLampsActive(false);
            DisableInteriorLighting();
        }

        void VehicleComponent.OnLightsOn()
        {
        }

        void VehicleComponent.OnLightsOff()
        {
        }

        void VehicleComponent.OnTakeDamage()
        {
            throw new NotImplementedException();
        }

        void VehicleComponent.OnAutoLevel()
        {
            throw new NotImplementedException();
        }

        void VehicleComponent.OnAutoPilotBegin()
        {
            throw new NotImplementedException();
        }

        void VehicleComponent.OnAutoPilotEnd()
        {
            throw new NotImplementedException();
        }
    }
}

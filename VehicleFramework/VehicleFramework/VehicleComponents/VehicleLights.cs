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
        public void checkPower()
        {
            if (mv.IsPowered())
            {
                // if newly powered
                if (!wasPowered)
                {
                    setFloodLampsActive(true);
                    enableInteriorLighting();
                }
                wasPowered = true;
            }
            else
            {
                // if newly unpowered
                if (wasPowered)
                {
                    setFloodLampsActive(false);
                    disableInteriorLighting();
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
            if (enabled)
            {
                BroadcastMessage("OnLightsOn");
            }
            {
                BroadcastMessage("OnLightsOff");
            }
        }
        public void enableInteriorLighting()
        {
            foreach (var renderer in mv.interiorRenderers)
            {
                foreach (Material mat in renderer.materials)
                {
                    // add emission to certain materials
                    if (
                        (renderer.gameObject.name == "Main-Body" && mat.name.Contains("Material"))
                        || renderer.gameObject.name != "Main-Body"
                        )
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
        public void disableInteriorLighting()
        {
            foreach (var renderer in mv.interiorRenderers)
            {
                foreach (Material mat in renderer.materials)
                {
                    // add emission to certain materials
                    if (
                        (renderer.gameObject.name == "Main-Body" && mat.name.Contains("Material"))
                        || renderer.gameObject.name != "Main-Body"
                        )
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
            enableInteriorLighting();
        }

        void VehicleComponent.OnPowerDown()
        {
            setFloodLampsActive(false);
            disableInteriorLighting();
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

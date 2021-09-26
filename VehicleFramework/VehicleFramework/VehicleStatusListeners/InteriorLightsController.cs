using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework
{
    public class InteriorLightsController : MonoBehaviour, IVehicleStatusListener
	{
		private ModVehicle mv;
        private bool isInteriorLightsOn = true;


        public void Awake()
        {
            mv = GetComponent<ModVehicle>();
        }

        public void ToggleInteriorLighting()
        {
            if (mv.IsPowered())
            {
                isInteriorLightsOn = !isInteriorLightsOn;
                if (isInteriorLightsOn)
                {
                    EnableInteriorLighting();
                }
                else
                {
                    DisableInteriorLighting();
                }
            }
            else
            {
                isInteriorLightsOn = false;
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
            isInteriorLightsOn = true;
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
            isInteriorLightsOn = false;
            mv.NotifyStatus(VehicleStatus.OnInteriorLightsOff);
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
            EnableInteriorLighting();
        }

        void IVehicleStatusListener.OnPowerDown()
        {
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework
{
    public class InteriorLightsController : MonoBehaviour, IPowerListener
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
                if (isInteriorLightsOn)
                {
                    DisableInteriorLighting();
                }
                else
                {
                    EnableInteriorLighting();
                }
            }
            else
            {
                DisableInteriorLighting();
            }
        }
        public void EnableInteriorLighting()
        {
            if (!isInteriorLightsOn)
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
        }
        public void DisableInteriorLighting()
        {
            if (isInteriorLightsOn)
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
        }

        void IPowerListener.OnPowerUp()
        {
            EnableInteriorLighting();
        }

        void IPowerListener.OnPowerDown()
        {
            DisableInteriorLighting();
        }

        void IPowerListener.OnBatterySafe()
        {
            EnableInteriorLighting();
        }

        void IPowerListener.OnBatteryLow()
        {
            EnableInteriorLighting();
        }

        void IPowerListener.OnBatteryNearlyEmpty()
        {
            DisableInteriorLighting();
        }

        void IPowerListener.OnBatteryDepleted()
        {
            DisableInteriorLighting();
        }

        void IPowerListener.OnBatteryDead()
        {
            DisableInteriorLighting();
        }

        void IPowerListener.OnBatteryRevive()
        {
            EnableInteriorLighting();
        }
    }
}

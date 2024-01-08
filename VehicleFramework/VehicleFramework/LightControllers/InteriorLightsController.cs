﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VehicleFramework.VehicleTypes;

namespace VehicleFramework
{
    public class InteriorLightsController : MonoBehaviour, IPowerListener, IVehicleStatusListener
    {
		private Submarine mv;
        private bool isInteriorLightsOn = true;
        
        public void Awake()
        {
            mv = GetComponent<Submarine>();
        }

        public void ToggleInteriorLighting()
        {
            if (mv.IsPowered())
            {
                if (isInteriorLightsOn)
                {
                    DisableInteriorLighting();
                    isInteriorLightsOn = false;
                    mv.NotifyStatus(LightsStatus.OnInteriorLightsOff);
                }
                else
                {
                    EnableInteriorLighting();
                    isInteriorLightsOn = true;
                    mv.NotifyStatus(LightsStatus.OnInteriorLightsOn);
                }
            }
            else
            {
                DisableInteriorLighting();
            }
        }
        public void EnableInteriorLighting()
        {
            mv.InteriorLights.ForEach(x => x.enabled = true);
        }
        public void DisableInteriorLighting()
        {
            mv.InteriorLights.ForEach(x => x.enabled = false);
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
            //EnableInteriorLighting();
        }

        void IPowerListener.OnBatteryLow()
        {
            //EnableInteriorLighting();
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
        void IVehicleStatusListener.OnNearbyLeviathan()
        {
            if (isInteriorLightsOn)
            {
                ToggleInteriorLighting();
            }
        }

        void IVehicleStatusListener.OnTakeDamage()
        {
            return;
        }
    }
}

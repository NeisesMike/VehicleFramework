using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework
{
    /*
     * We would like very much do consolidate all our power drains here
     * This is trivial for lights,
     * but a bit more difficult for driving
     */
    public class PowerManager : MonoBehaviour, IVehicleStatusListener, IAutoPilotListener, ILightsStatusListener
    {
        private PowerStatus currentPowerStatus = PowerStatus.OnBatterySafe;
        private bool isHeadlightsOn = false;
        private bool isFloodlightsOn = false;
        private bool isNavLightsOn = false;
        private bool isInteriorLightsOn = false;
        private bool isAutoLeveling = false;
        private bool isAutoPiloting = false;

        private ModVehicle _mv;
        private ModVehicle mv
        {
            get
            {
                if(_mv==null)
                {
                    _mv = GetComponent<ModVehicle>();
                }
                return _mv;
            }
        }
        private EnergyInterface _ei = null;
        private EnergyInterface ei
        {
            get
            {
                if (_ei == null)
                {
                    _ei = GetComponent<EnergyInterface>();
                }
                return _ei;
            }
        }

        public void Start()
        {
        }
        public void Update()
        {
            /*
             * research suggests engines should be between 10 and 100x more draining than the lights
             * engine takes [0,3], so we're justified for either [0,0.3] or [0,0.03]
             * we chose [0,0.1] for the lights
             */
            if (isHeadlightsOn)
            {
                var tmp = ei.ConsumeEnergy(0.01f * Time.deltaTime);
                Logger.Log(tmp.ToString());
            }
            if (isFloodlightsOn)
            {
                ei.ConsumeEnergy(0.1f * Time.deltaTime);
            }
            if(isNavLightsOn)
            {
                ei.ConsumeEnergy(0.001f * Time.deltaTime);
            }
            if(isInteriorLightsOn)
            {
                ei.ConsumeEnergy(0.001f * Time.deltaTime);
            }
            if (isInteriorLightsOn)
            {
                ei.ConsumeEnergy(0.001f * Time.deltaTime);
            }
            if (isAutoLeveling)
            {
                float scalarFactor = 1.0f;
                float basePowerConsumptionPerSecond = .15f;
                float upgradeModifier = Mathf.Pow(0.85f, mv.numEfficiencyModules);
                mv.TrySpendEnergy(scalarFactor * basePowerConsumptionPerSecond * upgradeModifier * Time.deltaTime);
            }
            if (isAutoPiloting)
            {
                float scalarFactor = 1.0f;
                float basePowerConsumptionPerSecond = 3f;
                float upgradeModifier = Mathf.Pow(0.85f, mv.numEfficiencyModules);
                mv.TrySpendEnergy(scalarFactor * basePowerConsumptionPerSecond * upgradeModifier * Time.deltaTime);
            }

            // check battery thresholds, and make notifications as appropriate
            PowerStatus thisPS = EvaluatePowerStatus();
            if(thisPS != currentPowerStatus)
            {
                currentPowerStatus = thisPS;
                mv.NotifyStatus(currentPowerStatus);
            }
        }
        private PowerStatus EvaluatePowerStatus()
        {
            mv.energyInterface.GetValues(out float charge, out _);
            if (charge < 1)
            {
                return PowerStatus.OnBatteryDepleted;
            }
            else if (charge < 8)
            {
                return PowerStatus.OnBatteryNearlyEmpty;
            }
            else if(charge < 80)
            {
                return PowerStatus.OnBatteryLow;
            }
            else
            {
                return PowerStatus.OnBatterySafe;
            }
        }
        void IAutoPilotListener.OnAutoLevelBegin()
        {
            isAutoLeveling = true;
        }
        void IAutoPilotListener.OnAutoLevelEnd()
        {
            isAutoLeveling = false;
        }

        void IAutoPilotListener.OnAutoPilotBegin()
        {
            isAutoPiloting = true;
        }

        void IAutoPilotListener.OnAutoPilotEnd()
        {
            isAutoPiloting = false;
        }

        void ILightsStatusListener.OnFloodLightsOff()
        {
            isFloodlightsOn = false;
        }

        void ILightsStatusListener.OnFloodLightsOn()
        {
            isFloodlightsOn = true;
        }

        void ILightsStatusListener.OnHeadLightsOff()
        {
            isHeadlightsOn = false;
        }

        void ILightsStatusListener.OnHeadLightsOn()
        {
            isHeadlightsOn = true;
        }

        void ILightsStatusListener.OnInteriorLightsOff()
        {
            isInteriorLightsOn = false;
        }

        void ILightsStatusListener.OnInteriorLightsOn()
        {
            isInteriorLightsOn = true;
        }

        void ILightsStatusListener.OnNavLightsOff()
        {
            isNavLightsOn = false;
        }

        void ILightsStatusListener.OnNavLightsOn()
        {
            isNavLightsOn = true;
        }

        void IVehicleStatusListener.OnTakeDamage()
        {
        }
    }
}

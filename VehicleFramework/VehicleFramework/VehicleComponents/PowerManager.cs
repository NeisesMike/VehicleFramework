using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework
{
    /*
     * The PowerManager handles all power drains for the ModVehicle.
     * It also monitors the two-bit power status of the ModVehicle.
     * It also handles the broadcasting of all power-related notifications.
     * We would like very much do consolidate all our power drains here
     * This is trivial for lights,
     * but a bit more difficult for driving
     */
    public class PowerManager : MonoBehaviour, IVehicleStatusListener, IAutoPilotListener, ILightsStatusListener
    {
        public struct PowerStatus
        {
            public bool hasFuel;
            public bool isPowered;
            public override bool Equals(object obj) => obj is PowerStatus other && this.Equals(other);
            public bool Equals(PowerStatus p) => hasFuel == p.hasFuel && isPowered == p.isPowered;
            public override int GetHashCode() => (hasFuel, isPowered).GetHashCode();
            public static bool operator ==(PowerStatus lhs, PowerStatus rhs) => lhs.Equals(rhs);
            public static bool operator !=(PowerStatus lhs, PowerStatus rhs) => !(lhs == rhs);
        }
        private PowerStatus lastStatus = new PowerStatus { hasFuel = false, isPowered = false };
        private PowerEvent latestPowerEvent = PowerEvent.OnBatterySafe;
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


        private PowerEvent EvaluatePowerEvent()
        {
            mv.energyInterface.GetValues(out float charge, out _);
            if (charge < 1)
            {
                return PowerEvent.OnBatteryDepleted;
            }
            else if (charge < 8)
            {
                return PowerEvent.OnBatteryNearlyEmpty;
            }
            else if (charge < 80)
            {
                return PowerEvent.OnBatteryLow;
            }
            else
            {
                return PowerEvent.OnBatterySafe;
            }
        }
        public PowerStatus EvaluatePowerStatus()
        {
            mv.energyInterface.GetValues(out float charge, out _);
            PowerStatus thisStatus = new PowerStatus();
            thisStatus.isPowered = mv.isPoweredOn;
            if(charge > 0)
            {
                thisStatus.hasFuel = true;
            }
            else
            {
                thisStatus.hasFuel = false;
            }
            return thisStatus;
        }
        public void TrySpendEnergy(float val)
        {
            float desired = val;
            float available = ei.TotalCanProvide(out _);
            if (available < desired)
            {
                desired = available;
            }
            ei.ConsumeEnergy(desired);
        }
        public void AccountForTheTypicalDrains()
        {
            /*
             * research suggests engines should be between 10 and 100x more draining than the lights
             * engine takes [0,3], so we're justified for either [0,0.3] or [0,0.03]
             * we chose [0,0.1] for the lights
             */
            if (isHeadlightsOn)
            {
                TrySpendEnergy(0.01f * Time.deltaTime);
            }
            if (isFloodlightsOn)
            {
                TrySpendEnergy(0.1f * Time.deltaTime);
            }
            if (isNavLightsOn)
            {
                TrySpendEnergy(0.001f * Time.deltaTime);
            }
            if (isInteriorLightsOn)
            {
                TrySpendEnergy(0.001f * Time.deltaTime);
            }
            if (isAutoLeveling)
            {
                float scalarFactor = 1.0f;
                float basePowerConsumptionPerSecond = .15f;
                float upgradeModifier = Mathf.Pow(0.85f, mv.numEfficiencyModules);
                TrySpendEnergy(scalarFactor * basePowerConsumptionPerSecond * upgradeModifier * Time.deltaTime);
            }
            if (isAutoPiloting)
            {
                float scalarFactor = 1.0f;
                float basePowerConsumptionPerSecond = 3f;
                float upgradeModifier = Mathf.Pow(0.85f, mv.numEfficiencyModules);
                TrySpendEnergy(scalarFactor * basePowerConsumptionPerSecond * upgradeModifier * Time.deltaTime);
            }
        }
        public void Start()
        {
        }
        public void Update()
        {
            AccountForTheTypicalDrains();

            // check battery thresholds, and make notifications as appropriate
            PowerEvent thisPS = EvaluatePowerEvent();
            if(thisPS != latestPowerEvent)
            {
                latestPowerEvent = thisPS;
                mv.NotifyStatus(latestPowerEvent);
            }

            // Update the current power status,
            // first sending notifications if necessary
            PowerStatus currentStatus = EvaluatePowerStatus();
            if(currentStatus != lastStatus)
            {
                if (lastStatus.isPowered != currentStatus.isPowered)
                {
                    if (currentStatus.isPowered)
                    {
                        mv.NotifyStatus(PowerEvent.OnPowerUp);
                    }
                    else
                    {
                        mv.NotifyStatus(PowerEvent.OnPowerDown);
                    }
                }
                if (lastStatus.hasFuel != currentStatus.hasFuel)
                {
                    if (currentStatus.hasFuel)
                    {
                        mv.isPoweredOn = true;
                        mv.NotifyStatus(PowerEvent.OnBatteryRevive);
                    }
                    else
                    {
                        mv.NotifyStatus(PowerEvent.OnBatteryDead);
                    }
                }
                lastStatus = currentStatus;
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

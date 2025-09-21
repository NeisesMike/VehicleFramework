using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VehicleFramework.VehicleTypes;
using VehicleFramework.Interfaces;
using VehicleFramework.Admin;

namespace VehicleFramework.VehicleComponents
{
    /*
     * The PowerManager handles all power drains for the ModVehicle.
     * It also monitors the two-bit power status of the ModVehicle.
     * It also handles the broadcasting of all power-related notifications.
     * We would like very much do consolidate all our power drains here
     * This is trivial for lights,
     * but a bit more difficult for driving
     */
    public class PowerManager : MonoBehaviour, IAutoPilotListener, ILightsStatusListener
    {
        public struct PowerStatus
        {
            public PowerStatus(bool fuel, bool power)
            {
                hasFuel = fuel;
                isPowered = power;
            }
            public bool hasFuel;
            public bool isPowered;
            public readonly override bool Equals(object obj) => obj is PowerStatus other && this.Equals(other);
            public readonly bool Equals(PowerStatus p) => hasFuel == p.hasFuel && isPowered == p.isPowered;
            public readonly override int GetHashCode() => (hasFuel, isPowered).GetHashCode();
            public static bool operator ==(PowerStatus lhs, PowerStatus rhs) => lhs.Equals(rhs);
            public static bool operator !=(PowerStatus lhs, PowerStatus rhs) => !(lhs == rhs);
        }
        private PowerStatus lastStatus = new() { hasFuel = false, isPowered = false };
        private PowerEvent latestPowerEvent = PowerEvent.OnBatterySafe;
        private bool isHeadlightsOn = false;
        private bool isFloodlightsOn = false;
        private bool isNavLightsOn = false;
        private bool isInteriorLightsOn = false;
        private bool isAutoLeveling = false;
        private bool isAutoPiloting = false;
        private ModVehicle MV => GetComponent<ModVehicle>();
        private EnergyInterface EI => MV.energyInterface;

        private void Awake()
        {
            if (MV == null)
            {
                throw SessionManager.Fatal($"{nameof(PowerManager)}.{nameof(Awake)}(): ModVehicle is null!");
            }
            if (EI == null)
            {
                throw SessionManager.Fatal($"{MV.GetName()}.TrySpendEnergy(): EnergyInterface is null!");
            }
        }

        private PowerEvent EvaluatePowerEvent()
        {
            EI.GetValues(out float charge, out _);
            if (charge < 5)
            {
                return PowerEvent.OnBatteryDepleted;
            }
            else if (charge < 100)
            {
                return PowerEvent.OnBatteryNearlyEmpty;
            }
            else if (charge < 320)
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
            EI.GetValues(out float charge, out _);
            return new PowerStatus
            {
                isPowered = MV.isPoweredOn,
                hasFuel = charge > 0
            };
        }
        public float TrySpendEnergy(float val)
        {
            float desired = val;
            float available = EI.TotalCanProvide(out _);
            if (available < desired)
            {
                desired = available;
            }
            return EI.ConsumeEnergy(desired);
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
                float upgradeModifier = Mathf.Pow(0.85f, MV.numEfficiencyModules);
                TrySpendEnergy(scalarFactor * basePowerConsumptionPerSecond * upgradeModifier * Time.deltaTime);
            }
            if (isAutoPiloting)
            {
                float scalarFactor = 1.0f;
                float basePowerConsumptionPerSecond = 3f;
                float upgradeModifier = Mathf.Pow(0.85f, MV.numEfficiencyModules);
                TrySpendEnergy(scalarFactor * basePowerConsumptionPerSecond * upgradeModifier * Time.deltaTime);
            }
        }
        public void Update()
        {
            AccountForTheTypicalDrains();

            PowerEvent currentPowerEvent = EvaluatePowerEvent();
            PowerStatus currentPowerStatus = EvaluatePowerStatus();

            if (currentPowerStatus != lastStatus)
            {
                NotifyPowerChanged(currentPowerStatus.hasFuel, currentPowerStatus.isPowered);
                NotifyBatteryChanged(currentPowerStatus, lastStatus);
                lastStatus = currentPowerStatus;
                latestPowerEvent = currentPowerEvent;
                return;
            }

            if (currentPowerStatus.hasFuel && currentPowerStatus.isPowered)
            {
                if (currentPowerEvent != latestPowerEvent)
                {
                    latestPowerEvent = currentPowerEvent;
                    NotifyPowerStatus(currentPowerEvent);
                }
            }
        }
        private void NotifyPowerChanged(bool isBatteryCharged, bool isSwitchedOn)
        {
            foreach (var component in GetComponentsInChildren<IPowerChanged>())
            {
                (component as IPowerChanged).OnPowerChanged(isBatteryCharged, isSwitchedOn);
            }
        }
        private void NotifyPowerStatus(PowerEvent newEvent)
        {
            foreach (var component in GetComponentsInChildren<IPowerListener>())
            {
                switch (newEvent)
                {
                    case PowerEvent.OnBatterySafe:
                        component.OnBatterySafe();
                        break;
                    case PowerEvent.OnBatteryLow:
                        component.OnBatteryLow();
                        break;
                    case PowerEvent.OnBatteryNearlyEmpty:
                        component.OnBatteryNearlyEmpty();
                        break;
                    case PowerEvent.OnBatteryDepleted:
                        component.OnBatteryDepleted();
                        break;
                    default:
                        Logger.Error("Error: tried to notify using an invalid status");
                        break;
                }
            }
        }
        private void NotifyBatteryChanged(PowerStatus newPS, PowerStatus oldPS)
        {
            foreach (var component in GetComponentsInChildren<IPowerListener>())
            {
                if (oldPS.isPowered != newPS.isPowered)
                {
                    if (newPS.isPowered)
                    {
                        (component as IPowerListener).OnPowerUp();
                    }
                    else
                    {
                        (component as IPowerListener).OnPowerDown();
                    }
                }
                if (oldPS.hasFuel != newPS.hasFuel)
                {
                    if (newPS.hasFuel)
                    {
                        (component as IPowerListener).OnBatteryRevive();
                    }
                    else
                    {
                        (component as IPowerListener).OnBatteryDead();
                    }
                }
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
    }
}

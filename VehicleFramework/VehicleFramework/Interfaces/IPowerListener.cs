using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleFramework
{
    public enum PowerStatus
    {
        OnPowerUp,
        OnPowerDown,
        OnBatterySafe,
        OnBatteryLow,
        OnBatteryNearlyEmpty,
        OnBatteryDepleted
    }
    public interface IPowerListener
    {
        void OnPowerUp();
        void OnPowerDown();
        void OnBatterySafe();
        void OnBatteryLow();
        void OnBatteryNearlyEmpty();
        void OnBatteryDepleted();
    }
}

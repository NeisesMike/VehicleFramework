using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleFramework
{
    public enum PowerEvent
    {
        OnBatterySafe,
        OnBatteryLow,
        OnBatteryNearlyEmpty,
        OnBatteryDepleted
    }
    public interface IPowerChanged // useful for managing things that need to be powered up or down
    {
        void OnPowerChanged(bool hasBatteryPower, bool isSwitchedOn);
    }
    public interface IPowerListener // useful for issuing power status notifications (ai voice, ui elements)
    {
        void OnPowerUp();
        void OnPowerDown();
        void OnBatteryDead();
        void OnBatteryRevive();

        // the following notifications are only sent when the vehicle has battery and is powered ON
        void OnBatterySafe();
        void OnBatteryLow();
        void OnBatteryNearlyEmpty();
        void OnBatteryDepleted();
    }
}

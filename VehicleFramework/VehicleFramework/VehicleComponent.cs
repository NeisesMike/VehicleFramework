using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleFramework
{
    public interface VehicleComponent
    {
        void OnPlayerEntry();
        void OnPlayerExit();
        void OnPilotBegin();
        void OnPilotEnd();
        void OnPowerUp();
        void OnPowerDown();
        void OnLightsOn();
        void OnLightsOff();
        void OnTakeDamage();
        void OnAutoLevel();
        void OnAutoPilotBegin();
        void OnAutoPilotEnd();
    }
}

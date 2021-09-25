using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleFramework
{
    public enum VehicleStatus
    {
        OnPlayerEntry,
        OnPlayerExit,
        OnPilotBegin,
        OnPilotEnd,
        OnPowerUp,
        OnPowerDown,
        OnExteriorLightsOn,
        OnExteriorLightsOff,
        OnInteriorLightsOn,
        OnInteriorLightsOff,
        OnTakeDamage,
        OnAutoLevel,
        OnAutoPilotBegin,
        OnAutoPilotEnd
    }
    public interface IVehicleStatusListener
    {
        void OnPlayerEntry();
        void OnPlayerExit();
        void OnPilotBegin();
        void OnPilotEnd();
        void OnPowerUp();
        void OnPowerDown();
        // TODO
        //Something is pulsing OnExteriorLightsOn every frame, but not sure what
        void OnExteriorLightsOn();
        void OnExteriorLightsOff();
        void OnInteriorLightsOn();
        void OnInteriorLightsOff();
        void OnTakeDamage();
        void OnAutoLevel();
        void OnAutoPilotBegin();
        void OnAutoPilotEnd();
    }
}

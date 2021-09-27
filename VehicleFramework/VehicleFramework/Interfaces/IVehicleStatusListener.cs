using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleFramework
{
    public enum VehicleStatus
    {
        OnHeadLightsOn,
        OnHeadLightsOff,
        OnInteriorLightsOn,
        OnInteriorLightsOff,
        OnFloodLightsOn,
        OnFloodLightsOff,
        OnNavLightsOn,
        OnNavLightsOff,
        OnTakeDamage,
        OnAutoLevel,
        OnAutoPilotBegin,
        OnAutoPilotEnd,
    }
    public interface IVehicleStatusListener
    {
        // TODO
        //Something is pulsing OnExteriorLightsOn every frame, but not sure what
        void OnHeadLightsOn();
        void OnHeadLightsOff();
        void OnInteriorLightsOn();
        void OnInteriorLightsOff();
        void OnNavLightsOn();
        void OnNavLightsOff();
        void OnFloodLightsOn();
        void OnFloodLightsOff();
        void OnTakeDamage();
        void OnAutoLevel();
        void OnAutoPilotBegin();
        void OnAutoPilotEnd();
    }
}

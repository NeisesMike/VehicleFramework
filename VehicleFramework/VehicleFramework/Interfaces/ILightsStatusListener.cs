using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleFramework
{
    public enum LightsStatus
    {
        OnHeadLightsOn,
        OnHeadLightsOff,
        OnInteriorLightsOn,
        OnInteriorLightsOff,
        OnFloodLightsOn,
        OnFloodLightsOff,
        OnNavLightsOn,
        OnNavLightsOff,
    }
    public interface ILightsStatusListener
    {
        void OnHeadLightsOn();
        void OnHeadLightsOff();
        void OnInteriorLightsOn();
        void OnInteriorLightsOff();
        void OnNavLightsOn();
        void OnNavLightsOff();
        void OnFloodLightsOn();
        void OnFloodLightsOff();
    }
}

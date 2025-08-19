using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleFramework.Interfaces
{
    public enum AutoPilotStatus
    {
        OnAutoLevelBegin,
        OnAutoLevelEnd,
        OnAutoPilotBegin,
        OnAutoPilotEnd,
    }
    public interface IAutoPilotListener
    {
        void OnAutoLevelBegin();
        void OnAutoLevelEnd();
        void OnAutoPilotBegin();
        void OnAutoPilotEnd();
    }
}

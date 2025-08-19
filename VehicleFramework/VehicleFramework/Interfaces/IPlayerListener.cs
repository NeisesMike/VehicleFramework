using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleFramework.Interfaces
{
    public enum PlayerStatus
    {
        OnPlayerEntry,
        OnPlayerExit,
        OnPilotBegin,
        OnPilotEnd
    }
    public interface IPlayerListener
    {
        void OnPlayerEntry();
        void OnPlayerExit();
        void OnPilotBegin();
        void OnPilotEnd();
    }
}

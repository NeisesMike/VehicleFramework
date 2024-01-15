using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleFramework
{
    public interface IDroneInterface
    {
        void InitiatePairingMode();
        void FinalizePairingMode();
        void RespondWithPairingMode();
        void ExitPairingMode();
        bool IsInPairingModeAsInitiator();
        bool IsInPairingModeAsResponder();
    }
}

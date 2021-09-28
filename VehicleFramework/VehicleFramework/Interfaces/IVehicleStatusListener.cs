using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleFramework
{
    public enum VehicleStatus
    {
        OnTakeDamage,
    }
    public interface IVehicleStatusListener
    {
        void OnTakeDamage();
    }
}

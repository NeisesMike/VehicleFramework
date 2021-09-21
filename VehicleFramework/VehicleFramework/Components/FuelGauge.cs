using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework
{
    public class FuelGauge : MonoBehaviour
	{
		public ModVehicle mv;
        private bool wasPowered = false;
        public void Update()
        {
            if (mv.IsPowered())
            {
                if (!wasPowered)
                {
                    mv.NotifyStatus(VehicleStatus.OnPowerUp);
                }
                wasPowered = true;
            }
            else
            {
                if (wasPowered)
                {
                    mv.NotifyStatus(VehicleStatus.OnPowerDown);
                }
                wasPowered = false;
            }
        }
    }
}

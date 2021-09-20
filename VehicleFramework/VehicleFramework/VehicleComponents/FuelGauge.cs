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
                    foreach (var component in GetComponentsInChildren<VehicleComponent>())
                    {
                        component.OnPowerUp();
                    }
                }
                wasPowered = true;
            }
            else
            {
                if (wasPowered)
                {
                    foreach (var component in GetComponentsInChildren<VehicleComponent>())
                    {
                        component.OnPowerDown();
                    }
                }
                wasPowered = false;
            }
        }
    }
}

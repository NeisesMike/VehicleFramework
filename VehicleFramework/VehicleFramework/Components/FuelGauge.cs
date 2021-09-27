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
        public void Awake()
        {
            mv = GetComponent<ModVehicle>();
        }
        public void Update()
        {
            if (!mv.IsDisengaged)
            {
                if (mv.IsPowered())
                {
                    if (!wasPowered)
                    {
                        mv.NotifyStatus(PowerStatus.OnBatteryRevive);
                    }
                    wasPowered = true;
                }
                else
                {
                    if (wasPowered)
                    {
                        mv.NotifyStatus(PowerStatus.OnBatteryDead);
                    }
                    wasPowered = false;
                }
            }
        }
    }
}

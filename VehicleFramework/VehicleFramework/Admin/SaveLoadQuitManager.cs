using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace VehicleFramework.Admin
{
    public static class SaveLoadQuitManager
    {
        public static void SetWorldNotLoaded()
        {
            VehicleManager.isWorldLoaded = false;
        }
        public static void SetWorldLoaded()
        {
            VehicleManager.isWorldLoaded = true;
        }
    }
}


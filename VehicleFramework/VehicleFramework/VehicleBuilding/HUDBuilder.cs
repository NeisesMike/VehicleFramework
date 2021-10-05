using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework
{
    public static class HUDBuilder
    {
        /*
         * This class hosts HUD-building functionality.
         * Currently it just steals the Seamoth HUD,
         * so the class is trivial.
         * Eventually I would like to support a new HUD,
         * so it's okay for the class to be trivial now.
         */
        public static void BuildNormalHUD()
        {
            // copy the seamoth hud for now
            GameObject seamothHUDElementsRoot= uGUI.main.transform.Find("ScreenCanvas/HUD/Content/Seamoth").gameObject;

            GameObject mvHUDElementsRoot = GameObject.Instantiate(seamothHUDElementsRoot, uGUI.main.transform.Find("ScreenCanvas/HUD/Content"));
            mvHUDElementsRoot.name = "ModVehicle";

            uGUI_VehicleHUD ret = uGUI.main.transform.Find("ScreenCanvas/HUD").gameObject.EnsureComponent<uGUI_VehicleHUD>();
            ret.root = mvHUDElementsRoot;
            ret.textHealth = mvHUDElementsRoot.transform.Find("Health").GetComponent<UnityEngine.UI.Text>();
            ret.textPower = mvHUDElementsRoot.transform.Find("Power").GetComponent<UnityEngine.UI.Text>();
            ret.textTemperature = mvHUDElementsRoot.transform.Find("Temperature/TemperatureValue").GetComponent<UnityEngine.UI.Text>();
            ret.textTemperatureSuffix = mvHUDElementsRoot.transform.Find("Temperature/TemperatureValue/TemperatureSuffix").GetComponent<UnityEngine.UI.Text>();
        }
    }
}

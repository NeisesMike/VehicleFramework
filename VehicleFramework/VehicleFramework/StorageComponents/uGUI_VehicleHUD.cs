using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace VehicleFramework
{
	public class uGUI_VehicleHUD : MonoBehaviour
	{
		public GameObject droneHUD = null;
		public void Update()
		{
			ModVehicle mv = null;
			PDA pda = null;
			Player main = Player.main;
			if (main != null)
			{
				mv = (main.GetVehicle() as ModVehicle);
				mv = mv ?? VehicleTypes.Drone.mountedDrone;
				pda = main.GetPDA();
			}
			bool mvflag = mv != null && (pda == null || !pda.isInUse);
			bool droneflag = mvflag && (VehicleTypes.Drone.mountedDrone != null);
			if (root.activeSelf != mvflag)
			{
				root.SetActive(mvflag);
			}
			if(droneHUD.activeSelf != droneflag)
            {
				droneHUD.SetActive(droneflag);
            }
			if (mvflag)
			{
                mv.GetHUDValues(out float num, out float num2);
                float temperature = mv.GetTemperature();
				int num3 = Mathf.CeilToInt(num * 100f);
				if (lastHealth != num3)
				{
					lastHealth = num3;
					textHealth.text = IntStringCache.GetStringForInt(lastHealth);
				}
				int num4 = Mathf.CeilToInt(num2 * 100f);
				if (lastPower != num4)
				{
					lastPower = num4;
					textPower.text = IntStringCache.GetStringForInt(lastPower);
				}
				temperatureSmoothValue = ((temperatureSmoothValue < -10000f) ? temperature : Mathf.SmoothDamp(temperatureSmoothValue, temperature, ref temperatureVelocity, 1f));
				int num5 = Mathf.CeilToInt(temperatureSmoothValue);
				if (lastTemperature != num5)
				{
					lastTemperature = num5;
					textTemperature.text = IntStringCache.GetStringForInt(lastTemperature);
					textTemperatureSuffix.text = Language.main.GetFormat("ThermometerFormat");
				}
			}
			if(droneflag)
            {
				DroneUpdate();
            }
		}
		public void DroneUpdate()
		{
			VehicleTypes.Drone drone = VehicleTypes.Drone.mountedDrone;
			if (drone.IsConnecting)
			{
				droneHUD.transform.Find("Connecting").gameObject.SetActive(true);
			}
			else
			{
				droneHUD.transform.Find("Connecting").gameObject.SetActive(false);
			}
			if (drone.pairedStation == null)
			{
				return;
			}
			int distance = Mathf.CeilToInt(Vector3.Distance(drone.transform.position, drone.pairedStation.transform.position));
			droneHUD.transform.Find("Title/DistanceText").gameObject.GetComponent<TextMeshProUGUI>().text = string.Format("<color=#6EFEFFFF>{0}</color> <size=26>{1} {2}</size>", Language.main.Get("CameraDroneDistance"), (distance >= 0) ? IntStringCache.GetStringForInt(distance) : "--", Language.main.Get("MeterSuffix"));
		}

		public const float temperatureSmoothTime = 1f;
		[AssertNotNull]
		public GameObject root;
		[AssertNotNull]
		public TextMeshProUGUI textHealth;
		[AssertNotNull]
		public TextMeshProUGUI textPower;
		[AssertNotNull]
		public TextMeshProUGUI textTemperature;
		[AssertNotNull]
		public TextMeshProUGUI textTemperatureSuffix;
		public int lastHealth = int.MinValue;
		public int lastPower = int.MinValue;
		public int lastTemperature = int.MinValue;
		public float temperatureSmoothValue = float.MinValue;
		public float temperatureVelocity;
		[AssertLocalization]
		public const string thermometerFormatKey = "ThermometerFormat";
	}
}
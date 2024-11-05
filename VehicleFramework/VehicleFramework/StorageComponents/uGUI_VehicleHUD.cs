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
				mv = main.GetModVehicle();
				pda = main.GetPDA();
			}
			bool mvflag = mv != null && (pda == null || !pda.isInUse);
			bool droneflag = mvflag && (VehicleTypes.Drone.mountedDrone != null);
			if (root.activeSelf != mvflag)
			{
				root.SetActive(mvflag);
			}
			if (droneHUD.activeSelf != droneflag)
			{
				droneHUD.SetActive(droneflag);
			}
			if (mvflag)
			{
				UpdateHealth();
				UpdatePower();
				UpdateTemperature();
				UpdateStorage();
			}
			if (droneflag)
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
			int distance = Mathf.CeilToInt(Vector3.Distance(drone.transform.position, drone.pairedStation.transform.position));
			droneHUD.transform.Find("Title/DistanceText").gameObject.GetComponent<TextMeshProUGUI>().text = string.Format("<color=#6EFEFFFF>{0}</color> <size=26>{1} {2}</size>", Language.main.Get("CameraDroneDistance"), (distance >= 0) ? IntStringCache.GetStringForInt(distance) : "--", Language.main.Get("MeterSuffix"));
		}

		public void UpdateHealth()
		{
			Player.main.GetModVehicle().GetHUDValues(out float num, out float num2);
			int num3 = Mathf.CeilToInt(num * 100f);
			if (lastHealth != num3)
			{
				lastHealth = num3;
				textHealth.text = IntStringCache.GetStringForInt(lastHealth);
			}
		}
		public void UpdateTemperature()
		{
			float temperature = Player.main.GetModVehicle().GetTemperature();
			temperatureSmoothValue = ((temperatureSmoothValue < -10000f) ? temperature : Mathf.SmoothDamp(temperatureSmoothValue, temperature, ref temperatureVelocity, 1f));
			int num5 = Mathf.CeilToInt(temperatureSmoothValue);
			if (lastTemperature != num5)
			{
				lastTemperature = num5;
				textTemperature.text = IntStringCache.GetStringForInt(lastTemperature);
				textTemperatureSuffix.text = Language.main.GetFormat("ThermometerFormat");
			}
		}
		public void UpdatePower()
		{
			Player.main.GetModVehicle().GetHUDValues(out float num, out float num2);
			int num4 = Mathf.CeilToInt(num2 * 100f);
			if (lastPower != num4)
			{
				lastPower = num4;
				textPower.text = IntStringCache.GetStringForInt(lastPower);
			}
		}
		public void UpdateStorage()
		{
			Player.main.GetModVehicle().GetStorageValues(out int stored, out int capacity);
			if (capacity > 0)
			{
				int ratio = (100 * stored) / capacity;
				textStorage.text = ratio.ToString();
			}
			else
			{
				textStorage.text = 100.ToString();
			}
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
		[AssertNotNull]
		public TextMeshProUGUI textStorage;
		public int lastHealth = int.MinValue;
		public int lastPower = int.MinValue;
		public int lastTemperature = int.MinValue;
		public float temperatureSmoothValue = float.MinValue;
		public float temperatureVelocity;
		[AssertLocalization]
		public const string thermometerFormatKey = "ThermometerFormat";
	}
}
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
			bool flag = mv != null && (pda == null || !pda.isInUse);
			if (this.root.activeSelf != flag)
			{
				this.root.SetActive(flag);
				droneHUD.SetActive(flag && (VehicleTypes.Drone.mountedDrone != null));
			}
			if (flag)
			{
				float num;
				float num2;
				mv.GetHUDValues(out num, out num2);
				float temperature = mv.GetTemperature();
				int num3 = Mathf.CeilToInt(num * 100f);
				if (this.lastHealth != num3)
				{
					this.lastHealth = num3;
					this.textHealth.text = IntStringCache.GetStringForInt(this.lastHealth);
				}
				int num4 = Mathf.CeilToInt(num2 * 100f);
				if (this.lastPower != num4)
				{
					this.lastPower = num4;
					this.textPower.text = IntStringCache.GetStringForInt(this.lastPower);
				}
				this.temperatureSmoothValue = ((this.temperatureSmoothValue < -10000f) ? temperature : Mathf.SmoothDamp(this.temperatureSmoothValue, temperature, ref this.temperatureVelocity, 1f));
				int num5 = Mathf.CeilToInt(this.temperatureSmoothValue);
				if (this.lastTemperature != num5)
				{
					this.lastTemperature = num5;
					this.textTemperature.text = IntStringCache.GetStringForInt(this.lastTemperature);
					this.textTemperatureSuffix.text = Language.main.GetFormat("ThermometerFormat");
				}
				if ((VehicleTypes.Drone.mountedDrone != null))
				{
					DroneUpdate();
				}
			}
		}
		public void DroneUpdate()
		{
			VehicleTypes.Drone drone = VehicleTypes.Drone.mountedDrone;
			int distance = Mathf.CeilToInt(Vector3.Distance(drone.transform.position, drone.pairedStation.transform.position));
			droneHUD.transform.Find("Title/DistanceText").gameObject.GetComponent<TextMeshProUGUI>().text = string.Format("<color=#6EFEFFFF>{0}</color> <size=26>{1} {2}</size>", Language.main.Get("CameraDroneDistance"), (distance >= 0) ? IntStringCache.GetStringForInt(distance) : "--", Language.main.Get("MeterSuffix"));
			if (drone.IsConnecting)
			{
				droneHUD.transform.Find("Connecting").gameObject.SetActive(true);
			}
            else
			{
				droneHUD.transform.Find("Connecting").gameObject.SetActive(false);
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
		public int lastHealth = int.MinValue;
		public int lastPower = int.MinValue;
		public int lastTemperature = int.MinValue;
		public float temperatureSmoothValue = float.MinValue;
		public float temperatureVelocity;
		[AssertLocalization]
		public const string thermometerFormatKey = "ThermometerFormat";
	}
}
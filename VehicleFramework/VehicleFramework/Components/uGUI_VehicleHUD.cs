using System;
using UnityEngine;
using UnityEngine.UI;


namespace VehicleFramework
{
	public class uGUI_VehicleHUD : MonoBehaviour
	{
		public void Update()
		{
			ModVehicle vehicle = null;
			PDA pda = null;
			Player main = Player.main;
			if (main != null)
			{
				vehicle = main.GetVehicle() as ModVehicle;
				pda = main.GetPDA();
			}
			bool flag = vehicle != null && (pda == null || !pda.isInUse);
			if (this.root.activeSelf != flag)
			{
				this.root.SetActive(flag);
			}
			if (flag)
			{
				float num;
				float num2;
				vehicle.GetHUDValues(out num, out num2);
				float temperature = vehicle.GetTemperature();
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
			}
		}

		public const float temperatureSmoothTime = 1f;

		public GameObject root;

		public Text textHealth;

		public Text textPower;

		public Text textTemperature;

		public Text textTemperatureSuffix;

		private int lastHealth = int.MinValue;

		private int lastPower = int.MinValue;

		private int lastTemperature = int.MinValue;

		private float temperatureSmoothValue = float.MinValue;

		private float temperatureVelocity;
	}
}
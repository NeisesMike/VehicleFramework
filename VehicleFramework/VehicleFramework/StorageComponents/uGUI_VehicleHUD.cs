using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace VehicleFramework
{
	public class uGUI_VehicleHUD : MonoBehaviour
	{
		public void Update()
		{
			ModVehicle mv = null;
			PDA pda = null;
			Player main = Player.main;
			if (main != null)
			{
				mv = (main.GetVehicle() as ModVehicle);
				pda = main.GetPDA();
			}
			bool flag = mv != null && (pda == null || !pda.isInUse);
			if (this.root.activeSelf != flag)
			{
				this.root.SetActive(flag);
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
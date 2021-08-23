using System;
using UnityEngine;
using UnityEngine.UI;


namespace AtramaVehicle
{
	public class uGUI_AtramaHUD : MonoBehaviour
	{
		public void Update()
		{
			AtramaVehicle atrama = null;
			PDA pda = null;
			Player main = Player.main;
			if (main != null)
			{
				atrama = main.GetVehicle() as AtramaVehicle;
				pda = main.GetPDA();
			}
			bool flag = atrama != null && (pda == null || !pda.isInUse);
			if (this.root.activeSelf != flag)
			{
				this.root.SetActive(flag);
			}
			if (flag)
			{
				float num;
				float num2;
				atrama.GetHUDValues(out num, out num2);
				float temperature = atrama.GetTemperature();
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
		public Text textHealth;

		[AssertNotNull]
		public Text textPower;

		[AssertNotNull]
		public Text textTemperature;

		[AssertNotNull]
		public Text textTemperatureSuffix;

		private int lastHealth = int.MinValue;

		private int lastPower = int.MinValue;

		private int lastTemperature = int.MinValue;

		private float temperatureSmoothValue = float.MinValue;

		private float temperatureVelocity;
	}
}
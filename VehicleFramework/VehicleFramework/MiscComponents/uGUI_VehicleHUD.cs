using UnityEngine;
using TMPro;
using VehicleFramework.VehicleTypes;
using VehicleFramework.StorageComponents;
using VehicleFramework.Extensions;
using VehicleFramework.Assets;


namespace VehicleFramework.MiscComponents
{
	public class UGUI_VehicleHUD : MonoBehaviour
	{
		public enum HUDChoice
        {
			Normal,
			Storage
        }
		public GameObject? droneHUD = null;
		private bool IsStorageHUD()
		{
			return textStorage != null;
		}
		private static bool HasMvStorage(ModVehicle mv)
		{
			return mv.InnateStorages != null || ModularStorageInput.GetAllModularStorageContainers(mv).Count > 0;
		}

		private void DeactivateAll()
		{
			if(root == null)
			{
				throw Admin.SessionManager.Fatal("uGUI_VehicleHUD: root is null!");
            }
            root.SetActive(false);
			if(droneHUD == null)
			{
				throw Admin.SessionManager.Fatal("uGUI_VehicleHUD: droneHUD is null!");
            }
			droneHUD.SetActive(false);
		}
		private bool ShouldIDie(ModVehicle? mv, PDA? pda)
		{
			if (mv == null || pda == null)
			{
				// show nothing if we're not in an MV
				// or if PDA isn't available
				return true;
			}

            if (IsStorageHUD())
            {
                if (HasMvStorage(mv))
				{
					switch (VehicleConfig.GetConfig(mv).HUDChoice.Value)
					{
						case HUDChoice.Normal:
							// I'm the storage HUD, and I can be displayed, but the user wants the normal HUD. I should die.
							return true;
						case HUDChoice.Storage:
							// I'm the storage HUD, and I can be displayed, and the user wants me. I should live.
							return false;
					}
				}
                else
				{
					// I'm the storage HUD, but I can't be displayed. I should die.
					return true;
				}
            }
            else
			{
				switch (VehicleConfig.GetConfig(mv).HUDChoice.Value)
				{
					case HUDChoice.Normal:
						// I'm the normal HUD, and the user wants me. I should live
						return false;
					case HUDChoice.Storage:
						// I'm the normal HUD, but the user wants storage. I should die if it is available.
						return HasMvStorage(mv);
				}
			}

			return true;
		}
        public void Update()
		{
			if (Player.main == null)
			{
				DeactivateAll();
				return;
			}
			ModVehicle? mv = Player.main.GetModVehicle();
			PDA? pda = Player.main.GetPDA();
			if (ShouldIDie(mv, pda))
			{
				DeactivateAll();
				return;
			}

            if (root == null)
            {
                throw Admin.SessionManager.Fatal("uGUI_VehicleHUD: root is null!");
            }
            root.transform.localPosition = Vector3.zero;

			bool mvflag = !pda.isInUse;
			bool droneflag = mvflag && (VehicleTypes.Drone.MountedDrone != null);
			if (root.activeSelf != mvflag)
			{
				root.SetActive(mvflag);
            }
            if (droneHUD == null)
            {
                throw Admin.SessionManager.Fatal("uGUI_VehicleHUD: droneHUD is null!");
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
            if (droneHUD == null)
            {
                throw Admin.SessionManager.Fatal("uGUI_VehicleHUD: droneHUD is null!");
            }
            VehicleTypes.Drone? drone = VehicleTypes.Drone.MountedDrone;
            if (drone == null)
            {
                droneHUD.transform.Find("Connecting").gameObject.SetActive(false);
                return;
            }
			if (drone.IsConnecting)
			{
				droneHUD.transform.Find("Connecting").gameObject.SetActive(true);
			}
			else
			{
				droneHUD.transform.Find("Connecting").gameObject.SetActive(false);
			}
			if(drone.pairedStation == null)
			{
				throw Admin.SessionManager.Fatal("uGUI_VehicleHUD: drone.pairedStation is null!");
            }
			int distance = Mathf.CeilToInt(Vector3.Distance(drone.transform.position, drone.pairedStation.transform.position));
			droneHUD.transform.Find("Title/DistanceText").gameObject.GetComponent<TextMeshProUGUI>().text = string.Format("<color=#6EFEFFFF>{0}</color> <size=26>{1} {2}</size>", Language.main.Get("CameraDroneDistance"), (distance >= 0) ? IntStringCache.GetStringForInt(distance) : "--", Language.main.Get("MeterSuffix"));
		}
		public void UpdateHealth()
		{
			ModVehicle? mv = Player.main.GetModVehicle();
            if (mv == null)
			{
				return;
			}
            mv.GetHUDValues(out float num, out float _);
			int num3 = Mathf.CeilToInt(num * 100f);
			if (lastHealth != num3)
			{
				lastHealth = num3;
                if (textHealth == null)
                {
                    throw Admin.SessionManager.Fatal("uGUI_VehicleHUD: textHealth is null!");
                }
                textHealth.text = IntStringCache.GetStringForInt(lastHealth);
			}
		}
		public void UpdateTemperature()
        {
            ModVehicle? mv = Player.main.GetModVehicle();
            if (mv == null)
            {
                return;
            }
            float temperature = mv.GetTemperature();
			temperatureSmoothValue = ((temperatureSmoothValue < -10000f) ? temperature : Mathf.SmoothDamp(temperatureSmoothValue, temperature, ref temperatureVelocity, 1f));
			int tempNum;
			if (MainPatcher.NautilusConfig.IsFahrenheit)
			{
				tempNum = Mathf.CeilToInt(temperatureSmoothValue * 1.8f + 32);
			}
			else
			{
				tempNum = Mathf.CeilToInt(temperatureSmoothValue);
			}
			if (lastTemperature != tempNum)
			{
				lastTemperature = tempNum;
                if (textTemperature == null)
                {
                    throw Admin.SessionManager.Fatal("uGUI_VehicleHUD: textTemperature is null!");
                }
                if (textTemperatureSuffix == null)
                {
                    throw Admin.SessionManager.Fatal("uGUI_VehicleHUD: textTemperatureSuffix is null!");
                }
                textTemperature.text = IntStringCache.GetStringForInt(lastTemperature);
				textTemperatureSuffix.color = new Color32(byte.MaxValue, 220, 0, byte.MaxValue);
				if (MainPatcher.NautilusConfig.IsFahrenheit)
				{
					textTemperatureSuffix.text = "°F";
				}
				else
				{
					textTemperatureSuffix.text = Language.main.GetFormat("ThermometerFormat");
				}
			}
		}
		public void UpdatePower()
        {
            ModVehicle? mv = Player.main.GetModVehicle();
            if (mv == null)
            {
                return;
            }
            mv.GetHUDValues(out float _, out float num2);
			int num4 = Mathf.CeilToInt(num2 * 100f);
			if (lastPower != num4)
			{
				lastPower = num4;
                if (textPower == null)
                {
                    throw Admin.SessionManager.Fatal("uGUI_VehicleHUD: textPower is null!");
                }
                textPower.text = IntStringCache.GetStringForInt(lastPower);
			}
		}
		public void UpdateStorage()
		{
			if (textStorage == null)
			{
				return;
            }
            ModVehicle? mv = Player.main.GetModVehicle();
            if (mv == null)
            {
                return;
            }
            mv.GetStorageValues(out int stored, out int capacity);
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
		public GameObject? root;
		[AssertNotNull]
		public TextMeshProUGUI? textHealth;
        [AssertNotNull]
		public TextMeshProUGUI? textPower;
        [AssertNotNull]
		public TextMeshProUGUI? textTemperature;
        [AssertNotNull]
		public TextMeshProUGUI? textTemperatureSuffix;
        [AssertNotNull]
		public TextMeshProUGUI textStorage = null!;
		public int lastHealth = int.MinValue;
		public int lastPower = int.MinValue;
		public int lastTemperature = int.MinValue;
		public float temperatureSmoothValue = float.MinValue;
		public float temperatureVelocity;
		[AssertLocalization]
		public const string thermometerFormatKey = "ThermometerFormat";
    }
}
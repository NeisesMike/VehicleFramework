using UnityEngine;
using TMPro;
using VehicleFramework.VehicleChildComponents;
using VehicleFramework.Extensions;
using VehicleFramework.Admin;

namespace VehicleFramework.MiscComponents
{
	public class UGUI_VehicleHUD : MonoBehaviour
	{
		private bool validated = false;

		// Cached references
		private Transform _droneConnecting = null!;
		private TextMeshProUGUI _droneDistanceText = null!;
		private PDA? _pda;
		private ModVehicle? _mv;
		private string _labelDroneDistance = "";
		private string _labelMeterSuffix = "";

		public enum HUDChoice
		{
			Normal,
			Storage
		}
		public GameObject droneHUD = null!;

		internal void Validate()
		{
			if (root == null)
			{
				throw Admin.SessionManager.Fatal("uGUI_VehicleHUD: root is null!");
			}
			if (droneHUD == null)
			{
				throw Admin.SessionManager.Fatal("uGUI_VehicleHUD: droneHUD is null!");
			}
			_droneConnecting = droneHUD.transform.Find("Connecting");
			if (_droneConnecting == null)
			{
				throw Admin.SessionManager.Fatal("uGUI_VehicleHUD: droneHUD.Connecting is null!");
			}
			_droneDistanceText = droneHUD.transform.Find("Title/DistanceText").GetComponent<TextMeshProUGUI>();
			if (_droneDistanceText == null)
			{
				throw Admin.SessionManager.Fatal("uGUI_VehicleHUD: droneHUD.DistanceText is null!");
			}
			if (textPower == null)
			{
				throw Admin.SessionManager.Fatal("uGUI_VehicleHUD: textPower is null!");
			}
			if (textHealth == null)
			{
				throw Admin.SessionManager.Fatal("uGUI_VehicleHUD: textHealth is null!");
			}
			if (textTemperature == null)
			{
				throw Admin.SessionManager.Fatal("uGUI_VehicleHUD: textTemperature is null!");
			}
			if (textTemperatureSuffix == null)
			{
				throw Admin.SessionManager.Fatal("uGUI_VehicleHUD: textTemperatureSuffix is null!");
			}
			_labelDroneDistance = Language.main.Get("CameraDroneDistance");
			_labelMeterSuffix = Language.main.Get("MeterSuffix");
			root.transform.localPosition = Vector3.zero; // do this once
			validated = true;
		}
		private void Start()
		{
			if (!validated)
			{
				throw SessionManager.Fatal("uGUI_VehicleHUD: Not validated in Start!");
			}
		}
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
			root.SetActive(false);
			droneHUD.SetActive(false);
		}
		private bool ShouldIDie(ModVehicle mv, PDA? pda)
		{
			if (pda == null)
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
			var player = Player.main;
			if (player == null) { DeactivateAll(); return; }

			_mv = player.GetModVehicle();
			_pda = player.GetPDA();

			if (_mv == null || ShouldIDie(_mv, _pda))
			{
				DeactivateAll();
				return;
			}
			root.transform.localPosition = Vector3.zero;

			bool mvflag = !_pda.isInUse;
			bool droneflag = mvflag && (VehicleTypes.Drone.MountedDrone != null);
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
				UpdateHealth(_mv);
				UpdatePower(_mv);
				UpdateTemperature(_mv);
				UpdateStorage(_mv);
			}
			if (droneflag)
			{
				DroneUpdate();
			}
		}
		public void DroneUpdate()
		{
			VehicleTypes.Drone? drone = VehicleTypes.Drone.MountedDrone;
			if (drone == null)
			{
				_droneConnecting.gameObject.SetActive(false);
				return;
			}
			if (drone.IsConnecting)
			{
				_droneConnecting.gameObject.SetActive(true);
			}
			else
			{
				_droneConnecting.gameObject.SetActive(false);
			}
			if (drone.pairedStation == null)
			{
				throw Admin.SessionManager.Fatal("uGUI_VehicleHUD: drone.pairedStation is null!");
			}
			int distance = Mathf.CeilToInt(Vector3.Distance(drone.transform.position, drone.pairedStation.transform.position));
			_droneDistanceText.text = string.Format("<color=#6EFEFFFF>{0}</color> <size=26>{1} {2}</size>", _labelDroneDistance, (distance >= 0) ? IntStringCache.GetStringForInt(distance) : "--", _labelMeterSuffix);
		}
		public void UpdateHealth(ModVehicle mv)
		{
			mv.GetHUDValues(out float num, out float _);
			int num3 = Mathf.CeilToInt(num * 100f);
			if (lastHealth != num3)
			{
				lastHealth = num3;
				textHealth.text = IntStringCache.GetStringForInt(lastHealth);
			}
		}
		public void UpdateTemperature(ModVehicle mv)
		{
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
		public void UpdatePower(ModVehicle mv)
		{
			mv.GetHUDValues(out float _, out float num2);
			int num4 = Mathf.CeilToInt(num2 * 100f);
			if (lastPower != num4)
			{
				lastPower = num4;
				textPower.text = IntStringCache.GetStringForInt(lastPower);
			}
		}
		public void UpdateStorage(ModVehicle mv)
		{
			if (textStorage == null) return;
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
		public GameObject root = null!;
		[AssertNotNull]
		public TextMeshProUGUI textHealth = null!;
		[AssertNotNull]
		public TextMeshProUGUI textPower = null!;
		[AssertNotNull]
		public TextMeshProUGUI textTemperature = null!;
		[AssertNotNull]
		public TextMeshProUGUI textTemperatureSuffix = null!;
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
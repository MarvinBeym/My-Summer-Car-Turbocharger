using HutongGames.PlayMaker.Actions;
using MSCLoader;
using MscModApi.Caching;
using UnityEngine;

namespace SatsumaTurboCharger
{
	public static class TransmissionHandler
	{
		private enum TransmissionType
		{
			FWD,
			RWD,
			AWD,
			IGNORE
		}

		private static SettingsDropDownList changeTransmission;
		private static string[] availableOptions = {
			TransmissionType.IGNORE.ToString(),
			TransmissionType.FWD.ToString(),
			TransmissionType.RWD.ToString(),
			TransmissionType.AWD.ToString(),
		};

		public static void SetupSettings(Mod mod)
		{
			if (changeTransmission != null)
			{
				return;
			}

			Settings.AddHeader("Change Car Transmission type", Color.clear);
			changeTransmission = Settings.AddDropDownList("changeTransmission", "Change Car Transmission type (Ignore disabled logic, allowing other mods to change)", availableOptions, 0, () =>
			{
				if (changeTransmission != null)
				{
					SetTransmission(StringToTransmissionEnum(availableOptions[changeTransmission.GetSelectedItemIndex()]));
				}
			});
		}

		private static bool TryGetGameTransmission(TransmissionType transmissionType, out Drivetrain.Transmissions gameTransmission)
		{
			switch (transmissionType)
			{
				case TransmissionType.FWD:
					gameTransmission = Drivetrain.Transmissions.FWD;
					return true;
				case TransmissionType.RWD:
					gameTransmission = Drivetrain.Transmissions.RWD;
					return true;
				case TransmissionType.AWD:
					gameTransmission = Drivetrain.Transmissions.AWD;
					return true;
				default:
					gameTransmission = Drivetrain.Transmissions.FWD;
					return false;
			}
		}

		private static void SetTransmission(TransmissionType transmissionType)
		{
			Drivetrain drivetrain = CarH.drivetrain;
			Drivetrain.Transmissions gameTransmission;
			if (drivetrain == null || !TryGetGameTransmission(transmissionType, out gameTransmission))
			{
				return;
			}

			//Keep drivetrain.transmission in sync
			drivetrain.transmission = gameTransmission;
			drivetrain.SetTransmission(gameTransmission);
		}

		private static TransmissionType StringToTransmissionEnum(string value)
		{
			if (value == TransmissionType.FWD.ToString())
			{
				return TransmissionType.FWD;
			}

			if (value == TransmissionType.RWD.ToString())
			{
				return TransmissionType.RWD;
			}

			if (value == TransmissionType.AWD.ToString())
			{
				return TransmissionType.AWD;
			}

			return TransmissionType.FWD;
		}


		public static void Handle()
		{
		    string transmissionToSet = availableOptions[changeTransmission.GetSelectedItemIndex()];
		    if (transmissionToSet == TransmissionType.IGNORE.ToString())
		    {
			    return;
		    }
			Drivetrain drivetrain = CarH.drivetrain;
			Drivetrain.Transmissions gameTransmission;
			TransmissionType transmissionType = StringToTransmissionEnum(transmissionToSet);
			//Only apply when it differs (SetTransmission resets wheel values)
			if (drivetrain == null || !TryGetGameTransmission(transmissionType, out gameTransmission) || drivetrain.transmission == gameTransmission)
			{
				return;
			}

			SetTransmission(transmissionType);
		}
	}
}
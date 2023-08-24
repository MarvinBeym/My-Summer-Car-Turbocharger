using HutongGames.PlayMaker.Actions;
using MSCLoader;
using MscModApi.Caching;

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
		private static string[] availableOptions = new[]
		{
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


			changeTransmission = Settings.AddDropDownList(mod, "changeTransmission", "Change Car Transmission type (Ignore disabled logic, allowing other mods to change)", availableOptions, 0, () =>
			{
				if (changeTransmission != null)
				{
					SetTransmission(StringToTransmissionEnum(availableOptions[changeTransmission.GetSelectedItemIndex()]));
				}
			});
		}

		private static void SetTransmission(TransmissionType transmissionType)
		{
			switch (transmissionType)
			{
				case TransmissionType.FWD:
					CarH.drivetrain.SetTransmission(Drivetrain.Transmissions.FWD);
					break;
				case TransmissionType.RWD:
					CarH.drivetrain.SetTransmission(Drivetrain.Transmissions.RWD);
					break;
				case TransmissionType.AWD:
					CarH.drivetrain.SetTransmission(Drivetrain.Transmissions.AWD);
					break;
			}
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
			SetTransmission(StringToTransmissionEnum(transmissionToSet));
		}
	}
}
using MSCLoader;
using MscModApi.Caching;
using UnityEngine;

namespace SatsumaTurboCharger
{
	public static class GearRatiosHandler
	{
		private enum GearRatioType
		{
			IGNORE,
			APPLY,
			ORIGINAL,
		}

		private static float[] originalGearRatios = {
			-4.093f,
			0f,
			3.673f,
			2.217f,
			1.448f,
			1f,
		};

		private static float[] newGearRatios = {
			-4.093f, // reverse
			0f, // neutral
			3.4f, // 1st
			1.8f, // 2nd
			1.4f, // 3rd
			1.0f, // 4th
			0.8f, // 5th
			0.65f // 6th
		};

		private static SettingsDropDownList changeGearRatios;
		private static string[] availableOptions = {
			GearRatioType.IGNORE.ToString(),
			GearRatioType.APPLY.ToString(),
			GearRatioType.ORIGINAL.ToString()
		};

		public static void SetupSettings(Mod mod)
		{

			if (changeGearRatios != null)
			{
				return;
			}

			Settings.AddHeader(mod, "Change Car Transmission type", Color.clear);
			changeGearRatios = Settings.AddDropDownList(mod, "changeGearRatios", "Change Car Gear ratios (IGNORE = disabled logic, allowing other mods to change)", availableOptions, 0, () =>
			{
				if (changeGearRatios != null)
				{
					Set(StringToEnum(availableOptions[changeGearRatios.GetSelectedItemIndex()]));
				}
			});
			Settings.AddText(mod, "Reapply by other mods may be required after setting this to 'IGNORE'");


			string gearRatioHelpText = "New Gear Ratios: \n";
			for (int i = 0; i < newGearRatios.Length; i++)
			{
				string gearName = i.ToString();
				if (i == 0)
				{
					gearName = "R";
				}

				if (i == 1)
				{
					gearName = "N";
				}
				gearRatioHelpText += $"{gearName}.Gear: {newGearRatios[i]}\n";
			}

			Settings.AddText(mod, gearRatioHelpText);
		}

		private static void Set(GearRatioType gearRatioType)
		{
			switch (gearRatioType)
			{
				case GearRatioType.APPLY:
					CarH.drivetrain.gearRatios = newGearRatios;
					break;
				case GearRatioType.ORIGINAL:
					CarH.drivetrain.gearRatios = originalGearRatios;
					break;
			}
		}

		private static GearRatioType StringToEnum(string value)
		{
			if (value == GearRatioType.APPLY.ToString())
			{
				return GearRatioType.APPLY;
			}

			if (value == GearRatioType.ORIGINAL.ToString())
			{
				return GearRatioType.ORIGINAL;
			}

			return GearRatioType.IGNORE;
		}

		public static void Handle()
		{
			string gearRatiosToSet = availableOptions[changeGearRatios.GetSelectedItemIndex()];
			if (gearRatiosToSet == GearRatioType.IGNORE.ToString())
			{
				return;
			}
			Set(StringToEnum(gearRatiosToSet));
		}
	}
}
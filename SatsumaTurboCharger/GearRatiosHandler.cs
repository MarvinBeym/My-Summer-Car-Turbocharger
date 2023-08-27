using MSCLoader;
using MscModApi.Caching;
using UnityEngine;

namespace SatsumaTurboCharger
{
	public static class GearRatiosHandler
	{
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

		public static SettingsCheckBox useCustomGearRatios;


		public static void SetupSettings(Mod mod)
		{

			if (useCustomGearRatios != null)
			{
				return;
			}

			Settings.AddHeader(mod, "Custom Gear Ratio", Color.clear);
			useCustomGearRatios = Settings.AddCheckBox(mod, "useCustomGearRatios", "Use custom gear ratios (adapted to help turbo speed)", false, () =>
			{
				CarH.drivetrain.gearRatios = useCustomGearRatios.GetValue() ? newGearRatios : originalGearRatios;
			});

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

		public static void Handle()
		{
			if (useCustomGearRatios.GetValue() && CarH.drivetrain.gearRatios != newGearRatios)
			{
				CarH.drivetrain.gearRatios = newGearRatios;
			} else if (!useCustomGearRatios.GetValue() && CarH.drivetrain.gearRatios == newGearRatios)
			{
				CarH.drivetrain.gearRatios = originalGearRatios;
			}
		}
	}
}
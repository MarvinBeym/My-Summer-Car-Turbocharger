using MscModApi.Caching;
using MscModApi.Parts;
using UnityEngine;

namespace SatsumaTurboCharger.part
{
	public class TurboSmallIntercoolerTube : DerivablePart
	{
		protected override string partId => "turboSmall-intercooler-tube";
		protected override string partName => "GT Turbo Intercooler Tube";
		protected override Vector3 partInstallPosition => new Vector3(0.316f, -0.041f, 1.518f);
		protected override Vector3 partInstallRotation => new Vector3(0, 180, 0);

		public TurboSmallIntercoolerTube() : base(CarH.satsuma, SatsumaTurboCharger.partBaseInfo)
		{
			AddClampModel(new Vector3(0.034f, -0.154f, -0.1548f),
				new Vector3(0, 90, 0), new Vector3(0.62f, 0.62f, 0.62f));
			AddClampModel(new Vector3(0.0225f, 0.24f, 0.313f), new Vector3(90, 0, 0),
				new Vector3(0.5f, 0.5f, 0.5f));
			AddScrews(new[]
			{
				new Screw(new Vector3(0.034f, -0.13f, -0.1638f), new Vector3(180f, 0f, 0f)),
				new Screw(new Vector3(0.014f, 0.24f, 0.332f), new Vector3(0f, -90f, 0f)),
			}, 0.4f);
		}
	}
}
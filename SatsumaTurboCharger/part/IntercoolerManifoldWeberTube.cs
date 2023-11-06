using MscModApi.Parts;
using UnityEngine;

namespace SatsumaTurboCharger.part
{
	public class IntercoolerManifoldWeberTube : DerivablePart
	{
		protected override string partId => "intercooler-manifold-weberCarb-tube";
		protected override string partName => "Weber Intercooler-Manifold Tube";
		protected override Vector3 partInstallPosition => new Vector3(0.365165f, -0.06377198f, -0.2655599f);
		protected override Vector3 partInstallRotation => new Vector3(17.3f, 0, 0);

		public IntercoolerManifoldWeberTube(ManifoldWeber parent) : base(parent, SatsumaTurboCharger.partBaseInfo)
		{
			AddClampModel(
				new Vector3(-0.053f, -0.2475f, -0.362f),
				new Vector3(0, 90, -90), new Vector3(0.65f, 0.65f, 0.65f));
			AddClampModel(
				new Vector3(-0.142f, 0.232f, 0.296f),
				new Vector3(0, 90, 0), new Vector3(0.65f, 0.65f, 0.65f));
			AddScrews(new[]
			{
				new Screw(new Vector3(-0.0530f, -0.2245f, -0.3870f), new Vector3(-90, 0, 0)),
				new Screw(new Vector3(-0.142f, 0.2571f, 0.2727f), new Vector3(0, 180, 0)),
			}, 0.4f, 8);
		}
		

	}
}
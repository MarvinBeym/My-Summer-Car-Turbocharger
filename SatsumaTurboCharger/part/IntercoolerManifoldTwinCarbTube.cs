using MscModApi.Parts;
using UnityEngine;

namespace SatsumaTurboCharger.part
{
	public class IntercoolerManifoldTwinCarbTube : DerivablePart
	{
		protected override string partId => "intercooler-manifold-twinCarb-tube";
		protected override string partName => "TwinCarb Intercooler-Manifold Tube";
		protected override Vector3 partInstallPosition => new Vector3(0.325294f, -0.180286f, -0.29813f);
		protected override Vector3 partInstallRotation => new Vector3(0, 0, 0);
		public IntercoolerManifoldTwinCarbTube(ManifoldTwinCarb parent) : base(parent, SatsumaTurboCharger.partBaseInfo)
		{ 
			AddClampModel(new Vector3(-0.027f, -0.144f, -0.312f),
				new Vector3(0, 90, 0), new Vector3(0.62f, 0.62f, 0.62f));
			AddScrew(new Screw(new Vector3(-0.027f, -0.1205f, -0.3352f),
				new Vector3(180, 0, 0), Screw.Type.Normal, 0.35f));
		}


	}
}
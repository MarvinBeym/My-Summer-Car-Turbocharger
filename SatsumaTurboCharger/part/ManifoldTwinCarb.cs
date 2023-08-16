using MscModApi.Caching;
using MscModApi.Parts;
using MscModApi.Parts.ReplacePart;
using MscModApi.Tools;
using UnityEngine;

namespace SatsumaTurboCharger.part
{
	public class ManifoldTwinCarb : DerivablePart
	{
		protected override string partId => "manifold-twinCarb";
		protected override string partName => "TwinCarb Manifold";
		protected override Vector3 partInstallPosition => new Vector3(-0.007944f, -0.13687f, -0.0326f);
		protected override Vector3 partInstallRotation => new Vector3(90, 0, 0);

		public ManifoldTwinCarb(GamePart parent) : base(parent, SatsumaTurboCharger.partBaseInfo)
		{
			AddClampModel(new Vector3(0.206f, -0.06f, 0.016f), new Vector3(0, 90, 0),
				new Vector3(0.8f, 0.8f, 0.65f));
			AddScrew(new Screw(new Vector3(0.206f, -0.029f, -0.0073f), new Vector3(0, -180, 0),
				Screw.Type.Normal, 0.4f));
		}
	}
}
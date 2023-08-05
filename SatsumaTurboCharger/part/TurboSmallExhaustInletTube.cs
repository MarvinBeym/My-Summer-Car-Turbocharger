using MscModApi.Caching;
using MscModApi.Parts;
using UnityEngine;

namespace SatsumaTurboCharger.part
{
	public class TurboSmallExhaustInletTube : DerivablePart
	{
		protected override string partId => "turboSmall-exhaust-inlet-tube";
		protected override string partName => "GT Turbo Exhaust Inlet Tube";
		protected override Vector3 partInstallPosition => new Vector3(-0.0918f, -0.1774f, -0.094f);
		protected override Vector3 partInstallRotation => new Vector3(90, 0, 0);

		public TurboSmallExhaustInletTube() : base(Cache.Find("cylinder head(Clone)"), SatsumaTurboCharger.partBaseInfo)
		{ 
			AddScrews(new[]
			{
				new Screw(new Vector3(0.114f, -0.044f, -0.035f), new Vector3(-90f, 0f, 0f)),
				new Screw(new Vector3(0.06f, -0.044f, -0.044f), new Vector3(-90f, 0f, 0f)),
			}, 0.7f);
		}


	}
}
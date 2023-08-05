using MscModApi.Caching;
using MscModApi.Parts;
using UnityEngine;

namespace SatsumaTurboCharger.part
{
	public class TurboBigExhaustInletTube : DerivablePart
	{
		protected override string partId => "turboBig-exhaust-inlet-tube";
		protected override string partName => "Racing Turbo Exhaust Inlet Tube";
		protected override Vector3 partInstallPosition => new Vector3(-0.234f, -0.02f, 0.099191f);
		protected override Vector3 partInstallRotation => new Vector3(0, 0, 0);
		
		public TurboBigExhaustInletTube(ExhaustHeader parent) : base(parent, SatsumaTurboCharger.partBaseInfo)
		{ 
			AddScrews(new[]
			{
				new Screw(new Vector3(0.262f, -0.041f, -0.05f), new Vector3(-90, 0, 0)),
				new Screw(new Vector3(0.205f, -0.041f, -0.05f), new Vector3(-90, 0, 0)),
			}, 0.6f);
		}
	}
}
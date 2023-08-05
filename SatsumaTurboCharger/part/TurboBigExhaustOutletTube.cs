using MscModApi.Caching;
using MscModApi.Parts;
using UnityEngine;

namespace SatsumaTurboCharger.part
{
	public class TurboBigExhaustOutletTube : DerivablePart
	{
		protected override string partId => "turboBig-exhaust-outlet-tube";
		protected override string partName => "Racing Turbo Exhaust Outlet Tube";
		protected override Vector3 partInstallPosition => new Vector3(-0.307337f, 1.044137f, 0.456764f);
		protected override Vector3 partInstallRotation => new Vector3(90, 0, 0);
		public TurboBigExhaustOutletTube() : base(Cache.Find("racing exhaust(Clone)"), SatsumaTurboCharger.partBaseInfo)
		{ 
			AddScrews(new[]
			{
				new Screw(new Vector3(0f, 0.206f, -0.0420f), new Vector3(0, 0, 0)),
				new Screw(new Vector3(-0.042f, 0.164f, -0.0420f), new Vector3(0, 0, 0)),
				new Screw(new Vector3(0f, 0.122f, -0.0420f), new Vector3(0, 0, 0)),
				new Screw(new Vector3(0.041f, 0.164f, -0.0420f), new Vector3(0, 0, 0)),
			}, 0.6f);
		}
	}
}
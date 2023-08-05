using MscModApi.Caching;
using MscModApi.Parts;
using UnityEngine;

namespace SatsumaTurboCharger.part
{
	public class TurboSmallExhaustOutletTube : DerivablePart
	{
		protected override string partId => "turboSmall-exhaust-outlet-tube";
		protected override string partName => "GT Turbo Exhaust Outlet Tube";
		protected override Vector3 partInstallPosition => new Vector3(-0.201f, 0.9963f, 0.2685f);
		protected override Vector3 partInstallRotation => new Vector3(90, 0, 0);

		public TurboSmallExhaustOutletTube() : base(Cache.Find("racing exhaust(Clone)"),
			SatsumaTurboCharger.partBaseInfo)
		{
			AddClampModel(new Vector3(-0.079f, 0.0897f, -0.0315f),
				new Vector3(0, 0, 0), new Vector3(0.83f, 0.83f, 0.83f));
			AddScrew(new Screw(new Vector3(0.0763f, -0.1631f, -0.2780f),
				new Vector3(0, -90, 0), Screw.Type.Normal, 0.32f));
		}
	}
}
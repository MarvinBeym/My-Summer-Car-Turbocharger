using MscModApi.Caching;
using MscModApi.Parts;
using UnityEngine;

namespace SatsumaTurboCharger.part
{
	public class TurboSmallExhaustOutletTube : DerivablePart
	{
		protected override string partId => "turboSmall-exhaust-outlet-tube";
		protected override string partName => "GT Turbo Exhaust Outlet Tube";
		protected override Vector3 partInstallPosition => new Vector3(-0.1825f, -0.267f, -0.145f);
		protected override Vector3 partInstallRotation => new Vector3(90, 0, 0);

		public TurboSmallExhaustOutletTube() : base(Cache.Find("cylinder head(Clone)"),
			SatsumaTurboCharger.partBaseInfo)
		{
			AddClampModel(new Vector3(-0.068f, 0.1445f, -0.0235f),
				new Vector3(0, 0, 0), new Vector3(0.67f, 0.67f, 0.67f));
			AddScrew(new Screw(new Vector3(-0.078f, 0.1708f, -0.0235f),
				new Vector3(0, -90, 0), Screw.Type.Normal, 0.5f));
		}
	}
}
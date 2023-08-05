using MscModApi.Caching;
using MscModApi.Parts;
using UnityEngine;

namespace SatsumaTurboCharger.part
{
	public class TurboSmallAirfilter : DerivablePart
	{
		protected override string partId => "turboSmall-airfilter";
		protected override string partName => "GT Turbo Airfilter";
		protected override Vector3 partInstallPosition => new Vector3(-0.25f, -0.04f, 0.0001f);
		protected override Vector3 partInstallRotation => new Vector3(90, 0, 0);

		public TurboSmallAirfilter() : base(Cache.Find("cylinder head(Clone)"), SatsumaTurboCharger.partBaseInfo)
		{
			AddClampModel(new Vector3(0f, 0f, 0.049f), new Vector3(0, 0, 0),
				new Vector3(0.65f, 0.65f, 0.65f));
			AddScrew(new Screw(new Vector3(0.0095f, 0.025f, 0.0488f), new Vector3(0, 90, 0),
				Screw.Type.Normal, 0.4f));
		}


	}
}
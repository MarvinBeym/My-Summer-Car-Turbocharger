using MscModApi.Caching;
using MscModApi.Parts;
using UnityEngine;

namespace SatsumaTurboCharger.part
{
	public class TurboSmallAirfilter : DerivablePart
	{
		protected override string partId => "turboSmall-airfilter";
		protected override string partName => "GT Turbo Airfilter";
		protected override Vector3 partInstallPosition => new Vector3(0f, 0.02525f, -0.172f);
		protected override Vector3 partInstallRotation => new Vector3(-90, 0, 0);

		public TurboSmallAirfilter(TurboSmall parent) : base(parent, SatsumaTurboCharger.partBaseInfo)
		{
			AddClampModel(new Vector3(0f, -0.115f, -0.025f), new Vector3(90, 0, 0),
				new Vector3(1.02f, 1.02f, 1.02f));
			AddScrew(new Screw(new Vector3(0.0254f, -0.1150f, 0.0145f), new Vector3(0, 90, 0),
				Screw.Type.Normal, 0.5f));
		}


	}
}
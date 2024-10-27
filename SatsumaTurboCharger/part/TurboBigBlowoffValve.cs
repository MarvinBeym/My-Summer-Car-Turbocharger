using MscModApi.Parts;
using SatsumaTurboCharger.part;
using UnityEngine;

namespace SatsumaTurboCharger.part
{
	public class TurboBigBlowoffValve : DerivablePart
	{
		protected override string partId => "turboBig-blowoff-valve";
		protected override string partName => "Racing Turbo Blowoff Valve";
		protected override Vector3 partInstallPosition => new Vector3(0.026583f, 0.139111f, 0.15561f);
		protected override Vector3 partInstallRotation => new Vector3(0, 0, 45);

		public TurboBigBlowoffValve(TurboBigIntercoolerTube parent) : base(parent, SatsumaTurboCharger.partBaseInfo)
		{
			AddClampModel(new Vector3(0f, -0.04f, 0f), new Vector3(90, 90, 0),
				new Vector3(0.43f, 0.43f, 0.43f));
			AddScrew(new Screw(new Vector3(0.0163f, -0.04f, 0.0218f), new Vector3(0, 0, 0),
				Screw.Type.Normal, 0.3f, 6));
		}
	}
}
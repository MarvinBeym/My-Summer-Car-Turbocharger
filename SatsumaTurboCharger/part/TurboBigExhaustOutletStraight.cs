using MscModApi.Parts;
using SatsumaTurboCharger.part;
using UnityEngine;

namespace SatsumaTurboCharger.part
{
	public class TurboBigExhaustOutletStraight : DerivablePart
	{
		protected override string partId => "turboBig-exhaust-outlet-straight-tube";
		protected override string partName => "Racing Turbo Exhaust Straight";
		protected override Vector3 partInstallPosition => new Vector3(-0.0185f, 0.0385f, 0.159f);
		protected override Vector3 partInstallRotation => new Vector3(0, 0, 0);

		public TurboBigExhaustOutletStraight(TurboBig parent) : base(parent, SatsumaTurboCharger.partBaseInfo)
		{ 
			AddScrews(new[]
			{
				new Screw(new Vector3(0.016f, 0.0015f, -0.0358f), new Vector3(0, 0, 0)),
				new Screw(new Vector3(0.057f, -0.03925f, -0.0358f), new Vector3(0, 0, 0)),
				new Screw(new Vector3(0.016f, -0.08f, -0.0358f), new Vector3(0, 0, 0)),
				new Screw(new Vector3(-0.02505f, -0.03925f, -0.0358f), new Vector3(0, 0, 0))
			}, 0.6f);
		}


	}
}
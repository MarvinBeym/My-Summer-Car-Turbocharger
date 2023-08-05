using MscModApi.Caching;
using MscModApi.Parts;
using UnityEngine;

namespace SatsumaTurboCharger.part
{
	public class TurboSmallExhaustInletTube : DerivablePart
	{
		protected override string partId => "turboSmall-exhaust-inlet-tube";
		protected override string partName => "GT Turbo Exhaust Inlet Tube";
		protected override Vector3 partInstallPosition => new Vector3(-0.1342f, -0.124f, 0.1565f);
		protected override Vector3 partInstallRotation => new Vector3(0, 0, 0);

		public TurboSmallExhaustInletTube(ExhaustHeader parent) : base(parent, SatsumaTurboCharger.partBaseInfo)
		{ 
			AddScrews(new[]
			{
				new Screw(new Vector3(0.1640f, 0.0622f, -0.1080f), new Vector3(-90f, 0f, 0f)),
				new Screw(new Vector3(0.1050f, 0.0622f, -0.1080f), new Vector3(-90f, 0f, 0f)),
			}, 0.7f);
		}


	}
}
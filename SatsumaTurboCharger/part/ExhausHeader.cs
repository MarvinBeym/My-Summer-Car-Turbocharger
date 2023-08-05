using MscModApi.Caching;
using MscModApi.Parts;
using UnityEngine;

namespace SatsumaTurboCharger.part
{
	public class ExhaustHeader : DerivablePart
	{
		protected override string partId => "exhaust-header";
		protected override string partName => "Turbo Exhaust Header";
		protected override Vector3 partInstallPosition => new Vector3(-0.005f, -0.089f, -0.0658f);
		protected override Vector3 partInstallRotation => new Vector3(90, 0, 0);

		public ExhaustHeader() : base(Cache.Find("cylinder head(Clone)"), SatsumaTurboCharger.partBaseInfo)
		{ 
			AddScrews(new[]
			{
				new Screw(new Vector3(0.1689f, 0.0779f, -0.00636f), new Vector3(0, 0, 0), Screw.Type.Nut),
				new Screw(new Vector3(0.1306f, 0.0314f, -0.00746f), new Vector3(0, 0, 0), Screw.Type.Nut),
				new Screw(new Vector3(-0.0028f, 0.0817f, -0.00746f), new Vector3(0, 0, 0), Screw.Type.Nut),
				new Screw(new Vector3(-0.137f, 0.0317f, -0.00746f), new Vector3(0, 0, 0), Screw.Type.Nut),
				new Screw(new Vector3(-0.173f, 0.0779f, -0.00746f), new Vector3(0, 0, 0), Screw.Type.Nut),
			}, 0.7f);
		}
	}
}
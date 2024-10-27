using MscModApi.Caching;
using MscModApi.Parts;
using UnityEngine;

namespace SatsumaTurboCharger.part
{
	public class TurboSmallIntercoolerTube : DerivablePart
	{
		protected override string partId => "turboSmall-intercooler-tube";
		protected override string partName => "GT Turbo Intercooler Tube";
		protected override Vector3 partInstallPosition => new Vector3(-0.3543f, 0.1301f, 0.2698f);
		protected override Vector3 partInstallRotation => new Vector3(5, 0, 0);

		public TurboSmallIntercoolerTube(Intercooler parent) : base(parent, SatsumaTurboCharger.partBaseInfo)
		{
			AddClampModel(new Vector3(0.0765f, -0.187f, -0.2548f),
				new Vector3(0, 90, 0), new Vector3(0.62f, 0.62f, 0.62f));
			
			AddScrews(new[]
			{
				new Screw(new Vector3(0.0763f, -0.163f,-0.2778f), new Vector3(180f, 0f, 0f)),
			}, 0.4f, 8);
		}
	}
}
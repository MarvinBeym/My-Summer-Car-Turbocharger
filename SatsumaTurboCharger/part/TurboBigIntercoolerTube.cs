using MscModApi.Parts;
using UnityEngine;

namespace SatsumaTurboCharger.part
{
	public class TurboBigIntercoolerTube : DerivablePart
	{
		protected override string partId => "turboBig-intercooler-tube";
		protected override string partName => "Racing Turbo Intercooler Tube";
		protected override Vector3 partInstallPosition => new Vector3(-0.346378f, 0.178461f, 0.2662498f);
		protected override Vector3 partInstallRotation => new Vector3(5, 0, 0);

		public TurboBigIntercoolerTube(Intercooler parent) : base(parent, SatsumaTurboCharger.partBaseInfo)
		{ 
			AddClampModel(new Vector3(0.065f, -0.235f, -0.2475f),
				new Vector3(0, 90, -90), new Vector3(0.68f, 0.68f, 0.68f));
			AddScrew(new Screw(new Vector3(0.0645f, -0.2120f, -0.2730f),
				new Vector3(-90, 0, 0), Screw.Type.Normal, 0.4f));
		}



	}
}
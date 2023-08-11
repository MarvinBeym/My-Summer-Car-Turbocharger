using MscModApi.Caching;
using MscModApi.PaintingSystem;
using MscModApi.Parts;
using UnityEngine;

namespace SatsumaTurboCharger.part
{
	public class TurboBigHood : DerivablePart
	{
		protected override string partId => "turboBig-hood";
		protected override string partName => "Racing Turbo Hood";
		protected override Vector3 partInstallPosition => new Vector3(0, 0.2408085f, 1.68f);
		protected override Vector3 partInstallRotation => new Vector3(0, 180, 0);

		public TurboBigHood() : base(CarH.satsuma, SatsumaTurboCharger.partBaseInfo)
		{ 
			AddScrews(new[]
			{
				new Screw(new Vector3(-0.4075f, 0.0353f, 0.0303f), new Vector3(250, 0, 0)),
				new Screw(new Vector3(-0.4075f, 0.0254f, 0.0032f), new Vector3(250, 0, 0)),
				new Screw(new Vector3(0.4075f, 0.0353f, 0.0303f), new Vector3(250, 0, 0)),
				new Screw(new Vector3(0.4075f, 0.0254f, 0.0032f), new Vector3(250, 0, 0)),
			}, 0.6f);
			
			PaintingSystem
				.Setup(partBaseInfo.mod, this)
				.ApplyMaterial("CAR_PAINT_REGULAR");
			HoodLogic hoodLogic = this.AddComponent<HoodLogic>();
			//hoodLogic.Init(turboBig_hood_part);
		}
	}
}
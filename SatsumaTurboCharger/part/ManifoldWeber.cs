using MscModApi.Caching;
using MscModApi.Parts;
using MscModApi.Tools;
using UnityEngine;

namespace SatsumaTurboCharger.part
{
	public class ManifoldWeber : DerivablePart
	{
		protected override string partName => "Weber Manifold";
		protected override string partId => "manifold-weberCarb";
		protected override Vector3 partInstallPosition => new Vector3(-0.009812f, -0.12167f, 0.031688f);
		protected override Vector3 partInstallRotation => new Vector3(72.5f, 0, 0);

		public ManifoldWeber() : base(Helper.GetGameObjectFromFsm(Cache.Find("Racing Carburators")), SatsumaTurboCharger.partBaseInfo)
		{
			AddClampModel(new Vector3(0.1275f, -0.0009f, -0.02f), new Vector3(0, 0, 0),
				new Vector3(0.59f, 0.59f, 0.59f));
			AddClampModel(new Vector3(0.051f, -0.0009f, -0.02f), new Vector3(0, 0, 0),
				new Vector3(0.59f, 0.59f, 0.59f));
			AddClampModel(new Vector3(-0.047f, -0.0009f, -0.02f), new Vector3(0, 0, 0),
				new Vector3(0.59f, 0.59f, 0.59f));
			AddClampModel(new Vector3(-0.1235f, -0.0009f, -0.02f), new Vector3(0, 0, 0),
				new Vector3(0.59f, 0.59f, 0.59f));
			AddScrews(new[]
			{
				new Screw(new Vector3(-0.1006f, 0.0219f, -0.0200f), new Vector3(0, 90, 0)),
				new Screw(new Vector3(-0.0240f, 0.0219f, -0.0200f), new Vector3(0, 90, 0)),
				new Screw(new Vector3(0.0740f, 0.0219f, -0.0200f), new Vector3(0, 90, 0)),
				new Screw(new Vector3(0.1504f, 0.0219f, -0.0200f), new Vector3(0, 90, 0)),
			}, 0.35f);
		}



	}
}
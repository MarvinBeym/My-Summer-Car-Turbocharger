using MscModApi.Caching;
using UnityEngine;

namespace SatsumaTurboCharger.part
{
	public class TurboExhaustHandler : MonoBehaviour
	{
		private GameObject fromMuffler;
		private GameObject fromHeaders;
		private GameObject fromPipe;
		private GameObject fromEngine;

		public void Start()
		{
			fromMuffler = Cache.Find("SATSUMA(557kg, 248)/CarSimulation/Exhaust/FromMuffler");
			fromHeaders = Cache.Find("SATSUMA(557kg, 248)/CarSimulation/Exhaust/FromHeaders");
			fromPipe = Cache.Find("SATSUMA(557kg, 248)/CarSimulation/Exhaust/FromPipe");
			fromEngine = Cache.Find("SATSUMA(557kg, 248)/CarSimulation/Exhaust/FromEngine");
		}

		public void LateUpdate()
		{

		}
	}
}
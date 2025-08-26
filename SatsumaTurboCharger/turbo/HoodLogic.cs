using MSCLoader;
using MscModApi.Caching;
using MscModApi.Parts;
using MscModApi.Tools;
using SatsumaTurboCharger.part;
using UnityEngine;
using static SatsumaTurboCharger.part.TurboBigHood;

namespace SatsumaTurboCharger
{
	public class HoodLogic : MonoBehaviour
	{
		private TurboBigHood turboHood;

		protected OpenState openState = OpenState.Closed;

		private float springSpeed = 24;
		private float springDamping = 12;


		public void Start()
		{
			Cache.Find("HoodLocking").transform.FindChild("Trigger").gameObject.FsmInject("Use", "Open", delegate ()
			{
				if (openState == OpenState.Open)
				{
					return;
				}


				SetHoodState(openState == OpenState.Closed ? OpenState.Lifted : OpenState.Closed);
			});
		}

		public void Init(TurboBigHood turboHood)
		{
			this.turboHood = turboHood;
		}

		void LateUpdate()
		{
			if (openState == OpenState.Closed || !turboHood.latchCollider.IsLookingAt())
			{
				return;
			}

			UserInteraction.GuiInteraction($"Press [{cInput.GetText("Use")}] to {(openState == OpenState.Lifted ? "Open" : "Close")} Hood");
			if (UserInteraction.UseButtonDown)
			{
				SetHoodState(openState == OpenState.Lifted ? OpenState.Open : OpenState.Lifted);
			}
		}

		public void SetHoodState(OpenState state)
		{
			JointSpring spring = turboHood.hingeJoint.spring;
			spring.spring = springSpeed;
			spring.damper = springDamping;
			spring.targetPosition = (float) state;
			turboHood.hingeJoint.spring = spring;

			openState = state;
		}
	}
}
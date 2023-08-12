using MSCLoader;
using MscModApi.Caching;
using MscModApi.Parts;
using MscModApi.Tools;
using UnityEngine;
using EventType = MscModApi.Parts.EventType;


namespace SatsumaTurboCharger
{
	public class HoodLogic : MonoBehaviour
	{
		protected enum OpenState
		{
			Closed = 0,
			Open = 87,
			Lifted = 2
		}

		private Part turboHood;
		private GameObject latchCollider;
		private HingeJoint hingeJoint;

		protected OpenState openState = OpenState.Closed;

		private float springSpeed = 24;
		private float springDamping = 12;


		public void Start()
		{
			FsmHook.FsmInject(Cache.Find("HoodLocking").transform.FindChild("Trigger").gameObject, "Open", delegate ()
			{
				if (openState == OpenState.Open)
				{
					return;
				}


				SetHoodState(openState == OpenState.Closed ? OpenState.Lifted : OpenState.Closed);
			});

			latchCollider = GameObject.CreatePrimitive(PrimitiveType.Cube);
			latchCollider.name = turboHood.name + "_LATCHCOLLIDER";
			latchCollider.transform.SetParent(turboHood.transform);
			latchCollider.GetComponent<Collider>().isTrigger = true;
			latchCollider.GetComponent<Renderer>().enabled = false;
			latchCollider.transform.localPosition = new Vector3(0f, 0.06f, 0.36f);
			latchCollider.transform.localEulerAngles = new Vector3(352f, 0f, 0f);
			latchCollider.transform.localScale = new Vector3(1.21f, 0.05f, 0.85f);

			turboHood.AddEventListener(EventTime.Post, EventType.Install, () =>
			{
				latchCollider.SetActive(true);
				hingeJoint = turboHood.AddComponent<HingeJoint>();

				hingeJoint.connectedBody = CarH.satsuma.GetComponent<Rigidbody>();
				hingeJoint.anchor = new Vector3(0, 0, 0);
				hingeJoint.axis = new Vector3(0, 0, 0);
				hingeJoint.useSpring = true;
				hingeJoint.useLimits = true;
				hingeJoint.enableCollision = false; 
				hingeJoint.limits = new JointLimits
				{
					min = -(float) OpenState.Open,
					max = (float) OpenState.Closed,
				};

				SetHoodState(OpenState.Closed);
			});

			turboHood.AddEventListener(EventTime.Pre, EventType.Uninstall, () =>
			{
				latchCollider.SetActive(false);
				Destroy(hingeJoint);
			});
		}

		public void Init(Part turboHood)
		{
			this.turboHood = turboHood;
		}

		void LateUpdate()
		{
			if (openState == OpenState.Closed || !latchCollider.IsLookingAt())
			{
				return;
			}

			UserInteraction.GuiInteraction($"Press [{cInput.GetText("Use")}] to {(openState == OpenState.Lifted ? "Open" : "Close")} Hood");
			if (UserInteraction.UseButtonDown)
			{
				SetHoodState(openState == OpenState.Lifted ? OpenState.Open : OpenState.Lifted);
			}
		}

		protected void SetHoodState(OpenState state)
		{
			JointSpring spring = hingeJoint.spring;
			spring.spring = springSpeed;
			spring.damper = springDamping;
			spring.targetPosition = (float) state;
			hingeJoint.spring = spring;

			openState = state;
		}
	}
}
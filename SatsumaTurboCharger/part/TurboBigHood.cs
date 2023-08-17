using MscModApi.Caching;
using MscModApi.PaintingSystem;
using MscModApi.Parts;
using UnityEngine;
using MSCLoader;
using MscModApi.Caching;
using MscModApi.Parts;
using MscModApi.Parts.ReplacePart;
using MscModApi.Tools;
using UnityEngine;
using EventType = MscModApi.Parts.EventType;

namespace SatsumaTurboCharger.part
{
	public class TurboBigHood : DerivablePart
	{
		public enum OpenState
		{
			Closed = 0,
			Open = 87,
			Lifted = 2
		}


		protected override string partId => "turboBig-hood";
		protected override string partName => "Racing Turbo Hood";
		protected override Vector3 partInstallPosition => new Vector3(0, 0.2408085f, 1.68f);
		protected override Vector3 partInstallRotation => new Vector3(0, 180, 0);

		public TurboBigHood() : base(SatsumaGamePart.GetInstance(), SatsumaTurboCharger.partBaseInfo)
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
			logic = AddEventBehaviour<HoodLogic>(EventType.Install);
			logic.Init(this);

			latchCollider = GameObject.CreatePrimitive(PrimitiveType.Cube);
			latchCollider.name = name + "_LATCHCOLLIDER";
			latchCollider.transform.SetParent(transform);
			latchCollider.GetComponent<Collider>().isTrigger = true;
			latchCollider.GetComponent<Renderer>().enabled = false;
			latchCollider.transform.localPosition = new Vector3(0f, 0.06f, 0.36f);
			latchCollider.transform.localEulerAngles = new Vector3(352f, 0f, 0f);
			latchCollider.transform.localScale = new Vector3(1.21f, 0.05f, 0.85f);

			AddEventListener(EventTime.Post, EventType.Install, () =>
			{
				//Unknown why this is required, without it, the hinge joint will still be there after uninstall
				Object.Destroy(GetComponent<HingeJoint>());

				latchCollider.SetActive(true);
				hingeJoint = AddComponent<HingeJoint>();

				hingeJoint.connectedBody = CarH.satsuma.GetComponent<Rigidbody>();
				hingeJoint.anchor = new Vector3(0, 0, 0);
				hingeJoint.axis = new Vector3(0, 0, 0);
				hingeJoint.useSpring = true;
				hingeJoint.useLimits = true;
				hingeJoint.enableCollision = false;
				hingeJoint.limits = new JointLimits
				{
					min = -(float)OpenState.Open,
					max = (float)OpenState.Closed,
				};

				logic.SetHoodState(OpenState.Closed);
			});

			AddEventListener(EventTime.Post, EventType.Uninstall, () =>
			{
				latchCollider.SetActive(false);
				Object.Destroy(hingeJoint);
			});
		}

		public HoodLogic logic { get; protected set; }

		public GameObject latchCollider { get; protected set; }
		public HingeJoint hingeJoint { get; protected set; }
	}
}
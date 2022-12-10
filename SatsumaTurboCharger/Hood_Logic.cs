using MSCLoader;
using MscModApi.Caching;
using MscModApi.Parts;
using MscModApi.Tools;
using UnityEngine;


namespace SatsumaTurboCharger
{
    public class Hood_Logic : MonoBehaviour
    {

		private Part turboHood;
		private GameObject turboHoodLatchCollider;
		private HingeJoint turboHoodHingeJoint;
		private Rigidbody turboHoodRigidBody;

        private GameObject originalHoodTrigger;

		private bool open = false;
		private float springSpeed = 16;
		private float springDamping = 4;

		private float openedAngle = 87;
		private float closedAngle = 0;
		public void Init(Part hoodPart)
		{
			turboHood = hoodPart;

			turboHoodRigidBody = turboHood.GetComponent<Rigidbody>();
			turboHoodHingeJoint = turboHood.AddComponent<HingeJoint>();
			turboHoodHingeJoint.connectedBody = CarH.satsuma.GetComponent<Rigidbody>();
			turboHoodHingeJoint.anchor = new Vector3(0, 0, 0);
			turboHoodHingeJoint.axis = new Vector3(0, 0, 0);
			turboHoodHingeJoint.useSpring = true;
			turboHoodHingeJoint.useLimits = true;
			turboHoodHingeJoint.limits = new JointLimits
			{
				min = -openedAngle,
				max = closedAngle,
			};
			SetHoodAngle(0);

			turboHoodLatchCollider = GameObject.CreatePrimitive(PrimitiveType.Cube);
			turboHoodLatchCollider.name = hoodPart.gameObject.name.Replace("(Clone)", "_LATCHCOLLIDER");
			turboHoodLatchCollider.transform.SetParent(turboHood.transform);
			turboHoodLatchCollider.GetComponent<Collider>().isTrigger = true;
			turboHoodLatchCollider.GetComponent<Renderer>().enabled = false;
			turboHoodLatchCollider.transform.localPosition = new Vector3(0f, 0.06f, 0.36f);
			turboHoodLatchCollider.transform.localEulerAngles = new Vector3(352f, 0f, 0f);
			turboHoodLatchCollider.transform.localScale = new Vector3(1.21f, 0.05f, 0.85f);

            GameObject normalHood = Game.Find("hood(Clone)");
            GameObject fiberglassHood = Game.Find("fiberglass hood(Clone)");

            originalHoodTrigger = Game.Find("trigger_hood");
            FsmHook.FsmInject(originalHoodTrigger, "Assemble 2", OnOriginalAssemble);
            FsmHook.FsmInject(originalHoodTrigger, "Assemble 3", OnOriginalAssemble);

            FsmHook.FsmInject(normalHood, "Remove part", OnOriginalDisassemble);
            FsmHook.FsmInject(fiberglassHood, "Remove part", OnOriginalDisassemble);



            bool normalHoodInstalled = (normalHood.transform.parent != null && normalHood.transform.parent.name == "pivot_hood");
            bool fiberglassHoodInstalled = (fiberglassHood.transform.parent != null && fiberglassHood.transform.parent.name == "pivot_hood");

			if (turboHood.IsInstalled())
			{
				if (normalHoodInstalled) { Helper.FindFsmOnGameObject(normalHood, "Removal").SendEvent("REMOVE"); }
				if (fiberglassHoodInstalled) { Helper.FindFsmOnGameObject(fiberglassHood, "Removal").SendEvent("REMOVE"); }
			}

			turboHood.BlockInstall(normalHoodInstalled || fiberglassHoodInstalled);
			originalHoodTrigger.SetActive(!turboHood.IsInstalled());

			turboHood.AddPostInstallAction(OnTurboHoodAssemble);
			turboHood.AddPostUninstallAction(OnTurboHoodDisassemble);
		}

        internal void OnTurboHoodAssemble()
        {
            originalHoodTrigger.SetActive(false);
        }
        internal void OnTurboHoodDisassemble()
        {
            JointSpring spring = turboHoodHingeJoint.spring;
            spring.spring = springSpeed;
            spring.damper = springDamping;
            spring.targetPosition = -closedAngle;
            turboHoodHingeJoint.spring = spring;
            open = false;
            originalHoodTrigger.SetActive(true);
        }

		private void OnOriginalAssemble()
		{
			turboHood.BlockInstall(true);
		}
		private void OnOriginalDisassemble()
		{
			turboHood.BlockInstall(false);
		}

        void Start()
        {
            FsmHook.FsmInject(Game.Find("HoodLocking").transform.FindChild("Trigger").gameObject, "Open", delegate () 
            {
                SetHoodAngle(openedAngle);
                open = true; 
            });
        }

        void Update()
        {
            if (open && Helper.DetectRaycastHitObject(turboHoodLatchCollider, "Default"))
            {
                if (Helper.LeftMouseDown)
                {
                    SetHoodAngle(closedAngle);
                    open = false;
                }
            }

        }

        private void SetHoodAngle(float angle)
        {
            JointSpring spring = turboHoodHingeJoint.spring;
            spring.spring = springSpeed;
            spring.damper = springDamping;
            spring.targetPosition = angle;
            turboHoodHingeJoint.spring = spring;
        }
    }
}
using ModApi.Attachable;
using MSCLoader;
using SatsumaTurboCharger.parts;
using UnityEngine;

namespace SatsumaTurboCharger
{
    public class Hood_Logic : MonoBehaviour
    {

        private GameObject turboHood;
        private GameObject turboHoodLatchCollider;
        private HingeJoint turboHoodHingeJoint;
        private Rigidbody turboHoodRigidBody;

        private GameObject originalHoodTrigger;

        private Trigger turboHoodTrigger;


        private bool open = false;
        private float springSpeed = 16;
        private float springDamping = 4;

        private float openedAngle = 87;
        private float closedAngle = 0;
        public void Init(AdvPart hoodPart)
        {
            this.turboHood = hoodPart.rigidPart;
            turboHoodRigidBody = turboHood.GetComponent<Rigidbody>();
            turboHoodHingeJoint = turboHood.AddComponent<HingeJoint>();
            turboHoodHingeJoint.connectedBody = Car.satsuma.GetComponent<Rigidbody>();
            turboHoodHingeJoint.anchor = new Vector3(0, 0, 0);
            turboHoodHingeJoint.axis = new Vector3(0, 0, 0);
            turboHoodHingeJoint.useSpring = true;
            turboHoodHingeJoint.useLimits = true;
            turboHoodHingeJoint.limits = new JointLimits
            {
                min = -openedAngle,
                max = closedAngle,
            };
            this.turboHoodTrigger = hoodPart.partTrigger;
            turboHoodLatchCollider = GameObject.CreatePrimitive(PrimitiveType.Cube);
            turboHoodLatchCollider.name = hoodPart.activePart.name.Replace("(Clone)", "_LATCHCOLLIDER");
            turboHoodLatchCollider.transform.SetParent(turboHood.transform);
            turboHoodLatchCollider.GetComponent<Collider>().isTrigger = true;
            turboHoodLatchCollider.GetComponent<Renderer>().enabled = false;
            turboHoodLatchCollider.transform.localPosition = new Vector3(0f, 0.06f, 0.36f);
            turboHoodLatchCollider.transform.localEulerAngles = new Vector3(352f, 0f, 0f);
            turboHoodLatchCollider.transform.localScale = new Vector3(1.21f, 0.05f, 0.85f);

            GameObject normalHood = GameObject.Find("hood(Clone)");
            GameObject fiberglassHood = GameObject.Find("fiberglass hood(Clone)");

            originalHoodTrigger = GameObject.Find("trigger_hood");
            FsmHook.FsmInject(originalHoodTrigger, "Assemble 2", OnOriginalAssemble);
            FsmHook.FsmInject(originalHoodTrigger, "Assemble 3", OnOriginalAssemble);

            FsmHook.FsmInject(normalHood, "Remove part", OnOriginalDisassemble);
            FsmHook.FsmInject(fiberglassHood, "Remove part", OnOriginalDisassemble);



            bool normalHoodInstalled = (normalHood.transform.parent != null && normalHood.transform.parent.name == "pivot_hood");
            bool fiberglassHoodInstalled = (fiberglassHood.transform.parent != null && fiberglassHood.transform.parent.name == "pivot_hood");

            if (hoodPart.installed)
            {
                if (normalHoodInstalled) { Helper.FindFsmOnGameObject(normalHood, "Removal").SendEvent("REMOVE"); }
                if (fiberglassHoodInstalled) { Helper.FindFsmOnGameObject(fiberglassHood, "Removal").SendEvent("REMOVE"); }
            }

            turboHoodTrigger.triggerGameObject.SetActive(!normalHoodInstalled && !fiberglassHoodInstalled);
            originalHoodTrigger.SetActive(!hoodPart.installed);
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
            turboHoodTrigger.triggerGameObject.SetActive(false);
        }
        private void OnOriginalDisassemble()
        {
            turboHoodTrigger.triggerGameObject.SetActive(true);
        }

        void Start()
        {
            FsmHook.FsmInject(GameObject.Find("HoodLocking").transform.FindChild("Trigger").gameObject, "Open", delegate () 
            {
                JointSpring spring = turboHoodHingeJoint.spring;
                spring.spring = springSpeed;
                spring.damper = springDamping;
                spring.targetPosition = openedAngle;
                turboHoodHingeJoint.spring = spring;
                open = true; 
            });
        }

        void Update()
        {
            if (open && Helper.DetectRaycastHitObject(turboHoodLatchCollider, "Default"))
            {
                if (Helper.LeftMouseDown)
                {
                    JointSpring spring = turboHoodHingeJoint.spring;
                    spring.spring = springSpeed;
                    spring.damper = springDamping;
                    spring.targetPosition = closedAngle;
                    turboHoodHingeJoint.spring = spring;
                    open = false;
                }
            }

        }
    }
}
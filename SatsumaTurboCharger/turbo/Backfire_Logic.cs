using MSCLoader;
using UnityEngine;

namespace SatsumaTurboCharger.turbo
{
    public class Backfire_Logic : MonoBehaviour
    {
        private AudioSource audioFx;
        private ParticleSystem particleFx;

        // Use this for initialization
        void Start()
        {
            audioFx = this.gameObject.GetComponentInChildren<AudioSource>();
            audioFx.spatialBlend = 0.8f;
            audioFx.rolloffMode = AudioRolloffMode.Linear;
            particleFx = this.gameObject.GetComponentInChildren<ParticleSystem>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        internal void TriggerBackfire()
        {
            if (audioFx != null && particleFx != null && !audioFx.isPlaying && !particleFx.isPlaying)
            {
                audioFx.Play();
                particleFx.Emit(2);
            }
        }

        internal Vector3 GetFireFXPos()
        {
            return particleFx.transform.localPosition;
        }

        internal Quaternion GetFireFXRot()
        {
            return particleFx.transform.localRotation;
        }
    }
}
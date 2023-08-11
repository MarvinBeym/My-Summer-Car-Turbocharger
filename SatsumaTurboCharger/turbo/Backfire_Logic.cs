using System;
using MSCLoader;
using UnityEngine;

namespace SatsumaTurboCharger.turbo
{
    public class Backfire_Logic : MonoBehaviour
    {
        private AudioSource backfireAudioSource;
        private ParticleSystem particleFx;

        public Vector3 fireFXPosition
        {
	        get => particleFx.transform.localPosition;
	        set => particleFx.transform.localPosition = value;
        }

        public Vector3 fireFxRotation
        {
	        get => particleFx.transform.localRotation.eulerAngles;
            set => particleFx.transform.localRotation = Quaternion.Euler(value);
        }

        public void Init(AudioSource backfireAudioSource)
        {
	        if (backfireAudioSource == null)
	        {
		        throw new Exception("No backfire audio source supplied");
	        }

	        this.backfireAudioSource = backfireAudioSource;
	        backfireAudioSource.spatialBlend = 0.8f;
	        backfireAudioSource.rolloffMode = AudioRolloffMode.Linear;
	        particleFx = gameObject.GetComponentInChildren<ParticleSystem>();

	        if (particleFx == null)
	        {
		        throw new Exception("No ParticleSystem found");
	        }
        }

        public void TriggerBackfire()
        {
            if (backfireAudioSource != null && particleFx != null && !backfireAudioSource.isPlaying && !particleFx.isPlaying)
            {
	            backfireAudioSource.Play();
                particleFx.Emit(2);
            }
        }
    }
}
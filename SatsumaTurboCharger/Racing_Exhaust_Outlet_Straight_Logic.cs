using MSCLoader;
using System;
using UnityEngine;

namespace SatsumaTurboCharger
{
    public class Racing_Exhaust_Outlet_Straight_Logic : MonoBehaviour
    {
        private AudioSource audio_fx_backfire;
        private ParticleSystem particle_fx_backfire;

        // Use this for initialization
        void Start(){ 
            audio_fx_backfire = this.gameObject.GetComponentInChildren<AudioSource>();
            audio_fx_backfire.spatialBlend = 0.8f;
            audio_fx_backfire.rolloffMode = AudioRolloffMode.Linear;
            particle_fx_backfire = this.gameObject.GetComponentInChildren<ParticleSystem>();
        }

        // Update is called once per frame
        void Update(){ 

        }

        internal void TriggerBackfire(){ 
            if(audio_fx_backfire != null && particle_fx_backfire != null && !audio_fx_backfire.isPlaying && !particle_fx_backfire.isPlaying)
            {
                audio_fx_backfire.Play();
                particle_fx_backfire.Emit(2);
            }
        }

        internal Vector3 GetFireFXPos(){ 
            return particle_fx_backfire.transform.localPosition;
        }

        internal Quaternion GetFireFXRot(){ 
            return particle_fx_backfire.transform.localRotation;
        }
    }
}
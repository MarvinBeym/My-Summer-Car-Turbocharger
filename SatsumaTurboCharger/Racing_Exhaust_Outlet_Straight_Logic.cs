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
        void Start()
        {
            audio_fx_backfire = this.gameObject.GetComponentInChildren<AudioSource>();
            particle_fx_backfire = this.gameObject.GetComponentInChildren<ParticleSystem>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        internal void TriggerBackfire()
        {
            audio_fx_backfire.Play();
            particle_fx_backfire.Emit(2);
        }
    }
}
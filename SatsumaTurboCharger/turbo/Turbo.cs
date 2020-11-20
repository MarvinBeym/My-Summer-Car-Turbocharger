using HutongGames.PlayMaker;
using MSCLoader;
using SatsumaTurboCharger.wear;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SatsumaTurboCharger.turbo
{
    public class Turbo
    {

        protected SatsumaTurboCharger mod;
        protected SimplePart part;
        protected Turbo_Logic logic;
        protected bool ecu_mod_installed = false;
        protected PlayMakerFSM ecu_installedFSM;
        protected PlayMakerFSM ecu_modulesFSM;

        public GameObject car;
        public Drivetrain carDriveTrain;

        public GameObject turbine;
        public GameObject boostChangingGameObject;
        protected bool bigRequired = false;
        protected bool smallRequired = false;
        protected bool otherRequired = false;

        protected bool allBigInstalled = false;
        protected bool allSmallInstalled = false;
        protected bool allOtherInstalled = false;

        public Backfire_Logic backfire_Logic;
        public bool canBackfire = false;

        public FsmFloat powerMultiplier;

        //Configurable by calls to turbo class
        public Configuration config;
        public float userSetBoost;


        //Calculated values
        public float boost = 0;
        public float rpm = 0;
        public float boostMaxConfigured { get; set; } = 0; //will be calculated based on configuration (installed parts)
        public float boostDelay = 0;
        public bool boostDelayThrottleUsed = false;
        public bool blowoffAllowed = false;

        //Wear
        public Wear[] wears = null;

        //Conditions
        public Dictionary<string, Condition> conditions; //Stores all the conditions for this turbo and what should be applied to the calculation
        public bool conditionsHaveUpdated = false; //Is set to true when a condition should be applied has updated This is used to then go through the Dictionary and check again

        //Sound
        public AudioSource loop_source;
        public AudioSource grinding_source;
        public AudioSource blowoff_source;

        private ModAudio loop_audio = new ModAudio();
        private ModAudio grinding_audio = new ModAudio();
        private ModAudio blowoff_audio = new ModAudio();

        public Turbo(SatsumaTurboCharger mod, SimplePart part, Dictionary<string, float> boostSave,string loopSoundFile, string grindingSoundFIle, string blowoffSoundFile, bool[] requiredInstalled, Configuration config, GameObject boostChangingGameObject)
        {
            this.mod = mod;
            this.part = part;
            this.config = config;
            logic = this.part.rigidPart.AddComponent<Turbo_Logic>();
            logic.Init(mod, this);
            ecu_mod_installed = SetupEcuMod();
            this.boostChangingGameObject = boostChangingGameObject;
            //userSetBoost = config.boostBase;

            try
            {
                userSetBoost = boostSave[part.id];
            }
            catch
            {
                userSetBoost = config.boostBase;
            }

            bigRequired = requiredInstalled[0];
            smallRequired = requiredInstalled[1];
            otherRequired = requiredInstalled[2];

            car = GameObject.Find("SATSUMA(557kg, 248)");
            carDriveTrain = car.GetComponent<Drivetrain>();
            carDriveTrain.clutchTorqueMultiplier = 10f;
            CreateSound(ref loop_source, ref loop_audio, loopSoundFile, true);
            CreateSound(ref grinding_source, ref grinding_audio, grindingSoundFIle, true);
            CreateSound(ref blowoff_source, ref blowoff_audio, blowoffSoundFile, true);

            foreach (var playMakerFloatVar in PlayMakerGlobals.Instance.Variables.FloatVariables)
            {
                switch (playMakerFloatVar.Name)
                {
                    case "EnginePowerMultiplier":
                        {
                            powerMultiplier = playMakerFloatVar;
                            break;
                        }
                }
            }
        }

        public bool CheckAllRequiredInstalled()
        {
            return allBigInstalled == bigRequired && allSmallInstalled == smallRequired && allOtherInstalled == otherRequired;
        }

        public float CalculateRpm(float boost, float multiplier)
        {
            float turboRpm = boost * multiplier;
            if (turboRpm <= 0)
            {
                turboRpm = 0;
            }
            return turboRpm;
        }


        public void CreateSound(ref AudioSource source, ref ModAudio modAudio, string soundFile, bool loop)
        {
            if(soundFile != null && soundFile != "")
            {
                source = part.rigidPart.AddComponent<AudioSource>();
                modAudio.audioSource = source;

                modAudio.LoadAudioFromFile(Path.Combine(ModLoader.GetModAssetsFolder(mod), soundFile), true, false);
                source.minDistance = 1;
                source.maxDistance = 10;
                source.spatialBlend = 1;
                source.loop = loop;
            }

        }

        public void Sound(bool playOrStop, AudioSource source)
        {
            if(source != null)
            {
                if (playOrStop)
                {
                    if (!source.isPlaying)
                    {
                        source.Play();
                    }
                }
                else
                {
                    source.Stop();
                }
            }
        }

        public void LoopSound(bool playOrStop)
        {
            Sound(playOrStop, loop_source);
        }
        public void BlowoffSound()
        {
            blowoff_source.PlayOneShot(blowoff_source.clip);
        }
        public void GrindingSound(bool playOrStop)
        {
            Sound(playOrStop, grinding_source);
        }

        private bool SetupEcuMod()
        {
            bool installed = ModLoader.IsModPresent("SatsumaTurboCharger");
            if (!installed)
            {
                return false;
            }
            try
            {
                foreach (PlayMakerFSM fsm in GameObject.Find("DonnerTech_ECU_Mod").GetComponents<PlayMakerFSM>())
                {
                    switch (fsm.FsmName)
                    {
                        case "Installed":
                            ecu_installedFSM = fsm;
                            break;
                        case "Modules":
                            ecu_modulesFSM = fsm;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                mod.logger.New("Ecu mod gameobject could not be found or the component is missing", "Mod was found but not gameobject", ex);
                return false;
            }
            return true;
        }

        public void Handle(bool allBigInstalled, bool allSmallInstalled, bool allOtherInstalled)
        {
            this.allBigInstalled = allBigInstalled;
            this.allSmallInstalled = allSmallInstalled;
            this.allOtherInstalled = allOtherInstalled;
        }

        public void SetConditions(Dictionary<string, Condition> conditions)
        {
            this.conditions = conditions;
        }

        public void UpdateCondition(string id, bool applyCondition)
        {
            if(conditions[id].applyCondition != applyCondition)
            {
                conditions[id].applyCondition = applyCondition;
                conditionsHaveUpdated = true;
            }
        }

        public Dictionary<string, float> GetBoost(Dictionary<string, float> boostSave)
        {
            boostSave[part.id] = userSetBoost;
            return boostSave;
        }
    }
}
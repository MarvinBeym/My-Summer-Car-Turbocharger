using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SatsumaTurboCharger.turbo
{
    public class TurboConfiguration
    {
		/*
	    public float boostBase => SatsumaTurboCharger.boostBase.GetValue();
		public float boostStartingRpm => SatsumaTurboCharger.boostStartingRpm.GetValue();
		public float boostMin => SatsumaTurboCharger.boostMin.GetValue();
		public float boostSettingSteps => SatsumaTurboCharger.boostSettingSteps.GetValue();
		public float minSettableBoost => SatsumaTurboCharger.minSettableBoost.GetValue();
		public float boostSteepness => SatsumaTurboCharger.boostSteepness.GetValue();
		public float boostStartingRpmOffset => SatsumaTurboCharger.boostStartingRpmOffset.GetValue();
		public float blowoffDelay => SatsumaTurboCharger.blowoffDelay.GetValue();
		public float blowoffTriggerBoost => SatsumaTurboCharger.blowoffTriggerBoost.GetValue();
		public float backfireThreshold => SatsumaTurboCharger.backfireThreshold.GetValue();
		public int backfireRandomRange => (int)SatsumaTurboCharger.backfireRandomRange.GetValue();
		public float rpmMultiplier => SatsumaTurboCharger.rpmMultiplier.GetValue();
		public float extraPowerMultiplicator => SatsumaTurboCharger.extraPowerMultiplicator.GetValue();
		public float soundboostMinVolume => SatsumaTurboCharger.soundboostMinVolume.GetValue();
		public float soundboostMaxVolume => SatsumaTurboCharger.soundboostMaxVolume.GetValue();
		public float soundboostPitchMultiplicator => SatsumaTurboCharger.soundboostPitchMultiplicator.GetValue();
		public float backfireDelay => SatsumaTurboCharger.backfireDelay.GetValue();
		*/
		public float boostBase;                     //The base boost conditions are either added or substracted from that resulting in the calculated max boost.
	    public float boostStartingRpm;              //The rpm at which boost should start to be generated.
	    public float boostMin;                      //The min boost possible (this will also be substracted from the power multiplier when boost is at boostMin.
	    public float boostSettingSteps;             //When increasing/decreasing userSetBoost, this value is used to add or substract
	    public float minSettableBoost;              //The minimum possible boost that the user should be able to define when setting the boost on the blowoff valve
	    public float boostSteepness;                //Used for calculation of the boost. Defines how steep the graph rises".
	    public float boostStartingRpmOffset;        //Added to the boost starting rpm to get the starting point of the graph (~zero y position) closer to the starting rpm
	    public float blowoffDelay;                  //How long to wait after a blowoff has happened until new boost can be produced.
	    public float blowoffTriggerBoost;           //Above how much boost the blowoff can happen.
	    public float backfireThreshold;             //This defines at which rpm it is possible for a backfire to happen / no longer happen
	    public int backfireRandomRange;             //This is the number used to find if a backfire should happen (ex. 20 would mean if the random value between 0 and 20 is == 1) -> backfire
	    public float rpmMultiplier;                 //Multiplier used for calculating the turbo rpm.
	    public float extraPowerMultiplicator;       //By how much to multiply the boost that is later applied to the engines power multiplier.
	    public float soundboostMinVolume;
	    public float soundboostMaxVolume;
	    public float soundboostPitchMultiplicator;
		public float backfireDelay;
    }
}

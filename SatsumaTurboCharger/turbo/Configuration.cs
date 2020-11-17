using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SatsumaTurboCharger.turbo
{
    public class Configuration
    {
        public float boostBase;                     //The base boost conditions are either added or substracted from that resulting in the calculated max boost.
        public float boostStartingRpm;              //The rpm at which boost should start to be generated.
        public float boostMin;                      //The min boost possible (this will also be substracted from the power multiplier when boost is at boostMin.
        public float minSettableBoost;              //The minimum possible boost that the user should be able to define when setting the boost on the blowoff valve
        public float boostIncreasement;             //Used for calculation of the boost. This is how the graph line is "rotated".
        public float blowoffDelay;                  //How long to wait after a blowoff has happened until new boost can be produced.
        public float blowoffTriggerBoost;           //Above how much boost the blowoff can happen.
        public float backfireThreshold;             //This defines at which rpm it is possible for a backfire to happen / no longer happen
        public int backfireRandomRange;           //This is the number used to find if a backfire should happen (ex. 20 would mean if the random value between 0 and 20 is == 1) -> backfire
        public float rpmMultiplier;                 //Multiplier used for calculating the turbo rpm.
        public float extraPowerMultiplicator;       //By how much to multiply the boost that is later applied to the engines power multiplier.
        public float soundBoostMaxVolume;           //Used for the turbo sound loops max volume.
        public float soundBoostIncreasement;        //Used the same as the boostIncreasement but for turbo sound loop.
        public float soundBoostPitchMultiplicator;  //Used to multiply the pitch of the turbo sound loop based on the sound boost calculation.
        public float boostSettingSteps;             //When increasing/decreasing userSetBoost, this value is used to add or substract
    }
}

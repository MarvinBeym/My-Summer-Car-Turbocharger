using HutongGames.PlayMaker;
using MSCLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Tools
{
    public static class CarH
    {
        private static GameObject car;
        private static Drivetrain carDrivetrain;
        private static AxisCarController carAxisController;
        private static CarController carCarController;
        private static FsmBool electricsOk;
        private static GameObject carElectricity;
        public static bool running
        {
            get { return drivetrain.rpm > 0; }
        }

        public static GameObject electricity
        {
            get
            {
                if(carElectricity == null)
                {
                    carElectricity = satsuma.transform.FindChild("Electricity").gameObject;
                }
                return carElectricity;
            }
        }
        
        public static CarController carController
        {
            get 
            { 
                if(carCarController == null)
                {
                    carCarController = satsuma.GetComponent<CarController>();
                }
                return carCarController;
            }
        }

        public static AxisCarController axisCarController
        {
            get
            {
                if(carAxisController == null)
                {
                    carAxisController = satsuma.GetComponent<AxisCarController>();
                }
                return carAxisController;
            }
        }

        public static Drivetrain drivetrain
        {
            get
            {
                if(carDrivetrain == null)
                {
                    carDrivetrain = satsuma.GetComponent<Drivetrain>();
                }
                return carDrivetrain;
            }
        }

        public static GameObject satsuma
        {
            get
            {
                if(car == null)
                {
                    car = Game.Find("SATSUMA(557kg, 248)");
                }
                return car;
            }
        }


        public static bool hasPower
        {
            get
            {
                if (electricsOk == null)
                {
                    PlayMakerFSM carElectricsPower = PlayMakerFSM.FindFsmOnGameObject(electricity, "Power");
                    electricsOk = carElectricsPower.FsmVariables.FindFsmBool("ElectricsOK");
                    return electricsOk.Value;
                }
                else
                {
                    return electricsOk.Value;
                }
            }
        }
    }
}

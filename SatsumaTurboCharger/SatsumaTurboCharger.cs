﻿using HutongGames.PlayMaker;
using ModApi;
using ModApi.Attachable;
using ModsShop;
using MSCLoader;
using ScrewablePartAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using UnityEngine;
using Random = System.Random;

namespace SatsumaTurboCharger
{

    public class SatsumaTurboCharger : Mod
    {
        /* Todo:
         *  -------------------------------------------------
         *  Draw power grapth/turbo grapth ?

         *  check for air fuel mixture and decrease/increase boost
         *  make n2o usable
         *  increase fuel consumption when turbo is used/installed
         *  more grip ?
         *  -------------------------------------------------
         */

        /* Changelog v2.0 first
         * Added exhaust header 
         * Changed exhaust pipes to fit with the new header
         * Changed models for racing (Racing) turbo to have connection plates for screws
         * Changed positions of parts slightly
         * Added screws to all Racing turbo parts
         * Added screws to intercooler manidold tubes
         * Added screws to manifolds
         * Added clamps to make tubes connected to each other more realistic looking
         */

        /* Changelog v2.0 second  
         * Added mods communication with DonnerTechRacing ecus mod
         * If you have the ecu mod installed and enable ALS (Antilag) the turbo will go to max boost and you can let of the throttle, drive around a corner and let it go again to have max boost
         * Changed Racing turbo boost calculation (will now increase slower so that the car won't just instantly loose grip)
         */

        /* Changelog v2.0 third 
         * Changed the multiplier used when the turbocharger value is applied.
         * Racing turbo: from 2.0 -> 1.5f
         * GT turbo: from 1.4 -> 1.5f
         * Changed the boost gauge text to be the pressure of the turbo itself. 
         * Racing turbo has a pressure of -0.10, meaning the car when idle or below turbo engage speed has less power (powerMultiplier = 0.9 instead of 1)
         * GT turbo has a pressure of -0.04, meaning the car when idle or below turbo engage speed has less power (powerMultiplier = 0.96 instead of 1)
         * Added parts wear to Racing turbo, GT turbo, intercooler and airfilter.
         * Racing turbo wears faster than GT turbo
         * Airfilter reduces wear on GT turbo based on it's own wear
         * If intercooler is damaged it will reduce the max pressure possible
         * Parts wear status can be checked by removing the part and aiming at it. It will show a text roughly telling the current wear status
         * Added Debug information for parts wear status
         * If the wear is below 15 (15%) there is a chance of 400:1 (0.25%) that the part will disassemble itself. The part can be reinstalled but the chance stays.
         * If the wear is at 0 (0%) the part will always disassemble when installed.
         * Added grinding sound when turbos wear is below 25%
         * Added repair "Products" to the ModsShop for Racing turbo, GT turbo, intercooler, airfilter
         * ModsShop might not display anything after buying something. Just wait a while and it will display again (something going on with ModsShop no turbo mod)
         * Adjusted Prices for "REPAIR" to be lower than the original buy price
         * Added workaround for ModsShop purchased logic so "REPAIR" products can be bought multiple times
         * Parts have to be removed from the car and placed on the Desk where the ModsShop sign is. Otherwise the parts won't be repaired (money is refunded)
         * Changed started/finished loading ModConsole output.
         * ModsShop save information changed. I might have to delete the modsShop bought save file.
         * Added screws/clamps to GT turbo, GT turbo intercooler tube and GT turbo exhaust inlet tube
         */
        /*Changelog v2.0 fourth
         * Changed GT turbo part for increasing/decreasing of the max boost to be the wastegate and nolonger the turbo itself.
         * Fixed the GT turbo smallest possible boost to not go below 1.01
         * Changed minimum boost possible to be 0.8 for GT turbo
         * Added Straight Tube for Racing turbo
         * Made the exhaust smoke 
         * come out the straight tube if it is installed
         * Added backfire sound if straight tube is installed and revLimiter is triggered (25% chance it will backfire)
         * Added fire fx to straight pipe that will shoot out the straight pipe if the engine backfires
         */
        /*Changelog v2.0 fifth
         * Boost gauge now displays if any part is not installed. It will then display "ERR" -> check that all needed parts are installed.
         * Car will now fail inspection if any Turbo part is installed. Including the gauge and hood as it implies that you might be using a turbo.
         * Updated all textures to be more realistic
         * all parts now also have to be screwed in fully to count as "installed"
         * Boost gauge also displays "ERR" if not all needed parts are screwed in
         * Improved Backfire logic
         * Backfire will only be triggered when having the straight pipe
         * If ALS (ECU-Mod) is enabled (and get's Triggered) the backfire has a chance of 100% to trigger above 4k rpm when not pressing the accelerator key
         * If ALS (ECU-Mod) is not enabled the backfire has a chance of 12.5% (1:8) to trigger above 4k rpm when not pressing the accelerator key
         * Backfire can trigger each frame if the rpm is above 4k (more inital rpm = more possible backfires) (only with ALS)
         * ALS Now can only trigger if you were accelerating before (Like in all the videos you can see on youtube about Antilag) (Antilag on Deceleration)
         */

        /*FIX (alot of this thanks to twitch LDB_Valow:
         *  FIXED: When ALS is enabled won't disable
         *  button should disable cruise control when cruise control is on (green)
         *  if cruise control is off and a speed was set it should enable cruise control and make it reach the originally set speed
         *  if cruise control is off and on button is pressed it should set the speed to the current car speed
         *  Do dynotest with big and small turbo and adjust max boost possible to make it more realistic
         *  add boost leak when not screwed in.
         *  add screws to intercooler
         *  clamp on twincarb offset?
         *  color is wrong for all parts (red) (stay red)
         *  when normal steel header/exhaust is installed will do the wrong exhaus sound/location
         *  check if small turbo works like it is supposed to
         *  add product images that are missing
         *  add screws to original hood.
         *  ScrewableAPI: screws hitbox different than original (make larger)
         *  add clamp and screws to straight pipe
         *  check/make sure parts are only installable if parts connecting them are
         *  EDU mod: adjust triggers to be at the same location as the part itself and smaller trigger area
         *  ECU mod: add ERRor to display if something is wrong
         *  make texture of parts darker (steel texture)
         *  add clamp to blowoff valve
         *  exhaust smoke bugged  when turbo with outlet tube (not straight)
         *  ECU mod: cruise control not displaying anything (after saving or reload it works)
         *  FIXED: Turbo not reaching top boost
         *  FIXED: Turbo boost value is wrong displayed
         *  make fire of exhaust light up house?????
         *  FIXED? : when als is enabled normale flames don't work
         *  try adding MonoBehaviour to gameObject and then try and use it.
         *  Add Gear adjustment back into the mod but only for 1-4th gear.
         *  exhaust sound/smoke stays when car is stalled
         *  Don't do blowoff sound when als triggers
         *  -------------------------------------------------
         *  Known bugs:
         *  
         *  
         *  Use Key still has to be "F" for Painting
         */

        //Material[] tttt = Resources.FindObjectsOfTypeAll<Material>(); //Get all Materials of game
        //Component sprayCanPlayMakerFSM = tests.Value.GetComponentInChildren<PlayMakerFSM>(); // get PlayMaker Component from GameObject
        public override string ID => "SatsumaTurboCharger"; //Your mod ID (unique)
        public override string Name => "DonnerTechRacing Turbocharger"; //You mod name
        public override string Author => "DonnerPlays"; //Your Username
        public override string Version => "2.0"; //Version
        public override bool UseAssetsFolder => true;

        //Saves
        private static PartBuySave partBuySave;
        private static OthersSave othersSave;
        private PartsWearSave partsWearSave;
        
        //
        //Install & Spawn Locations
        //
        //Big Turbo
        private Vector3 turbocharger_big_installLocation = new Vector3(-0.2705f, -0.064f, 0.273f);                              //Cylinder Head
        private Vector3 turbocharger_big_intercooler_tube_installLocation = new Vector3(0.318f, -0.041f, 1.52f);                //Satsuma       //Done
        private Vector3 turbocharger_big_exhaust_inlet_tube_installLocation = new Vector3(-0.179f, -0.1506f, -0.037f);          //Cylinder Head
        private Vector3 turbocharger_big_exhaust_outlet_tube_installLocation = new Vector3(-0.217f, -0.229f, -0.059f);          //Cylinder Head
        private Vector3 turbocharger_big_blowoff_valve_installLocation = new Vector3(0.315f, 0.258f, 1.332f);                  //Satsuma       //Done
        private Vector3 turbocharger_big_exhaust_outlet_straight_installLocation = new Vector3(-0.316f, -0.2f, 0.307f);

        public static Vector3 turbocharger_big_spawnLocation = new Vector3(1558.366f, 5f, 742.5068f);
        public static Vector3 turbocharger_big_intercooler_tube_spawnLocation = new Vector3(1556.846f, 5f, 741.4836f);
        public static Vector3 turbocharger_big_exhaust_inlet_tube_spawnLocation = new Vector3(1557.866f, 5.5f, 741.9728f);
        public static Vector3 turbocharger_big_exhaust_outlet_tube_spawnLocation = new Vector3(1557.352f, 5f, 741.7303f);
        public static Vector3 turbocharger_big_blowoff_valve_spawnLocation = new Vector3(1555.136f, 5.8f, 737.2324f);
        public static Vector3 turbocharger_big_exhaust_outlet_straight_spawnLocation = new Vector3(0, 0, 0);

        //Small Turbo
        private Vector3 turbocharger_small_installLocation = new Vector3(-0.25f, -0.1665f, 0.0001f);                              //Cylinder Head
        private Vector3 turbocharger_small_manifold_twinCarb_tube_installLocation = new Vector3(-0.188f, -0.23f, 0.14f);        //Cylinder Head
        private Vector3 turbocharger_small_intercooler_tube_installLocation = new Vector3(0.316f, -0.041f, 1.518f);              //Satsuma
        private Vector3 turbocharger_small_exhaust_inlet_tube_installLocation = new Vector3(-0.0918f, -0.1774f, -0.094f);         //Cylinder Head
        private Vector3 turbocharger_small_exhaust_outlet_tube_installLocation = new Vector3(-0.1825f, -0.267f, -0.145f);         //Cylinder Head
        private Vector3 turbocharger_small_airfilter_installLocation = new Vector3(-0.25f, -0.04f, 0.0001f);                     //Cylinder Head

        public static Vector3 turbocharger_small_spawnLocation = new Vector3(1457.509f, -1.8f, 716.0f);
        public static Vector3 turbocharger_small_exhaust_inlet_tube_spawnLocation = new Vector3(1457.509f, -1.8f, 715.5f);
        public static Vector3 turbocharger_small_exhaust_outlet_tube_spawnLocation = new Vector3(1457.509f, -1.8f, 715.0f);
        public static Vector3 turbocharger_small_manifold_twinCarb_tube_spawnLocation = new Vector3(1457.509f, -1.8f, 714.5f);
        public static Vector3 turbocharger_small_airfilter_spawnLocation = new Vector3(1555.174f, 5.8f, 736.9866f);
        public static Vector3 turbocharger_small_intercooler_tube_spawnLocation = new Vector3(1554.144f, 5f, 738.733f);

        //Other Parts
        private Vector3 turbocharger_hood_installLocation = new Vector3(0.0f, 0.241f, 1.68f);                                   //Satsuma
        private Vector3 turbocharger_manifold_weber_installLocation = new Vector3(0f, -0.3f, 0.1f);                             //Cylinder Head
        private Vector3 turbocharger_manifold_twinCarb_installLocation = new Vector3(-0.007f, -0.265f, 0.006f);                 //Cylinder Head
        private Vector3 turbocharger_boost_gauge_installLocation = new Vector3(0.5f, -0.04f, 0.125f);                           //Dashboard
        private Vector3 turbocharger_intercooler_installLocation = new Vector3(0.0f, -0.162f, 1.682f);                          //Satsuma
        private Vector3 turbocharger_intercooler_manifold_tube_weber_installLocation = new Vector3(-0.34f, -0.047f, 1.445f);    //Satsuma
        private Vector3 turbocharger_intercooler_manifold_tube_twinCarb_installLocation = new Vector3(-0.332f, -0.047f, 1.445f); //Satsuma
        private Vector3 turbocharger_exhaust_header_installLocation = new Vector3(-0.005f, -0.089f, -0.064f);               //Cylinder Head

        public static Vector3 turbocharger_exhaust_header_spawnLocation = new Vector3(1555.136f, 5.8f, 737.2324f);
        public static Vector3 turbocharger_hood_spawnLocation = new Vector3(1559.46f, 5f, 742.296f);
        public static Vector3 turbocharger_manifold_weber_spawnLocation = new Vector3(1555.18f, 5.8f, 737.8821f);
        public static Vector3 turbocharger_manifold_twinCarb_spawnLocation = new Vector3(1555.07f, 5.8f, 737.6261f);
        public static Vector3 turbocharger_boost_gauge_spawnLocation = new Vector3(1555.383f, 5.8f, 737.058f);
        public static Vector3 turbocharger_intercooler_spawnLocation = new Vector3(1555.382f, 5.8f, 737.3588f);
        public static Vector3 turbocharger_intercooler_manifold_tube_weber_spawnLocation = new Vector3(1554.56f, 5f, 737.2017f);
        public static Vector3 turbocharger_intercooler_manifold_tube_twinCarb_spawnLocation = new Vector3(1554.339f, 5.5f, 737.913f);

        //Mods Shop
        private ModsShop.ShopItem shop;
        private ModsShop.ProductDetails repair_turbocharger_big_Product;
        private ModsShop.ProductDetails repair_turbocharger_small_Product;
        private ModsShop.ProductDetails repair_turbocharger_small_airfilter_Product;
        private ModsShop.ProductDetails repair_intercooler_Product;

        //
        //Part Triggers
        //
        //Big Turbo
        private Trigger turbocharger_big_trigger;
        private Trigger turbocharger_big_intercooler_tube_trigger;
        private Trigger turbocharger_big_exhaust_inlet_tube_trigger;
        private Trigger turbocharger_big_exhaust_outlet_tube_trigger;
        private Trigger turbocharger_big_blowoff_valve_trigger;
        private Trigger turbocharger_big_exhaust_outlet_straight_trigger;

        //Small Turbo
        private Trigger turbocharger_small_trigger;
        private Trigger turbocharger_small_intercooler_tube_trigger;
        private Trigger turbocharger_small_manifold_twinCarb_tube_trigger;
        private Trigger turbocharger_small_exhaust_inlet_tube_trigger;
        private Trigger turbocharger_small_exhaust_outlet_tube_trigger;
        private Trigger turbocharger_small_airfilter_trigger;

        //Other Parts
        private Trigger turbocharger_exhaust_header_trigger;
        private Trigger turbocharger_manifoldWeber_trigger;
        private Trigger turbocharger_manifoldTwinCarb_trigger;
        private Trigger turbocharger_hood_trigger;
        private Trigger turbocharger_boostGauge_trigger;
        private Trigger turbocharger_intercooler_trigger;
        private Trigger turbocharger_intercooler_manifold_tube_weber_trigger;
        private Trigger turbocharger_intercooler_manifold_tube_twinCarb_trigger;
        
        //
        //Mods Gameobjects
        //
        //Big Turbo
        private static GameObject turbocharger_big = new GameObject();
        private static GameObject turbocharger_big_intercooler_tube = new GameObject();
        private static GameObject turbocharger_big_exhaust_inlet_tube = new GameObject();
        private static GameObject turbocharger_big_exhaust_outlet_tube = new GameObject();
        private static GameObject turbocharger_big_blowoff_valve = new GameObject();
        private static GameObject turbocharger_big_exhaust_outlet_straight = new GameObject();

        //Small Turbo
        private static GameObject turbocharger_small = new GameObject();
        private static GameObject turbocharger_small_intercooler_tube = new GameObject();
        private static GameObject turbocharger_small_manifold_twinCarb_tube = new GameObject();
        private static GameObject turbocharger_small_exhaust_inlet_tube = new GameObject();
        private static GameObject turbocharger_small_exhaust_outlet_tube = new GameObject();
        private static GameObject turbocharger_small_airfilter = new GameObject();

        //Other Parts
        private static GameObject turbocharger_exhaust_header = new GameObject();
        private static GameObject turbocharger_hood = new GameObject();
        private static GameObject turbocharger_manifold_weber = new GameObject();
        private static GameObject turbocharger_manifold_twinCarb = new GameObject();
        private static GameObject turbocharger_boost_gauge = new GameObject();
        private static GameObject turbocharger_intercooler = new GameObject();
        private static GameObject turbocharger_intercooler_manifold_tube_weber = new GameObject();
        private static GameObject turbocharger_intercooler_manifold_tube_twinCarb = new GameObject();
        private static GameObject boostGauge;

        //
        //Game Objects
        //
        //Parts
        private GameObject satsuma;
        private GameObject weberCarb;
        private GameObject twinCarb;
        private GameObject racingExhaustPipe;
        private GameObject steelHeaders;
        private GameObject racingExhaustMuffler;
        private GameObject originalCylinerHead;
        private GameObject exhaustRaceMuffler;
        private GameObject exhaustEngine;
        private GameObject exhaustPipeRace;
        private Transform originalExhaustPipeRaceTransform;
        private static Drivetrain satsumaDriveTrain;

        //Power/Electricity
        private GameObject elect;
        private PlayMakerFSM power;

        //Inspection
        private PlayMakerFSM inspectionPlayMakerFsm;
        private FsmEvent inspectionFailedEvent;

        //Parts installed
        private bool weberCarb_inst = false;
        private bool twinCarb_inst = false;
        private bool steelHeaders_inst = false;
        private bool racingExhaustPipe_inst = false;
        private bool racingExhaustMuffler_inst = false;

        //Part Tightness
        private FsmFloat racingExhaustPipeTightness;

        //Engine Values
        private FsmFloat _engineTorqueRpm;
        private FsmFloat _engineFriction;
        private FsmFloat _engineTorque;
        //private FsmState n2oBottle;
        //private FsmFloat n2oBottlePSI;
        //private CarController satsumaCarController;
        //private Axles satsumaAxles;

        //NOS
        private FsmState n2oState;
        private FsmState no_n2oState;
        private GameObject N2O;
        private PlayMakerFSM n2oPlayMaker;

        //Painting System
        private MeshRenderer[] sprayCansMeshRenders;
        private Material regularCarPaintMaterial;
        private MeshRenderer turbocharger_hood_renderer;

        //Audio
        private ModAudio turbocharger_loop_big = new ModAudio();
        private ModAudio turbocharger_loop_small = new ModAudio();
        private ModAudio turbocharger_blowoff = new ModAudio();
        private ModAudio turbocharger_grinding_loop = new ModAudio();
        private AudioSource turboLoopBig;
        private AudioSource turboGrindingLoop;
        private AudioSource turboLoopSmall;
        private AudioSource turboBlowOffShot;
        private AudioSource backfire_fx_big_turbo_exhaust_straight;

        //Mod Settings
        private static bool partsWearDEBUG = false;
        private static bool turboValuesDEBUG = false;
        private static bool useDefaultColors = false;
        private Settings DEBUG_parts_wear = new Settings("debugPartsWear", "Enable/Disable", SwitchPartsWearDEBUG);
        private Settings DEBUG_turbo_values = new Settings("debugTurboValues", "Enable/Disable", SwitchTurboChargerValuesDEBUG);
        private Settings useDefaultColorsSetting = new Settings("useDefaultColors", "Use default game colors for painting", false, new Action(SatsumaTurboCharger.ToggleUseDefaultColors));
        private Settings resetPosSetting = new Settings("resetPos", "Reset", new Action(SatsumaTurboCharger.PosReset));
        
        //Car values
        private float engineRPM = 0;
        private float enginePowerMultiplier;
        private float enginePowerCurrent;
        private FsmFloat _enginePowerMultiplier;
        private float newTurboChargerBar = 0;
        private bool isItemInHand;
        private bool electricityOn = false;

        //
        //ModApi Parts
        //
        //Big Turbo
        private static Racing_Turbocharger_Part turbocharger_big_part;
        private static Racing_Intercooler_Tube_Part turbocharger_big_intercooler_tube_part;
        private static Racing_Exhaust_Inlet_Tube_Part turbocharger_big_exhaust_inlet_tube_part;
        private static Racing_Exhaust_Outlet_Tube_Part turbocharger_big_exhaust_outlet_tube_part;
        private static Racing_Blowoff_Valve_Part turbocharger_big_blowoff_valve_part;
        private static Racing_Exhaust_Outlet_Straight_Part turbocharger_big_exhaust_outlet_straight_part;
        private static Racing_Hood_Part turbocharger_hood_part;
        private static Exhaust_Header_Part turbocharger_exhaust_header_part;

        //Small Turbo
        private static GT_Turbocharger_Part turbocharger_small_part;
        private static GT_Intercooler_Tube_Part turbocharger_small_intercooler_tube_part;
        private static GT_Exhaust_Inlet_Tube_Part turbocharger_small_exhaust_inlet_tube_part;
        private static GT_Exhaust_Outlet_Tube_Part turbocharger_small_exhaust_outlet_tube_part;
        private static GT_Airfilter_Part turbocharger_small_airfilter_part;
        private static GT_Manifold_TwinCarb_Tube_Part turbocharger_small_manifold_twinCarb_tube_part;

        //Other Parts
        private static Manifold_Weber_Part turbocharger_manifold_weber_part;
        private static Manifold_TwinCarb_Part turbocharger_manifold_twinCarb_part;
        private static Boost_Gauge_Part turbocharger_boost_gauge_part;
        private static Intercooler_Part turbocharger_intercooler_part;
        private static Intercooler_Manifold_Tube_Weber_Part turbocharger_intercooler_manifold_tube_weber_part;
        private static Intercooler_Manifold_Tube_TwinCarb_Part turbocharger_intercooler_manifold_tube_twinCarb_part;

        //
        //ScrewableAPI
        //
        //Big Turbo
        public static ScrewablePart turbocharger_big_intercooler_tube_screwable;
        public static ScrewablePart turbocharger_big_screwable;
        public static ScrewablePart turbocharger_big_exhaust_inlet_tube_screwable;
        public static ScrewablePart turbocharger_big_exhaust_outlet_tube_screwable;

        //Small Turbo
        public static ScrewablePart turbocharger_small_intercooler_tube_screwable;
        public static ScrewablePart turbocharger_small_screwable;
        public static ScrewablePart turbocharger_small_exhaust_inlet_tube_screwable;
        public static ScrewablePart turbocharger_small_exhaust_outlet_tube_screwable;
        public static ScrewablePart turbocharger_small_manifold_twinCarb_tube_screwable;

        //Other Parts
        public static ScrewablePart turbocharger_intercooler_manifold_weberCarb_tube_screwable;
        public static ScrewablePart turbocharger_intercooler_manifold_twinCarb_tube_screwable;
        public static ScrewablePart turbocharger_exhaust_header_screwable;
        public static ScrewablePart turbocharger_manifold_weberCarb_screwable;
        public static ScrewablePart turbocharger_manifold_twinCarb_screwable;

        //Flags
        private bool turbocharger_blowoffShotAllowed = false;
        private bool canBackfire = false;

        //Time Comparer
        private float timeSinceLastBlowOff;

        //Wear Logic
        private Random randDestroyValue;
        private float timer_wear_turbocharger_big;
        private float timer_wear_turbocharger_small;
        private float timer_wear_airfilter;
        private float timer_wear_intercooler;

        //Part Coloring
        private Color pickedUPsprayCanColor;

        private Color hoodColor = new Color(0.800f, 0.800f, 0.800f);
        private Color turbocharger_bigColor = new Color(0.800f, 0.800f, 0.800f);
        private Color intercoolerColor = new Color(0.800f, 0.800f, 0.800f);
        private Color turbochargerSmallColor = new Color(0.800f, 0.800f, 0.800f);
        private Color turbochargerManifoldWeberColor = new Color(0.800f, 0.800f, 0.800f);
        private Color turbochargerManifoldTwinCarbColor = new Color(0.800f, 0.800f, 0.800f);
        private Color turbocharger_bigBlowoffValveColor = new Color(0.800f, 0.800f, 0.800f);
        private Color turbocharger_small_airfilter_color = new Color(0.800f, 0.800f, 0.800f);

        private Color originalTurbocchargerBigColor = new Color(0.800f, 0.800f, 0.800f);
        private Color originalIntercoolerColor = new Color(0.800f, 0.800f, 0.800f);
        private Color originalTurbochargerSmallColor = new Color(0.800f, 0.800f, 0.800f);
        private Color originalTurbochargerManifoldWeberColor = new Color(0.800f, 0.800f, 0.800f);
        private Color originalTurbochargerManifoldTwinCarbColor = new Color(0.800f, 0.800f, 0.800f);
        private Color originalturbocharger_bigBlowoffValveColor = new Color(0.800f, 0.800f, 0.800f);
        private Color original_turbocharger_small_airfilter_color = new Color(0.800f, 0.800f, 0.800f);
        private Color[] modSprayColors = new Color[13];
        

        //Mod Parts Installed
        private bool allBigPartsInstalled = false;
        private bool allSmallPartsInstalled = false;
        private bool allOtherPartsInstalled = false;

        //Mod Parts Screwed
        private bool allBigPartsScrewed = false;
        private bool allSmallPartsScrewed = false;
        private bool allOtherPartsScrewed = false;

        //Input detector
        internal static bool useThrottleButton
        {
            get
            {
                return cInput.GetKey("Throttle");
            }
        }

        //
        //Save File Locations
        //
        //Big Turbo
        private const string turbocharger_big_SaveFile = "turbocharger_big_partSave.txt";
        private const string turbocharger_big_intercooler_tube_SaveFile = "turbocharger_big_intercooler_tube_partSave.txt";
        private const string turbocharger_big_exhaust_inlet_tube_SaveFile = "turbocharger_big_exhaust_inlet_tube_partSave.txt";
        private const string turbocharger_big_exhaust_outlet_tube_SaveFile = "turbocharger_big_exhaust_outlet_tube_partSave.txt";
        private const string turbocharger_big_blowoff_valve_SaveFile = "turbocharger_big_blowoff_valve_partSave.txt";
        private const string turbocharger_big_exhaust_outlet_straight_SaveFile = "turbocharger_big_exhaust_outlet_straight_partSave.txt";

        //Small Turbo
        private const string turbocharger_small_SaveFile = "turbocharger_small_partSave.txt";
        private const string turbocharger_small_intercooler_tube_SaveFile = "turbocharger_small_intercooler_tube_partSave.txt";
        private const string turbocharger_small_exhaust_inlet_tube_SaveFile = "turbocharger_small_exhaust_inlet_tube_partSave.txt";
        private const string turbocharger_small_exhaust_outlet_tube_SaveFile = "turbocharger_small_exhaust_outlet_tube_partSave.txt";
        private const string turbocharger_small_manifold_twinCarb_tube_SaveFile = "turbocharger_small_manifold_twinCarb_tube_partSave.txt";
        private const string turbocharger_small_airfilter_SaveFile = "turbocharger_small_airfilter_partSave.txt";

        //Other Parts
        private const string turbocharger_exhaust_header_SaveFile = "turbocharger_exhaust_header_partSave.txt";
        private const string turbocharger_hood_SaveFile = "turbocharger_hood_partSave.txt";
        private const string turbocharger_manifold_weber_SaveFile = "turbocharger_manifold_weber_partSave.txt";
        private const string turbocharger_manifold_twinCarb_SaveFile = "turbocharger_manifold_twinCarb_partSave.txt";
        private const string turbocharger_boost_gauge_SaveFile = "turbocharger_boost_gauge_partSave.txt";
        private const string turbocharger_intercooler_SaveFile = "turbocharger_intercooler_partSave.txt";
        private const string turbocharger_intercooler_manifold_tube_weber_SaveFile = "turbocharger_intercooler_manifold_tube_weber_partSave.txt";
        private const string turbocharger_intercooler_manifold_tube_twinCarb_SaveFile = "turbocharger_intercooler_manifold_tube_twinCarb_partSave.txt";

        //Other Saves
        private const string turbocharger_mod_ModsShop_SaveFile = "turbocharger_mod_ModsShop_SaveFile.txt";
        private const string turbocharger_mod_others_SaveFile = "turbocharger_mod_Others_SaveFile.txt";
        private const string turbocharger_mod_wear_SaveFile = "turbocharger_mod_wear_SaveFile.txt";
        private const string turbocharger_mod_screws_SaveFile = "turbocharger_mod_screws_SaveFile.txt";
        private string partsColorSave_SaveFile;
        //ECU-Mod Communication
        private bool ecu_mod_installed = false;
        private GameObject ecu_mod_SmartEngineModule;
        private bool ecu_mod_alsEnabled = false;

        //Everything Else
        private int currentGear = 0;
        private bool errorDetected = false;
        /* Gear: R = 0
         * Gear: N = 1
         * Gear: 1 = 2
         * Gear: 2 = 3
         * Gear: 3 = 4
         * Gear: 4 = 5
         */
        private RaycastHit hit;
        private AssetBundle assets;
        private TextMesh boostGaugeTextMesh;
        private ParticleSystem fire_fx_big_turbo_exhaust_straight;
        GameObject turbocharger_big_turbine;


        private PartSaveInfo loadSaveData(string saveFile)
        {
            try
            {
                return SaveLoad.DeserializeSaveFile<PartSaveInfo>(this, saveFile);
            }
            catch (System.NullReferenceException)
            {
                // no save file exists.. //loading default save data.
                return null;
            }
        }
        private static void SwitchPartsWearDEBUG()
        {
            partsWearDEBUG = !partsWearDEBUG;
        }

        public override void OnNewGame()
        {
            // Called once, when starting a New Game, you can reset your saves here
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_big_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_big_intercooler_tube_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_big_exhaust_inlet_tube_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_big_exhaust_outlet_tube_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_big_blowoff_valve_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_big_exhaust_outlet_straight_SaveFile);

            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_exhaust_header_SaveFile);

            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_small_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_small_intercooler_tube_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_small_exhaust_inlet_tube_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_small_exhaust_outlet_tube_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_small_manifold_twinCarb_tube_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_small_airfilter_SaveFile);

            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_hood_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_manifold_weber_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_manifold_twinCarb_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_boost_gauge_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_intercooler_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_intercooler_manifold_tube_weber_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, turbocharger_intercooler_manifold_tube_twinCarb_SaveFile);

            SaveLoad.SerializeSaveFile<PartBuySave>(this, null, turbocharger_mod_ModsShop_SaveFile);
            SaveLoad.SerializeSaveFile<OthersSave>(this, null, turbocharger_mod_others_SaveFile);
            SaveLoad.SerializeSaveFile<PartsWearSave>(this, null, turbocharger_mod_wear_SaveFile);
            WritePartsColorSave(true);
        }



        public override void OnLoad()
        {
            ModConsole.Print("DonnerTechRacing Turbocharger Mod [v" + this.Version + "]" + " started loading");
            ecu_mod_installed = ModLoader.IsModPresent("DonnerTech_ECU_Mod");
            randDestroyValue = new Random();

            modSprayColors[0] = new Color(205f / 255, 205f / 255, 205f / 255, 1f);    // white
            modSprayColors[1] = new Color(40f / 255, 40f / 255, 40f / 255, 1f);       // black
            modSprayColors[2] = new Color(205f / 255, 0f / 255, 0f / 255, 1f);        // red
            modSprayColors[3] = new Color(0f / 255, 0f / 255, 220f / 255, 1f);        // blue
            modSprayColors[4] = new Color(130f / 255, 60f / 255, 0f / 255, 1f);       // brown
            modSprayColors[5] = new Color(250f / 255, 105f / 255, 0f / 255, 1f);      // orange
            modSprayColors[6] = new Color(190f / 255, 190f / 255, 0f / 255, 1f);      // yellow
            modSprayColors[7] = new Color(0f / 255, 120f / 255, 0f / 255, 1f);        // green
            modSprayColors[8] = new Color(0f / 255, 170f / 255, 210f / 255, 1f);      // lightblue
            modSprayColors[9] = new Color(130f / 255, 130f / 255, 130f / 255, 1f);    // grey
            modSprayColors[10] = new Color(220f / 255, 55f / 255, 220f / 255, 1f);     // pink
            modSprayColors[11] = new Color(0f / 255, 0f / 255, 220f / 255, 1f);        // turquoise
            modSprayColors[12] = new Color(40f / 255, 40f / 255, 40f / 255, 1f);       // mattblack


            
            try
            {
                string modapiPath = ModLoader.GetModAssetsFolder(this);
                modapiPath = modapiPath.Replace("Assets\\SatsumaTurboCharger", "References\\modapi_v0130-alpha.dll");
                if (File.Exists(modapiPath))
                {
                    ModUI.ShowMessage(
                        "ModAPI v130 detected!! \n" +
                        "This is a modified version of the original modapi (made by @tommojphillips)\n" +
                        "The modified version was made by the maker of the Supercharger mod (@Spysi)\n" +
                        "He has now removed this version from this mod and uses the same as me\n" +
                        "This file should still be deleted as it will still cause problems for any other mod\n" +
                        "Please remove this file and restart the game."
                        , "modapi v130 detected!!!!!");
                    errorDetected = true;
                }
            }
            catch
            {
                ModConsole.Error("Could not check modapiv130 status, please check your paths");
            }

            if (errorDetected == false)
            {
                try
                {
                    elect = GameObject.Find("SATSUMA(557kg, 248)/Electricity");
                }
                catch
                {
                    ModConsole.Print("Could not find Electricity of Car");
                }
                try
                {
                    power = PlayMakerFSM.FindFsmOnGameObject(elect, "Power");
                }
                catch
                {
                    ModConsole.Print("Could not find Power of Car");
                }
                try
                {
                    satsuma = GameObject.Find("SATSUMA(557kg, 248)");
                    /*
                    N2O = satsuma.transform.Find("CarSimulation/Engine/N2O").gameObject;
                    n2oPlayMaker = N2O.GetComponent<PlayMakerFSM>();
                    for(int i = 0; i < n2oPlayMaker.FsmStates.Length; i++)
                    {
                        if(n2oPlayMaker.FsmStates[i].Name == "Boost")
                        {
                            n2oState = n2oPlayMaker.FsmStates[i];
                        }
                        else if(n2oPlayMaker.FsmStates[i].Name == "No NOS")
                        {
                            no_n2oState = n2oPlayMaker.FsmStates[i];
                        }
                    }
                    */
                    satsumaDriveTrain = satsuma.GetComponent<Drivetrain>();
                    //satsumaCarController = satsuma.GetComponent<CarController>();
                    //satsumaAxles = satsuma.GetComponent<Axles>();
                }
                catch
                {
                    ModConsole.Error("Could not find Satsuma, Drivetrain");
                }

                try
                {
                    exhaustEngine = GameObject.Find("SATSUMA(557kg, 248)/CarSimulation/Exhaust/FromEngine");
                    exhaustPipeRace = GameObject.Find("SATSUMA(557kg, 248)/CarSimulation/Exhaust/FromPipe");

                    if(exhaustPipeRace != null)
                    {
                        originalExhaustPipeRaceTransform = exhaustPipeRace.transform;
                    }
                    exhaustRaceMuffler = GameObject.Find("SATSUMA(557kg, 248)/CarSimulation/Exhaust/FromMuffler");
                    //exhaustRaceMuffler.transform.localPosition = new Vector3(-0.4f, -0.1f, -1.7f);
                }
                catch
                {

                }


                //n2oBottle = satsuma.transform.GetChild(13).GetChild(1).GetChild(7).gameObject.GetComponent<PlayMakerFSM>().FsmStates[4];
                //n2oBottlePSI = satsuma.transform.GetChild(13).GetChild(1).GetChild(7).gameObject.GetComponent<PlayMakerFSM>().FsmVariables.FloatVariables[4];
                weberCarb = GameObject.Find("racing carburators(Clone)");
                twinCarb = GameObject.Find("twin carburators(Clone)");
                steelHeaders = GameObject.Find("steel_headers(Clone)");
                racingExhaustPipe = GameObject.Find("racing exhaust(Clone)");
                racingExhaustMuffler = GameObject.Find("racing muffler(Clone)");
                originalCylinerHead = GameObject.Find("cylinder head(Clone)");

                try
                {
                    partBuySave = SaveLoad.DeserializeSaveFile<PartBuySave>(this, turbocharger_mod_ModsShop_SaveFile);
                    othersSave = SaveLoad.DeserializeSaveFile<OthersSave>(this, turbocharger_mod_others_SaveFile);
                    partsWearSave = SaveLoad.DeserializeSaveFile<PartsWearSave>(this, turbocharger_mod_wear_SaveFile);
                    if (othersSave == null || othersSave.turbocharger_big_max_boost <= 0 || othersSave.turbocharger_big_max_boost_limit <= 0 || othersSave.turbocharger_small_max_boost <= 0 || othersSave.turbocharger_small_max_boost_limit <= 0)
                    {
                        othersSave = new OthersSave
                        {
                            turbocharger_big_max_boost = 1.55f,
                            turbocharger_big_max_boost_limit = 2f,
                            turbocharger_small_max_boost = 0.8f,
                            turbocharger_small_max_boost_limit = 1f
                        };
                    }
                    if(partsWearSave == null)
                    {
                        partsWearSave = new PartsWearSave
                        {
                            turbocharger_big_wear = 100f,
                            turbocharger_small_wear = 100f,
                            intercooler_wear = 100f,
                            airfilter_wear = 100f
                        };
                    }
                }
                catch
                {

                }
                satsumaDriveTrain.clutchTorqueMultiplier = 10f;
                partsColorSave_SaveFile = ModLoader.GetModConfigFolder(this) + "\\turbocharger_parts_ColorSave.xml";
                Material[] materialCollecion = Resources.FindObjectsOfTypeAll<Material>();
                foreach (Material material in materialCollecion)
                {
                    if (material.name == "CAR_PAINT_REGULAR")
                    {
                        regularCarPaintMaterial = material;
                        break;
                    }

                }
                try
                {
                    assets = LoadAssets.LoadBundle(this, "turbochargermod.unity3d");
                }
                catch
                {
                    ModConsole.Error("turbochargermod.unity3d could not be loaded \n check Mods/Assets/SatsumaTurboCharger/turbochargermod.unity3d");
                    errorDetected = true;
                }

                turbocharger_big = (assets.LoadAsset("turbocharger_big.prefab") as GameObject);
                turbocharger_big_intercooler_tube = (assets.LoadAsset("turbocharger_big_intercooler_tube.prefab") as GameObject);
                turbocharger_big_exhaust_inlet_tube = (assets.LoadAsset("turbocharger_big_exhaust_inlet_tube.prefab") as GameObject);
                turbocharger_big_exhaust_outlet_tube = (assets.LoadAsset("turbocharger_big_exhaust_outlet_tube.prefab") as GameObject);
                turbocharger_big_blowoff_valve = (assets.LoadAsset("turbocharger_big_blowoff_valve.prefab") as GameObject);
                turbocharger_big_exhaust_outlet_straight = (assets.LoadAsset("turbocharger_big_exhaust_outlet_straight.prefab") as GameObject);

                turbocharger_exhaust_header = (assets.LoadAsset("turbocharger_big_exhaust_header.prefab") as GameObject);

                turbocharger_small = (assets.LoadAsset("turbocharger_small.prefab") as GameObject);

                turbocharger_small_intercooler_tube = (assets.LoadAsset("turbocharger_small_intercooler_tube.prefab") as GameObject);
                turbocharger_small_exhaust_inlet_tube = (assets.LoadAsset("turbocharger_small_exhaust_inlet_tube.prefab") as GameObject);
                turbocharger_small_exhaust_outlet_tube = (assets.LoadAsset("turbocharger_small_exhaust_outlet_tube.prefab") as GameObject);
                turbocharger_small_manifold_twinCarb_tube = (assets.LoadAsset("turbocharger_small_tube_manifold_twinCarb.prefab") as GameObject);
                turbocharger_small_airfilter = (assets.LoadAsset("turbocharger_small_airfilter.prefab") as GameObject);

                turbocharger_hood = (assets.LoadAsset("turbocharger_hood.prefab") as GameObject);
                turbocharger_manifold_weber = (assets.LoadAsset("turbocharger_manifold_weber.prefab") as GameObject);
                turbocharger_manifold_twinCarb = (assets.LoadAsset("turbocharger_manifold_twinCarb.prefab") as GameObject);
                turbocharger_intercooler = (assets.LoadAsset("turbocharger_intercooler.prefab") as GameObject);
                turbocharger_intercooler_manifold_tube_weber = (assets.LoadAsset("turbocharger_tube_intercooler_manifold_weber.prefab") as GameObject);
                turbocharger_intercooler_manifold_tube_twinCarb = (assets.LoadAsset("turbocharger_tube_intercooler_manifold_twinCarb.prefab") as GameObject);
                turbocharger_boost_gauge = (assets.LoadAsset("turbocharger_boost_gauge.prefab") as GameObject);



                AddPartsNames();
                AddParts_trigger(originalCylinerHead);
                AddParts();

                SortedList<String, Screws> screwListSave = ScrewablePart.LoadScrews(this, turbocharger_mod_screws_SaveFile);


                turbocharger_big_exhaust_inlet_tube_screwable = new ScrewablePart(screwListSave, this, turbocharger_big_exhaust_inlet_tube_part.rigidPart,
                    new Vector3[]
                    {
                        new Vector3(0.202f, -0.1f, -0.01f),
                        new Vector3(0.145f, -0.1f, -0.018f),
                        new Vector3(-0.12f, 0.19f, -0.005f),
                        new Vector3(-0.2f, 0.19f, -0.005f),
                    },
                    new Vector3[]
                    {
                        new Vector3(-90, 0, 0),
                        new Vector3(-90, 0, 0),
                        new Vector3(90, 0, 0),
                        new Vector3(90, 0, 0),
                    },
                    new Vector3[]
                    {
                        new Vector3(0.7f, 0.7f, 0.7f),
                        new Vector3(0.7f, 0.7f, 0.7f),
                        new Vector3(0.7f, 0.7f, 0.7f),
                        new Vector3(0.7f, 0.7f, 0.7f),
                    }, 10, "screwable_screw2");
                turbocharger_big_exhaust_outlet_tube_screwable = new ScrewablePart(screwListSave, this, turbocharger_big_exhaust_outlet_tube_part.rigidPart,
                    new Vector3[]
                    {
                        new Vector3(-0.0655f, 0.372f, -0.0425f),
                    },
                    new Vector3[]
                    {
                        new Vector3(0, -90, 0),   //Bolt1
                    },
                    new Vector3[]
                    {
                        new Vector3(0.7f, 0.7f, 0.7f),
                    }, 10, "screwable_screw2");

                turbocharger_small_screwable = new ScrewablePart(screwListSave, this, turbocharger_small_part.rigidPart,
                     new Vector3[]
                    {
                        new Vector3(0.0715f, -0.024f, 0.044f),
                        new Vector3(-0.0528f, 0.068f, -0.034f),
                    },
                    new Vector3[]
                    {
                        new Vector3(180f, 0f, 0f),
                        new Vector3(0f, -90f, 0f),
                    },
                    new Vector3[]
                    {
                        new Vector3(0.4f, 0.4f, 0.4f),
                        new Vector3(0.4f, 0.4f, 0.4f),
                    }, 10, "screwable_screw2");
                turbocharger_small_intercooler_tube_screwable = new ScrewablePart(screwListSave, this, turbocharger_small_intercooler_tube_part.rigidPart,
                    new Vector3[]
                    {
                        new Vector3(0.034f, -0.13f, -0.1638f),
                    },
                    new Vector3[]
                    {
                         new Vector3(180f, 0f, 0f),
                    },
                    new Vector3[]
                    {
                         new Vector3(0.4f, 0.4f, 0.4f),
                    }, 10, "screwable_screw2");
                //
                turbocharger_small_exhaust_inlet_tube_screwable = new ScrewablePart(screwListSave, this, turbocharger_small_exhaust_inlet_tube_part.rigidPart,
                    new Vector3[]
                    {
                        new Vector3(0.114f, -0.044f, -0.035f),
                        new Vector3(0.06f, -0.044f, -0.044f),
                    },
                    new Vector3[]
                    {
                        new Vector3(-90f, 0f, 0f),
                        new Vector3(-90f, 0f, 0f),
                    },
                    new Vector3[]
                    {
                        new Vector3(0.7f, 0.7f, 0.7f),
                        new Vector3(0.7f, 0.7f, 0.7f),
                    }, 10, "screwable_screw2");
                /*
                turbocharger_small_exhaust_outlet_tube_screwable = new ScrewablePart(screwListSave, this, turbocharger_small_exhaust_outlet_tube_part.rigidPart,
                    new Vector3[]
                    {

                    },
                    new Vector3[]
                    {

                    },
                    new Vector3[]
                    {

                    }, 10, "screwable_screw2");
                turbocharger_small_manifold_twinCarb_tube_screwable = new ScrewablePart(screwListSave, this, turbocharger_small_manifold_twinCarb_tube_part.rigidPart,
                    new Vector3[]
                    {

                    },
                    new Vector3[]
                    {

                    },
                    new Vector3[]
                    {

                    }, 10, "screwable_screw2");
                    */
                turbocharger_exhaust_header_screwable = new ScrewablePart(screwListSave, this, turbocharger_exhaust_header_part.rigidPart,
                    new Vector3[]
                    {
                        new Vector3(0.169f, 0.076f, -0.022f),   //Bolt1
                        new Vector3(0.13f, 0.0296f, -0.022f),   //Bolt2
                        new Vector3(-0.003f, 0.08f, -0.022f),   //Bolt3
                        new Vector3(-0.137f, 0.0296f, -0.022f),   //Bolt4
                        new Vector3(-0.174f, 0.076f, -0.022f)  //Bolt5
                    },
                    new Vector3[]
                    {
                        new Vector3(0, 0, 0),   //Bolt1
                        new Vector3(0, 0, 0),   //Bolt2
                        new Vector3(0, 0, 0),   //Bolt3
                        new Vector3(0, 0, 0),   //Bolt4
                        new Vector3(0, 0, 0)  //Bolt5
                    },
                    new Vector3[]
                    {
                        new Vector3(0.7f, 0.7f, 0.7f),
                        new Vector3(0.7f, 0.7f, 0.7f),
                        new Vector3(0.7f, 0.7f, 0.7f),
                        new Vector3(0.7f, 0.7f, 0.7f),
                        new Vector3(0.7f, 0.7f, 0.7f),
                    }, 8, "screwable_nut");

                turbocharger_big_intercooler_tube_screwable = new ScrewablePart(screwListSave, this, turbocharger_big_intercooler_tube_part.rigidPart,
                    new Vector3[]
                    {
                        new Vector3(0.153f, 0.324f, 0.1835f),
                        new Vector3(0.0779f, 0.324f, 0.1835f),
                        new Vector3(0.031f, -0.13f, -0.1632f),
                    },
                    new Vector3[]
                    {
                        new Vector3(90, 0, 0),
                        new Vector3(90, 0, 0),
                        new Vector3(180, 0, 0),
                    },
                    new Vector3[]
                    {
                        new Vector3(0.7f, 0.7f, 0.7f),
                        new Vector3(0.7f, 0.7f, 0.7f),
                        new Vector3(0.4f, 0.4f, 0.4f),
                    }, 10, "screwable_screw2");
                turbocharger_big_screwable = new ScrewablePart(screwListSave, this, turbocharger_big_part.rigidPart,
                    new Vector3[]
                    {
                        new Vector3(0.105f, -0.098f, -0.082f),
                        new Vector3(0.03f, -0.098f, -0.082f),
                        new Vector3(-0.0288f, -0.098f, 0.0817f),
                        new Vector3(-0.1085f, -0.098f, 0.0817f),
                    },
                    new Vector3[]
                    {
                        new Vector3(-90, 0, 0),   //Bolt1
                        new Vector3(-90, 0, 0),   //Bolt2
                        new Vector3(-90, 0, 0),   //Bolt3
                        new Vector3(-90, 0, 0),   //Bolt4
                    },
                    new Vector3[]
                    {
                        new Vector3(0.7f, 0.7f, 0.7f),
                        new Vector3(0.7f, 0.7f, 0.7f),
                        new Vector3(0.7f, 0.7f, 0.7f),
                        new Vector3(0.7f, 0.7f, 0.7f),
                    }, 10, "screwable_nut");

                turbocharger_intercooler_manifold_weberCarb_tube_screwable = new ScrewablePart(screwListSave, this, turbocharger_intercooler_manifold_tube_weber_part.rigidPart,
                    new Vector3[]
                    {
                        new Vector3(-0.0473f, -0.1205f, -0.241f),
                    },
                    new Vector3[]
                    {
                        new Vector3(180, 0, 0),
                    },
                    new Vector3[]
                    {
                        new Vector3(0.4f, 0.4f, 0.4f),
                    }, 10, "screwable_screw2");
                turbocharger_intercooler_manifold_twinCarb_tube_screwable = new ScrewablePart(screwListSave, this, turbocharger_intercooler_manifold_tube_twinCarb_part.rigidPart,
                    new Vector3[]
                    {
                        new Vector3(-0.0425f, -0.1205f, -0.241f),
                    },
                    new Vector3[]
                    {
                        new Vector3(180, 0, 0),
                    },
                    new Vector3[]
                    {
                        new Vector3(0.4f, 0.4f, 0.4f),
                    }, 10, "screwable_screw2");

                turbocharger_manifold_weberCarb_screwable = new ScrewablePart(screwListSave, this, turbocharger_manifold_weber_part.rigidPart,
                    new Vector3[]
                    {
                        new Vector3(0.2f, 0.03f, -0.009f),
                    },
                    new Vector3[]
                    {
                        new Vector3(180, 0, 0),
                    },
                    new Vector3[]
                    {
                        new Vector3(0.4f, 0.4f, 0.4f),
                    }, 10, "screwable_screw2");
                turbocharger_manifold_twinCarb_screwable = new ScrewablePart(screwListSave, this, turbocharger_manifold_twinCarb_part.rigidPart,
                    new Vector3[]
                    {
                        new Vector3(0.0105f, 0.105f, 0.03f),
                    },
                    new Vector3[]
                    {
                        new Vector3(0, 90, 0),
                    },
                    new Vector3[]
                    {
                        new Vector3(0.5f, 0.5f, 0.5f),
                    }, 10, "screwable_screw2");

                turbocharger_big_exhaust_outlet_tube_screwable.AddClampModel(new Vector3(-0.055f, 0.334f, -0.0425f), new Vector3(0, 0, 0), new Vector3(1, 1, 1));
                turbocharger_big_intercooler_tube_screwable.AddClampModel(new Vector3(0.031f, -0.154f, -0.1545f), new Vector3(0, 90, 0), new Vector3(0.62f, 0.62f, 0.62f));
                turbocharger_intercooler_manifold_weberCarb_tube_screwable.AddClampModel(new Vector3(-0.047f, -0.1465f, -0.232f), new Vector3(0, 90, 0), new Vector3(0.68f, 0.68f, 0.68f));
                turbocharger_intercooler_manifold_twinCarb_tube_screwable.AddClampModel(new Vector3(-0.042f, -0.1465f, -0.232f), new Vector3(0, 90, 0), new Vector3(0.68f, 0.68f, 0.68f));
                turbocharger_manifold_weberCarb_screwable.AddClampModel(new Vector3(0.2f, -0.002f, 0.001f), new Vector3(0, 90, 0), new Vector3(0.82f, 0.82f, 0.82f));
                turbocharger_manifold_twinCarb_screwable.AddClampModel(new Vector3(0f, 0.105f, 0f), new Vector3(90, 0, 0), new Vector3(0.8f, 0.8f, 0.8f));

                
                turbocharger_small_intercooler_tube_screwable.AddClampModel(new Vector3(0.034f, -0.154f, -0.1548f), new Vector3(0, 90, 0), new Vector3(0.62f, 0.62f, 0.62f));

                turbocharger_small_screwable.AddClampModel(new Vector3(0.0715f, -0.043f, 0.052f), new Vector3(0, 90, 0), new Vector3(0.5f, 0.5f, 0.5f));
                turbocharger_small_screwable.AddClampModel(new Vector3(-0.044f, 0.068f, -0.0525f), new Vector3(90, 0, 0), new Vector3(0.5f, 0.5f, 0.5f));
                /*
                
                turbocharger_small_exhaust_inlet_tube_screwable.AddClampModel(new Vector3(0f, 0f, 0f), new Vector3(0, 0, 0), new Vector3(1, 1, 1));
                turbocharger_small_exhaust_outlet_tube_screwable.AddClampModel(new Vector3(0f, 0f, 0f), new Vector3(0, 0, 0), new Vector3(1, 1, 1));
                turbocharger_small_manifold_twinCarb_tube_screwable.AddClampModel(new Vector3(0f, 0f, 0f), new Vector3(0, 0, 0), new Vector3(1, 1, 1));
                */

                if (ecu_mod_installed)
                {
                    ecu_mod_SmartEngineModule = GameObject.Find("Smart Engine ECU(Clone)");
                }

                assets.Unload(false);
                UnityEngine.Object.Destroy(turbocharger_big);
                UnityEngine.Object.Destroy(turbocharger_big_intercooler_tube);
                UnityEngine.Object.Destroy(turbocharger_big_exhaust_inlet_tube);
                UnityEngine.Object.Destroy(turbocharger_big_exhaust_outlet_tube);
                UnityEngine.Object.Destroy(turbocharger_big_blowoff_valve);
                UnityEngine.Object.Destroy(turbocharger_big_exhaust_outlet_straight);

                UnityEngine.Object.Destroy(turbocharger_exhaust_header);

                UnityEngine.Object.Destroy(turbocharger_small);
                UnityEngine.Object.Destroy(turbocharger_small_intercooler_tube);
                UnityEngine.Object.Destroy(turbocharger_small_exhaust_inlet_tube);
                UnityEngine.Object.Destroy(turbocharger_small_exhaust_outlet_tube);
                UnityEngine.Object.Destroy(turbocharger_small_manifold_twinCarb_tube);
                UnityEngine.Object.Destroy(turbocharger_small_airfilter);

                UnityEngine.Object.Destroy(turbocharger_hood);
                UnityEngine.Object.Destroy(turbocharger_manifold_weber);
                UnityEngine.Object.Destroy(turbocharger_manifold_twinCarb);
                UnityEngine.Object.Destroy(turbocharger_intercooler);
                UnityEngine.Object.Destroy(turbocharger_intercooler_manifold_tube_weber);
                UnityEngine.Object.Destroy(turbocharger_intercooler_manifold_tube_twinCarb);
                UnityEngine.Object.Destroy(turbocharger_boost_gauge);

                //Get PlayMaker Variables
                foreach (var playMakerFloatVar in PlayMakerGlobals.Instance.Variables.FloatVariables)
                {
                    switch (playMakerFloatVar.Name)
                    {
                        case "EnginePowerMultiplier":
                            _enginePowerMultiplier = playMakerFloatVar;
                            break;
                        case "EngineTorque":
                            _engineTorque = playMakerFloatVar;
                            break;
                        case "EngineTorqueRPM":
                            _engineTorqueRpm = playMakerFloatVar;
                            break;
                        case "EngineFriction":
                            _engineFriction = playMakerFloatVar;
                            break;
                    }
                }

                GameObject inspectionProcess = GameObject.Find("InspectionProcess");
                inspectionPlayMakerFsm = inspectionProcess.GetComponents<PlayMakerFSM>()[0];
                foreach (PlayMakerFSM playMakerFSM in inspectionProcess.GetComponents<PlayMakerFSM>())
                {
                    if (playMakerFSM.FsmName == "Inspect")
                    {
                        inspectionPlayMakerFsm = playMakerFSM;
                        break;
                    }
                }
                foreach (FsmEvent fsmEvent in inspectionPlayMakerFsm.FsmEvents)
                {
                    if (fsmEvent.Name == "FAIL")
                    {
                        inspectionFailedEvent = fsmEvent;
                        break;
                    }
                }

                FsmHook.FsmInject(inspectionProcess, "Results", InspectionResults);


                ModConsole.Print("DonnerTechRacing Turbocharger Mod [v" + this.Version + "]" + " finished loading");
            }
            else
            {
                ModConsole.Error("<b>[SatsumaTurbocharger]</b> - an error occured while trying to load mods");
            }

            // Increasing grip of tyres
            /*
            foreach (Wheel wheel in satsumaDriveTrain.poweredWheels)
            {
                PlayMakerFSM wheelDataFSM = PlayMakerFSM.FindFsmOnGameObject(wheel.gameObject, "Data");
                foreach (FsmState t in wheelDataFSM.FsmStates)
                {
                    if(t.Name == "State 2")
                    {
                    }
                }
            }

            */
            
        }
        public void InspectionResults()
        {
            if (
                turbocharger_big_part.installed
                || turbocharger_big_intercooler_tube_part.installed
                || turbocharger_big_exhaust_inlet_tube_part.installed
                || turbocharger_big_exhaust_outlet_tube_part.installed
                || turbocharger_big_blowoff_valve_part.installed
                || turbocharger_big_exhaust_outlet_straight_part.installed

                || turbocharger_exhaust_header_part.installed

                || turbocharger_small_part.installed
                || turbocharger_small_intercooler_tube_part.installed
                || turbocharger_small_exhaust_inlet_tube_part.installed
                || turbocharger_small_exhaust_outlet_tube_part.installed
                || turbocharger_small_airfilter_part.installed
                || turbocharger_small_manifold_twinCarb_tube_part.installed

                || turbocharger_hood_part.installed

                || turbocharger_manifold_weber_part.installed
                || turbocharger_manifold_twinCarb_part.installed
                || turbocharger_boost_gauge_part.installed
                || turbocharger_intercooler_part.installed
                || turbocharger_intercooler_manifold_tube_weber_part.installed
                || turbocharger_intercooler_manifold_tube_twinCarb_part.installed
            )
            {
                PlayMakerFSM.BroadcastEvent(inspectionFailedEvent);
            }
        }

        public override void ModSettings()
        {
            Settings.AddHeader(this, "DEBUG");
            Settings.AddButton(this, DEBUG_parts_wear, "DEBUG parts wear");
            Settings.AddButton(this, DEBUG_turbo_values, "DEBUG TurboCharger GUI");
            Settings.AddText(this, "");
            Settings.AddHeader(this, "Settings");
            Settings.AddCheckBox(this, useDefaultColorsSetting);
            Settings.AddButton(this, resetPosSetting, "reset part location");
        }

        public override void OnSave()
        {
            try
            {
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, turbocharger_big_part.getSaveInfo(), turbocharger_big_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, turbocharger_big_intercooler_tube_part.getSaveInfo(), turbocharger_big_intercooler_tube_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, turbocharger_big_exhaust_inlet_tube_part.getSaveInfo(), turbocharger_big_exhaust_inlet_tube_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, turbocharger_big_exhaust_outlet_tube_part.getSaveInfo(), turbocharger_big_exhaust_outlet_tube_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, turbocharger_big_blowoff_valve_part.getSaveInfo(), turbocharger_big_blowoff_valve_SaveFile);

                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, turbocharger_big_exhaust_outlet_straight_part.getSaveInfo(), turbocharger_big_exhaust_outlet_straight_SaveFile);


                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, turbocharger_exhaust_header_part.getSaveInfo(), turbocharger_exhaust_header_SaveFile);

                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, turbocharger_small_part.getSaveInfo(), turbocharger_small_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, turbocharger_small_intercooler_tube_part.getSaveInfo(), turbocharger_small_intercooler_tube_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, turbocharger_small_exhaust_inlet_tube_part.getSaveInfo(), turbocharger_small_exhaust_inlet_tube_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, turbocharger_small_exhaust_outlet_tube_part.getSaveInfo(), turbocharger_small_exhaust_outlet_tube_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, turbocharger_small_manifold_twinCarb_tube_part.getSaveInfo(), turbocharger_small_manifold_twinCarb_tube_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, turbocharger_small_airfilter_part.getSaveInfo(), turbocharger_small_airfilter_SaveFile);

                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, turbocharger_hood_part.getSaveInfo(), turbocharger_hood_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, turbocharger_manifold_weber_part.getSaveInfo(), turbocharger_manifold_weber_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, turbocharger_manifold_twinCarb_part.getSaveInfo(), turbocharger_manifold_twinCarb_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, turbocharger_boost_gauge_part.getSaveInfo(), turbocharger_boost_gauge_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, turbocharger_intercooler_part.getSaveInfo(), turbocharger_intercooler_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, turbocharger_intercooler_manifold_tube_weber_part.getSaveInfo(), turbocharger_intercooler_manifold_tube_weber_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, turbocharger_intercooler_manifold_tube_twinCarb_part.getSaveInfo(), turbocharger_intercooler_manifold_tube_twinCarb_SaveFile);
                SaveLoad.SerializeSaveFile<PartBuySave>(this, partBuySave, turbocharger_mod_ModsShop_SaveFile);
                SaveLoad.SerializeSaveFile<OthersSave>(this, othersSave, turbocharger_mod_others_SaveFile);
                SaveLoad.SerializeSaveFile<PartsWearSave>(this, partsWearSave, turbocharger_mod_wear_SaveFile);


                ScrewablePart.SaveScrews(this, new ScrewablePart[]
                {
                    turbocharger_exhaust_header_screwable,
                    turbocharger_big_exhaust_inlet_tube_screwable,
                    turbocharger_big_exhaust_outlet_tube_screwable,
                    turbocharger_big_intercooler_tube_screwable,
                    turbocharger_big_screwable,
                    turbocharger_manifold_weberCarb_screwable,
                    turbocharger_manifold_twinCarb_screwable,
                    turbocharger_intercooler_manifold_weberCarb_tube_screwable,
                    turbocharger_intercooler_manifold_twinCarb_tube_screwable,
                }, turbocharger_mod_screws_SaveFile);

                WritePartsColorSave(false);
            }
            catch (System.Exception ex)
            {
                ModConsole.Error("<b>[SatsumaTurbocharger]</b> - an error occured while attempting to save part info.. see: " + ex.ToString());
            }
        }

        public override void OnGUI()
        {
            if (partsWearDEBUG)
            {
                GUI.Label(new Rect(20, 20, 200, 100), "Wear information:");
                GUI.Label(new Rect(20, 40, 200, 100), "Big turbo wear:   " + partsWearSave.turbocharger_big_wear.ToString("0.000"));
                GUI.Label(new Rect(20, 60, 200, 100), "Small turbo wear: " + partsWearSave.turbocharger_small_wear.ToString("0.000"));
                GUI.Label(new Rect(20, 80, 200, 100), "Intercooler wear: " + partsWearSave.intercooler_wear.ToString("0.000"));
                GUI.Label(new Rect(20, 100, 200, 100), "Airfilter wear:   " + partsWearSave.airfilter_wear.ToString("0.000"));
                GUI.Label(new Rect(20, 120, 200, 100), "-----------------------------------");
            }

            if (turboValuesDEBUG == true)
            {
                GUI.Label(new Rect(20, 140, 200, 100), "------------------------------------");
                GUI.Label(new Rect(20, 160, 200, 100), "Engine RPM: " + ((int)engineRPM).ToString());
                GUI.Label(new Rect(20, 180, 200, 100), "Turbo Charger bar: " + newTurboChargerBar.ToString("n3"));
                GUI.Label(new Rect(20, 200, 200, 100), "Power Current: " + ((int)enginePowerCurrent).ToString());
                GUI.Label(new Rect(20, 220, 200, 100), "Power Multiplier: " + _enginePowerMultiplier.Value.ToString("n2"));
                GUI.Label(new Rect(20, 240, 200, 100), "km/h: " + ((int)satsumaDriveTrain.differentialSpeed));
                GUI.Label(new Rect(20, 260, 200, 100), "Torque: " + satsumaDriveTrain.torque);
                GUI.Label(new Rect(20, 280, 200, 100), "Clutch Max Torque: " + satsumaDriveTrain.clutchMaxTorque);
                GUI.Label(new Rect(20, 300, 200, 100), "Clutch Torque Multiplier: " + satsumaDriveTrain.clutchTorqueMultiplier);
                //GUI.Label(new Rect(20, 200, 200, 100), "N2o active: " + n2oBottle.Active);
                GUI.Label(new Rect(20, 320, 200, 100), "Electricity on: " + electricityOn);
                GUI.Label(new Rect(20, 340, 200, 100), "------------------------------------");
            }
        }
        public override void Update()
        {
            /*
            satsumaDriveTrain.canStall = false;
            if (Input.GetKeyDown(KeyCode.Keypad7))
            {
                sidewaysGrip += 0.1f;
            }
            else if (Input.GetKeyDown(KeyCode.Keypad1))
            {
                sidewaysGrip -= 0.1f;
            }

            if (Input.GetKeyDown(KeyCode.Keypad8))
            {
                forwardGrip += 0.1f;
            }
            else if (Input.GetKeyDown(KeyCode.Keypad2))
            {
                forwardGrip -= 0.1f;
            }

            if (Input.GetKeyDown(KeyCode.Keypad9))
            {

            }
            else if (Input.GetKeyDown(KeyCode.Keypad3))
            {

            }

            foreach (Wheel wheel in satsumaDriveTrain.poweredWheels)
            {
                if(wheel.name == "wheelRL" || wheel.name == "wheelRR")
                {
                    PlayMakerFSM wheelDataFSM = PlayMakerFSM.FindFsmOnGameObject(wheel.gameObject, "SetGrip");
                    FsmFloat forwardGrip = wheelDataFSM.FsmVariables.FindFsmFloat("GripForward");
                    FsmFloat sidewaysGrip = wheelDataFSM.FsmVariables.FindFsmFloat("GripSideways");
                    sidewaysGrip.Value = this.sidewaysGrip;
                    forwardGrip.Value = this.forwardGrip;
                    ModConsole.Print(forwardGrip.Value + " | " + sidewaysGrip.Value);
                }
                else if (wheel.name == "wheelFL" || wheel.name == "wheelFR")
                {
                    wheel.maxSteeringAngle = 60f;
                }
                
            }
            */

            if (turbocharger_big_exhaust_outlet_straight_part.installed && fire_fx_big_turbo_exhaust_straight == null && backfire_fx_big_turbo_exhaust_straight == null)
            {
                backfire_fx_big_turbo_exhaust_straight = turbocharger_big_exhaust_outlet_straight_part.rigidPart.GetComponentInChildren<AudioSource>();
                fire_fx_big_turbo_exhaust_straight = turbocharger_big_exhaust_outlet_straight_part.rigidPart.GetComponentInChildren<ParticleSystem>();
            }
            

            //ModsShop purchashed workaround
            List<ModsShop.ShopItems> shopItems = shop.fleetariShopItems;
            foreach (ModsShop.ShopItems shopItem in shopItems)
            {
                if(shopItem.details.productName == "REPAIR Racing Turbocharger")
                {
                    shopItem.purchashed = false;
                }
                else if (shopItem.details.productName == "REPAIR GT Turbocharger")
                {
                    shopItem.purchashed = false;
                }
                else if(shopItem.details.productName == "REPAIR GT Turbo Airfilter")
                {
                    shopItem.purchashed = false;
                }
                else if(shopItem.details.productName == "REPAIR Intercooler")
                {
                    shopItem.purchashed = false;
                }
            }


            electricityOn = power.FsmVariables.FindFsmBool("ElectricsOK").Value;
            AddPartsColorMaterial();
            DetectPaintingPart();
            
            turbocharger_exhaust_header_screwable.DetectScrewing();
            turbocharger_big_exhaust_inlet_tube_screwable.DetectScrewing();
            turbocharger_big_exhaust_outlet_tube_screwable.DetectScrewing();
            turbocharger_big_intercooler_tube_screwable.DetectScrewing();
            turbocharger_big_screwable.DetectScrewing();

            turbocharger_small_intercooler_tube_screwable.DetectScrewing();
            turbocharger_small_screwable.DetectScrewing();
            turbocharger_small_exhaust_inlet_tube_screwable.DetectScrewing();
            //turbocharger_small_exhaust_outlet_tube_screwable.DetectScrewing();
            //turbocharger_small_manifold_twinCarb_tube_screwable.DetectScrewing();

            turbocharger_manifold_weberCarb_screwable.DetectScrewing();
            turbocharger_manifold_twinCarb_screwable.DetectScrewing();
            turbocharger_intercooler_manifold_weberCarb_tube_screwable.DetectScrewing();
            turbocharger_intercooler_manifold_twinCarb_tube_screwable.DetectScrewing();

            CheckPartsInstalled_trigger();
            if (turbocharger_boost_gauge_part.installed)
            {
                boostGauge = turbocharger_boost_gauge_part.rigidPart;
                boostGaugeTextMesh = boostGauge.GetComponentInChildren<TextMesh>();
            }
            else
                boostGauge = null;
            
            if (!turbocharger_big_blowoff_valve_part.installed)
            {
                if (turboLoopBig != null && turboLoopBig.isPlaying)
                {
                    turbocharger_loop_big.Stop();
                }
                if (turboGrindingLoop != null && turboGrindingLoop.isPlaying)
                {
                    turbocharger_grinding_loop.Stop();
                }
                if (turboBlowOffShot != null && turboBlowOffShot.isPlaying)
                {
                    turbocharger_blowoff.Stop();
                }
            }

            if(turbocharger_big_part.installed && turbocharger_big_intercooler_tube_part.installed && turbocharger_big_exhaust_inlet_tube_part.installed && (turbocharger_big_exhaust_outlet_tube_part.installed || turbocharger_big_exhaust_outlet_straight_part.installed) && turbocharger_big_blowoff_valve_part.installed)
            {
                allBigPartsInstalled = true;
            }
            else
            {
                allBigPartsInstalled = false;
            }
            if(turbocharger_small_part.installed && (turbocharger_small_intercooler_tube_part.installed || turbocharger_small_manifold_twinCarb_tube_part.installed) && turbocharger_small_exhaust_inlet_tube_part.installed && turbocharger_small_exhaust_outlet_tube_part.installed)
            {
                allSmallPartsInstalled = true;
            }
            else
            {
                allSmallPartsInstalled = false;
            }

            if (
                (turbocharger_manifold_weber_part.installed || turbocharger_manifold_twinCarb_part.installed) && ((turbocharger_intercooler_part.installed && (turbocharger_intercooler_manifold_tube_weber_part.installed || turbocharger_intercooler_manifold_tube_twinCarb_part.installed)) || turbocharger_small_manifold_twinCarb_tube_part.installed))
            {
                allOtherPartsInstalled = true;
            }
            else
            {
                allOtherPartsInstalled = false;
            }

            if(turbocharger_big_screwable.partFixed && turbocharger_big_intercooler_tube_screwable.partFixed && turbocharger_big_exhaust_inlet_tube_screwable.partFixed /*&& (turbocharger_big_exhaust_outlet_tube_screwable.partFixed) */)
            {
                allBigPartsScrewed = true;
            }
            else
            {
                allBigPartsScrewed = false;
            }

            if(turbocharger_small_screwable.partFixed && turbocharger_small_exhaust_inlet_tube_screwable.partFixed && turbocharger_small_exhaust_outlet_tube_screwable.partFixed && (turbocharger_small_intercooler_tube_screwable.partFixed || turbocharger_small_manifold_twinCarb_tube_screwable.partFixed))
            {
                allSmallPartsScrewed = true;
            }
            else
            {
                allSmallPartsScrewed = false;
            }

            if((turbocharger_intercooler_manifold_twinCarb_tube_screwable.partFixed || turbocharger_intercooler_manifold_weberCarb_tube_screwable.partFixed) && (turbocharger_manifold_twinCarb_screwable.partFixed || turbocharger_manifold_weberCarb_screwable.partFixed) && turbocharger_exhaust_header_screwable.partFixed)
            {
                allOtherPartsScrewed = true;
            }
            else
            {
                allOtherPartsScrewed = false;
            }

            if(((!allBigPartsInstalled && !allSmallPartsInstalled) || !allOtherPartsInstalled) || ((!allBigPartsScrewed && !allSmallPartsScrewed) || !allOtherPartsScrewed))
            {
                if (turbocharger_boost_gauge_part.installed)
                {
                    if (electricityOn == true)
                    {
                        boostGaugeTextMesh.text = "ERR";
                    }
                    else
                    {
                        boostGaugeTextMesh.text = "";
                    }
                }
                if (turboGrindingLoop != null && turboGrindingLoop.isPlaying)
                {
                    turbocharger_grinding_loop.Stop();
                }
            }

            if (!allBigPartsInstalled || !allOtherPartsInstalled)
            {
                if(turboLoopBig != null && turboLoopBig.isPlaying)
                {
                    turbocharger_loop_big.Stop();
                }
                
                if(turboBlowOffShot != null && turboBlowOffShot.isPlaying)
                {
                    turbocharger_blowoff.Stop();
                }
            }

            if(turboGrindingLoop != null && turboGrindingLoop.isPlaying && satsumaDriveTrain.rpm <= 200)
            {
                turbocharger_grinding_loop.Stop();
            }

            if (!allSmallPartsInstalled ||! allOtherPartsInstalled)
            {
                if(turboLoopSmall != null && turboLoopSmall.isPlaying)
                {
                    turbocharger_loop_small.Stop();
                }
            }

            if (((allBigPartsInstalled || allSmallPartsInstalled) && allOtherPartsInstalled) && ((allBigPartsScrewed || allSmallPartsScrewed) && allOtherPartsScrewed))
            {
                if (turbocharger_big_part.installed)
                {
                    timer_wear_turbocharger_big += Time.deltaTime;
                }
                else if (turbocharger_small_part.installed)
                {
                    timer_wear_turbocharger_small += Time.deltaTime;
                }
                if (turbocharger_small_airfilter_part.installed)
                {
                    timer_wear_airfilter += Time.deltaTime;
                }
                if (turbocharger_intercooler_part.installed)
                {
                    timer_wear_intercooler += Time.deltaTime;
                }

                timeSinceLastBlowOff += Time.deltaTime; //timer for checking if turbocharger blowoff sound is allowed to trigger

                enginePowerMultiplier = satsumaDriveTrain.powerMultiplier; //load the powerMultiplier of the car into a variable to be used later
                enginePowerCurrent = satsumaDriveTrain.currentPower; //load the current power generated of the car into a variable to be used later
                engineRPM = satsumaDriveTrain.rpm; //load the engine rpm of the car into a variable to be used later      

                RotateTurbineWheel();

                if (timeSinceLastBlowOff >= 0.3f)
                {
                    CalculateAndSetEnginePowerTurbo();
                }
                else
                {
                    if (allBigPartsInstalled)
                    {
                        newTurboChargerBar = 0.90f;
                        enginePowerMultiplier = 0.90f;
                    }
                    else if (allSmallPartsInstalled)
                    {
                        newTurboChargerBar = 0.96f;
                        enginePowerMultiplier = 0.96f;
                    }
                }

                

                //Continous Loop of turbo sound -> if not already exists it will be created and played
                if (allBigPartsInstalled && allBigPartsScrewed)
                {
                    if (turboLoopBig == null)
                    {
                        CreateTurboLoopBig();
                    }
                    else if (turboLoopBig.isPlaying == false)
                        turboLoopBig.Play();
                }
                else if (allSmallPartsInstalled && allSmallPartsScrewed)
                {
                    if (turboLoopSmall == null)
                    {
                        CreateTurboLoopSmall();
                    }
                    else if (turboLoopSmall.isPlaying == false)
                        turboLoopSmall.Play();
                }

                //Set Volume and Pitch based on engineRPM -> to make sound go ssssSuuuuuUUUUUUU
                if (allBigPartsInstalled && allBigPartsScrewed)
                {
                    if (turboLoopBig != null)
                    {
                        turboLoopBig.volume = engineRPM * 0.00005f;
                        turboLoopBig.pitch = engineRPM * 0.00018f;
                    }

                }
                else if (allSmallPartsInstalled && allSmallPartsScrewed && turbocharger_small_airfilter_part.installed)
                {
                    if (turboLoopSmall != null)
                    {
                        turboLoopSmall.volume = engineRPM * 0.00003f;
                        turboLoopSmall.pitch = (engineRPM - 500) * 0.0003f;
                    }
                }
                else if (allSmallPartsInstalled && allSmallPartsScrewed)
                {
                    if (turboLoopSmall != null)
                    {
                        turboLoopSmall.volume = engineRPM * 0.000026f;
                        turboLoopSmall.pitch = (engineRPM - 500) * 0.00045f;
                    }
                }


                //If throttleKey is Pressed (W) and engineRPM is above 4000rpm AND one second has past since last blowoff sound
                //set turbocharger_blowoffShotAllowed to true -> a blowoff sound can be fired
                if (useThrottleButton && engineRPM > 4000 && timeSinceLastBlowOff > 1)
                {
                    turbocharger_blowoffShotAllowed = true;
                }

                // || (currentGear != satsumaDriveTrain.gear && engineRPM > 4000 && turbocharger_blowoffShotAllowed == true)
                //If ThrottleKey is NOT Pressed (W) and blowoff sound is allowed -> create blowoff sound if not already exists and set turbocharger_blowoffShotAllowed back to false
                if ((!useThrottleButton && turbocharger_blowoffShotAllowed == true))
                {
                    currentGear = satsumaDriveTrain.gear;
                    if (allBigPartsInstalled && allBigPartsScrewed && !ecu_mod_alsEnabled)
                    {
                        SetBoostGaugeText(0.10f, false);
                    }
                    else if (allSmallPartsInstalled && allBigPartsScrewed && ecu_mod_alsEnabled)
                    {
                        SetBoostGaugeText(0.04f, false);
                    }
                    _triggerBlowOff();
                }
            }

            CheckPartsForDamage();
        }

        private void CheckPartsForDamage()
        {
            if (Camera.main != null)
            {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 0.8f, 1 << LayerMask.NameToLayer("Parts")) != false)
                {
                    GameObject gameObjectHit;
                    gameObjectHit = hit.collider?.gameObject;
                    if (gameObjectHit != null)
                    {
                        if (hit.collider)
                        {
                            if (gameObjectHit.name == "Racing Turbocharger" || gameObjectHit.name == "Racing Turbocharger(Clone)")
                            {
                                if (!turbocharger_big_part.installed)
                                {
                                    if (partsWearSave.turbocharger_big_wear >= 75f)
                                        ModClient.guiInteraction = "Looks brand new...";
                                    else if (partsWearSave.turbocharger_big_wear >= 50f)
                                        ModClient.guiInteraction = "Not sure if this thing should wobble that much";
                                    else if (partsWearSave.turbocharger_big_wear >= 25f)
                                        ModClient.guiInteraction = "Nope... This should not wobble that much";
                                    else if (partsWearSave.turbocharger_big_wear >= 15f)
                                        ModClient.guiInteraction = "I could pull it out. If I wanted to...";
                                    else if (partsWearSave.turbocharger_big_wear < 15f)
                                        ModClient.guiInteraction = "Well... I think it's fucked";
                                }
                            }
                            else if (gameObjectHit.name == "GT Turbocharger" || gameObjectHit.name == "GT Turbocharger(Clone)")
                            {
                                if (!turbocharger_small_part.installed)
                                {
                                    if (partsWearSave.turbocharger_small_wear >= 75f)
                                        ModClient.guiInteraction = "Looks brand new...";
                                    else if (partsWearSave.turbocharger_small_wear >= 50f)
                                        ModClient.guiInteraction = "Not sure if this thing should wobble that much";
                                    else if (partsWearSave.turbocharger_small_wear >= 25f)
                                        ModClient.guiInteraction = "Nope... This should not wobble that much";
                                    else if (partsWearSave.turbocharger_small_wear >= 15f)
                                        ModClient.guiInteraction = "I could pull it out. If I wanted to...";
                                    else if (partsWearSave.turbocharger_small_wear < 15f)
                                        ModClient.guiInteraction = "Well... I think it's fucked";
                                }
                            }
                            else if (gameObjectHit.name == "GT Turbocharger Airfilter" || gameObjectHit.name == "GT Turbocharger Airfilter(Clone)")
                            {
                                if(!turbocharger_small_airfilter_part.installed){
                                    if (partsWearSave.airfilter_wear >= 75f)
                                        ModClient.guiInteraction = "Looks brand new...";
                                    else if (partsWearSave.airfilter_wear >= 50f)
                                        ModClient.guiInteraction = "I can see some small holes in the fabric...";
                                    else if (partsWearSave.airfilter_wear >= 25f)
                                        ModClient.guiInteraction = "With those large holes I should get more power right?";
                                    else if (partsWearSave.airfilter_wear >= 15f)
                                        ModClient.guiInteraction = "I can nearly fit my hand into the holes";
                                    else if (partsWearSave.airfilter_wear < 15f)
                                        ModClient.guiInteraction = "Well... I think it's fucked";
                                }
                            }
                            else if (gameObjectHit.name == "Intercooler" || gameObjectHit.name == "Intercooler(Clone)")
                            {
                                if(!turbocharger_intercooler_part.installed){
                                    if (partsWearSave.intercooler_wear >= 75f)
                                        ModClient.guiInteraction = "Looks brand new...";
                                    else if (partsWearSave.intercooler_wear >= 50f)
                                        ModClient.guiInteraction = "Some scratches and little damage. Should be fine I guess...";
                                    else if (partsWearSave.intercooler_wear >= 25f)
                                        ModClient.guiInteraction = "I can hear air escaping more than before";
                                    else if (partsWearSave.intercooler_wear >= 15f)
                                        ModClient.guiInteraction = "It sounds like a leaf blower from those holes";
                                    else if (partsWearSave.intercooler_wear < 15f)
                                        ModClient.guiInteraction = "Well... I think it's fucked";
                                }
                            }
                        }
                    }
                }
            }
        }

        private void CheckPartsWear()
        {      
            if (turbocharger_big_part.installed)
            {
                if (partsWearSave.turbocharger_big_wear <= 0f)
                {
                    turbocharger_big_part.removePart();
                }
                else if (partsWearSave.turbocharger_big_wear <= 15f)
                {

                    int randVal = randDestroyValue.Next(400);
                    if (randVal == 1)
                    {
                        //Part should destroy
                        turbocharger_big_part.removePart();
                    }
                }
            }
            else if (turbocharger_small_part.installed)
            {
                if (partsWearSave.turbocharger_small_wear <= 0f)
                {
                    turbocharger_small_part.removePart();
                }
                else if (partsWearSave.turbocharger_small_wear <= 15f)
                {

                    int randVal = randDestroyValue.Next(400);
                    if (randVal == 1)
                    {
                        //Part should destroy
                        turbocharger_small_part.removePart();
                    }
                }
            }

            if (turbocharger_small_airfilter_part.installed)
            {
                if (partsWearSave.airfilter_wear <= 0f)
                {
                    turbocharger_small_airfilter_part.removePart();
                }
                else if (partsWearSave.airfilter_wear <= 15f)
                {

                    int randVal = randDestroyValue.Next(400);
                    if (randVal == 1)
                    {
                        //Part should destroy
                        turbocharger_small_airfilter_part.removePart();
                    }
                }
            }
            if (turbocharger_intercooler_part.installed)
            {
                if (partsWearSave.intercooler_wear <= 0f)
                {
                    turbocharger_intercooler_part.removePart();
                }
                else if (partsWearSave.intercooler_wear <= 15f)
                {
                    
                    int randVal = randDestroyValue.Next(400);
                    if(randVal == 1)
                    {
                        //Part should destroy
                        turbocharger_intercooler_part.removePart();
                    }
                }
            }
            
            
            
            
        }

        private void CheckPartsInstalled_trigger()
        {
            if (turbocharger_big_part.installed || turbocharger_big_intercooler_tube_part.installed || turbocharger_big_exhaust_inlet_tube_part.installed || (turbocharger_big_exhaust_outlet_tube_part.installed || turbocharger_big_exhaust_outlet_straight_part.installed) || turbocharger_big_blowoff_valve_part.installed)
            {
                turbocharger_small_trigger.triggerGameObject.SetActive(false);
                turbocharger_small_intercooler_tube_trigger.triggerGameObject.SetActive(false);
                turbocharger_small_exhaust_inlet_tube_trigger.triggerGameObject.SetActive(false);
                turbocharger_small_exhaust_outlet_tube_trigger.triggerGameObject.SetActive(false);
                turbocharger_small_manifold_twinCarb_tube_trigger.triggerGameObject.SetActive(false);
            }
            else
            {
                turbocharger_small_trigger.triggerGameObject.SetActive(true);
                turbocharger_small_intercooler_tube_trigger.triggerGameObject.SetActive(true);
                turbocharger_small_exhaust_inlet_tube_trigger.triggerGameObject.SetActive(true);
                turbocharger_small_exhaust_outlet_tube_trigger.triggerGameObject.SetActive(true);
                turbocharger_small_manifold_twinCarb_tube_trigger.triggerGameObject.SetActive(true);
            }

            if (turbocharger_big_exhaust_outlet_straight_part.installed)
            {
                turbocharger_big_exhaust_outlet_tube_trigger.triggerGameObject.SetActive(false);
            }
            else
            {
                turbocharger_big_exhaust_outlet_tube_trigger.triggerGameObject.SetActive(true);
            }

            if (turbocharger_big_exhaust_outlet_tube_part.installed)
            {
                turbocharger_big_exhaust_outlet_straight_trigger.triggerGameObject.SetActive(false);
            }
            else
            {
                turbocharger_big_exhaust_outlet_straight_trigger.triggerGameObject.SetActive(true);
            }

            if (turbocharger_small_part.installed || (turbocharger_small_intercooler_tube_part.installed || turbocharger_small_manifold_twinCarb_tube_part.installed) || turbocharger_small_exhaust_inlet_tube_part.installed || turbocharger_small_exhaust_outlet_tube_part.installed)
            {
                turbocharger_big_trigger.triggerGameObject.SetActive(false);
                turbocharger_big_intercooler_tube_trigger.triggerGameObject.SetActive(false);
                turbocharger_big_exhaust_inlet_tube_trigger.triggerGameObject.SetActive(false);
                turbocharger_big_exhaust_outlet_tube_trigger.triggerGameObject.SetActive(false);
                turbocharger_big_blowoff_valve_trigger.triggerGameObject.SetActive(false);
                turbocharger_big_exhaust_outlet_straight_trigger.triggerGameObject.SetActive(false);
            }
            else
            {
                turbocharger_big_trigger.triggerGameObject.SetActive(true);
                turbocharger_big_intercooler_tube_trigger.triggerGameObject.SetActive(true);
                turbocharger_big_exhaust_inlet_tube_trigger.triggerGameObject.SetActive(true);
                turbocharger_big_exhaust_outlet_tube_trigger.triggerGameObject.SetActive(true);
                turbocharger_big_blowoff_valve_trigger.triggerGameObject.SetActive(true);
                turbocharger_big_exhaust_outlet_straight_trigger.triggerGameObject.SetActive(true);
            }

            if (turbocharger_small_intercooler_tube_part.installed)
            {
                turbocharger_small_manifold_twinCarb_tube_trigger.triggerGameObject.SetActive(false);
            }
            else
            {
                turbocharger_small_manifold_twinCarb_tube_trigger.triggerGameObject.SetActive(true);
            }

            if (turbocharger_small_manifold_twinCarb_tube_part.installed)
            {
                turbocharger_small_intercooler_tube_trigger.triggerGameObject.SetActive(false);
                turbocharger_manifoldTwinCarb_trigger.triggerGameObject.SetActive(false);

                turbocharger_intercooler_trigger.triggerGameObject.SetActive(false);
                turbocharger_intercooler_manifold_tube_weber_trigger.triggerGameObject.SetActive(false);
                turbocharger_intercooler_manifold_tube_twinCarb_trigger.triggerGameObject.SetActive(false);
            }
            else
            {
                turbocharger_small_intercooler_tube_trigger.triggerGameObject.SetActive(true);
                turbocharger_manifoldTwinCarb_trigger.triggerGameObject.SetActive(true);

                turbocharger_intercooler_trigger.triggerGameObject.SetActive(true);
                turbocharger_intercooler_manifold_tube_weber_trigger.triggerGameObject.SetActive(true);
                turbocharger_intercooler_manifold_tube_twinCarb_trigger.triggerGameObject.SetActive(true);
            }

            try
            {
                weberCarb_inst = weberCarb.transform.parent.transform.parent.name == "cylinder head(Clone)";
            }
            catch
            {
                try
                {
                    weberCarb_inst = GameObject.Find("racing carburators(Clone)").transform.parent.transform.parent.name == "cylinder head(Clone)";
                }
                catch { }
            }
            try
            {
                twinCarb_inst = twinCarb.transform.parent.transform.parent.name == "cylinder head(Clone)";
            }
            catch
            {
                try
                {
                    twinCarb_inst = GameObject.Find("twin carburators(Clone)").transform.parent.transform.parent.name == "cylinder head(Clone)";
                }
                catch { }
            }
            try
            {
                racingExhaustPipe_inst = racingExhaustPipe.transform.parent.name == "pivot_exhaust pipe";
            }
            catch
            {
                try
                {
                    racingExhaustPipe_inst = GameObject.Find("racing exhaust(Clone)").transform.parent.transform.parent.name == "pivot_exhaust pipe";
                }
                catch { }
            }

            try
            {
                racingExhaustPipeTightness = racingExhaustPipe.GetComponent<PlayMakerFSM>().FsmVariables.GetFsmFloat("Tightness");
            }
            catch
            {

            }

            try
            {
                racingExhaustMuffler_inst = racingExhaustMuffler.transform.parent.name == "pivot_exhaust_muffler";
            }
            catch
            {
                try
                {
                    racingExhaustMuffler_inst = GameObject.Find("racing muffler(Clone)").transform.parent.transform.parent.name == "pivot_exhaust_muffler";
                }
                catch { }
            }


            if (weberCarb_inst)
            {

                turbocharger_manifoldWeber_trigger.triggerGameObject.SetActive(true);
                turbocharger_intercooler_manifold_tube_weber_trigger.triggerGameObject.SetActive(true);

                turbocharger_manifoldTwinCarb_trigger.triggerGameObject.SetActive(false);
                turbocharger_intercooler_manifold_tube_twinCarb_trigger.triggerGameObject.SetActive(false);
            }
            else
            {
                turbocharger_manifoldWeber_trigger.triggerGameObject.SetActive(false);
                turbocharger_intercooler_manifold_tube_weber_trigger.triggerGameObject.SetActive(false);

                turbocharger_manifoldTwinCarb_trigger.triggerGameObject.SetActive(true);
                turbocharger_intercooler_manifold_tube_twinCarb_trigger.triggerGameObject.SetActive(true);

            }

            if (twinCarb_inst)
            {
                turbocharger_manifoldTwinCarb_trigger.triggerGameObject.SetActive(true);
                turbocharger_intercooler_manifold_tube_twinCarb_trigger.triggerGameObject.SetActive(true);

                turbocharger_manifoldWeber_trigger.triggerGameObject.SetActive(false);
                turbocharger_intercooler_manifold_tube_weber_trigger.triggerGameObject.SetActive(false);
            }
            else
            {
                turbocharger_manifoldTwinCarb_trigger.triggerGameObject.SetActive(false);
                turbocharger_intercooler_manifold_tube_twinCarb_trigger.triggerGameObject.SetActive(false);

                turbocharger_manifoldWeber_trigger.triggerGameObject.SetActive(true);
                turbocharger_intercooler_manifold_tube_weber_trigger.triggerGameObject.SetActive(true);
            }

            if (racingExhaustPipe_inst)
            {
                turbocharger_big_exhaust_outlet_tube_trigger.triggerGameObject.SetActive(true);
            }
            else
            {
                turbocharger_big_exhaust_outlet_tube_trigger.triggerGameObject.SetActive(false);
            }

            if (turbocharger_big_exhaust_outlet_straight_part.installed && satsumaDriveTrain.rpm >= 300)
            {
                exhaustEngine.SetActive(false);
                exhaustPipeRace.SetActive(true);

                exhaustPipeRace.transform.parent = turbocharger_big_exhaust_outlet_straight_part.rigidPart.transform;
                exhaustPipeRace.transform.localPosition = fire_fx_big_turbo_exhaust_straight.transform.localPosition;
                exhaustPipeRace.transform.localRotation = fire_fx_big_turbo_exhaust_straight.transform.localRotation;
                exhaustRaceMuffler.SetActive(false);
            }
            else
            {
                if(originalExhaustPipeRaceTransform != null)
                {
                    exhaustPipeRace.transform.localPosition = originalExhaustPipeRaceTransform.localPosition;
                    exhaustPipeRace.transform.localRotation = originalExhaustPipeRaceTransform.localRotation;
                    exhaustPipeRace.transform.parent = originalExhaustPipeRaceTransform.parent;
                }


                if (racingExhaustPipe_inst && racingExhaustMuffler_inst && turbocharger_exhaust_header_screwable.partFixed && turbocharger_big_exhaust_inlet_tube_screwable.partFixed && turbocharger_big_exhaust_outlet_tube_screwable.partFixed && racingExhaustPipeTightness.Value >= 24)
                {
                    exhaustEngine.SetActive(false);
                    exhaustPipeRace.SetActive(false);
                    exhaustRaceMuffler.SetActive(true);
                }
                else
                {

                    if (!turbocharger_exhaust_header_screwable.partFixed || !turbocharger_big_exhaust_inlet_tube_screwable.partFixed || !turbocharger_big_exhaust_outlet_tube_screwable.partFixed)
                    {
                        //Should be fixed now
                        if(steelHeaders_inst && racingExhaustPipe_inst && racingExhaustPipe_inst)
                        {
                            exhaustEngine.SetActive(false);
                            exhaustPipeRace.SetActive(false);
                            exhaustRaceMuffler.SetActive(true);
                        }
                        else if(steelHeaders_inst && !racingExhaustPipe_inst && !racingExhaustPipe_inst)
                        {
                            exhaustEngine.SetActive(true);
                            exhaustPipeRace.SetActive(false);
                            exhaustRaceMuffler.SetActive(false);
                        }
                        else if (steelHeaders_inst && racingExhaustPipe_inst)
                        {
                            exhaustEngine.SetActive(false);
                            exhaustPipeRace.SetActive(true);
                            exhaustRaceMuffler.SetActive(false);
                        }
                    }
                    else
                    {
                        if (racingExhaustPipe_inst && !racingExhaustMuffler_inst)
                        {
                            exhaustEngine.SetActive(false);
                            exhaustPipeRace.SetActive(true);
                            exhaustRaceMuffler.SetActive(false);
                        }
                        else if (!racingExhaustPipe_inst && !racingExhaustMuffler_inst)
                        {
                            exhaustEngine.SetActive(true);
                            exhaustPipeRace.SetActive(false);
                            exhaustRaceMuffler.SetActive(false);
                        }
                    }
                }
            }
            
            

            if(turbocharger_intercooler_part.installed || (turbocharger_intercooler_manifold_tube_weber_part.installed || turbocharger_intercooler_manifold_tube_twinCarb_part.installed) || turbocharger_manifold_weber_part.installed){
                turbocharger_small_manifold_twinCarb_tube_trigger.triggerGameObject.SetActive(false);
            }
            else
            {
                turbocharger_small_manifold_twinCarb_tube_trigger.triggerGameObject.SetActive(true);
            }
        }

        private static void SwitchTurboChargerValuesDEBUG()
        {
            turboValuesDEBUG = !turboValuesDEBUG;
        }

        private void AddParts()
        {
            PartSaveInfo turbocharger_big_SaveInfo = null;
            PartSaveInfo turbocharger_big_intercooler_tube_SaveInfo = null;
            PartSaveInfo turbocharger_big_exhaust_inlet_tube_SaveInfo = null;
            PartSaveInfo turbocharger_big_exhaust_outlet_tube_SaveInfo = null;
            PartSaveInfo turbocharger_big_blowoff_valve_SaveInfo = null;
            PartSaveInfo turbocharger_big_exhaust_outlet_straight_SaveInfo = null;

            PartSaveInfo turbocharger_exhaust_header_SaveInfo = null;

            PartSaveInfo turbocharger_small_SaveInfo = null;
            PartSaveInfo turbocharger_small_intercooler_tube_SaveInfo = null;
            PartSaveInfo turbocharger_small_exhaust_inlet_tube_SaveInfo = null;
            PartSaveInfo turbocharger_small_exhaust_outlet_tube_SaveInfo = null;
            PartSaveInfo turbocharger_small_manifold_twinCarb_tube_SaveInfo = null;
            PartSaveInfo turbocharger_small_airfilter_SaveInfo = null;

            PartSaveInfo turbocharger_hood_SaveInfo = null;
            PartSaveInfo turbocharger_manifold_weber_SaveInfo = null;
            PartSaveInfo turbocharger_manifold_twinCarb_SaveInfo = null;
            PartSaveInfo turbocharger_boost_gauge_SaveInfo = null;
            PartSaveInfo turbocharger_intercooler_SaveInfo = null;
            PartSaveInfo turbocharger_intercooler_manifold_tube_weber_SaveInfo = null;
            PartSaveInfo turbocharger_intercooler_manifold_tube_twinCarb_SaveInfo = null;

            turbocharger_big_SaveInfo = this.loadSaveData(turbocharger_big_SaveFile);
            turbocharger_big_intercooler_tube_SaveInfo = this.loadSaveData(turbocharger_big_intercooler_tube_SaveFile);
            turbocharger_big_exhaust_inlet_tube_SaveInfo = this.loadSaveData(turbocharger_big_exhaust_inlet_tube_SaveFile);
            turbocharger_big_exhaust_outlet_tube_SaveInfo = this.loadSaveData(turbocharger_big_exhaust_outlet_tube_SaveFile);
            turbocharger_big_blowoff_valve_SaveInfo = this.loadSaveData(turbocharger_big_blowoff_valve_SaveFile);
            turbocharger_big_exhaust_outlet_straight_SaveInfo = this.loadSaveData(turbocharger_big_exhaust_outlet_straight_SaveFile);

            turbocharger_exhaust_header_SaveInfo = this.loadSaveData(turbocharger_exhaust_header_SaveFile);

            turbocharger_small_SaveInfo = this.loadSaveData(turbocharger_small_SaveFile);
            turbocharger_small_intercooler_tube_SaveInfo = this.loadSaveData(turbocharger_small_intercooler_tube_SaveFile);
            turbocharger_small_exhaust_inlet_tube_SaveInfo = this.loadSaveData(turbocharger_small_exhaust_inlet_tube_SaveFile);
            turbocharger_small_exhaust_outlet_tube_SaveInfo = this.loadSaveData(turbocharger_small_exhaust_outlet_tube_SaveFile);
            turbocharger_small_manifold_twinCarb_tube_SaveInfo = this.loadSaveData(turbocharger_small_manifold_twinCarb_tube_SaveFile);
            turbocharger_small_airfilter_SaveInfo = this.loadSaveData(turbocharger_small_airfilter_SaveFile);

            turbocharger_hood_SaveInfo = this.loadSaveData(turbocharger_hood_SaveFile);
            turbocharger_manifold_weber_SaveInfo = this.loadSaveData(turbocharger_manifold_weber_SaveFile);
            turbocharger_manifold_twinCarb_SaveInfo = this.loadSaveData(turbocharger_manifold_twinCarb_SaveFile);
            turbocharger_boost_gauge_SaveInfo = this.loadSaveData(turbocharger_boost_gauge_SaveFile);
            turbocharger_intercooler_SaveInfo = this.loadSaveData(turbocharger_intercooler_SaveFile);
            turbocharger_intercooler_manifold_tube_weber_SaveInfo = this.loadSaveData(turbocharger_intercooler_manifold_tube_weber_SaveFile);
            turbocharger_intercooler_manifold_tube_twinCarb_SaveInfo = this.loadSaveData(turbocharger_intercooler_manifold_tube_twinCarb_SaveFile);

            SaveLoad.SerializeSaveFile<PartBuySave>(this, null, turbocharger_mod_others_SaveFile);

            try
            {
                partBuySave = SaveLoad.DeserializeSaveFile<PartBuySave>(this, turbocharger_mod_ModsShop_SaveFile);
            }
            catch
            {

            }
            if (partBuySave == null)
            {
                partBuySave = new PartBuySave
                {
                    bought_turbocharger_big_kit = false,
                    bought_turbocharger_exhaust_header = false,
                    bought_turbocharger_small_intercooler_tube = false,
                    bought_turbocharger_small_airfilter = false,
                    bought_turbocharger_big_blowoff_valve = false,
                    bought_turbocharger_big_exhaust_outlet_straight = false,
                    bought_turbocharger_manifold_twinCarb_kit = false,
                    bought_turbocharger_manifold_weber_kit = false,
                    bought_turbocharger_hood = false,
                    bought_turbocharger_intercooler = false,
                    bought_turbocharger_boost_gauge = false
                };
            }
            if (!partBuySave.bought_turbocharger_big_kit)
            {
                turbocharger_big_SaveInfo = null;
                turbocharger_big_intercooler_tube_SaveInfo = null;
                turbocharger_big_exhaust_inlet_tube_SaveInfo = null;
                turbocharger_big_exhaust_outlet_tube_SaveInfo = null;
            }
            if (!partBuySave.bought_turbocharger_big_exhaust_outlet_straight)
            {
                turbocharger_big_exhaust_outlet_straight_SaveInfo = null;
            }

            if (!partBuySave.bought_turbocharger_exhaust_header)
            {
                turbocharger_exhaust_header_SaveInfo = null;
            }
            if (!partBuySave.bought_turbocharger_small_intercooler_tube)
            {
                turbocharger_small_intercooler_tube_SaveInfo = null;
            }
            if (!partBuySave.bought_turbocharger_small_airfilter)
            {
                turbocharger_small_airfilter_SaveInfo = null;
            }
            if (!partBuySave.bought_turbocharger_big_blowoff_valve)
            {
                turbocharger_big_blowoff_valve_SaveInfo = null;
            }
            if (!partBuySave.bought_turbocharger_manifold_weber_kit)
            {
                turbocharger_manifold_weber_SaveInfo = null;
                turbocharger_intercooler_manifold_tube_weber_SaveInfo = null;
            }
            if (!partBuySave.bought_turbocharger_manifold_twinCarb_kit)
            {
                turbocharger_manifold_twinCarb_SaveInfo = null;
                turbocharger_intercooler_manifold_tube_twinCarb_SaveInfo = null;
            }
            if (!partBuySave.bought_turbocharger_hood)
            {
                turbocharger_hood_SaveInfo = null;
            }
            if (!partBuySave.bought_turbocharger_intercooler)
            {
                turbocharger_intercooler_SaveInfo = null;
            }
            if (!partBuySave.bought_turbocharger_boost_gauge)
            {
                turbocharger_boost_gauge_SaveInfo = null;
            }


            turbocharger_big_part = new Racing_Turbocharger_Part(
                turbocharger_big_SaveInfo,
                turbocharger_big,
                originalCylinerHead,
                turbocharger_big_trigger,
                turbocharger_big_installLocation,
                new Quaternion
                {
                    eulerAngles = new Vector3(90, 0, 0)
                }
            );
            turbocharger_big_part.rigidPart.AddComponent<Racing_Turbocharger_Logic>();

            turbocharger_big_exhaust_outlet_straight_part = new Racing_Exhaust_Outlet_Straight_Part(
                turbocharger_big_exhaust_outlet_straight_SaveInfo,
                turbocharger_big_exhaust_outlet_straight,
                originalCylinerHead,
                turbocharger_big_exhaust_outlet_straight_trigger,
                turbocharger_big_exhaust_outlet_straight_installLocation,
                new Quaternion
                {
                    eulerAngles = new Vector3(90, 0, 0)
                }
            );

            turbocharger_exhaust_header_part = new Exhaust_Header_Part(
                turbocharger_exhaust_header_SaveInfo,
                turbocharger_exhaust_header,
                originalCylinerHead,
                turbocharger_exhaust_header_trigger,
                turbocharger_exhaust_header_installLocation,
                new Quaternion
                {
                    eulerAngles = new Vector3(90, 0, 0)
                }
            );

            turbocharger_big_intercooler_tube_part = new Racing_Intercooler_Tube_Part(
                turbocharger_big_intercooler_tube_SaveInfo,
                turbocharger_big_intercooler_tube,
                satsuma, 
                turbocharger_big_intercooler_tube_trigger,
                turbocharger_big_intercooler_tube_installLocation,
                new Quaternion
                {
                    eulerAngles = new Vector3(0, 180, 0)
                }
                // -0.195f, 0.071f, 0.145f old

            );
            turbocharger_big_exhaust_inlet_tube_part = new Racing_Exhaust_Inlet_Tube_Part(
                turbocharger_big_exhaust_inlet_tube_SaveInfo,
                turbocharger_big_exhaust_inlet_tube,
                originalCylinerHead,
                turbocharger_big_exhaust_inlet_tube_trigger,
                turbocharger_big_exhaust_inlet_tube_installLocation,
                new Quaternion
                {
                    eulerAngles = new Vector3(90, 0, 0)
                }
            );
            turbocharger_big_exhaust_outlet_tube_part = new Racing_Exhaust_Outlet_Tube_Part(
                turbocharger_big_exhaust_outlet_tube_SaveInfo,
                turbocharger_big_exhaust_outlet_tube,
                originalCylinerHead,
                turbocharger_big_exhaust_outlet_tube_trigger,
                turbocharger_big_exhaust_outlet_tube_installLocation,
                new Quaternion
                {
                    eulerAngles = new Vector3(90, 0, 0)
                }
            );
            turbocharger_big_blowoff_valve_part = new Racing_Blowoff_Valve_Part(
                turbocharger_big_blowoff_valve_SaveInfo,
                turbocharger_big_blowoff_valve,
                satsuma,
                turbocharger_big_blowoff_valve_trigger,
                turbocharger_big_blowoff_valve_installLocation,
                new Quaternion
                {
                    eulerAngles = new Vector3(0, 180, 0)
                }
            );

            turbocharger_small_part = new GT_Turbocharger_Part(
                turbocharger_small_SaveInfo,
                turbocharger_small,
                originalCylinerHead,
                turbocharger_small_trigger,
                turbocharger_small_installLocation,
                new Quaternion
                {
                    eulerAngles = new Vector3(90, 0, 0)
                }
            );
            turbocharger_small_intercooler_tube_part = new GT_Intercooler_Tube_Part(
                turbocharger_small_intercooler_tube_SaveInfo,
                turbocharger_small_intercooler_tube,
                satsuma,
                turbocharger_small_intercooler_tube_trigger,
                turbocharger_small_intercooler_tube_installLocation,
                new Quaternion
                {
                    eulerAngles = new Vector3(0, 180, 0)
                }
            );
            turbocharger_small_manifold_twinCarb_tube_part = new GT_Manifold_TwinCarb_Tube_Part(
                turbocharger_small_manifold_twinCarb_tube_SaveInfo,
                turbocharger_small_manifold_twinCarb_tube,
                originalCylinerHead,
                turbocharger_small_manifold_twinCarb_tube_trigger,
                turbocharger_small_manifold_twinCarb_tube_installLocation,
                new Quaternion
                {
                    eulerAngles = new Vector3(90, 0, 0)
                }
            );
            turbocharger_small_airfilter_part = new GT_Airfilter_Part(
                turbocharger_small_airfilter_SaveInfo,
                turbocharger_small_airfilter,
                originalCylinerHead,
                turbocharger_small_airfilter_trigger,
                turbocharger_small_airfilter_installLocation,
                new Quaternion
                {
                    eulerAngles = new Vector3(90, 0, 0)
                }
            );


            turbocharger_small_exhaust_inlet_tube_part = new GT_Exhaust_Inlet_Tube_Part(
                turbocharger_small_exhaust_inlet_tube_SaveInfo,
                turbocharger_small_exhaust_inlet_tube,
                originalCylinerHead,
                turbocharger_small_exhaust_inlet_tube_trigger,
                turbocharger_small_exhaust_inlet_tube_installLocation,
                new Quaternion
                {
                    eulerAngles = new Vector3(90, 0, 0)
                }
            );
            turbocharger_small_exhaust_outlet_tube_part = new GT_Exhaust_Outlet_Tube_Part(
                turbocharger_small_exhaust_outlet_tube_SaveInfo,
                turbocharger_small_exhaust_outlet_tube,
                originalCylinerHead,
                turbocharger_small_exhaust_outlet_tube_trigger,
                turbocharger_small_exhaust_outlet_tube_installLocation,
                new Quaternion
                {
                    eulerAngles = new Vector3(90, 0, 0)
                }
            );


            turbocharger_hood_part = new Racing_Hood_Part(
                turbocharger_hood_SaveInfo,
                turbocharger_hood,
                satsuma,
                turbocharger_hood_trigger,
                turbocharger_hood_installLocation,
                new Quaternion(0, 180, 0, 0)
            );


            turbocharger_manifold_weber_part = new Manifold_Weber_Part(
                turbocharger_manifold_weber_SaveInfo,
                turbocharger_manifold_weber,
                originalCylinerHead,
                turbocharger_manifoldWeber_trigger,
                turbocharger_manifold_weber_installLocation,
                new Quaternion
                {
                    eulerAngles = new Vector3(80, 0, 0)
                }
            );

            turbocharger_manifold_twinCarb_part = new Manifold_TwinCarb_Part(
                turbocharger_manifold_twinCarb_SaveInfo,
                turbocharger_manifold_twinCarb,
                originalCylinerHead,
                turbocharger_manifoldTwinCarb_trigger,
                turbocharger_manifold_twinCarb_installLocation,
                new Quaternion
                {
                    eulerAngles = new Vector3(90, 0, 0)
                }
            );


            turbocharger_boost_gauge_part = new Boost_Gauge_Part(
                turbocharger_boost_gauge_SaveInfo,
                turbocharger_boost_gauge,
                GameObject.Find("dashboard(Clone)"),
                turbocharger_boostGauge_trigger,
                turbocharger_boost_gauge_installLocation,
                new Quaternion
                {
                    eulerAngles = new Vector3(90, 0, 0)
                }
            );
            turbocharger_intercooler_part = new Intercooler_Part(
               turbocharger_intercooler_SaveInfo,
               turbocharger_intercooler,
               satsuma,
               turbocharger_intercooler_trigger,
               turbocharger_intercooler_installLocation,
               new Quaternion
               {
                   eulerAngles = new Vector3(-15, 180, 0)
               }
               //old: 0.03f, -0.015f, 0.142f
           );
            turbocharger_intercooler_manifold_tube_weber_part = new Intercooler_Manifold_Tube_Weber_Part(
                turbocharger_intercooler_manifold_tube_weber_SaveInfo,
                turbocharger_intercooler_manifold_tube_weber,
                satsuma,
                turbocharger_intercooler_manifold_tube_weber_trigger,
                turbocharger_intercooler_manifold_tube_weber_installLocation,
                new Quaternion
                {
                    eulerAngles = new Vector3(0, 180, 0)
                }
            );
            turbocharger_intercooler_manifold_tube_twinCarb_part = new Intercooler_Manifold_Tube_TwinCarb_Part(
                turbocharger_intercooler_manifold_tube_twinCarb_SaveInfo,
                turbocharger_intercooler_manifold_tube_twinCarb,
                satsuma,
                turbocharger_intercooler_manifold_tube_twinCarb_trigger,
                turbocharger_intercooler_manifold_tube_twinCarb_installLocation,
                new Quaternion
                {
                    eulerAngles = new Vector3(0, 180, 0)
                }
            );


            LoadPartsColorSave();
            SetModsShop();
        }

        private void SetModsShop()
        {
            if (GameObject.Find("Shop for mods") != null)
            {
                shop = GameObject.Find("Shop for mods").GetComponent<ModsShop.ShopItem>();

                //Repair products.
                repair_turbocharger_big_Product = new ModsShop.ProductDetails
                {
                    productName = "REPAIR Racing Turbocharger",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assets.LoadAsset<Sprite>("Big_Turbocharger_Repair_ProductImage.png"),
                    productPrice = 4000
                };
                repair_turbocharger_small_Product = new ModsShop.ProductDetails
                {
                    productName = "REPAIR GT Turbocharger",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assets.LoadAsset<Sprite>("Small_Turbocharger_Repair_ProductImage.png"),
                    productPrice = 2500
                };
                shop.Add(this, repair_turbocharger_small_Product, ModsShop.ShopType.Fleetari, RepairPurchaseMadeTurbochargerSmall, null);

                repair_turbocharger_small_airfilter_Product = new ModsShop.ProductDetails
                {
                    productName = "REPAIR GT Turbo Airfilter",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assets.LoadAsset<Sprite>("Small_Turbocharger_Airfilter_Repair_ProductImage.png"),
                    productPrice = 400
                };
                repair_intercooler_Product = new ModsShop.ProductDetails
                {
                    productName = "REPAIR Intercooler",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assets.LoadAsset<Sprite>("Intercooler_Repair_ProductImage.png"),
                    productPrice = 1500
                };

                //All other parts
                ModsShop.ProductDetails turbocharger_big_kit_Product = new ModsShop.ProductDetails
                {
                    productName = "Racing Turbocharger Kit",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assets.LoadAsset<Sprite>("Big_Turbocharger_Kit_ProductImage.png"),
                    productPrice = 8100
                };
                if (!partBuySave.bought_turbocharger_big_kit)
                {
                    shop.Add(this, turbocharger_big_kit_Product, ModsShop.ShopType.Fleetari, PurchaseMadeturbocharger_bigKit, null);
                    turbocharger_big_part.activePart.SetActive(false);
                    turbocharger_big_intercooler_tube_part.activePart.SetActive(false);
                    turbocharger_big_exhaust_inlet_tube_part.activePart.SetActive(false);
                    turbocharger_big_exhaust_outlet_tube_part.activePart.SetActive(false);
                    turbocharger_bigColor = new Color(0.800f, 0.800f, 0.800f);
                    originalTurbocchargerBigColor = new Color(0.800f, 0.800f, 0.800f);

                }
                else
                {
                    shop.Add(this, repair_turbocharger_big_Product, ModsShop.ShopType.Fleetari, RepairPurchaseMadeturbocharger_big, null);
                }

                ModsShop.ProductDetails turbocharger_big_exhaust_outlet_straight_Product = new ModsShop.ProductDetails
                {
                    productName = "Racing Turbocharger Straight Exhaust",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assets.LoadAsset<Sprite>("Big_Turbocharger_Straight_Exhaust_ProductImage.png"),
                    productPrice = 1000
                };
                if (!partBuySave.bought_turbocharger_big_exhaust_outlet_straight)
                {
                    shop.Add(this, turbocharger_big_exhaust_outlet_straight_Product, ModsShop.ShopType.Fleetari, PurchaseMadeTurbochargerExhaustStraight, null);
                    turbocharger_big_exhaust_outlet_straight_part.activePart.SetActive(false);
                }

                ModsShop.ProductDetails turbocharger_exhaust_header_Product = new ModsShop.ProductDetails
                {
                    productName = "Turbocharger Exhaust Header",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    //productIcon = assets.LoadAsset<Sprite>("Big_Turbocharger_Kit_ProductImage.png"),
                    productPrice = 2100
                };
                if (!partBuySave.bought_turbocharger_exhaust_header)
                {
                    shop.Add(this, turbocharger_exhaust_header_Product, ModsShop.ShopType.Fleetari, PurchaseMadeTurbochargerExhaustHeader, null);
                    turbocharger_exhaust_header_part.activePart.SetActive(false);
                }

                ModsShop.ProductDetails turbocharger_big_blowoff_valve_Product = new ModsShop.ProductDetails
                {
                    productName = "Racing Turbocharger Blowoff Valve",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assets.LoadAsset<Sprite>("Big_Turbocharger_Blowoff_Valve_ProductImage.png"),
                    productPrice = 1350
                };
                if (!partBuySave.bought_turbocharger_big_blowoff_valve)
                {
                    shop.Add(this, turbocharger_big_blowoff_valve_Product, ModsShop.ShopType.Fleetari, PurchaseMadeturbocharger_bigBlowoffValve, null);
                    turbocharger_big_blowoff_valve_part.activePart.SetActive(false);
                    turbocharger_bigBlowoffValveColor = new Color(0.800f, 0.800f, 0.800f);
                    originalturbocharger_bigBlowoffValveColor = new Color(0.800f, 0.800f, 0.800f);
                }

                ModsShop.ProductDetails turbocharger_small_intercooler_tube_Product = new ModsShop.ProductDetails
                {
                    productName = "GT Turbocharger Intercooler Tube",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assets.LoadAsset<Sprite>("Small_Turbocharger_Tube_Intercooler_ProductImage.png"),
                    productPrice = 500
                };
                if (!partBuySave.bought_turbocharger_small_intercooler_tube)
                {
                    shop.Add(this, turbocharger_small_intercooler_tube_Product, ModsShop.ShopType.Fleetari, PurchaseMadeTurbochargerSmallIntercoolerTube, null);
                    turbocharger_small_intercooler_tube_part.activePart.SetActive(false);
                }
                ModsShop.ProductDetails turbocharger_small_airfilter_Product = new ModsShop.ProductDetails
                {
                    productName = "GT Turbocharger Airfilter",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assets.LoadAsset<Sprite>("Small_Turbocharger_Airfilter_ProductImage.png"),
                    productPrice = 800
                };
                if (!partBuySave.bought_turbocharger_small_airfilter)
                {
                    shop.Add(this, turbocharger_small_airfilter_Product, ModsShop.ShopType.Fleetari, PurchaseMadeTurbochargerSmallAirfilter, null);
                    turbocharger_small_airfilter_part.activePart.SetActive(false);
                    turbocharger_small_airfilter_color = new Color(0.800f, 0.800f, 0.800f);
                    original_turbocharger_small_airfilter_color = new Color(0.800f, 0.800f, 0.800f);
                }
                else
                {
                    shop.Add(this, repair_turbocharger_small_airfilter_Product, ModsShop.ShopType.Fleetari, RepairPurchaseMadeTurbochargerSmallAirfilter, null);
                }

                ModsShop.ProductDetails turbocharger_manifold_twinCarb_kit_Product = new ModsShop.ProductDetails
                {
                    productName = "TwinCarb Manifold Kit",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assets.LoadAsset<Sprite>("TwinCarb_Manifold_Kit_ProductImage.png"),
                    productPrice = 1950
                };
                if (!partBuySave.bought_turbocharger_manifold_twinCarb_kit)
                {
                    shop.Add(this, turbocharger_manifold_twinCarb_kit_Product, ModsShop.ShopType.Fleetari, PurchaseMadeTurbochargerManifoldTwinCarbKit, null);
                    turbocharger_manifold_twinCarb_part.activePart.SetActive(false);
                    turbocharger_intercooler_manifold_tube_twinCarb_part.activePart.SetActive(false);
                    turbochargerManifoldTwinCarbColor = new Color(0.800f, 0.800f, 0.800f);
                    originalTurbochargerManifoldTwinCarbColor = new Color(0.800f, 0.800f, 0.800f);
                }
                ModsShop.ProductDetails turbocharger_manifold_weber_kit_Product = new ModsShop.ProductDetails
                {
                    productName = "Weber Manifold Kit",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assets.LoadAsset<Sprite>("Weber_Manifold_Kit_ProductImage.png"),
                    productPrice = 2250
                };
                if (!partBuySave.bought_turbocharger_manifold_weber_kit)
                {
                    shop.Add(this, turbocharger_manifold_weber_kit_Product, ModsShop.ShopType.Fleetari, PurchaseMadeTurbochargerManifoldbWeberKit, null);
                    turbocharger_manifold_weber_part.activePart.SetActive(false);
                    turbocharger_intercooler_manifold_tube_weber_part.activePart.SetActive(false);
                    turbochargerManifoldWeberColor = new Color(0.800f, 0.800f, 0.800f);
                    originalTurbochargerManifoldWeberColor = new Color(0.800f, 0.800f, 0.800f);
                }
                ModsShop.ProductDetails turbocharger_hood_Product = new ModsShop.ProductDetails
                {
                    productName = "Racing Turbocharger Hood",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assets.LoadAsset<Sprite>("Turbocharger_Hood_ProductImage.png"),
                    productPrice = 1800
                };
                if (!partBuySave.bought_turbocharger_hood)
                {
                    shop.Add(this, turbocharger_hood_Product, ModsShop.ShopType.Fleetari, PurchaseMadeTurbochargerHood, null);
                    turbocharger_hood_part.activePart.SetActive(false);
                    hoodColor = new Color(0.800f, 0.800f, 0.800f);
                }
                ModsShop.ProductDetails turbocharger_intercooler_Product = new ModsShop.ProductDetails
                {
                    productName = "Intercooler",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assets.LoadAsset<Sprite>("Turbocharger_Intercooler_ProductImage.png"),
                    productPrice = 3500
                };
                if (!partBuySave.bought_turbocharger_intercooler)
                {
                    shop.Add(this, turbocharger_intercooler_Product, ModsShop.ShopType.Fleetari, PurchaseMadeTurbochargerIntercooler, null);
                    turbocharger_intercooler_part.activePart.SetActive(false);
                    intercoolerColor = new Color(0.800f, 0.800f, 0.800f);
                    originalIntercoolerColor = new Color(0.800f, 0.800f, 0.800f);
                }
                else
                {
                    shop.Add(this, repair_intercooler_Product, ModsShop.ShopType.Fleetari, RepairPurchaseMadeIntercooler, null);
                }
                ModsShop.ProductDetails turbocharger_boost_gauge_Product = new ModsShop.ProductDetails
                {
                    productName = "Boost Gauge",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assets.LoadAsset<Sprite>("Turbocharger_Boost_Gauge_ProductImage.png"),
                    productPrice = 180
                };
                if (!partBuySave.bought_turbocharger_boost_gauge)
                {
                    shop.Add(this, turbocharger_boost_gauge_Product, ModsShop.ShopType.Fleetari, PurchaseMadeTurbochargerBoostGauge, null);
                    turbocharger_boost_gauge_part.activePart.SetActive(false);
                }
            }
            else
            {
                ModUI.ShowMessage(
               "You need to install ModsShop by piotrulos for this mod\n" +
               "Please close the game and install the mod\n" +
               "There should have been a ModsShop.dll and unity3d file (inside Assets) inside the archive of this mod",
               "Installation of ModsShop (by piotrulos) needed");
            }
        }
        public void RepairPurchaseMadeturbocharger_big(ModsShop.PurchaseInfo item)
        {
            if (CheckCloseToPosition(turbocharger_big_part.activePart.transform.position, ModsShop.FleetariSpawnLocation.desk, 0.8f))
            {
                partsWearSave.turbocharger_big_wear = 100;
                turbocharger_bigColor = new Color(0.800f, 0.800f, 0.800f);
                originalTurbocchargerBigColor = new Color(0.800f, 0.800f, 0.800f);
                turbocharger_big_part.activePart.transform.position = ModsShop.FleetariSpawnLocation.desk;
                turbocharger_big_part.activePart.SetActive(true);
            }
            else
            {
                ModUI.ShowMessage("Please put the part on the desk where the ModsShop sign is and try again" + "\n" + "Money has been refunded");
                PlayMakerGlobals.Instance.Variables.FindFsmFloat("PlayerMoney").Value += repair_turbocharger_big_Product.productPrice;
            }
        }
        public void RepairPurchaseMadeTurbochargerSmall(PurchaseInfo purchaseInfo)
        {
            if (CheckCloseToPosition(turbocharger_small_part.activePart.transform.position, ModsShop.FleetariSpawnLocation.desk, 0.8f))
            {
                partsWearSave.turbocharger_small_wear = 100;
                turbochargerSmallColor = new Color(0.800f, 0.800f, 0.800f);
                originalTurbochargerSmallColor = new Color(0.800f, 0.800f, 0.800f);
                turbocharger_small_part.activePart.transform.position = ModsShop.FleetariSpawnLocation.desk;
                turbocharger_small_part.activePart.SetActive(true);
            }
            else
            {
                ModUI.ShowMessage("Please put the part on the desk where the ModsShop sign is and try again" + "\n" + "Money has been refunded");
                PlayMakerGlobals.Instance.Variables.FindFsmFloat("PlayerMoney").Value += repair_turbocharger_small_Product.productPrice;
            }
        }
        public void RepairPurchaseMadeTurbochargerSmallAirfilter(PurchaseInfo purchaseInfo)
        {
            if (CheckCloseToPosition(turbocharger_small_airfilter_part.activePart.transform.position, ModsShop.FleetariSpawnLocation.desk, 0.8f))
            {
                partsWearSave.airfilter_wear = 100;
                turbocharger_small_airfilter_color = new Color(0.800f, 0.800f, 0.800f);
                original_turbocharger_small_airfilter_color = new Color(0.800f, 0.800f, 0.800f);
                turbocharger_small_airfilter_part.activePart.transform.position = ModsShop.FleetariSpawnLocation.desk;
                turbocharger_small_airfilter_part.activePart.SetActive(true);
            }
            else
            {
                ModUI.ShowMessage("Please put the part on the desk where the ModsShop sign is and try again" + "\n" + "Money has been refunded");
                PlayMakerGlobals.Instance.Variables.FindFsmFloat("PlayerMoney").Value += repair_turbocharger_small_airfilter_Product.productPrice;
            }
        }
        public void RepairPurchaseMadeIntercooler(PurchaseInfo purchaseInfo)
        {
            if (CheckCloseToPosition(turbocharger_intercooler_part.activePart.transform.position, ModsShop.FleetariSpawnLocation.desk, 0.8f))
            {
                if (turbocharger_intercooler_part.installed)
                {
                    turbocharger_intercooler_part.removePart();
                }
                partsWearSave.intercooler_wear = 100;
                intercoolerColor = new Color(0.800f, 0.800f, 0.800f);
                originalIntercoolerColor = new Color(0.800f, 0.800f, 0.800f);
                turbocharger_intercooler_part.activePart.transform.position = ModsShop.FleetariSpawnLocation.desk;
                turbocharger_intercooler_part.activePart.SetActive(true);
            }
            else
            {
                ModUI.ShowMessage("Please put the part on the desk where the ModsShop sign is and try again" + "\n" + "Money has been refunded");
                PlayMakerGlobals.Instance.Variables.FindFsmFloat("PlayerMoney").Value += repair_intercooler_Product.productPrice;
            }
        }

        public void PurchaseMadeTurbochargerExhaustStraight(ModsShop.PurchaseInfo item)
        {
            turbocharger_big_exhaust_outlet_straight_part.activePart.transform.position = ModsShop.FleetariSpawnLocation.desk;
            turbocharger_big_exhaust_outlet_straight_part.activePart.SetActive(true);
            partBuySave.bought_turbocharger_big_exhaust_outlet_straight = true;
        }

        public void PurchaseMadeturbocharger_bigKit(ModsShop.PurchaseInfo item)
        {
            partsWearSave.turbocharger_big_wear = 100;
            List<ModsShop.ShopItems> shopItems = shop.fleetariShopItems;
            int counter = 0;
            foreach(ModsShop.ShopItems shopItem in shopItems)
            {
                if(shopItem.details.productName == "REPAIR Racing Turbocharger")
                {
                    counter++;
                }
            }
            if(counter == 0)
            {
                shop.Add(this, repair_turbocharger_big_Product, ModsShop.ShopType.Fleetari, RepairPurchaseMadeturbocharger_big, null);
            }
            turbocharger_big_part.activePart.transform.position = new Vector3(1558.366f, 5f, 742.5068f);
            turbocharger_big_intercooler_tube_part.activePart.transform.position = new Vector3(1556.846f, 5f, 741.4836f);
            turbocharger_big_exhaust_inlet_tube_part.activePart.transform.position = new Vector3(1557.866f, 5f, 741.9728f);
            turbocharger_big_exhaust_outlet_tube_part.activePart.transform.position = new Vector3(1557.352f, 5f, 741.7303f);


            turbocharger_big_part.activePart.SetActive(true);
            turbocharger_big_intercooler_tube_part.activePart.SetActive(true);
            turbocharger_big_exhaust_inlet_tube_part.activePart.SetActive(true);
            turbocharger_big_exhaust_outlet_tube_part.activePart.SetActive(true);
            partBuySave.bought_turbocharger_big_kit = true;
        }
        public void PurchaseMadeTurbochargerExhaustHeader(ModsShop.PurchaseInfo item)
        {
            turbocharger_exhaust_header_part.activePart.transform.position = new Vector3(1555.136f, 5.8f, 737.2324f); //CHANGE
            turbocharger_exhaust_header_part.activePart.SetActive(true);
            partBuySave.bought_turbocharger_exhaust_header = true;
        }
        public void PurchaseMadeturbocharger_bigBlowoffValve(ModsShop.PurchaseInfo item)
        {
            turbocharger_big_blowoff_valve_part.activePart.transform.position = new Vector3(1555.136f, 5.8f, 737.2324f);

            turbocharger_big_blowoff_valve_part.activePart.SetActive(true);
            partBuySave.bought_turbocharger_big_blowoff_valve = true;
        }
        public void PurchaseMadeTurbochargerSmallIntercoolerTube(ModsShop.PurchaseInfo item)
        {
            turbocharger_small_intercooler_tube_part.activePart.transform.position = new Vector3(1554.144f, 5f, 738.733f);
            turbocharger_small_intercooler_tube_part.activePart.SetActive(true);
            partBuySave.bought_turbocharger_small_intercooler_tube = true;
        }
        public void PurchaseMadeTurbochargerSmallAirfilter(ModsShop.PurchaseInfo item)
        {
            partsWearSave.airfilter_wear = 100;
            List<ModsShop.ShopItems> shopItems = shop.fleetariShopItems;
            int counter = 0;
            foreach (ModsShop.ShopItems shopItem in shopItems)
            {
                if (shopItem.details.productName == "REPAIR GT Turbo Airfilter")
                {
                    counter++;
                }
            }
            if (counter == 0)
            {
                shop.Add(this, repair_turbocharger_small_airfilter_Product, ModsShop.ShopType.Fleetari, RepairPurchaseMadeTurbochargerSmallAirfilter, null);
            }
            turbocharger_small_airfilter_part.activePart.transform.position = new Vector3(1555.174f, 5.8f, 736.9866f);
            turbocharger_small_airfilter_part.activePart.SetActive(true);
            partBuySave.bought_turbocharger_small_airfilter = true;
        }

        public void PurchaseMadeTurbochargerManifoldTwinCarbKit(ModsShop.PurchaseInfo item)
        {
            turbocharger_manifold_twinCarb_part.activePart.transform.position = new Vector3(1555.07f, 5.8f, 737.6261f);
            turbocharger_intercooler_manifold_tube_twinCarb_part.activePart.transform.position = new Vector3(1554.339f, 5.5f, 737.913f);

            turbocharger_manifold_twinCarb_part.activePart.SetActive(true);
            turbocharger_intercooler_manifold_tube_twinCarb_part.activePart.SetActive(true);
            partBuySave.bought_turbocharger_manifold_twinCarb_kit = true;
        }
        public void PurchaseMadeTurbochargerManifoldbWeberKit(ModsShop.PurchaseInfo item)
        {
            turbocharger_manifold_weber_part.activePart.transform.position = new Vector3(1555.18f, 5.8f, 737.8821f);
            turbocharger_intercooler_manifold_tube_weber_part.activePart.transform.position = new Vector3(1554.56f, 5f, 737.2017f);

            turbocharger_manifold_weber_part.activePart.SetActive(true);
            turbocharger_intercooler_manifold_tube_weber_part.activePart.SetActive(true);
            partBuySave.bought_turbocharger_manifold_weber_kit = true;

        }
        public void PurchaseMadeTurbochargerHood(ModsShop.PurchaseInfo item)
        {
            turbocharger_hood_part.activePart.transform.position = new Vector3(1559.46f, 5f, 742.296f);

            turbocharger_hood_part.activePart.SetActive(true);
            partBuySave.bought_turbocharger_hood = true;
            
        }
        public void PurchaseMadeTurbochargerIntercooler(ModsShop.PurchaseInfo item)
        {
            partsWearSave.intercooler_wear = 100;
            List<ModsShop.ShopItems> shopItems = shop.fleetariShopItems;
            int counter = 0;
            foreach (ModsShop.ShopItems shopItem in shopItems)
            {
                if (shopItem.details.productName == "REPAIR Intercooler")
                {
                    counter++;
                }
            }
            if (counter == 0)
            {
                shop.Add(this, repair_intercooler_Product, ModsShop.ShopType.Fleetari, RepairPurchaseMadeIntercooler, null);
            }
            turbocharger_intercooler_part.activePart.transform.position = new Vector3(1555.382f, 5.8f, 737.3588f);

            turbocharger_intercooler_part.activePart.SetActive(true);
            partBuySave.bought_turbocharger_intercooler = true;
        }
        public void PurchaseMadeTurbochargerBoostGauge(ModsShop.PurchaseInfo item)
        {
            turbocharger_boost_gauge_part.activePart.transform.position = new Vector3(1555.383f, 5.8f, 737.058f);

            turbocharger_boost_gauge_part.activePart.SetActive(true);
            partBuySave.bought_turbocharger_boost_gauge = true;
        }

        private void LoadPartsColorSave()
        {
            if (File.Exists(partsColorSave_SaveFile))
            {
                XmlReader xmlReader = XmlReader.Create(partsColorSave_SaveFile);
                while (xmlReader.Read())
                {
                    if ((xmlReader.NodeType == XmlNodeType.Element))
                    {
                        if (xmlReader.HasAttributes)
                        {
                            string r = xmlReader.GetAttribute("r");
                            string g = xmlReader.GetAttribute("g");
                            string b = xmlReader.GetAttribute("b");
                            float rFloat = 0f;
                            float gFloat = 0f;
                            float bFloat = 0f;

                            float.TryParse(r, out rFloat);
                            float.TryParse(g, out gFloat);
                            float.TryParse(b, out bFloat);
                            if (xmlReader.Name == "hood-color")
                            {
                                hoodColor = new Color(rFloat, gFloat, bFloat);
                            }
                            else if (xmlReader.Name == "intercooler-color")
                            {
                                intercoolerColor = new Color(rFloat, gFloat, bFloat);
                            }
                            else if (xmlReader.Name == "turbocharger_big-color")
                            {
                                turbocharger_bigColor = new Color(rFloat, gFloat, bFloat);
                            }
                            else if (xmlReader.Name == "turbochargerSmall-color")
                            {
                                turbochargerSmallColor = new Color(rFloat, gFloat, bFloat);
                            }
                            else if (xmlReader.Name == "weber-color")
                            {
                                turbochargerManifoldWeberColor = new Color(rFloat, gFloat, bFloat);
                            }
                            else if (xmlReader.Name == "twincarb-color")
                            {
                                turbochargerManifoldTwinCarbColor = new Color(rFloat, gFloat, bFloat);
                            }
                            else if (xmlReader.Name == "blowoffValve-color")
                            {
                                turbocharger_bigBlowoffValveColor = new Color(rFloat, gFloat, bFloat);
                            }
                            else if(xmlReader.Name == "turbocharger_small_airfilter-color")
                            {
                                turbocharger_small_airfilter_color = new Color(rFloat, gFloat, bFloat);
                            }
                            else if (xmlReader.Name == "original_intercooler-color")
                            {
                                originalIntercoolerColor = new Color(rFloat, gFloat, bFloat);
                            }
                            else if (xmlReader.Name == "original_turbocharger_big-color")
                            {
                                originalTurbocchargerBigColor = new Color(rFloat, gFloat, bFloat);
                            }
                            else if (xmlReader.Name == "original_turbochargerSmall-color")
                            {
                                originalTurbochargerSmallColor = new Color(rFloat, gFloat, bFloat);
                            }
                            else if (xmlReader.Name == "original_weber-color")
                            {
                                originalTurbochargerManifoldWeberColor = new Color(rFloat, gFloat, bFloat);
                            }
                            else if (xmlReader.Name == "original_twincarb-color")
                            {
                                originalTurbochargerManifoldTwinCarbColor = new Color(rFloat, gFloat, bFloat);
                            }
                            else if(xmlReader.Name == "original_blowoffValve-color")
                            {
                                originalturbocharger_bigBlowoffValveColor = new Color(rFloat, gFloat, bFloat);
                            }
                            else if (xmlReader.Name == "original_turbocharger_small_airfilter-color")
                            {
                                turbocharger_small_airfilter_color = new Color(rFloat, gFloat, bFloat);
                            }
                        }
                    }
                }
                xmlReader.Close();
            }
        }

        private void WritePartsColorSave(bool newGame)
        {
            partsColorSave_SaveFile = ModLoader.GetModConfigFolder(this) + "\\turbocharger_parts_ColorSave.xml";
            XmlWriter xmlWriter = XmlWriter.Create(partsColorSave_SaveFile);
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("color-save");
            if (newGame == false)
            {
                WriteXMLColorSaveElement(xmlWriter, "hood-color", hoodColor);
                WriteXMLColorSaveElement(xmlWriter, "intercooler-color", intercoolerColor);
                WriteXMLColorSaveElement(xmlWriter, "turbocharger_big-color", turbocharger_bigColor);
                WriteXMLColorSaveElement(xmlWriter, "turbochargerSmall-color", turbochargerSmallColor);
                WriteXMLColorSaveElement(xmlWriter, "turbocharger_small_airfilter-color", turbocharger_small_airfilter_color);

                WriteXMLColorSaveElement(xmlWriter, "weber-color", turbochargerManifoldWeberColor);
                WriteXMLColorSaveElement(xmlWriter, "twincarb-color", turbochargerManifoldTwinCarbColor);
                WriteXMLColorSaveElement(xmlWriter, "blowoffValve-color", turbocharger_bigBlowoffValveColor);

                WriteXMLColorSaveElement(xmlWriter, "original_intercooler-color", originalIntercoolerColor);
                WriteXMLColorSaveElement(xmlWriter, "original_turbocharger_big-color", originalTurbocchargerBigColor);
                WriteXMLColorSaveElement(xmlWriter, "original_turbochargerSmall-color", originalTurbochargerSmallColor);
                WriteXMLColorSaveElement(xmlWriter, "original_turbocharger_small_airfilter-color", original_turbocharger_small_airfilter_color);

                WriteXMLColorSaveElement(xmlWriter, "original_weber-color", originalTurbochargerManifoldWeberColor);
                WriteXMLColorSaveElement(xmlWriter, "original_twincarb-color", originalTurbochargerManifoldTwinCarbColor);
                WriteXMLColorSaveElement(xmlWriter, "original_blowoffValve-color", originalturbocharger_bigBlowoffValveColor);
            }

            else
            {
                Color defaultColor = new Color(0.800f, 0.800f, 0.800f);
                WriteXMLColorSaveElement(xmlWriter, "hood-color", defaultColor);
                WriteXMLColorSaveElement(xmlWriter, "intercooler-color", defaultColor);
                WriteXMLColorSaveElement(xmlWriter, "turbocharger_big-color", defaultColor);
                WriteXMLColorSaveElement(xmlWriter, "turbochargerSmall-color", defaultColor);
                WriteXMLColorSaveElement(xmlWriter, "weber-color", defaultColor);
                WriteXMLColorSaveElement(xmlWriter, "twincarb-color", defaultColor);
                WriteXMLColorSaveElement(xmlWriter, "blowoffValve-color", defaultColor);
                WriteXMLColorSaveElement(xmlWriter, "turbocharger_small_airfilter-color", defaultColor);

                WriteXMLColorSaveElement(xmlWriter, "original_intercooler-color", defaultColor);
                WriteXMLColorSaveElement(xmlWriter, "original_turbocharger_big-color", defaultColor);
                WriteXMLColorSaveElement(xmlWriter, "original_turbochargerSmall-color", defaultColor);
                WriteXMLColorSaveElement(xmlWriter, "original_turbocharger_small_airfilter-color", defaultColor);
                WriteXMLColorSaveElement(xmlWriter, "original_weber-color", defaultColor);
                WriteXMLColorSaveElement(xmlWriter, "original_twincarb-color", defaultColor);
                WriteXMLColorSaveElement(xmlWriter, "original_blowoffValve-color", defaultColor);
            }
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
        }

        private void WriteXMLColorSaveElement(XmlWriter xmlWriter, string elementName, Color colorToSave)
        {
            xmlWriter.WriteStartElement(elementName);
            xmlWriter.WriteAttributeString("r", colorToSave.r.ToString());
            xmlWriter.WriteAttributeString("g", colorToSave.g.ToString());
            xmlWriter.WriteAttributeString("b", colorToSave.b.ToString());
            xmlWriter.WriteEndElement();
        }

        private void AddPartsColorMaterial()
        {

            try
            {
                turbocharger_hood_renderer = turbocharger_hood_part.rigidPart.GetComponentInChildren<MeshRenderer>();
                if (turbocharger_hood_renderer == null)
                {
                    turbocharger_hood_renderer = turbocharger_hood_part.activePart.GetComponentInChildren<MeshRenderer>();

                }
                if (turbocharger_hood_renderer != null)
                {
                    if (turbocharger_hood_renderer.material.name != "CAR_PAINT_REGULAR (Instance)")
                    {
                        turbocharger_hood_renderer.material = regularCarPaintMaterial;
                    }
                }
            }
            catch
            {

            }
            try
            {
                if (useDefaultColors)
                {
                    SetPartMaterialColor(turbocharger_hood_part, hoodColor);
                    SetPartMaterialColor(turbocharger_big_part, originalTurbocchargerBigColor);
                    SetPartMaterialColor(turbocharger_small_part, originalTurbochargerSmallColor);
                    SetPartMaterialColor(turbocharger_intercooler_part, originalIntercoolerColor);
                    SetPartMaterialColor(turbocharger_manifold_weber_part, originalTurbochargerManifoldWeberColor);
                    SetPartMaterialColor(turbocharger_manifold_twinCarb_part, originalTurbochargerManifoldTwinCarbColor);
                    SetPartMaterialColor(turbocharger_big_blowoff_valve_part, originalturbocharger_bigBlowoffValveColor);
                    SetPartMaterialColor(turbocharger_small_airfilter_part, original_turbocharger_small_airfilter_color);
                }
                else
                {
                    SetPartMaterialColor(turbocharger_hood_part, hoodColor);
                    SetPartMaterialColor(turbocharger_big_part, turbocharger_bigColor);
                    SetPartMaterialColor(turbocharger_small_part, turbochargerSmallColor);
                    SetPartMaterialColor(turbocharger_intercooler_part, intercoolerColor);
                    SetPartMaterialColor(turbocharger_manifold_weber_part, turbochargerManifoldWeberColor);
                    SetPartMaterialColor(turbocharger_manifold_twinCarb_part, turbochargerManifoldTwinCarbColor);
                    SetPartMaterialColor(turbocharger_big_blowoff_valve_part, turbocharger_bigBlowoffValveColor);
                    SetPartMaterialColor(turbocharger_small_airfilter_part, turbocharger_small_airfilter_color);
                }

            }
            catch
            {

            }
        }

        private void SetPartMaterialColor(Part part, Color colorToPaint)
        {
            try
            {
                MeshRenderer meshRenderer = part.rigidPart.GetComponentInChildren<MeshRenderer>();
                if (meshRenderer == null)
                {
                    meshRenderer = part.activePart.GetComponentInChildren<MeshRenderer>();
                }
                if (meshRenderer.material.color != colorToPaint)
                {
                    for (int i = 0; i < meshRenderer.materials.Length; i++)
                    {
                        if (meshRenderer.materials[i].name == "Red_Acent (Instance)" || meshRenderer.materials[i].name == "CAR_PAINT_REGULAR (Instance)")
                        {
                            meshRenderer.materials[i].SetColor("_Color", colorToPaint);
                        }
                    }
                }
            }
            catch
            {
        
            }

        }

        private void SetBoostGaugeText(float valueToDisplay, bool positive)
        {
            try
            {
                if (turbocharger_boost_gauge_part.installed)
                {
                    if (electricityOn == true)
                    {
                        if (positive == true)
                        {
                            boostGaugeTextMesh.text = valueToDisplay.ToString("0.00");
                        }
                        else
                            boostGaugeTextMesh.text = "-" + valueToDisplay.ToString(".00");
                    }
                    else
                    {
                        boostGaugeTextMesh.text = "";

                    }
                }
            }
            catch
            {

            }
            
        }

        private void _triggerBlowOff()
        {
            if(turbocharger_big_part.installed && turbocharger_big_blowoff_valve_part.installed)
            {
                if (turboBlowOffShot == null)
                {
                    CreateTurboBlowoff();
                }
                turbocharger_blowoff.Play();
            }


            if (turbocharger_big_part.installed && turboBlowOffShot != null && turboBlowOffShot.volume != 0.20f)
            {
                turboBlowOffShot.volume = 0.20f;
            }
            
            timeSinceLastBlowOff = 0;
            turbocharger_blowoffShotAllowed = false;
        }

        private void RotateTurbineWheel()
        {
            if (turbocharger_big_turbine == null)
            {
                turbocharger_big_turbine = GameObject.Find("TurboCharger_Big_Compressor_Turbine");
            }
            if (turbocharger_big_turbine != null)
            {
                turbocharger_big_turbine.transform.Rotate(0, 0, (engineRPM / 500));
            }
        }

        private void CalculateAndSetEnginePowerTurbo()
        {
            if (turbocharger_big_part.installed && turbocharger_big_blowoff_valve_part.installed)
            {
                if (twinCarb_inst)
                {
                    othersSave.turbocharger_big_max_boost_limit = (2.2f + 0.05f);
                }
                else if (weberCarb_inst)
                {
                    othersSave.turbocharger_big_max_boost_limit = (2.2f + 0.30f);
                }
                if (othersSave.turbocharger_big_max_boost >= othersSave.turbocharger_big_max_boost_limit)
                {
                    othersSave.turbocharger_big_max_boost = othersSave.turbocharger_big_max_boost_limit;
                }
            }
            else if (turbocharger_small_part.installed && !turbocharger_big_blowoff_valve_part.installed)
            {
                if (twinCarb_inst && turbocharger_intercooler_part.installed && turbocharger_small_airfilter_part.installed)
                {
                    othersSave.turbocharger_small_max_boost_limit = (0.95f + 0.05f + 0.15f + 0.19f);
                }
                else if (twinCarb_inst && turbocharger_intercooler_part.installed)
                {
                    othersSave.turbocharger_small_max_boost_limit = (0.95f + 0.05f + 0.15f);
                }
                else if (twinCarb_inst && turbocharger_small_airfilter_part.installed)
                {
                    othersSave.turbocharger_small_max_boost_limit = (0.95f + 0.05f + 0.00f + 0.11f);
                }
                else if (weberCarb_inst && turbocharger_intercooler_part.installed && turbocharger_small_airfilter_part.installed)
                {
                    othersSave.turbocharger_small_max_boost_limit = (0.95f + 0.11f + 0.15f + 0.19f);
                }
                else if (weberCarb_inst && turbocharger_intercooler_part.installed)
                {
                    othersSave.turbocharger_small_max_boost_limit = (0.95f + 0.11f + 0.15f);
                }
                else
                {
                    othersSave.turbocharger_small_max_boost_limit = (0.95f + 0.05f);
                }
                if (othersSave.turbocharger_small_max_boost >= othersSave.turbocharger_small_max_boost_limit)
                {
                    othersSave.turbocharger_small_max_boost = othersSave.turbocharger_small_max_boost_limit;
                }
            }

            if (electricityOn == true)
            {
                if (ecu_mod_installed)
                {
                    if (ecu_mod_SmartEngineModule == null)
                    {
                        ecu_mod_SmartEngineModule = GameObject.Find("Smart Engine ECU(Clone)");
                    }
                    if (ecu_mod_SmartEngineModule != null)
                    {
                        Component ecu_mod_ModCommunication = ecu_mod_SmartEngineModule.GetComponent("ModCommunication");
                        Type compType = ecu_mod_ModCommunication.GetType();
                        FieldInfo alsEnabledInfo = compType.GetField("alsEnabled");
                        ecu_mod_alsEnabled = (bool)alsEnabledInfo.GetValue(ecu_mod_ModCommunication);
                    }
                }
                if (turbocharger_big_part.installed && turbocharger_big_blowoff_valve_part.installed)
                {
                    if (ecu_mod_alsEnabled)
                    {
                        newTurboChargerBar = Convert.ToSingle(Math.Log(satsumaDriveTrain.maxRPM / 2800, 100)) * 13f;
                    }
                    else
                    {
                        newTurboChargerBar = Convert.ToSingle(Math.Log(engineRPM / 2800, 100)) * 13f;
                    }

                    if (newTurboChargerBar > othersSave.turbocharger_big_max_boost)
                    {
                        newTurboChargerBar = othersSave.turbocharger_big_max_boost;
                    }

                    if (turbocharger_big_part.installed)
                    {
                        if (newTurboChargerBar >= 0.2f)
                        {
                            if (partsWearSave.turbocharger_big_wear <= 0)
                            {
                                partsWearSave.turbocharger_big_wear = 0;
                            }
                            else if (timer_wear_turbocharger_big >= 0.5f)
                            {
                                timer_wear_turbocharger_big = 0;
                                partsWearSave.turbocharger_big_wear -= (newTurboChargerBar * 0.025f);
                            }
                            if(partsWearSave.turbocharger_big_wear < 25f)
                            {
                                if (!turboGrindingLoop.isPlaying)
                                {
                                    turboGrindingLoop.Play();
                                }
                                turboGrindingLoop.volume = engineRPM * 0.00008f;
                                turboGrindingLoop.pitch = engineRPM * 0.00012f;
                            }

                        }
                        else
                        {
                            timer_wear_turbocharger_big = 0;
                        }
                    }
                    else if (turbocharger_small_part.installed)
                    {
                        if (newTurboChargerBar >= 0.2f)
                        {
                            if (partsWearSave.turbocharger_small_wear <= 0)
                            {
                                partsWearSave.turbocharger_small_wear = 0;
                            }
                            else if (timer_wear_turbocharger_small >= 0.5f)
                            {
                                timer_wear_turbocharger_small = 0;
                                if (turbocharger_small_airfilter_part.installed)
                                {
                                    partsWearSave.turbocharger_small_wear -= (newTurboChargerBar * ((0.025f + 0.035f) - (partsWearSave.airfilter_wear / 10000)));
                                }
                                else
                                {
                                    partsWearSave.turbocharger_small_wear -= (newTurboChargerBar * 0.025f);
                                }
                                if (partsWearSave.turbocharger_big_wear < 25f)
                                {
                                    if (!turboGrindingLoop.isPlaying)
                                    {
                                        turboGrindingLoop.Play();
                                    }
                                    turboGrindingLoop.volume = engineRPM * 0.00008f;
                                    turboGrindingLoop.pitch = engineRPM * 0.00012f;
                                }
                            }
                        }
                        else
                        {
                            timer_wear_turbocharger_small = 0;
                        }
                    }
                    if (turbocharger_small_airfilter_part.installed)
                    {
                        if (newTurboChargerBar >= 0.2f)
                        {
                            if (partsWearSave.airfilter_wear <= 0)
                            {
                                partsWearSave.airfilter_wear = 0;
                            }
                            else if (timer_wear_airfilter >= 0.5f)
                            {
                                timer_wear_airfilter = 0;
                                partsWearSave.airfilter_wear -= (newTurboChargerBar * 0.035f);
                            }
                        }
                        else
                        {
                            timer_wear_airfilter = 0;
                        }
                    }
                    if (turbocharger_intercooler_part.installed)
                    {
                        if (newTurboChargerBar >= 0.2f)
                        {
                            if (partsWearSave.intercooler_wear <= 0)
                            {
                                partsWearSave.intercooler_wear = 0;
                            }
                            else if (timer_wear_intercooler >= 0.5f)
                            {
                                timer_wear_intercooler = 0;
                                partsWearSave.intercooler_wear -= (newTurboChargerBar * 0.035f);
                            }
                        }
                        else
                        {
                            timer_wear_intercooler = 0;
                        }
                    }

                    


                    if (newTurboChargerBar > 0f)
                    {
                        if (partsWearSave.intercooler_wear >= 75)
                        {
                        }
                        else if (partsWearSave.intercooler_wear >= 50f)
                        {
                            newTurboChargerBar /= 1.2f;
                        }
                        else if (partsWearSave.intercooler_wear >= 25f)
                        {
                            newTurboChargerBar /= 1.4f;
                        }
                        else if (partsWearSave.intercooler_wear >= 15f)
                        {
                            newTurboChargerBar /= 1.8f;
                        }
                        else if (partsWearSave.intercooler_wear < 15f)
                        {
                            newTurboChargerBar = 0;
                        }

                        if(satsumaDriveTrain.rpm >= 4000 && !useThrottleButton && fire_fx_big_turbo_exhaust_straight != null && !fire_fx_big_turbo_exhaust_straight.isPlaying)
                        {
                            
                            if (ecu_mod_installed && ecu_mod_alsEnabled)
                            {
                                Random randomShouldBackfire = new Random();
                                if (randomShouldBackfire.Next(3) == 1)
                                {
                                    backfire_fx_big_turbo_exhaust_straight.Play();
                                    fire_fx_big_turbo_exhaust_straight.Emit(2);
                                }
                                
                            }
                            else if(canBackfire)
                            {
                                Random randomShouldBackfire = new Random();
                                if (randomShouldBackfire.Next(8) == 1)
                                {
                                    backfire_fx_big_turbo_exhaust_straight.Play();
                                    fire_fx_big_turbo_exhaust_straight.Emit(2);
                                    canBackfire = false;
                                }
                            }
                        }
                        if(satsumaDriveTrain.rpm <= 3500)
                        {
                            canBackfire = true;
                        }
                        
                        SetBoostGaugeText(newTurboChargerBar, true);
                        _enginePowerMultiplier.Value = (0.90f + (newTurboChargerBar * 1.5f));
                    }
                    else
                    {
                        SetBoostGaugeText(0.10f, false);
                        _enginePowerMultiplier.Value = 0.90f;
                    }
                }
                else if (turbocharger_small_part.installed && !turbocharger_big_blowoff_valve_part.installed)
                {
                    if (ecu_mod_alsEnabled)
                    {
                        newTurboChargerBar = Convert.ToSingle(Math.Log(satsumaDriveTrain.maxRPM / 1600, 10)) * 2.2f;
                    }
                    else
                    {
                        newTurboChargerBar = Convert.ToSingle(Math.Log(engineRPM / 1600, 10)) * 2.2f;
                    }

                    if (newTurboChargerBar > othersSave.turbocharger_small_max_boost)
                    {
                        newTurboChargerBar = othersSave.turbocharger_small_max_boost;
                    }

                    if (newTurboChargerBar > 0f)
                    {
                        if (partsWearSave.intercooler_wear >= 75f)
                        {

                        }
                        else if (partsWearSave.intercooler_wear >= 50f)
                        {
                            newTurboChargerBar /= 1.2f;
                        }
                        else if (partsWearSave.intercooler_wear >= 25f)
                        {
                            newTurboChargerBar /= 1.4f;
                        }
                        else if (partsWearSave.intercooler_wear >= 15f)
                        {
                            newTurboChargerBar /= 1.8f;
                        }
                        else if (partsWearSave.intercooler_wear < 15f)
                        {
                            newTurboChargerBar = 0;
                        }

                        SetBoostGaugeText(newTurboChargerBar, true);
                        _enginePowerMultiplier.Value = (0.96f + (newTurboChargerBar * 1.5f));
                    }
                    else
                    {
                        SetBoostGaugeText(0.04f, false);
                        _enginePowerMultiplier.Value = 0.96f;
                    }

                    /*
                    if (small_turbo_fire_fx != null)
                    {
                        if (newTurboChargerBar >= othersSave.turbocharger_small_max_boost - 0.1f)
                        {
                            if (!small_turbo_fire_fx.isPlaying)
                            {
                                small_turbo_fire_fx.Play();
                            }
                        }
                        else
                        {
                            small_turbo_fire_fx.Stop();
                        }
                    }
                    */
                }
                if(satsumaDriveTrain.rpm >= 400)
                {
                    CheckPartsWear();
                }
            }
            else
            {
                
                timer_wear_turbocharger_big = 0;
                timer_wear_turbocharger_small = 0;
                timer_wear_airfilter = 0;
                timer_wear_intercooler = 0;

                SetBoostGaugeText(0.00f, false);
                enginePowerMultiplier = 0;

            }

        }

        private void CreateTurboGrindingLoop()
        {
            turboGrindingLoop = turbocharger_big_part.rigidPart.AddComponent<AudioSource>();
            turbocharger_grinding_loop.audioSource = turboGrindingLoop;
            turbocharger_grinding_loop.LoadAudioFromFile(Path.Combine(ModLoader.GetModAssetsFolder(this), "grinding sound.wav"), true, false);

            turboGrindingLoop.rolloffMode = AudioRolloffMode.Linear;
            turboGrindingLoop.minDistance = 1;
            turboGrindingLoop.maxDistance = 10;
            turboGrindingLoop.spatialBlend = 0.6f;
            turboGrindingLoop.loop = true;
        }

        public void CreateTurboLoopBig()
        {
            if (turboGrindingLoop == null)
            {
                CreateTurboGrindingLoop();
            }
            //Creates the TurboLoop loading the file "turbocharger_loop.wav" from the Asset folder of the mod
            //And setting it to loop

            turboLoopBig = turbocharger_big_part.rigidPart.AddComponent<AudioSource>();
            turbocharger_loop_big.audioSource = turboLoopBig;
            turbocharger_loop_big.LoadAudioFromFile(Path.Combine(ModLoader.GetModAssetsFolder(this), "turbocharger_loop.wav"), true, false);

            

            turboLoopBig.rolloffMode = AudioRolloffMode.Custom;
            turboLoopBig.minDistance = 1;
            turboLoopBig.maxDistance = 10;
            turboLoopBig.spatialBlend = 1;
            turboLoopBig.loop = true;
            turbocharger_loop_big.Play();

        }
        public void CreateTurboLoopSmall()
        {
            turboLoopSmall = turbocharger_small_part.rigidPart.AddComponent<AudioSource>();
            turbocharger_loop_small.audioSource = turboLoopSmall;
            turbocharger_loop_small.LoadAudioFromFile(Path.Combine(ModLoader.GetModAssetsFolder(this), "turbocharger_loop.wav"), true, false);

            turboLoopSmall.rolloffMode = AudioRolloffMode.Custom;
            turboLoopSmall.minDistance = 1;
            turboLoopSmall.maxDistance = 10;
            turboLoopSmall.spatialBlend = 1;
            turboLoopSmall.loop = true;
            turbocharger_loop_small.Play();
        }

        public void CreateTurboBlowoff()
        {
            //Creates the TurboBlowoff  loading the file "turbocharger_blowoff.wav" from the Asset folder of the mod

            turboBlowOffShot = turbocharger_big_blowoff_valve_part.rigidPart.AddComponent<AudioSource>();
            turbocharger_blowoff.audioSource = turboBlowOffShot;

            turbocharger_blowoff.LoadAudioFromFile(Path.Combine(ModLoader.GetModAssetsFolder(this), "turbocharger_blowoff.wav"), true, false);
            turboBlowOffShot.minDistance = 1;
            turboBlowOffShot.maxDistance = 10;
            turboBlowOffShot.spatialBlend = 1;
        }

        private void DetectPaintingPart()
        {
            try
            {
                FsmGameObject itemInHand = PlayMakerGlobals.Instance.Variables.FindFsmGameObject("ItemPivot");
                if (itemInHand.Value.GetComponentInChildren<MeshRenderer>() != null)
                {

                    if (itemInHand.Value.GetComponentInChildren<MeshRenderer>().name == "spray can(itemx)" && Input.GetKey(KeyCode.F))
                    {
                        sprayCansMeshRenders = itemInHand.Value.GetComponentsInChildren<MeshRenderer>();
                        isItemInHand = true;
                    }
                    else if(itemInHand.Value.GetComponentInChildren<MeshRenderer>().name == "spray can(itemx)" && isItemInHand == true)
                    {
                        isItemInHand = false;
                    }

                }
                if (Camera.main != null)
                {
                    if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 0.8f, 1 << LayerMask.NameToLayer("Parts")) != false)
                    {
                        GameObject gameObjectHit;
                        gameObjectHit = hit.collider?.gameObject;
                        if (gameObjectHit != null)
                        {
                            if (hit.collider)
                            {
                                if (isItemInHand)
                                {
                                    if (
                                     gameObjectHit.name == "Racing Turbocharger Hood"
                                     || gameObjectHit.name == "Racing Turbocharger Hood(Clone)"
                                     || gameObjectHit.name == "Intercooler"
                                     || gameObjectHit.name == "Intercooler(Clone)"
                                     || gameObjectHit.name == "Racing Turbocharger"
                                     || gameObjectHit.name == "Racing Turbocharger(Clone)"
                                     || gameObjectHit.name == "GT Turbocharger"
                                     || gameObjectHit.name == "GT Turbocharger(Clone)"
                                     || gameObjectHit.name == "Weber Manifold"
                                     || gameObjectHit.name == "Weber Manifold(Clone)"
                                     || gameObjectHit.name == "TwinCarb Manifold"
                                     || gameObjectHit.name == "TwinCarb Manifold(Clone)"
                                     || gameObjectHit.name == "Racing Turbocharger Blowoff Valve"
                                     || gameObjectHit.name == "Racing Turbocharger Blowoff Valve(Clone)"
                                     || gameObjectHit.name == "GT Turbocharger Airfilter"
                                     || gameObjectHit.name == "GT Turbocharger Airfilter(Clone)"
                                     )
                                    {
                                        if (Input.GetMouseButton(0))
                                        {
                                            pickedUPsprayCanColor = sprayCansMeshRenders[1].material.color;
                                            string test2 = sprayCansMeshRenders[1].material.name;
                                            int colorID = 0;
                                            if (sprayCansMeshRenders[1].material.name == "colormatte01 (Instance)")
                                            {
                                                colorID = 13;
                                            }
                                            else
                                            {
                                                colorID = Convert.ToInt32(sprayCansMeshRenders[1].material.name.Replace("color", "").Replace(" (Instance)", ""));
                                            }
                                            int arrIndex = colorID - 1;
                                            if (arrIndex < 0 || arrIndex > 12)
                                            {
                                                arrIndex = 0;
                                            }


                                            if (gameObjectHit.name == "Racing Turbocharger Hood" || gameObjectHit.name == "Racing Turbocharger Hood(Clone)")
                                            {
                                                hoodColor = pickedUPsprayCanColor;

                                            }
                                            if (gameObjectHit.name == "Intercooler" || gameObjectHit.name == "Intercooler(Clone)")
                                            {
                                                intercoolerColor = modSprayColors[arrIndex];
                                                originalIntercoolerColor = pickedUPsprayCanColor;
                                            }

                                            if (gameObjectHit.name == "Racing Turbocharger" || gameObjectHit.name == "Racing Turbocharger(Clone)")
                                            {
                                                turbocharger_bigColor = modSprayColors[arrIndex];
                                                originalTurbocchargerBigColor = pickedUPsprayCanColor;
                                            }

                                            if (gameObjectHit.name == "GT Turbocharger" || gameObjectHit.name == "GT Turbocharger(Clone)")
                                            {
                                                turbochargerSmallColor = modSprayColors[arrIndex];
                                                originalTurbochargerSmallColor = pickedUPsprayCanColor;
                                            }

                                            if (gameObjectHit.name == "Weber Manifold" || gameObjectHit.name == "Weber Manifold(Clone)")
                                            {
                                                turbochargerManifoldWeberColor = modSprayColors[arrIndex];
                                                originalTurbochargerManifoldWeberColor = pickedUPsprayCanColor;
                                            }

                                            if (gameObjectHit.name == "TwinCarb Manifold" || gameObjectHit.name == "TwinCarb Manifold(Clone)")
                                            {
                                                turbochargerManifoldTwinCarbColor = modSprayColors[arrIndex];
                                                originalTurbochargerManifoldTwinCarbColor = pickedUPsprayCanColor;
                                            }

                                            if (gameObjectHit.name == "Racing Turbocharger Blowoff Valve" || gameObjectHit.name == "Racing Turbocharger Blowoff Valve(Clone)")
                                            {
                                                turbocharger_bigBlowoffValveColor = modSprayColors[arrIndex];
                                                originalturbocharger_bigBlowoffValveColor = pickedUPsprayCanColor;
                                            }
                                            if(gameObjectHit.name == "GT Turbocharger Airfilter" || gameObjectHit.name == "GT Turbocharger Airfilter(Clone)")
                                            {
                                                turbocharger_small_airfilter_color = modSprayColors[arrIndex];
                                                original_turbocharger_small_airfilter_color = pickedUPsprayCanColor;
                                            }

                                            MeshRenderer partRenderer = hit.collider.GetComponentInChildren<MeshRenderer>();
                                            for (int i = 0; i < partRenderer.materials.Length; i++)
                                            {
                                                if (partRenderer.materials[i].name == "Red_Acent (Instance)" || partRenderer.materials[i].name == "CAR_PAINT_REGULAR (Instance)")
                                                {
                                                    if (useDefaultColors != true)
                                                    {
                                                        partRenderer.materials[i].SetColor("_Color", modSprayColors[arrIndex]);
                                                    }

                                                }
                                            }

                                        }
                                    }
                                }//Move somewhere else Boost changer
                                else if (gameObjectHit.name == "Racing Turbocharger Blowoff Valve" || gameObjectHit.name == "Racing Turbocharger Blowoff Valve(Clone)"){
                                    if (turbocharger_big_blowoff_valve_part.installed && turbocharger_big_screwable.partFixed && turbocharger_big_part.installed)
                                    {
                                        
                                        if (Input.GetAxis("Mouse ScrollWheel") > 0f) // forward
                                        {
                                            if (othersSave.turbocharger_big_max_boost != othersSave.turbocharger_big_max_boost_limit)
                                            {
                                                othersSave.turbocharger_big_max_boost += 0.05f;
                                            }
                                        }
                                        else if (Input.GetAxis("Mouse ScrollWheel") < 0f) // backwards
                                        {
                                            if (othersSave.turbocharger_big_max_boost > 1.55f)
                                            {
                                                othersSave.turbocharger_big_max_boost -= 0.05f;
                                            }
                                        }
                                        ModClient.guiInteract("Increase/Decrease Max Boost: " + othersSave.turbocharger_big_max_boost.ToString("0.00"));
                                    }
                                }
                                else if(gameObjectHit.name == "Turbocharger_Small_Wastegate" || gameObjectHit.name == "Turbocharger_Small_Wastegate(Clone)")
                                {
                                    if (turbocharger_small_part.installed && turbocharger_small_screwable.partFixed)
                                    {
                                        if (Input.GetAxis("Mouse ScrollWheel") > 0f) // forward
                                        {
                                            if (othersSave.turbocharger_small_max_boost != othersSave.turbocharger_small_max_boost_limit)
                                            {
                                                othersSave.turbocharger_small_max_boost += 0.01f;
                                            }
                                        }
                                        else if (Input.GetAxis("Mouse ScrollWheel") < 0f) // backwards
                                        {
                                            
                                            if (othersSave.turbocharger_small_max_boost > 0.8f)
                                            {
                                                othersSave.turbocharger_small_max_boost -= 0.01f;
                                            }
                                        }
                                        ModClient.guiInteract("Increase/Decrease Max Boost: " + othersSave.turbocharger_small_max_boost.ToString("0.00"));
                                    }
                                    
                                }
                            }
                        }

                    }
                }
            }
            catch
            {

            }
            
        }

        private static void ToggleUseDefaultColors()
        {
            useDefaultColors = !useDefaultColors;
        }

        private static void PosReset()
        {
            if (!turbocharger_big_part.installed)
            {
                turbocharger_big_part.activePart.transform.position = turbocharger_big_part.defaultPartSaveInfo.position;
            }

            if (!turbocharger_big_intercooler_tube_part.installed)
            {
                turbocharger_big_intercooler_tube_part.activePart.transform.position = turbocharger_big_intercooler_tube_part.defaultPartSaveInfo.position;
            }

            if (!turbocharger_big_exhaust_inlet_tube_part.installed)
            {
                turbocharger_big_exhaust_inlet_tube_part.activePart.transform.position = turbocharger_big_exhaust_inlet_tube_part.defaultPartSaveInfo.position;
            }

            if (!turbocharger_big_exhaust_outlet_tube_part.installed)
            {
                turbocharger_big_exhaust_outlet_tube_part.activePart.transform.position = turbocharger_big_exhaust_outlet_tube_part.defaultPartSaveInfo.position;
            }

            if (!turbocharger_big_blowoff_valve_part.installed)
            {
                turbocharger_big_blowoff_valve_part.activePart.transform.position = turbocharger_big_blowoff_valve_part.defaultPartSaveInfo.position;
            }

            if (!turbocharger_big_exhaust_outlet_straight_part.installed)
            {
                turbocharger_big_exhaust_outlet_straight_part.activePart.transform.position = turbocharger_big_exhaust_outlet_straight_part.defaultPartSaveInfo.position;
            }


            if (!turbocharger_small_part.installed)
            {
                turbocharger_small_part.activePart.transform.position = turbocharger_small_part.defaultPartSaveInfo.position;
            }

            if (!turbocharger_small_intercooler_tube_part.installed)
            {
                turbocharger_small_intercooler_tube_part.activePart.transform.position = turbocharger_small_intercooler_tube_part.defaultPartSaveInfo.position;
            }

            if (!turbocharger_small_exhaust_inlet_tube_part.installed)
            {
                turbocharger_small_exhaust_inlet_tube_part.activePart.transform.position = turbocharger_small_exhaust_inlet_tube_part.defaultPartSaveInfo.position;
            }

            if (!turbocharger_small_exhaust_outlet_tube_part.installed)
            {
                turbocharger_small_exhaust_outlet_tube_part.activePart.transform.position = turbocharger_small_exhaust_outlet_tube_part.defaultPartSaveInfo.position;
            }


            if (!turbocharger_exhaust_header_part.installed)
            {
                turbocharger_exhaust_header_part.activePart.transform.position = turbocharger_exhaust_header_part.defaultPartSaveInfo.position;
            }
            if (!turbocharger_hood_part.installed)
            {
                turbocharger_hood_part.activePart.transform.position = turbocharger_hood_part.defaultPartSaveInfo.position;
            }

            if (!turbocharger_manifold_weber_part.installed)
            {
                turbocharger_manifold_weber_part.activePart.transform.position = turbocharger_manifold_weber_part.defaultPartSaveInfo.position;
            }

            if (!turbocharger_manifold_twinCarb_part.installed)
            {
                turbocharger_manifold_twinCarb_part.activePart.transform.position = turbocharger_manifold_twinCarb_part.defaultPartSaveInfo.position;
            }

            if (!turbocharger_boost_gauge_part.installed)
            {
                turbocharger_boost_gauge_part.activePart.transform.position = turbocharger_boost_gauge_part.defaultPartSaveInfo.position;
            }
            if (!turbocharger_intercooler_part.installed)
            {
                turbocharger_intercooler_part.activePart.transform.position = turbocharger_intercooler_part.defaultPartSaveInfo.position;
            }

            if (!turbocharger_intercooler_manifold_tube_weber_part.installed)
            {
                turbocharger_intercooler_manifold_tube_weber_part.activePart.transform.position = turbocharger_intercooler_manifold_tube_weber_part.defaultPartSaveInfo.position;
            }

            if (!turbocharger_intercooler_manifold_tube_twinCarb_part.installed)
            {
                turbocharger_intercooler_manifold_tube_twinCarb_part.activePart.transform.position = turbocharger_intercooler_manifold_tube_twinCarb_part.defaultPartSaveInfo.position;
            }

            if (!turbocharger_small_airfilter_part.installed)
            {
                turbocharger_small_airfilter_part.activePart.transform.position = turbocharger_small_airfilter_part.defaultPartSaveInfo.position;
            }

            if (!turbocharger_small_manifold_twinCarb_tube_part.installed)
            {
                turbocharger_small_manifold_twinCarb_tube_part.activePart.transform.position = turbocharger_small_manifold_twinCarb_tube_part.defaultPartSaveInfo.position;
            }
        }

        private void AddPartsNames()
        {
            //Big Turbocharger
            turbocharger_big.name = "Racing Turbocharger";
            turbocharger_big_intercooler_tube.name = "Racing Turbocharger Intercooler Tube";
            turbocharger_big_exhaust_inlet_tube.name = "Racing Turbocharger Exhaust Inlet Tube";
            turbocharger_big_exhaust_outlet_tube.name = "Racing Turbocharger Exhaust Outlet Tube";
            turbocharger_big_blowoff_valve.name = "Racing Turbocharger Blowoff Valve";
            turbocharger_big_exhaust_outlet_straight.name = "Racing Turbocharger Exhaust Straight";

            turbocharger_exhaust_header.name = "Turbocharger Exhaust Header";

            //Small Turbocharger
            turbocharger_small.name = "GT Turbocharger";
            turbocharger_small_intercooler_tube.name = "GT Turbocharger Intercooler Tube";
            turbocharger_small_exhaust_inlet_tube.name = "GT Turbocharger Exhaust Inlet Tube";
            turbocharger_small_exhaust_outlet_tube.name = "GT Turbocharger Exhaust Outlet Tube";
            turbocharger_small_manifold_twinCarb_tube.name = "GT Turbocharger Manifold TwinCarb Tube";
            turbocharger_small_airfilter.name = "GT Turbocharger Airfilter";

            //Other Turbo parts
            turbocharger_hood.name = "Racing Turbocharger Hood";
            turbocharger_manifold_weber.name = "Weber Manifold";
            turbocharger_manifold_twinCarb.name = "TwinCarb Manifold";
            turbocharger_intercooler.name = "Intercooler";
            turbocharger_intercooler_manifold_tube_weber.name = "Weber Intercooler-Manifold Tube";
            turbocharger_intercooler_manifold_tube_twinCarb.name = "TwinCarb Intercooler-Manifold Tube";
            turbocharger_boost_gauge.name = "Boost Gauge";
        }

        private void AddParts_trigger(GameObject originalCylinerHead)
        {
            turbocharger_big_trigger = new Trigger("turbocharger_big_trigger", originalCylinerHead, turbocharger_big_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.3f, 0.3f, 0.3f), false);
            turbocharger_big_intercooler_tube_trigger = new Trigger("turbocharger_bigIntercoolerTube_trigger", satsuma, turbocharger_big_intercooler_tube_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.3f, 0.3f, 0.3f), false);
            turbocharger_big_exhaust_inlet_tube_trigger = new Trigger("turbocharger_bigExhaustInletTube_trigger", originalCylinerHead, turbocharger_big_exhaust_inlet_tube_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.3f, 0.3f, 0.3f), false);
            turbocharger_big_exhaust_outlet_tube_trigger = new Trigger("turbocharger_bigExhaustOutletTube_trigger", originalCylinerHead, turbocharger_small_exhaust_outlet_tube_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.3f, 0.3f, 0.3f), false);
            turbocharger_big_blowoff_valve_trigger = new Trigger("turbocharger_bigBlowoffValve_trigger", satsuma, turbocharger_big_blowoff_valve_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.3f, 0.3f, 0.3f), false);
            turbocharger_big_exhaust_outlet_straight_trigger = new Trigger("turbocharger_bigExhaustOutletStraight_trigger", originalCylinerHead, turbocharger_big_exhaust_outlet_straight_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.3f, 0.3f, 0.3f), false);

            turbocharger_exhaust_header_trigger = new Trigger("turbocharger_big_exhaust_header_trigger", originalCylinerHead, turbocharger_exhaust_header_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.3f, 0.3f, 0.3f), false);

            turbocharger_small_trigger = new Trigger("TurbochargerSmall_trigger", originalCylinerHead, turbocharger_small_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.3f, 0.3f, 0.3f), false);
            turbocharger_small_intercooler_tube_trigger = new Trigger("TurbochargerSmallIntercoolerTube_trigger", satsuma, turbocharger_small_intercooler_tube_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.3f, 0.3f, 0.3f), false);
            turbocharger_small_exhaust_inlet_tube_trigger = new Trigger("TurbochargerSmallExhaustInletTube_trigger", originalCylinerHead, turbocharger_small_exhaust_inlet_tube_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.3f, 0.3f, 0.3f), false);
            turbocharger_small_exhaust_outlet_tube_trigger = new Trigger("TurbochargerSmallExhaustOutletTube_trigger", originalCylinerHead, turbocharger_small_exhaust_outlet_tube_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.3f, 0.3f, 0.3f), false);
            turbocharger_small_manifold_twinCarb_tube_trigger = new Trigger("turbocharger_small_manifold_twinCarb_tube_trigger", originalCylinerHead, turbocharger_small_manifold_twinCarb_tube_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.3f, 0.3f, 0.3f), false);
            turbocharger_small_airfilter_trigger = new Trigger("turbocharger_small_manifold_twinCarb_tube_trigger", originalCylinerHead, turbocharger_small_airfilter_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.3f, 0.3f, 0.3f), false);

            turbocharger_hood_trigger = new Trigger("TurbochargerHood_trigger", satsuma, turbocharger_hood_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.3f, 0.3f, 0.3f), false);
            turbocharger_manifoldWeber_trigger = new Trigger("TurbochargerManifoldWeber_trigger", originalCylinerHead, turbocharger_manifold_weber_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.3f, 0.3f, 0.3f), false);
            turbocharger_manifoldTwinCarb_trigger = new Trigger("TurbochargerManifoldTwinCarb_trigger", originalCylinerHead, turbocharger_manifold_twinCarb_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.3f, 0.3f, 0.3f), false);

            turbocharger_boostGauge_trigger = new Trigger("TurbochargerBoostGauge_trigger", GameObject.Find("dashboard(Clone)"), turbocharger_boost_gauge_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.3f, 0.3f, 0.3f), false);
            turbocharger_intercooler_trigger = new Trigger("TurbochargerIntercooler_trigger", satsuma, turbocharger_intercooler_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.3f, 0.3f, 0.3f), false);
            turbocharger_intercooler_manifold_tube_weber_trigger = new Trigger("TurbochargerIntercoolerManifoldTubeWeber_trigger", satsuma, turbocharger_intercooler_manifold_tube_weber_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.3f, 0.3f, 0.3f), false);
            turbocharger_intercooler_manifold_tube_twinCarb_trigger = new Trigger("TurbochargerIntercoolerManifoldTubeTwinCarb_trigger", satsuma, turbocharger_intercooler_manifold_tube_twinCarb_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.3f, 0.3f, 0.3f), false);

        }

        private bool CheckCloseToPosition(Vector3 positionOfPartTocheck, Vector3 position, float minimumDistance)
        {
            if (Vector3.Distance(positionOfPartTocheck, position) <= minimumDistance)
                return true;
            else
                return false;
        }
    }
}
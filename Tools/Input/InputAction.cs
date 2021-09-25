using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Eitrix
{
    /// ------------------------------------------------------
    /// <summary>
    /// Types of input
    /// </summary>
    /// ------------------------------------------------------
    public enum InputActionType
    {
        None,

        // General actions
        MenuSelect,
        MenuUp,
        MenuDown,
        MenuLeft,
        MenuRight,
        MenuDelete,
        Exit,
        ToggleStress,
        Advance,
        InstantComputer,

        // Game play actions

        MoveLeft,
        MoveRight,
        RotateRight,
        MoveDown,
        DropAndStick,
        DropAndSlide,
        RotateLeft,
        ChangeVictim,
        ApplyAntidote,
        DebugAction


    }

    /// ------------------------------------------------------
    /// <summary>
    /// Class for tracking input actions
    /// </summary>
    /// ------------------------------------------------------
    public class InputAction
    {
        public static float RepeatRate = 0.05f; // Seconds between click repeats
        public static float RepeatDelay = 0.30f; // Seconds until start of repeat
        public int PlayerID { get; set; }
        public InputActionType ActionType { get; set; }
        public int FrameCount { get; set; }
        public DateTime TimeStarted { get; set; }
        public int ID { get { return (int)ActionType * 1000000 + PlayerID; } }
        //public int FakeControllerID { get; set; }
        public int ControllerID { get; set; }
        public float AnalogValue { get; set; } // If available, this is a 0-1 floating value.  Otherwise 0,1 for buttons
        DateTime timeOfLastRepeat;

        /// <summary>
        /// True if this represents a "click" type action
        /// </summary>
        public bool Click
        {
            get
            {
                //if (ActionType == InputActionType.MenuDown || ActionType == InputActionType.MenuUp)
                //{
                //    //Debug.WriteLine(ActionType + " (" + ControllerID +", " + ID + ") " + FrameCount + ", " + (DateTime.Now - timeOfLastRepeat).TotalSeconds);
                //}
                if (FrameCount == 1) return true;
                if ((DateTime.Now - timeOfLastRepeat).TotalSeconds > RepeatRate) 
                {
                    timeOfLastRepeat = DateTime.Now;
                    return true;
                }
                return false;
            }
        }

        /// ------------------------------------------------------
        /// <summary>
        /// Constructor
        /// </summary>
        /// ------------------------------------------------------
        public InputAction()
        {
            Reset();
        }

        /// ------------------------------------------------------
        /// <summary>
        /// Reset this object with fresh input
        /// </summary>
        /// ------------------------------------------------------
        public void Reset()
        {
            TimeStarted = DateTime.Now;
            timeOfLastRepeat = TimeStarted.AddSeconds(RepeatDelay); 
            FrameCount = 0;

        }
    }
}

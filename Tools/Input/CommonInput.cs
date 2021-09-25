using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Eitrix
{
    /// --------------------------------------------------------------
    /// <summary>
    ///  CommonCode for an input Device
    /// </summary>
    /// --------------------------------------------------------------
    public abstract class CommonInput : IInputTool
    {
        List<InputAction> actionQueue = new List<InputAction>();
        Dictionary<int, InputAction> recentActions = new Dictionary<int, InputAction>();

        /// <summary>
        /// The screen being controlled. When swapping screens, set this to the current screen
        /// </summary>
        public ScreenType ScreenBeingControlled { get; set; }

        /// ---------------------------------------------------------------------------------
        /// <summary>
        /// Add an action to the actionQueue
        /// </summary>
        /// ---------------------------------------------------------------------------------
        protected void AddAction(InputAction action)
        {

            if (recentActions.ContainsKey(action.ID))
            {
                //Debug.WriteLine("Added " + action.ActionType);
                action = recentActions[action.ID];
            }
            else
            {
                //Debug.WriteLine("Reset " + action.ActionType);
                action.Reset();
            }

            actionQueue.Add(action);
        }

        /// ---------------------------------------------------------------------------------
        /// <summary>
        /// Indicates that we are done checking for input on this frame
        /// </summary>
        /// ---------------------------------------------------------------------------------
        protected void DoneWithFrame()
        {
            recentActions.Clear();
            foreach (InputAction action in actionQueue)
            {
                if (!recentActions.ContainsKey(action.ID))
                {
                    recentActions.Add(action.ID, action);
                    action.FrameCount++;
                }
            }
        }

        /// ---------------------------------------------------------------------------------
        /// <summary>
        ///  Get input items off the stack
        /// </summary>
        /// ---------------------------------------------------------------------------------
        public virtual IEnumerable<InputAction> ActionQueue
        {
            get
            {
                while (actionQueue.Count > 0)
                {
                    //Debug.WriteLine("Yielding Action: " + actionQueue[0].PlayerID + ")" + actionQueue[0].ActionType);
                    InputAction returnMe = actionQueue[0];
                    actionQueue.RemoveAt(0);
                    yield return returnMe;
                }
            }
        }

        /// ---------------------------------------------------------------------------------
        /// <summary>
        ///  Descendents have to implement this
        /// </summary>
        /// ---------------------------------------------------------------------------------
        public abstract void UpdateInput();

        /// ---------------------------------------------------------------------------------
        /// <summary>
        ///  Descendents have to implement this
        /// </summary>
        /// ---------------------------------------------------------------------------------
        public abstract void ReevaluateMappings();

        /// ---------------------------------------------------------------------------------
        /// <summary>
        /// Descendents implement this
        /// </summary>
        /// ---------------------------------------------------------------------------------
        public virtual void Reset()
        {
            actionQueue.Clear();
            recentActions.Clear();
        }


        /// ---------------------------------------------------------------------------------
        /// <summary>
        /// Descendents implement these
        /// </summary>
        /// ---------------------------------------------------------------------------------
        public abstract void SaveToDisk();
        public abstract void ReadFromDisk();
    }
}

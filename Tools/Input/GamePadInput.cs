using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

namespace Eitrix
{
    /// ------------------------------------------------------------------------
    /// <summary>
    /// Code for tracking gamepad input
    /// </summary>
    /// ------------------------------------------------------------------------
    public class GamePadInput : CommonInput
    {
        delegate InputAction PadReader(GamePadState padState, int controllerNumber);
        public static bool TwoPlayersPerController = false;

        Dictionary<int, int> groupToShipMap = new Dictionary<int, int>();

        const float StickLow = -0.7f;
        const float StickHigh = 0.7f;
        PadReader[] readerMethods = new PadReader[]{
            // General actions
            (padState, controllerNumber) => { return (padState.Buttons.Back == ButtonState.Pressed) ?  new InputAction(){ PlayerID = Globals.NonPlayer, ActionType = InputActionType.Exit} : null;},

            // Menu Actions
            (padState, controllerNumber) => { return (padState.ThumbSticks.Left.Y < StickLow) ?  new InputAction(){ PlayerID = Globals.NonPlayer, ActionType = InputActionType.MenuDown} : null;},
            (padState, controllerNumber) => { return (padState.ThumbSticks.Left.Y > StickHigh) ? new InputAction(){ PlayerID = Globals.NonPlayer, ActionType = InputActionType.MenuUp} : null;},
            (padState, controllerNumber) => { return (padState.ThumbSticks.Left.X < StickLow) ?  new InputAction(){ PlayerID = Globals.NonPlayer, ActionType = InputActionType.MenuLeft} : null;},
            (padState, controllerNumber) => { return (padState.ThumbSticks.Left.X > StickHigh) ? new InputAction(){ PlayerID = Globals.NonPlayer, ActionType = InputActionType.MenuRight} : null;},
            (padState, controllerNumber) => { return (padState.DPad.Down == ButtonState.Pressed) ?  new InputAction(){ PlayerID = Globals.NonPlayer, ActionType = InputActionType.MenuDown} : null;},
            (padState, controllerNumber) => { return (padState.DPad.Up == ButtonState.Pressed) ? new InputAction(){ PlayerID = Globals.NonPlayer, ActionType = InputActionType.MenuUp} : null;},
            (padState, controllerNumber) => { return (padState.DPad.Left == ButtonState.Pressed) ?  new InputAction(){ PlayerID = Globals.NonPlayer, ActionType = InputActionType.MenuLeft} : null;},
            (padState, controllerNumber) => { return (padState.DPad.Right == ButtonState.Pressed) ? new InputAction(){ PlayerID = Globals.NonPlayer, ActionType = InputActionType.MenuRight} : null;},
            (padState, controllerNumber) => { return (padState.Buttons.A == ButtonState.Pressed) ?  new InputAction(){ PlayerID = Globals.NonPlayer, ActionType = InputActionType.MenuSelect} : null;},
            (padState, controllerNumber) => { return (padState.Buttons.Start == ButtonState.Pressed) ?  new InputAction(){ PlayerID = Globals.NonPlayer, ActionType = InputActionType.Advance} : null;},

            // Left Analog
            (padState, controllerNumber) => { return (padState.ThumbSticks.Left.X < StickLow) ?  new InputAction(){ PlayerID = controllerNumber * 100 + 0, ActionType = InputActionType.MoveLeft, AnalogValue = -padState.ThumbSticks.Left.X} : null;},
            (padState, controllerNumber) => { return (padState.ThumbSticks.Left.X > StickHigh) ? new InputAction(){ PlayerID = controllerNumber * 100 + 0, ActionType = InputActionType.MoveRight, AnalogValue = padState.ThumbSticks.Left.X} : null;},
            (padState, controllerNumber) => { return (padState.ThumbSticks.Left.Y < StickLow) ?  new InputAction(){ PlayerID = controllerNumber * 100 + 0, ActionType = InputActionType.MoveDown, AnalogValue = -padState.ThumbSticks.Left.Y} : null;},
            (padState, controllerNumber) => { return (padState.ThumbSticks.Left.Y > StickHigh) ? new InputAction(){ PlayerID = controllerNumber * 100 + 0, ActionType = InputActionType.RotateRight, AnalogValue = padState.ThumbSticks.Left.Y} : null;},
            (padState, controllerNumber) => { return (padState.Buttons.LeftStick == ButtonState.Pressed) ? new InputAction(){ PlayerID = controllerNumber * 100 + 0, ActionType = InputActionType.DropAndStick, AnalogValue = 1} : null;},
            (padState, controllerNumber) => { return (padState.Triggers.Left > StickHigh ) ? new InputAction(){ PlayerID = controllerNumber * 100 + 0, ActionType = InputActionType.DropAndStick, AnalogValue = padState.Triggers.Left} : null;},

            // Left Pad
            (padState, controllerNumber) => { return (padState.DPad.Left == ButtonState.Pressed) ?  new InputAction(){ PlayerID = controllerNumber * 100 + 0, ActionType = InputActionType.RotateLeft, AnalogValue = 1} : null;},
            (padState, controllerNumber) => { return (padState.DPad.Right == ButtonState.Pressed) ? new InputAction(){ PlayerID = controllerNumber * 100 + 0, ActionType = InputActionType.ChangeVictim, AnalogValue = 1} : null;},
            (padState, controllerNumber) => { return (padState.DPad.Down == ButtonState.Pressed) ?  new InputAction(){ PlayerID = controllerNumber * 100 + 0, ActionType = InputActionType.DropAndSlide, AnalogValue = 1} : null;},
            (padState, controllerNumber) => { return (padState.DPad.Up == ButtonState.Pressed) ? new InputAction(){ PlayerID = controllerNumber * 100 + 0, ActionType = InputActionType.DropAndStick, AnalogValue = 1} : null;},
            (padState, controllerNumber) => { return (padState.Buttons.LeftShoulder == ButtonState.Pressed ) ? new InputAction(){ PlayerID = controllerNumber * 100 + 0, ActionType = InputActionType.ApplyAntidote, AnalogValue = 1} : null;},

            // Right Analog
            (padState, controllerNumber) => { return (padState.ThumbSticks.Right.X < StickLow) ?  new InputAction(){ PlayerID = controllerNumber * 100 + (TwoPlayersPerController ? 1 : 0), ActionType = InputActionType.MoveLeft, AnalogValue = -padState.ThumbSticks.Right.X} : null;},
            (padState, controllerNumber) => { return (padState.ThumbSticks.Right.X > StickHigh) ? new InputAction(){ PlayerID = controllerNumber * 100 + (TwoPlayersPerController ? 1 : 0), ActionType = InputActionType.MoveRight, AnalogValue = padState.ThumbSticks.Right.X} : null;},
            (padState, controllerNumber) => { return (padState.ThumbSticks.Right.Y < StickLow) ?  new InputAction(){ PlayerID = controllerNumber * 100 + (TwoPlayersPerController ? 1 : 0), ActionType = InputActionType.MoveDown, AnalogValue = -padState.ThumbSticks.Right.Y} : null;},
            (padState, controllerNumber) => { return (padState.ThumbSticks.Right.Y > StickHigh) ? new InputAction(){ PlayerID = controllerNumber * 100 + (TwoPlayersPerController ? 1 : 0), ActionType = InputActionType.RotateRight, AnalogValue = padState.ThumbSticks.Right.Y} : null;},
            (padState, controllerNumber) => { return (padState.Buttons.RightStick == ButtonState.Pressed) ? new InputAction(){ PlayerID = controllerNumber * 100 + (TwoPlayersPerController ? 1 : 0), ActionType = InputActionType.DropAndStick, AnalogValue = 1} : null;},
            (padState, controllerNumber) => { return (padState.Triggers.Right > StickHigh ) ? new InputAction(){ PlayerID = controllerNumber * 100 + (TwoPlayersPerController ? 1 : 0), ActionType = InputActionType.DropAndStick, AnalogValue = padState.Triggers.Right} : null;},

            // Right BUttons
            (padState, controllerNumber) => { return (padState.Buttons.X == ButtonState.Pressed) ?  new InputAction(){ PlayerID = controllerNumber * 100 + (TwoPlayersPerController ? 1 : 0), ActionType = InputActionType.RotateLeft, AnalogValue = 1 } : null;},
            (padState, controllerNumber) => { return (padState.Buttons.B == ButtonState.Pressed) ? new InputAction(){ PlayerID = controllerNumber * 100 + (TwoPlayersPerController ? 1 : 0), ActionType = InputActionType.ChangeVictim, AnalogValue = 1} : null;},
            (padState, controllerNumber) => { return (padState.Buttons.A == ButtonState.Pressed) ?  new InputAction(){ PlayerID = controllerNumber * 100 + (TwoPlayersPerController ? 1 : 0), ActionType = InputActionType.DropAndSlide, AnalogValue = 1} : null;},
            (padState, controllerNumber) => { return (padState.Buttons.Y == ButtonState.Pressed) ? new InputAction(){ PlayerID = controllerNumber * 100 + (TwoPlayersPerController ? 1 : 0), ActionType = InputActionType.DropAndStick, AnalogValue = 1} : null;},
            (padState, controllerNumber) => { return (padState.Buttons.RightShoulder == ButtonState.Pressed ) ? new InputAction(){ PlayerID = controllerNumber * 100 + (TwoPlayersPerController ? 1 : 0), ActionType = InputActionType.ApplyAntidote, AnalogValue = 1} : null;},
        };

        public GamePadInput()
        {
            Reset();
        }

        /// ------------------------------------------------------------------------
        /// <summary>
        /// Update input from the controllers
        /// </summary>
        /// ------------------------------------------------------------------------
        public override void UpdateInput()
        {
            UpdateInputFromController(PlayerIndex.One);
            UpdateInputFromController(PlayerIndex.Two);
            UpdateInputFromController(PlayerIndex.Three);
            UpdateInputFromController(PlayerIndex.Four);
            DoneWithFrame();
        }

        // Keep track of wether we should try to read a controller or not
        bool[] controllerUsable = new bool[] { false, false, false, false, false, false, false, false, false, };
        bool[] controllerGotAHighStick = new bool[] { false, false, false, false, false, false, false, false, false, };
        bool[] controllerGotALowStick = new bool[] { false, false, false, false, false, false, false, false, false, };




        /// ------------------------------------------------------------------------
        /// <summary>
        /// Update input from a single controller
        /// </summary>
        /// ------------------------------------------------------------------------
        private void UpdateInputFromController(PlayerIndex playerIndex)
        {
            GamePadState padState = GamePad.GetState(playerIndex);
            if (!padState.IsConnected) return;

            // for guitar controllers, we don't want to try to use it because the
            // wammy bar has a high value at rest.  Instead, we'll look for presses
            // or movement
            if (!controllerUsable[(int)playerIndex])
            {
                if (padState.DPad.Down == ButtonState.Pressed ||
                    padState.DPad.Up == ButtonState.Pressed ||
                    padState.DPad.Left == ButtonState.Pressed ||
                    padState.DPad.Right == ButtonState.Pressed ||
                    padState.Buttons.LeftShoulder == ButtonState.Pressed ||
                    padState.Buttons.RightShoulder == ButtonState.Pressed ||
                    padState.Buttons.X == ButtonState.Pressed ||
                    padState.Buttons.Y == ButtonState.Pressed ||
                    padState.Buttons.A == ButtonState.Pressed ||
                    padState.Buttons.B == ButtonState.Pressed ||
                    padState.Buttons.Start == ButtonState.Pressed ||
                    (controllerGotAHighStick[(int)playerIndex] && controllerGotALowStick[(int)playerIndex]))
                {
                    controllerUsable[(int)playerIndex] = true;
                }

                if (Math.Abs(padState.ThumbSticks.Left.Y) > 0.6) controllerGotAHighStick[(int)playerIndex] = true;
                if (Math.Abs(padState.ThumbSticks.Left.Y) < 0.2) controllerGotALowStick[(int)playerIndex] = true;

                return;
            }

            foreach (PadReader readerMethod in readerMethods)
            {
                InputAction newAction = readerMethod(padState, (int)playerIndex);
                if (newAction != null)
                {
                    newAction.ControllerID = (int)playerIndex;
                    AddAction(newAction);
                }
            }


        }

        /// ------------------------------------------------------------------------
        /// <summary>
        /// Reset any mappings
        /// </summary>
        /// ------------------------------------------------------------------------
        public override void ReevaluateMappings()
        {
            return; // don't need to reset mappings stress input
        }

        public override void ReadFromDisk()
        {
        }

        public override void SaveToDisk()
        {
        }

    }
}
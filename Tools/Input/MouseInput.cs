using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Eitrix
{
    public class MouseInput : CommonInput
    {
        MouseState lastState;
        MouseState currentState;
        int StickLow = -1;
        int StickHigh = 1;
        public MouseInput()
        {
            Reset();
        }

        public override void UpdateInput()
        {
            lastState = currentState;
            currentState = Mouse.GetState();
            int XDelta = currentState.X - lastState.X;
            int YDelta = currentState.Y - lastState.Y;
            int scrollWheelDelta = currentState.ScrollWheelValue - lastState.ScrollWheelValue;

            if (XDelta < StickLow)
                AddAction(new InputAction() { PlayerID = 5000, ActionType = InputActionType.MoveLeft, AnalogValue = 1 });
            else if (XDelta > StickHigh)
                AddAction(new InputAction() { PlayerID = 5000, ActionType = InputActionType.MoveRight, AnalogValue = 1});
            if (scrollWheelDelta < 0)
            {
                AddAction(new InputAction() { PlayerID = 5000, ActionType = InputActionType.MenuDown });
                AddAction(new InputAction() { PlayerID = 5000, ActionType = InputActionType.MoveDown, AnalogValue = 1 });
            }
            else if (scrollWheelDelta > 0)
            {
                AddAction(new InputAction() { PlayerID = 5000, ActionType = InputActionType.MenuUp });
                AddAction(new InputAction() { PlayerID = 5000, ActionType = InputActionType.RotateRight, AnalogValue = 1 });
            }
            if (currentState.LeftButton == ButtonState.Pressed)
            {
                AddAction(new InputAction() { PlayerID = 5000, ActionType = InputActionType.MenuSelect });
                AddAction(new InputAction() { PlayerID = 5000, ActionType = InputActionType.DropAndStick, AnalogValue = 1 });
            }
            if (currentState.RightButton == ButtonState.Pressed)
            {
                AddAction(new InputAction() { PlayerID = 5000, ActionType = InputActionType.ApplyAntidote });
            }
            if (currentState.XButton1 == ButtonState.Pressed)
                AddAction(new InputAction() { PlayerID = 5000, ActionType = InputActionType.DropAndStick, AnalogValue = 1 });
            if (currentState.XButton2 == ButtonState.Pressed)
                AddAction(new InputAction() { PlayerID = 5000, ActionType = InputActionType.ChangeVictim, AnalogValue = 1 });
            DoneWithFrame();
        }

        public override void Reset()
        {
            lastState = currentState = Mouse.GetState();
        }

        public override void ReevaluateMappings()
        {
        }

        public override void ReadFromDisk()
        {
        }

        public override void SaveToDisk()
        {
        }
    }
}

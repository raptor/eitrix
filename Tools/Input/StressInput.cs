using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace Eitrix
{
    public class StressInput : CommonInput
    {
        List<MockAction> actions = new List<MockAction>();

        /// ---------------------------------------------------------------------------------
        /// <summary>
        ///  Helper class for mocking actions fromt the user
        /// </summary>
        /// ---------------------------------------------------------------------------------
        class MockAction : InputAction
        {
            public DateTime StopTime;
                
            public MockAction()
            {
                PickNext();
            }

            public void PickNext()
            {
                this.TimeStarted = DateTime.Now.AddSeconds(Globals.rand.NextDouble() * 3);
                this.StopTime = TimeStarted.AddSeconds(Globals.rand.NextDouble() * 2 + .1);
            }


        }

        /// ---------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor
        /// </summary>
        /// ---------------------------------------------------------------------------------
        public StressInput()
        {
            for (int i = 0; i < Globals.MaxPlayers; i++)
            {
                for (int j = 0; j < (int)InputActionType.Exit; j++)
                {
                    actions.Add(new MockAction() { ControllerID = i / 4, PlayerID = i, ActionType = (InputActionType)j });
                }
            }
        }

        public override void Reset()
        {
            return; // don't need to reset stress input
        }

        public override void ReevaluateMappings()
        {
            return; // don't need to reset mappings stress input
        }
        /// ---------------------------------------------------------------------------------
        /// <summary>
        ///  Refresh input state
        /// </summary>
        /// ---------------------------------------------------------------------------------
        public override void UpdateInput()
        {
            foreach (MockAction action in actions)
            {
                if (DateTime.Now > action.StopTime) action.PickNext();
                else if (DateTime.Now > action.TimeStarted)
                {
                    AddAction((InputAction)action);
                }
            }

            DoneWithFrame();
        }

        public override void ReadFromDisk()
        {
        }

        public override void SaveToDisk()
        {
        }


    }
}

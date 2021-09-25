using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eitrix
{
    public class MultiInput : IInputTool
    {
        List<IInputTool> inputTools = new List<IInputTool>();

        public MultiInput()
        {
            inputTools.Add(new KeyboardInput());
            inputTools.Add(new GamePadInput());
            //inputTools.Add(new MouseInput());
        }

        public void Reset()
        {
            foreach (IInputTool inputTool in inputTools)
            {
                inputTool.Reset();
            }
        }

        public void ReevaluateMappings()
        {
            foreach (IInputTool inputTool in inputTools)
            {
                inputTool.ReevaluateMappings();
            }
        }

        public ScreenType ScreenBeingControlled
        {
            get { return inputTools[0].ScreenBeingControlled; }
            set { 
                foreach (IInputTool tool in inputTools)
                    tool.ScreenBeingControlled = value;
            }
        }

        public IEnumerable<InputAction> ActionQueue
        {
            get
            {
                foreach (IInputTool tool in inputTools)
                {
                    foreach (InputAction action in tool.ActionQueue)
                    {
                        yield return action;
                    }
                }
            }
                
        }

        public void UpdateInput()
        {
            foreach (IInputTool tool in inputTools)
            {
                tool.UpdateInput();
            }
        }

        public void SaveToDisk()
        {
            foreach (IInputTool tool in inputTools)
            {
                tool.SaveToDisk();
            }
        }

        public void ReadFromDisk()
        {
            foreach (IInputTool tool in inputTools)
            {
                tool.ReadFromDisk();
            }
        }
    }
}

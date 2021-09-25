using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eitrix
{
    public interface IInputTool
    {
        IEnumerable<InputAction> ActionQueue { get; }
        ScreenType ScreenBeingControlled { get; set; }
        void UpdateInput();
        void Reset();
        void ReevaluateMappings();
        void SaveToDisk();
        void ReadFromDisk();
    }
}

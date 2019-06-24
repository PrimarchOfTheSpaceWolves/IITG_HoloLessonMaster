using System;
using UnityEngine;
using HoloToolkit.Unity.InputModule.Utilities.Interactions;

namespace HoloToolkit.Unity.InputModule.Tests
{
    /// <summary>
    /// This class implements IInputClickHandler to handle the tap gesture.
    /// </summary>
    public class TextTapActionResponder : TapActionResponder
    {
        private LessonTextDisplay textDisplay = null;
        
        public void setTextDisplay(LessonTextDisplay display)
        {
            textDisplay = display;
        }

        public override void performSpecialAction()
        {
            Debug.Log("Next page...");
            textDisplay.nextPage();
        }
    }
}




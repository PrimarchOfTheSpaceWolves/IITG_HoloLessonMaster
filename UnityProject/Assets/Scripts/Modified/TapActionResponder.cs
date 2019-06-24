// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

//  Modified

using System;
using UnityEngine;
using HoloToolkit.Unity.InputModule.Utilities.Interactions;

namespace HoloToolkit.Unity.InputModule.Tests
{
    /// <summary>
    /// This class implements IInputClickHandler to handle the tap gesture.
    /// </summary>
    public class TapActionResponder : MonoBehaviour, IInputClickHandler
    {
        private LessonManager lessonManager = null;
        private string lessonObjRef = "";

        public void OnInputClicked(InputClickedEventData eventData)
        {
            lessonManager.NotifyClicked(lessonObjRef);
            eventData.Use(); // Mark the event as used, so it doesn't fall through to other handlers.
        }

        public void setLessonManagerData(LessonManager manager, string objRef)
        {
            lessonManager = manager;
            lessonObjRef = objRef;
        }

        public virtual void performSpecialAction()
        {
            Debug.Log("Default special action!");
        }
    }
}

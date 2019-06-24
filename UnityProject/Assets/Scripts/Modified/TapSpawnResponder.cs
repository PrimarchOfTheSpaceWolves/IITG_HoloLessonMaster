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
    /// It spawns a new object of the same kind (but larger).
    /// </summary>
    public class TapSpawnResponder : MonoBehaviour, IInputClickHandler
    {
        private LessonManager lessonManager = null;
        private string lessonObjRef = "";

        public void OnInputClicked(InputClickedEventData eventData)
        {            
            Vector3 offset = new Vector3(0.1f, 0.1f, -0.1f);
            Vector3 spawnPosition = gameObject.transform.position + offset;
            Quaternion spawnRotation = gameObject.transform.rotation;
            Vector3 spawnScale = 1.3f * gameObject.transform.localScale;
            
            // Notify LessonManager to create a new object
            lessonManager.AddLessonObjectRef(   lessonObjRef, spawnPosition,
                                                spawnRotation, spawnScale);
            
            eventData.Use(); // Mark the event as used, so it doesn't fall through to other handlers.
        }

        public void setLessonManagerData(LessonManager manager, string objRef)
        {
            lessonManager = manager;
            lessonObjRef = objRef;
        }
    }
}
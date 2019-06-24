using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusDisplayText : MonoBehaviour {

    public LessonManager lessonManager = null;
    private TextMesh textDisplay = null;

	// Use this for initialization
	void Start () {
        textDisplay = GetComponent<TextMesh>();
	}
	
	// Update is called once per frame
	void Update () {
		if(lessonManager != null)
        {
            textDisplay.text = "";
            if(lessonManager.isInTeacherMode())
            {
                textDisplay.text += "TEACHER MODE\n";
            }
            else
            {
                textDisplay.text += "STUDENT MODE\n";
            }
        }
	}
}

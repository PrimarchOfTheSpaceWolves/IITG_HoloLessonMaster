using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;

public class LessonDesc {
    public string textDesc = "";

    public LessonDesc() {}

    public LessonDesc(string text)
    {
        textDesc = text;
    }

    public LessonDesc Clone()
    {
        LessonDesc other = new LessonDesc();
        other.textDesc = textDesc;
        return other;
    }

    public override string ToString()
    {
        return textDesc;
    }

    public void SetTextDesc(string text)
    {
        textDesc = text;
    }

    public string GetTextDesc()
    {
        return textDesc;
    }
}

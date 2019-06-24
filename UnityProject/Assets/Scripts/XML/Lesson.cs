using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

public class Lesson {

    [XmlAttribute("ID")]
    public string ID = "";

    public LessonDesc description = new LessonDesc();

    [XmlArray("LessonSceneList")]
    [XmlArrayItem("LessonScene")]
    public List<LessonScene> LessonSceneList = new List<LessonScene>();

    public Lesson() { }

    public Lesson(string ID)
    {
        this.ID = ID;
    }

    public void SetID(string id)
    {
        ID = id;
    }

    public string GetID()
    {
        return ID;
    }


    public void AddLessonScene(LessonScene scene)
    {
        LessonSceneList.Add(scene);
    }
    
    public LessonScene FindLessonScene(string ID)
    {
        LessonScene scene = null;
        foreach (LessonScene current in LessonSceneList)
        {
            if (current.ID.Equals(ID))
            {
                scene = current;
                break;
            }
        }

        return scene;
    }

    public LessonScene GetScene(int index)
    {
        LessonScene scene = null;
        if (index >= 0 && index < LessonSceneList.Count)
        {
            scene = LessonSceneList[index];
        }
        return scene;
    }


    public void SetDescription(LessonDesc desc)
    {
        description = desc;
    }

    public LessonDesc GetDescription()
    {
        return description;
    }

    public override string ToString()
    {
        string output = "LESSON: " + ID + "\n";
        output += "*******************" + "\n";
        foreach (LessonScene current in LessonSceneList)
        {
            output += current.ToString();
            output += "\n";
        }

        return output;
    }
}

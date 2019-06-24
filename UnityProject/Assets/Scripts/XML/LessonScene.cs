using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;

public class LessonScene {
    [XmlAttribute("ID")]
    public string ID = "";

    public LessonDesc description = new LessonDesc();

    [XmlArray("LessonObjRefList")]
    [XmlArrayItem("LessonObjRef")]
    public List<LessonObjRef> LessonObjRefList = new List<LessonObjRef>();

    public LessonScene() { }

    public LessonScene(string ID)
    {
        this.ID = ID;
    }

    public override string ToString()
    {
        string output = "LESSON_SCENE: " + ID + "\n";
        output += "\t" + "DESC: " + description.ToString() + "\n";
        foreach (LessonObjRef current in LessonObjRefList)
        {
            output += current;
            output += "\n";
        }

        return output;
    }

    public void SetID(string id)
    {
        ID = id;
    }

    public string GetID()
    {
        return ID;
    }

    public void SetDescription(LessonDesc desc)
    {
        description = desc;
    }

    public LessonDesc GetDescription()
    {
        return description;
    }

    public void AddLessonObjRef(LessonObjRef obj)
    {
        LessonObjRefList.Add(obj);
    }
    
    public LessonObjRef FindLessonObjRef(string ID)
    {
        LessonObjRef obj = null;
        foreach (LessonObjRef current in LessonObjRefList)
        {
            if (current.ID.Equals(ID))
            {
                obj = current;
                break;
            }
        }

        return obj;
    }

    public List<LessonObjRef> GetLessonObjRefList()
    {
        return LessonObjRefList;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;

public class LessonObject {

    [XmlAttribute("ID")]
    public string ID = "";

    [XmlAttribute("dataType")]
    public string dataType = "";

    public LessonDesc description = new LessonDesc();

    [XmlArray("Resources")]
    [XmlArrayItem("Resource")]
    public List<LessonObjResource> resourceList = new List<LessonObjResource>();

    public LessonObject() { }

    public LessonObject(string ID, string dataType)
    {
        this.ID = ID;
        this.dataType = dataType;
    }

    public LessonObject Clone()
    {
        LessonObject obj = new LessonObject();
        obj.ID = ID;
        obj.dataType = dataType;
        obj.description = description.Clone();
        foreach(LessonObjResource res in resourceList)
        {
            obj.resourceList.Add(res.Clone());
        }
        return obj;
    }

    public override string ToString()
    {
        string output = "LESSON_OBJECT: " + ID + "\n";        
        output += "\t" + "DESC: " + description;
        foreach (LessonObjResource resource in resourceList)
        {
            output += "\t" + "RESOURCE: " + resource + "\n";
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

    public void SetDataType(string s)
    {
        dataType = s;
    }

    public string GetDataType()
    {
        return dataType;
    }

    public void SetDescription(LessonDesc desc)
    {
        description = desc;
    }

    public LessonDesc GetDescription()
    {
        return description;
    }

    public void AddLessonObjResource(LessonObjResource obj)
    {
        resourceList.Add(obj);
    }

    public List<LessonObjResource> GetLessonObjResourceList()
    {
        return resourceList;
    }

    public LessonObjResource FindLessonObjResource(string ID)
    {
        LessonObjResource obj = null;
        foreach (LessonObjResource current in resourceList)
        {
            if (current.ID.Equals(ID))
            {
                obj = current;
                break;
            }
        }

        return obj;
    }
}

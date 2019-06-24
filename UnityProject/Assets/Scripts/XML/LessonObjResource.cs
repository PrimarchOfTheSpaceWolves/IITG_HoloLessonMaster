using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;

public class LessonObjResource {

    [XmlAttribute("ID")]
    public string ID = "";

    [XmlAttribute("dataType")]
    public string dataType = "";

    [XmlAttribute("path")]
    public string path = "";

    public LessonObjResource() { }

    public LessonObjResource(string ID, string dataType, string path)
    {
        this.ID = ID;
        this.dataType = dataType;
        this.path = path;
    }

    public LessonObjResource Clone()
    {
        return new LessonObjResource(ID, dataType, path);
    }

    public override string ToString()
    {
        string output = ID + ", " + dataType + ": " + path;
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

    public void SetPath(string s)
    {
        path = s;
    }

    public string GetPath()
    {
        return path;
    }
}

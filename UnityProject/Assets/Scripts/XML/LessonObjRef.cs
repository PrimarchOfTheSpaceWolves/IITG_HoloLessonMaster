using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;

public class LessonObjRef {

    [XmlAttribute("ID")]
    public string ID = "";

    [XmlAttribute("parent")]
    public string parent = "";

    [XmlArray("TapActions")]
    [XmlArrayItem("TapAction")]
    public List<LessonTapAction> tapList = new List<LessonTapAction>();
    
    public string refObjID = "";
    public LessonDesc description = null;
    public Vector3 translate = new Vector3(0, 0, 0);
    public Quaternion rotate;
    public Vector3 scale = new Vector3(1, 1, 1);
    public bool startEnabled = true;

    public LessonObjRef() { }

    public LessonObjRef(string ID, string refObjID)
    {
        this.ID = ID;
        this.refObjID = refObjID;
    }

    public override string ToString()
    {
        string output = "\t\t" + "LESSON_OBJECT_REF: " + ID + "\n";        
        output += "\t\t\t" + "DESC: " + description + "\n";
        output += "\t\t\t" + "REF ID: " + refObjID + "\n";
        output += "\t\t\t" + "TRANSLATE: " + translate + "\n";
        output += "\t\t\t" + "ROTATE: " + rotate + "\n";
        output += "\t\t\t" + "SCALE: " + scale + "\n";
        foreach (LessonTapAction action in tapList)
        {
            output += "\t\t\t\t" + "ACTION: " + action + "\n";
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

    public void SetRefObjID(string otherID)
    {
        refObjID = otherID;
    }

    public string GetRefObjID()
    {
        return refObjID;
    }

    public void SetTranslate(Vector3 t)
    {
        translate = t;
    }

    public Vector3 GetTranslate()
    {
        return translate;
    }

    public void SetRotation(Quaternion r)
    {
        rotate = r;
    }

    public Quaternion GetRotation()
    {
        return rotate;
    }

    public void SetScaling(Vector3 s)
    {
        scale = s;
    }

    public Vector3 GetScaling()
    {
        return scale;
    }

    public void SetStartEnabled(bool b)
    {
        startEnabled = b;
    }

    public bool GetStartEnabled()
    {
        return startEnabled;
    }

    public void AddLessonTapAction(LessonTapAction obj)
    {
        tapList.Add(obj);
    }

    public List<LessonTapAction> GetLessonTapActionList()
    {
        return tapList;
    }

    public LessonTapAction FindLessonTapAction(string ID)
    {
        LessonTapAction obj = null;
        foreach (LessonTapAction current in tapList)
        {
            if (current.refObjID.Equals(ID))
            {
                obj = current;
                break;
            }
        }

        return obj;
    }

    public void SetParent(string other)
    {
        parent = other;
    }

    public string GetParent()
    {
        return parent;
    }

    public bool HasParent()
    {
        return (!parent.Equals(""));
    }
}

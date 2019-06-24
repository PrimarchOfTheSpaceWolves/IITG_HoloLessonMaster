using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;

public class LessonTapAction {
    
    [XmlAttribute("refObjID")]
    public string refObjID = "";

    public LessonTapAction() { }

    public LessonTapAction(string refObjID)
    {        
        this.refObjID = refObjID;
    }

    public override string ToString()
    {
        string output = refObjID;
        return output;
    }

    public void SetRefObjID(string id)
    {
        refObjID = id;
    }

    public string GetRefObjID()
    {
        return refObjID;
    }
}

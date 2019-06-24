using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

// Some code taken from: http://wiki.unity3d.com/index.php?title=Saving_and_Loading_Data:_XmlSerializer 

[XmlRoot("LessonResources")]
public class LessonResources {

    [XmlArray("LessonObjList")]
    [XmlArrayItem("LessonObject")]
    public List<LessonObject> LessonObjList = new List<LessonObject>();
        
    public void AddLessonObject(LessonObject obj)
    {
        LessonObjList.Add(obj);        
    }

    public void AddLessonResources(LessonResources resources)
    {
        foreach(LessonObject obj in resources.GetLessonObjList())
        {
            LessonObjList.Add(obj);
        }
    }

    public void Reset()
    {
        LessonObjList.Clear();
    }

    public List<LessonObject> GetLessonObjList()
    {
        return LessonObjList;
    }

    public LessonObject FindLessonObject(string ID)
    {
        LessonObject obj = null;
        foreach(LessonObject current in LessonObjList)
        {
            if(current.ID.Equals(ID))
            {
                obj = current;
                break;
            }
        }

        return obj;
    }

    public override string ToString()
    {
        string output = "LESSON_RESOURCES" + "\n";
        output += "*******************" + "\n";
        foreach (LessonObject current in LessonObjList)
        {
            output += current;
            output += "\n";
        }

        return output;            
    }
    
    public void Save(string path) { 
        var serializer = new XmlSerializer(typeof(LessonResources));
        using (var stream = new FileStream(path, FileMode.Create)) { 
            serializer.Serialize(stream, this);
        }
    }

#if (WINDOWS_UWP)
    public async void SaveFile(Windows.Storage.StorageFile file)
    {
        var serializer = new XmlSerializer(typeof(LessonResources));
        using (var stream = await file.OpenStreamForWriteAsync())
        {
            serializer.Serialize(stream, this);
        }                
    }
#endif

    public static LessonResources Load(string path) {
        var serializer = new XmlSerializer(typeof(LessonResources));
        using (var stream = new FileStream(path, FileMode.Open)) {
            return serializer.Deserialize(stream) as LessonResources;
        }
    }
    
    public static LessonResources LoadFromText(string text) {
        var serializer = new XmlSerializer(typeof(LessonResources));
        return serializer.Deserialize(new StringReader(text)) as LessonResources;
    }
}



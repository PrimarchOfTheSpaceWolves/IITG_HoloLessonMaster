using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

[XmlRoot("LessonList")]
public class LessonList {

    [XmlArray("AllLessions")]
    [XmlArrayItem("Lesson")]
    public List<Lesson> AllLessions = new List<Lesson>();

    public LessonResources resources = new LessonResources();

    public void AddLesson(Lesson lesson)
    {
        AllLessions.Add(lesson);
    }

    public Lesson GetLesson(int index)
    {
        Lesson lesson = null;
        if (index >= 0 && index < AllLessions.Count) {
            lesson = AllLessions[index];
        }
        return lesson;
    }

    public void Reset()
    {
        AllLessions.Clear();
    }

    public LessonResources GetResources()
    {
        return resources;
    }

    public void SetResources(LessonResources r)
    {
        resources = r;
    }

    public Lesson FindLesson(string ID)
    {
        Lesson lesson = null;
        foreach (Lesson current in AllLessions)
        {
            if (current.ID.Equals(ID))
            {
                lesson = current;
                break;
            }
        }

        return lesson;
    }

    public override string ToString()
    {
        string output = "LESSON_LIST" + "\n";
        output += "%%%%%%%%%%%%%%%%%%%%%%%%%%%" + "\n";
        foreach (Lesson current in AllLessions)
        {
            output += current;
            output += "\n";
        }

        return output;
    }

    public void Save(string path)
    {
        var serializer = new XmlSerializer(typeof(LessonList));
        using (var stream = new FileStream(path, FileMode.Create))
        {
            serializer.Serialize(stream, this);
        }
    }

#if (WINDOWS_UWP)
    public async void SaveFile(Windows.Storage.StorageFile file)
    {
        var serializer = new XmlSerializer(typeof(LessonList));
        using (var stream = await file.OpenStreamForWriteAsync())
        {
            serializer.Serialize(stream, this);
        }
    }
#endif

    public static LessonList Load(string path)
    {
        var serializer = new XmlSerializer(typeof(LessonList));
        using (var stream = new FileStream(path, FileMode.Open))
        {
            return serializer.Deserialize(stream) as LessonList;
        }
    }

    public static LessonList LoadFromText(string text)
    {
        var serializer = new XmlSerializer(typeof(LessonList));
        return serializer.Deserialize(new StringReader(text)) as LessonList;
    }
}

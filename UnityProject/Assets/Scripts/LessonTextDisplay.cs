using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LessonTextDisplay : MonoBehaviour {

    [SerializeField]
    private List<string> allTextData = new List<string>();

    private TextMesh textMesh = null;
    private const int MAX_LENGTH = 30;
    private int pageIndex = 0;

    public void setTextData(List<string> textData)
    {
        allTextData.Clear();
        allTextData.AddRange(textData);
        pageIndex = 0;
    }

    public void setTextMesh(TextMesh mesh)
    {
        textMesh = mesh;        
    }

    public bool changeText(int index)
    {
        bool success = false;
        if(index >= 0 && index < allTextData.Count)
        {
            textMesh.text = wrapText(allTextData[index]);
            pageIndex = index;
            success = true;
        }
        Debug.Log("Current page: " + (pageIndex+1) + " of " + allTextData.Count);
        return success;
    }

    public bool nextPage()
    {
        bool goodPage = true;
        pageIndex++;
        if(pageIndex >= allTextData.Count)
        {
            pageIndex = 0;
            goodPage = false;
        }
        changeText(pageIndex);
        return goodPage;
    }

    private static string wrapText(string s)
    {
        string output = "";
        while (s.Length > MAX_LENGTH)
        {
            // Find space nearest end
            string subline = s.Substring(0, MAX_LENGTH);
            int lastSpace = subline.LastIndexOf(" ");
            int cutOff = lastSpace + 1;
            if (lastSpace == -1)
            {
                lastSpace = MAX_LENGTH;
                cutOff = MAX_LENGTH;
            }
            
            subline = s.Substring(0, lastSpace);
            output += subline + "\n";
            s = s.Substring(cutOff);
        }
        output += s + "\n";
        return output;
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

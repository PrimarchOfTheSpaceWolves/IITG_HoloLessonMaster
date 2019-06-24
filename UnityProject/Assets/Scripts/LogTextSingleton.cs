using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogTextSingleton : MonoBehaviour {
        
    private TextMesh textObject = null;
    private List<string> allLines = new List<string>();
    private const int MAX_LINES = 7;
    private const int MAX_LENGTH = 50;

    // Use this for initialization
    void Start () {        
        textObject = gameObject.GetComponent<TextMesh>();
        allLines.Add("Welcome!");
    }
	
	// Update is called once per frame
	void Update () {

        string output = "";
        for(int i = 0; i < MAX_LINES; i++)
        {
            int index = allLines.Count - 1 - i;
            if(index >= 0)
            {
                string line = allLines[index];                
                output = line + "\n" + output;
            }
        }
        textObject.text = output;

    }
    
    public void println(string text)
    {       
        print(text);        
        while(text.Length > MAX_LENGTH)
        {
            string subline = text.Substring(0, MAX_LENGTH);
            allLines.Add(subline);
            text = text.Substring(MAX_LENGTH);
        }
        allLines.Add(text);        
    }
}

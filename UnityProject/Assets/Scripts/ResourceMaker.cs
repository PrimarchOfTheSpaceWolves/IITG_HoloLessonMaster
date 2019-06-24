using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.Networking;

public class ResourceMaker {
    
    public static GameObject Create3DModelFromString(string fileData, Texture2D tex) //byte [] textureData)
    {        
        // Create Mesh from OBJ data
        Mesh mesh = loadFromOBJ(fileData);
        
        GameObject gameObject = new GameObject();

        // Create MeshRenderer and MeshFilter
        MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
        MeshFilter filter = gameObject.AddComponent<MeshFilter>();

        // Set mesh
        filter.mesh = mesh;

        // Create collider
        Bounds bounds = mesh.bounds;
        BoxCollider collider = gameObject.AddComponent<BoxCollider>();
        collider.center = gameObject.transform.position;
        collider.size = bounds.size;

        // Create texture
        //Texture2D tex = new Texture2D(2, 2);
        //tex.LoadImage(textureData);

        // Add texture to model
        renderer.materials[0].mainTexture = tex;
        
        return gameObject;
    }

    private class Triplet
    {
        public int a;
        public int b;
        public int c;

        public Triplet() { }

        public Triplet(int a, int b, int c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }

        public Triplet(string s)
        {
            string[] tokens = s.Split('_');
            a = Int32.Parse(tokens[0]);
            b = Int32.Parse(tokens[1]);
            c = Int32.Parse(tokens[2]);
        }

        public bool Equals(Triplet other)
        {
            return (a == other.a && b == other.b && c == other.c);
        }

        public override string ToString()
        {
            return a + "_" + b + "_" + c;
        }
    }

    private static Mesh loadFromOBJ(string fileData)
    {        
        // Set up lists
        List<Vector3> vertList = new List<Vector3>();
        List<Vector2> uvList = new List<Vector2>();
        List<Vector3> normList = new List<Vector3>();
        List<int> triList = new List<int>();
                
        Dictionary<int, int> vertToUVMap = new Dictionary<int, int>();
        Dictionary<int, int> vertToNormMap = new Dictionary<int, int>();

        Dictionary<string, int> fullIndexDict = new Dictionary<string, int>();
        List<string> fullIndexList = new List<string>();
        
        // Read through each line
        using (StringReader sr = new StringReader(fileData))
        {
            string line;
            while((line = sr.ReadLine()) != null)
            {
                
                string[] tokens = line.Split(' ');
               
                if(tokens.Length > 0)
                {
                    switch(tokens[0])
                    {
                        case "v":
                            {
                                // Vertex
                                Vector3 v = new Vector3(Convert.ToSingle(tokens[1]),
                                                        Convert.ToSingle(tokens[2]),
                                                        Convert.ToSingle(tokens[3]));
                                vertList.Add(v);
                            }
                            break;
                        case "f":
                            {
                                // Face
                                // Is it index or index/index/index?
                                string[] subtokens = tokens[1].Split('/');
                                if(subtokens.Length > 1)
                                {
                                    int v1 = Convert.ToInt32(tokens[1].Split('/')[0]) - 1;
                                    int v2 = Convert.ToInt32(tokens[2].Split('/')[0]) - 1;
                                    int v3 = Convert.ToInt32(tokens[3].Split('/')[0]) - 1;

                                    int t1 = v1;
                                    int t2 = v2;
                                    int t3 = v3;

                                    // If there are texture indices specified...
                                    if (tokens[1].Split('/')[1].Length > 0) {
                                        t1 = Convert.ToInt32(tokens[1].Split('/')[1]) - 1;
                                        t2 = Convert.ToInt32(tokens[2].Split('/')[1]) - 1;
                                        t3 = Convert.ToInt32(tokens[3].Split('/')[1]) - 1;
                                    }

                                    int n1 = v1;
                                    int n2 = v2;
                                    int n3 = v3;

                                    // If there are normal indices specified...
                                    if (tokens[1].Split('/')[2].Length > 0)
                                    {
                                        n1 = Convert.ToInt32(tokens[1].Split('/')[2]) - 1;
                                        n2 = Convert.ToInt32(tokens[2].Split('/')[2]) - 1;
                                        n3 = Convert.ToInt32(tokens[3].Split('/')[2]) - 1;
                                    }

                                    // DEBUG: For now, just use the same indices for everyone
                                    triList.Add(v1);
                                    triList.Add(v2);
                                    triList.Add(v3);

                                    // Get mapping from vertex to UV
                                    vertToUVMap[v1] = t1;
                                    vertToUVMap[v2] = t2;
                                    vertToUVMap[v3] = t3;

                                    // Get mapping from vertex to normals
                                    vertToNormMap[v1] = n1;
                                    vertToNormMap[v2] = n2;
                                    vertToNormMap[v3] = n3;

                                    // Add triplets
                                    string trip1 = new Triplet(v1, t1, n1).ToString();
                                    string trip2 = new Triplet(v2, t2, n2).ToString();
                                    string trip3 = new Triplet(v3, t3, n3).ToString();

                                    fullIndexList.Add(trip1);
                                    fullIndexList.Add(trip2);
                                    fullIndexList.Add(trip3);

                                    fullIndexDict[trip1] = 0;
                                    fullIndexDict[trip2] = 0;
                                    fullIndexDict[trip3] = 0;
                                    
                                }
                                else
                                {
                                    int v1 = Convert.ToInt32(tokens[1]) - 1;
                                    int v2 = Convert.ToInt32(tokens[2]) - 1;
                                    int v3 = Convert.ToInt32(tokens[3]) - 1;
                                    
                                    triList.Add(v1);
                                    triList.Add(v2);
                                    triList.Add(v3);

                                    vertToUVMap[v1] = v1;
                                    vertToUVMap[v2] = v2;
                                    vertToUVMap[v3] = v3;

                                    vertToNormMap[v1] = v1;
                                    vertToNormMap[v2] = v2;
                                    vertToNormMap[v3] = v3;

                                    string trip1 = new Triplet(v1, v1, v1).ToString();
                                    string trip2 = new Triplet(v2, v2, v2).ToString();
                                    string trip3 = new Triplet(v3, v3, v3).ToString();

                                    fullIndexList.Add(trip1);
                                    fullIndexList.Add(trip2);
                                    fullIndexList.Add(trip3);

                                    fullIndexDict[trip1] = 0;
                                    fullIndexDict[trip2] = 0;
                                    fullIndexDict[trip3] = 0;
                                }
                            }
                            break;
                        case "vt":
                            {
                                // UV
                                Vector2 uv = new Vector2(Convert.ToSingle(tokens[1]),
                                                        Convert.ToSingle(tokens[2]));
                                // DEBUG: Have to flip in X
                                uv.x = 1.0f - uv.x;
                                uvList.Add(uv);
                            }
                            break;
                        case "vn":
                            {
                                // Normal
                                Vector3 n = new Vector3(Convert.ToSingle(tokens[1]),
                                                        Convert.ToSingle(tokens[2]),
                                                        Convert.ToSingle(tokens[3]));
                                normList.Add(n);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        // Assign indices to each
        Dictionary<int, Triplet> vertMap = new Dictionary<int, Triplet>();
        int index = 0;
        List<string> allKeys = new List<string>(fullIndexDict.Keys);
        foreach(string key in allKeys)
        {
            fullIndexDict[key] = index;
            vertMap[index] = new Triplet(key);
            index++;
        }

        List<Vector3> finalVerts = new List<Vector3>();
        List<Vector2> finalUVs = new List<Vector2>();
        List<Vector3> finalNormals = new List<Vector3>();
        List<int> finalTriangles = new List<int>();

        for (int i = 0; i < index; i++)
        {
            Triplet t = vertMap[i];

            // Add vertex
            finalVerts.Add(vertList[t.a]);

            // Add UV
            if (t.b < uvList.Count)
            {
                finalUVs.Add(uvList[t.b]);
            }

            // Add normal (if present)
            if(t.c < normList.Count)
            {
                finalNormals.Add(normList[t.c]);
            }
        }

        for(int i = 0; i < fullIndexList.Count; i++)
        {
            string key = fullIndexList[i];
            finalTriangles.Add(fullIndexDict[key]);
        }

        /*
        // OLD APPROACH

        // Clean up uv list
        List<Vector2> finalUVs = new List<Vector2>();
        for(int i = 0; i < vertList.Count; i++)
        {
            int uvIndex = i;

            if (vertToUVMap.ContainsKey(i))
            {
                uvIndex = vertToUVMap[i];
            }

            if (uvIndex < uvList.Count)
            {
                finalUVs.Add(uvList[uvIndex]);
            }            
        }

        // Clean up normals
        List<Vector3> finalNormals = new List<Vector3>();
        for (int i = 0; i < vertList.Count; i++)
        {
            int normIndex = i;

            if (vertToNormMap.ContainsKey(i))
            {
                normIndex = vertToNormMap[i];
            }

            if (normIndex < normList.Count)
            {
                finalNormals.Add(normList[normIndex]);
            }
        }
        */

        // Create Mesh
        Mesh mesh = new Mesh();
        mesh.vertices = finalVerts.ToArray();
        mesh.uv = finalUVs.ToArray();
        mesh.triangles = finalTriangles.ToArray();
        if(finalNormals.Count > 0)
        {
            mesh.normals = finalNormals.ToArray();
        }
        else
        {
            Debug.Log("RECALC NORMALS");
            mesh.RecalculateNormals();
        }

        if(finalUVs.Count == 0)
        {
            for(int i = 0; i < finalVerts.Count; i++)
            {
                finalUVs.Add(new Vector2(0, 0));
            }
            mesh.uv = finalUVs.ToArray();
        }
        
        mesh.RecalculateBounds();        
        mesh.RecalculateTangents();

        return mesh;
    }

    public static GameObject CreateTextDisplay(List<string> allTextData)
    {
        GameObject gameObject = new GameObject();
                
        // Create TextMesh
        TextMesh textMesh = gameObject.AddComponent<TextMesh>();
        textMesh.richText = true;
        textMesh.fontSize = 72;
        textMesh.anchor = TextAnchor.MiddleCenter;
        
        // Create LessonTextDisplay object
        LessonTextDisplay textDisplay = gameObject.AddComponent<LessonTextDisplay>();
        textDisplay.setTextData(allTextData);
        textDisplay.setTextMesh(textMesh);
        textDisplay.changeText(0);

        // Add Billboard
        gameObject.AddComponent<HoloToolkit.Unity.Billboard>();
                
        // Create collider
        Bounds bounds = gameObject.GetComponent<MeshRenderer>().bounds;
        BoxCollider collider = gameObject.AddComponent<BoxCollider>();
        collider.center = gameObject.transform.position;
        collider.size = bounds.size;
        
        return gameObject;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule.Utilities.Interactions;
using HoloToolkit.Unity.Boundary;
using HoloToolkit.Unity.InputModule.Tests;
using HoloToolkit.Sharing.Spawning;
using HoloToolkit.Sharing;
using HoloToolkit.Sharing.Tests;
using System;
using System.IO;
using HoloToolkit.Unity;
using UnityEngine.Networking;

public class LessonManager : MonoBehaviour {

    public LogTextSingleton logText = null;

    private bool inTeacherMode = true;

    // Global - contains original paths
    // Local - contains local paths
    private LessonResources allResourcesGlobal = new LessonResources();
    private LessonResources allResourcesLocal = new LessonResources();

    // Lesson information
    private LessonList allLessons;

    private string currentLessonID;
    private string currentSceneID;
    private Lesson currentLesson;
    private LessonScene currentScene;

    // Stores ID to prefab
    private Dictionary<string, GameObject> allResourceData = new Dictionary<string, GameObject>();

    // Stores scales for spawner objects
    private Dictionary<string, Vector3> allSpawnerScales = new Dictionary<string, Vector3>();
       
    // Stores all spawner objects created
    private List<GameObject> allSpawners = new List<GameObject>();
    // Stores all objects EXCEPT spawners
    private Dictionary<string, GameObject> allCreatedObjects = new Dictionary<string, GameObject>();
         
    // Temporary state variables
    private int objectCreationCounter = 0;
    private LessonObjRef objRefToAttach = null;
    private LessonResources allResourcesTmp = null;
    private bool resourcePickingDone = false;
    private bool spawnerSetupDone = false;

    // Use this for initialization
    void Start () {
        // Create a default blank lesson
        allLessons = new LessonList();
        currentLessonID = "DefaultLesson";
        currentSceneID = "DefaultScene";
        currentLesson = new Lesson(currentLessonID);
        currentScene = new LessonScene(currentSceneID);
        currentLesson.AddLessonScene(currentScene);
        allLessons.AddLesson(currentLesson);   
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    // Callback:
    // - open new XML resource file
    // - download data and save it locally
    // - add to total list of resources
    // - recreate spawners
    public void OpenResource()
    {
#if (WINDOWS_UWP)
        spawnerSetupDone = false;
        resourcePickingDone = false;
        allResourcesTmp = null;
        UnityEngine.WSA.Application.InvokeOnUIThread(() => PickXMLResourceFile(), false);
        StartCoroutine(WaitForPicking());
#endif
    }

#if (WINDOWS_UWP)
    // Creates XML open picker
    private Windows.Storage.Pickers.FileOpenPicker makeXMLOpenPicker()
    {
        // Code modified from: https://docs.microsoft.com/en-us/windows/uwp/files/quickstart-using-file-and-folder-pickers
        var picker = new Windows.Storage.Pickers.FileOpenPicker();
        picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
        picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads;
        picker.FileTypeFilter.Add(".xml");
        return picker;
    }

    // Create XML save picker
    private Windows.Storage.Pickers.FileSavePicker makeXMLSavePicker()
    {
        var picker = new Windows.Storage.Pickers.FileSavePicker();
        picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads;
        picker.FileTypeChoices.Add("XML File", new List<string>() { ".xml" });
        picker.DefaultFileExtension = ".xml";        
        return picker;
    }

    // Open picker for getting resource file
    private async void PickXMLResourceFile()
    {
        // Pick XML Resource File
        Windows.Storage.Pickers.FileOpenPicker picker = makeXMLOpenPicker();
        Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();

        if (file != null)
        {
            // Application now has read/write access to the picked file            
            print("Picked file: " + file.Name + " -- " + file.Path);
            logText.println("XML Resource: " + file.Name);

            // Open and load XML resources
            logText.println("Loading XML file...");
            string resoureFileText = await Windows.Storage.FileIO.ReadTextAsync(file);
            allResourcesTmp = LessonResources.LoadFromText(resoureFileText);
            logText.println(allResourcesTmp.ToString());
        }
        else
        {
            print("Operation cancelled.");
        }

        resourcePickingDone = true;
    }

    // Waiting for picking of XML file to be complete...
    IEnumerator WaitForPicking()
    {
        while (!resourcePickingDone)
        {
            yield return null;
        }

        // Picking done, is this valid?
        if (allResourcesTmp != null)
        {
            yield return StartCoroutine(SaveRemoteResourcesToDisk());
        }
    }

    // Getting all remote resources and saving them to disk.
    // Also, updates local/global resource list
    IEnumerator SaveRemoteResourcesToDisk()
    {
        print("ABOUT TO LOAD ALL RESOURCES...");

        string resourceSavePath = Application.persistentDataPath + "/";
        string localResourceFile = resourceSavePath + "LOCAL_Resources.xml";
        string globalResourceFile = resourceSavePath + "GLOBAL_Resources.xml";

        /////////////////////////////////////////////////////////
        // Loading and saving resources
        /////////////////////////////////////////////////////////
        logText.println("Loading/saving resources...");
        List<LessonObject> objList = allResourcesTmp.GetLessonObjList();

        foreach (LessonObject currentObj in objList)
        {
            // Add resource to global list as-is
            allResourcesGlobal.AddLessonObject(currentObj.Clone());

            // For each resource...
            List<LessonObjResource> resourceList = currentObj.GetLessonObjResourceList();

            foreach (LessonObjResource currentResource in resourceList)
            {
                yield return SaveOneRemoteResource(currentResource, resourceSavePath);
            }

            // Add to local list (since path has changed)
            allResourcesLocal.AddLessonObject(currentObj);
        }

        logText.println(allResourcesLocal.ToString());
        logText.println(allResourcesGlobal.ToString());

        /////////////////////////////////////////////////////////
        // Resaving XML file
        /////////////////////////////////////////////////////////        
        allResourcesLocal.Save(localResourceFile);
        allResourcesGlobal.Save(globalResourceFile);
        logText.println("Resaved XML resource files");

        // Reinit spawners
        recreateSpawners();

        yield return new WaitForSeconds(0.1f);
    }

    IEnumerator SaveOneRemoteResource(LessonObjResource currentResource, string resourceSavePath)
    {
        // Get the resource path
        string resourcePath = currentResource.GetPath();
        logText.println("\t" + resourcePath);

        // We will look at the resource path's prefix:            
        // URL: Load from URL
        string prefix = resourcePath.Substring(0, 4);

        if (prefix.Equals("URL:"))
        {
            string address = resourcePath.Substring(4);
            Debug.Log("WEB ADDRESS: " + address);

            UnityWebRequest www = UnityWebRequest.Get(address);
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log("COULD NOT DOWNLOAD: " + address);
                Debug.Log(www.error);
                logText.println("\t" + "Could not download!");
            }
            else
            {
                // Get file name
                string filename = Path.GetFileName(address);

                // Append to save path
                string objectSavePath = resourceSavePath + filename;

                // Save new path
                currentResource.SetPath(objectSavePath);

                logText.println("SAVED OBJECT: " + objectSavePath);

                // Save actual object
                System.IO.File.WriteAllBytes(objectSavePath, www.downloadHandler.data);
                //System.IO.File.WriteAllText(objectSavePath, www.downloadHandler.text);
            }
        }

        yield return new WaitForSeconds(1);
    }


#endif

    // Callback: used if you are manually re-creating spawners
    public void InitializeSpawners()
    {
        reloadXMLResourceFiles();
        recreateSpawners();
    }

    public void reloadXMLResourceFiles()
    {
        string resourceSavePath = Application.persistentDataPath + "/";
        string localResourceFile = resourceSavePath + "LOCAL_Resources.xml";
        string globalResourceFile = resourceSavePath + "GLOBAL_Resources.xml";

        logText.println("Reloading local XML resources file...");
        allResourcesLocal = LessonResources.Load(localResourceFile);
        logText.println(allResourcesLocal.ToString());
        allResourcesGlobal = LessonResources.Load(globalResourceFile);
        logText.println(allResourcesGlobal.ToString());
    }

    public void recreateSpawners()
    {
#if (WINDOWS_UWP)
        // Clean up old spawners
        deleteSpawners();

        // Create spawners
        StartCoroutine(loadDataAndCreateSpawners());
#endif 
    }

    public void deleteSpawners()
    {
        logText.println("Deleting previous spawners...");

        // Loop through and destroy all spawners
        foreach (GameObject obj in allSpawners)
        {
            Destroy(obj);
        }
        allSpawners.Clear();
        
        // Clean up all loaded resources
        foreach (KeyValuePair<string, GameObject> entry in allResourceData)
        {
            Destroy(entry.Value);
        }
        allResourceData.Clear();
        allSpawnerScales.Clear();
    }

#if (WINDOWS_UWP)

    IEnumerator loadDataAndCreateSpawners()
    {
        logText.println("Creating spawners...");

        // Get all objects
        List<LessonObject> objList = allResourcesLocal.GetLessonObjList();

        // For each object...
        foreach (LessonObject currentObj in objList)
        {
            // Prepare game object
            GameObject prefabToSpawn = null;
            Vector3 prefabScale = new Vector3(0.3f, 0.3f, 0.3f);

            // Get all resources
            List<LessonObjResource> resourceList = currentObj.GetLessonObjResourceList();

            string dataType = currentObj.GetDataType();
            if (dataType.Equals("3D"))
            {
                // 3D object

                // Need to load the following:
                // - MODEL --> mesh data                
                // - TEXTURE --> 2D image
                // TODO: For now, we can only handle ONE texture and ONE mesh
                // TODO: Only handles OBJ files

                string modelPath = "";
                string texturePath = "";

                foreach (LessonObjResource currentResource in resourceList)
                {
                    string resDataType = currentResource.GetDataType();

                    if (resDataType.Equals("MODEL"))
                    {
                        modelPath = currentResource.GetPath();
                    }
                    else if (resDataType.Equals("TEXTURE"))
                    {
                        texturePath = currentResource.GetPath();
                    }
                }

                logText.println("\t" + "MODEL: " + modelPath);
                logText.println("\t" + "TEXTURE: " + texturePath);

                // Load text of 3D model file
                string modelDataString = System.IO.File.ReadAllText(modelPath);

                // Load texture data
                WWW textureWWW = new WWW("file:///" + texturePath);
                while (!textureWWW.isDone)
                {
                    print(textureWWW.bytesDownloaded);
                    yield return null;
                }

                //byte[] textureData = File.ReadAllBytes(texturePath);

                // Create game object
                prefabToSpawn = ResourceMaker.Create3DModelFromString(modelDataString, textureWWW.texture); // textureData);

                // Set scale
                prefabScale = new Vector3(0.3f, 0.3f, 0.3f);
            }
            else if (dataType.Equals("TEXT"))
            {
                // TEXT data

                // Get all text paths
                List<string> allTextData = new List<string>();

                foreach (LessonObjResource currentResource in resourceList)
                {
                    // Get path
                    string textPath = currentResource.GetPath();

                    // Read in text
                    string textString = System.IO.File.ReadAllText(textPath);

                    // Add to list
                    allTextData.Add(textString);

                    yield return new WaitForSeconds(0.01f);
                }

                // Create game object
                prefabToSpawn = ResourceMaker.CreateTextDisplay(allTextData);

                // Set scale
                float textScale = 0.007f;
                prefabScale = new Vector3(textScale, textScale, textScale);
            }

            /*
            // Add DefaultSyncModelAccessor
            //Warning: Probably necessary for sharing
            DefaultSyncModelAccessor syncer = (DefaultSyncModelAccessor)prefabToSpawn.AddComponent(typeof(DefaultSyncModelAccessor));
            */

            // Add to list of resources
            if (!allResourceData.ContainsKey(currentObj.GetID()))
            {
                allResourceData.Add(currentObj.GetID(), prefabToSpawn);
                allSpawnerScales.Add(currentObj.GetID(), prefabScale);
            }

            // Make prefab disabled at first
            prefabToSpawn.SetActive(false);
            //prefabToSpawn.GetComponent<MeshRenderer>().enabled = false;

            logText.println("\t" + "Data loaded!");
            yield return new WaitForSeconds(0.1f);
        }

        logText.println("ALL DATA LOADED.");
        yield return createSpawners();
    }

    IEnumerator createSpawners()
    {
        //Save local Resource path
        // For each object in the resource list
        List<LessonObject> objList = allResourcesLocal.GetLessonObjList();

        Vector3 currentPos = new Vector3(0, 0, 2);
        Vector3 incPos = new Vector3(1, 0, 0);
        Vector3 incRowPos = new Vector3(0, 0, -1);
        int index = 0;
        int colCnt = 4;

        foreach (LessonObject currentObj in objList)
        {
            // Set position
            //Vector3 spawnPosition = currentPos + index * incPos;
            int rowIndex = (index / colCnt);
            int colIndex = index % colCnt;
            Vector3 spawnPosition = currentPos + colIndex * incPos + rowIndex * incRowPos;
            
            index++;

            // Use default rotation
            Quaternion spawnRotation = new Quaternion();

            // Use spawner scale            
            Vector3 spawnScale = GetSpawnerScale(currentObj.GetID());

            GameObject spawnedObject = SpawnObject(currentObj, spawnPosition,
                                                   spawnRotation, spawnScale);

            // Add TapSpawnResponder
            TapSpawnResponder tapper = (TapSpawnResponder)spawnedObject.AddComponent(typeof(TapSpawnResponder));
            tapper.setLessonManagerData(this, currentObj.GetID());

            // Add to list of spawners
            allSpawners.Add(spawnedObject);

            logText.println("Created spawner " + currentObj.GetID() + "...");
            yield return new WaitForSeconds(0.1f);
        }

        spawnerSetupDone = true;

        yield return new WaitForSeconds(0.1f);
    }
#endif
     
    public void SaveResources()
    {
#if (WINDOWS_UWP)        
        UnityEngine.WSA.Application.InvokeOnUIThread(() => SaveXMLResourceFile(), false);        
#endif
    }

#if (WINDOWS_UWP)  
    // Open picker for saving resource file
    private async void SaveXMLResourceFile()
    {
        // Pick XML Resource File
        Windows.Storage.Pickers.FileSavePicker picker = makeXMLSavePicker();
        Windows.Storage.StorageFile file = await picker.PickSaveFileAsync();

        if (file != null)
        {
            // Application now has read/write access to the picked file            
            print("Picked file: " + file.Name + " -- " + file.Path);
            logText.println("XML Resource: " + file.Name);

            // Open and save XML resources
            logText.println("Saving XML file...");

            //await Windows.Storage.FileIO.ReadTextAsync(file)
            allResourcesGlobal.SaveFile(file);
            //await Task.Run(() => { allResourcesGlobal.Save(file.Path); });        
        }
        else
        {
            print("Operation cancelled.");
        }
    }
#endif
          
    // Callback:
    // - reset EVERYTHING
    // - load XML lesson file
    // - load and save resources
    // - create spawners
    // - (set inactive if Student mode)
    // - spawn items in world
    public void OpenLesson()
    {
#if (WINDOWS_UWP)
        resetAll();
        spawnerSetupDone = false;
        resourcePickingDone = false;
        allResourcesTmp = null;
        UnityEngine.WSA.Application.InvokeOnUIThread(() => PickXMLLessonFile(), false);
        StartCoroutine(WaitForLessonPicking());
#endif
    }

#if (WINDOWS_UWP)
    // Open picker for getting lesson file
    private async void PickXMLLessonFile()
    {
        // Pick XML Lesson File
        Windows.Storage.Pickers.FileOpenPicker picker = makeXMLOpenPicker();
        Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();

        if (file != null)
        {
            // Application now has read/write access to the picked file            
            print("Picked file: " + file.Name + " -- " + file.Path);
            logText.println("XML Lesson: " + file.Name);

            // Open and load XML lesson list
            logText.println("Loading XML file...");
            string xmlFileText = await Windows.Storage.FileIO.ReadTextAsync(file);
            print(xmlFileText);

            // Just grab the first scene
            allLessons = LessonList.LoadFromText(xmlFileText);
            print(allLessons.ToString());
            currentLesson = allLessons.GetLesson(0);
            print(currentLesson);
            currentScene = currentLesson.GetScene(0);
            print(currentScene);
            currentLessonID = currentLesson.GetID();
            currentSceneID = currentScene.GetID();
                       
            // Grab the lesson resources from this lesson
            allResourcesTmp = allLessons.GetResources();
            print(allResourcesTmp);

            logText.println(allLessons.ToString());
        }
        else
        {
            print("Operation cancelled.");
        }

        resourcePickingDone = true;
    }

    // Waiting for picking of XML file to be complete...
    IEnumerator WaitForLessonPicking()
    {
        while (!resourcePickingDone)
        {
            yield return null;
        }

        // Picking done, is this valid?
        if (allResourcesTmp != null)
        {
            yield return StartCoroutine(SaveRemoteResourcesToDisk());

            while (!spawnerSetupDone)
            {
                yield return null;
            }

            // Spawn all objects in scene
            yield return StartCoroutine(SpawnLessonObjects());
        }

        yield return null;
    }

    IEnumerator SpawnLessonObjects()
    {
        List<LessonObjRef> objRefList = currentScene.GetLessonObjRefList();
        foreach(LessonObjRef objRef in objRefList)
        {
            // Actually create object
            GameObject spawnedItem = SpawnObjectFromRef(objRef);

            // Add to total list of objects
            allCreatedObjects[objRef.GetID()] = spawnedItem;

            yield return new WaitForSeconds(0.01f);
        }

        // Set up parental guidance
        foreach (LessonObjRef objRef in objRefList)
        {
            // Does this have a parent?
            if (objRef.HasParent())
            {
                string parentID = objRef.GetParent();
                GameObject parentObj = allCreatedObjects[parentID];
                GameObject childObj = allCreatedObjects[objRef.GetID()];

                // Make other object disabled
                childObj.SetActive(false);
                                
                // Add as child object
                childObj.transform.parent = parentObj.transform;
                childObj.transform.position = objRef.GetTranslate();
                childObj.transform.rotation = objRef.GetRotation();
                childObj.transform.localScale = objRef.GetScaling();
            }

            yield return new WaitForSeconds(0.01f);
        }

        logText.println("LESSON PREPARED!!!");
        yield return new WaitForSeconds(0.1f);
    }
#endif

    public void SaveLesson()
    {
#if (WINDOWS_UWP)        
        UnityEngine.WSA.Application.InvokeOnUIThread(() => SaveXMLLessonFile(), false);
#endif
    }


#if (WINDOWS_UWP)  
    // Open picker for saving lesson file
    private async void SaveXMLLessonFile()
    {
        // Pick XML Resource File
        Windows.Storage.Pickers.FileSavePicker picker = makeXMLSavePicker();
        Windows.Storage.StorageFile file = await picker.PickSaveFileAsync();

        if (file != null)
        {
            // Application now has read/write access to the picked file            
            print("Picked file: " + file.Name + " -- " + file.Path);
            logText.println("XML Lesson: " + file.Name);

            // Open and save XML resources
            logText.println("Saving XML file...");

            allLessons.SetResources(allResourcesGlobal);
            allLessons.SaveFile(file);
            //await Task.Run(() => { allResourcesGlobal.Save(file.Path); });        
        }
        else
        {
            print("Operation cancelled.");
        }
    }
#endif




    public GameObject AddLessonObjectRef(string lessonObjRef, Vector3 spawnPosition,
                                            Quaternion spawnRotation, Vector3 spawnScale)
    {
        // Create object ref
        LessonObjRef objRef = new LessonObjRef();
        System.Random rnd = new System.Random();
        int randInt = rnd.Next(1000000);
        string randomID = "" + randInt;
        objRef.SetID(randomID);
        objRef.SetRefObjID(lessonObjRef);
        objRef.SetTranslate(spawnPosition);
        objRef.SetRotation(spawnRotation);
        objRef.SetScaling(spawnScale);

        // Add ref to scene        
        currentScene.AddLessonObjRef(objRef);

        // Actually create object
        GameObject spawnedItem = SpawnObjectFromRef(objRef);

        // Add to total list of objects
        allCreatedObjects[objRef.GetID()] = spawnedItem;

        // Return spawned object
        return spawnedItem;
    }
    
    public GameObject SpawnObject(LessonObject lessonObj, Vector3 spawnPosition,
                                    Quaternion spawnRotation, Vector3 spawnScale)
    {
        // Get the resource name
        string resourceName = lessonObj.GetID();
        
        // Get the appropriate prefab
        GameObject prefabToSpawn = GetPrefab(resourceName);

        // Spawn item
        GameObject spawnedItem = Instantiate(prefabToSpawn);

        // Set position, rotation, and scale
        spawnedItem.transform.position = spawnPosition;
        spawnedItem.transform.rotation = spawnRotation;
        spawnedItem.transform.localScale = spawnScale;
        
        // Make active
        spawnedItem.SetActive(true);
        //spawnedItem.GetComponent<MeshRenderer>().enabled = true;

        return spawnedItem;
    }

    public GameObject SpawnObjectFromRef(LessonObjRef objRef)
    {
        // Get position and rotation
        Vector3 spawnPosition = objRef.GetTranslate();
        Quaternion spawnRotation = objRef.GetRotation();
        Vector3 spawnScale = objRef.GetScaling();

        // Find object in resources
        LessonObject lessonObj = allResourcesLocal.FindLessonObject(objRef.GetRefObjID());

        // Spawn object
        GameObject spawnedItem = SpawnObject(lessonObj, spawnPosition, spawnRotation, spawnScale);

        // Add TwoHandManipulatable component and object reference
        TwoHandManipulatableObj manip = (TwoHandManipulatableObj)spawnedItem.AddComponent(typeof(TwoHandManipulatableObj));
        manip.setLessonObjRef(objRef);

        // Add click action responder as well
        if (lessonObj.GetDataType().Equals("TEXT"))
        {
            TextTapActionResponder tapper = (TextTapActionResponder)spawnedItem.AddComponent(typeof(TextTapActionResponder));
            tapper.setLessonManagerData(this, objRef.GetID());
            LessonTextDisplay textDisplay = spawnedItem.GetComponent<LessonTextDisplay>();
            textDisplay.setTextMesh(spawnedItem.GetComponent<TextMesh>());
            tapper.setTextDisplay(textDisplay);
        }
        else
        {
            TapActionResponder tapper = (TapActionResponder)spawnedItem.AddComponent(typeof(TapActionResponder));
            tapper.setLessonManagerData(this, objRef.GetID());
        }
        
        // Return new shiny object
        return spawnedItem;
    }

    protected virtual Vector3 GetSpawnerScale(string key)
    {
        Vector3 prefabScale = allSpawnerScales[key];
        return prefabScale;
    }

    protected virtual GameObject GetPrefab(string key)
    {
        GameObject prefabToSpawn = allResourceData[key];

        if (prefabToSpawn == null)
        {
            logText.println(key + ": PREFAB NULL!!!");
        }

        return prefabToSpawn;
    }

    private LessonObjRef getObjRefInFocus()
    {
        // Check what object is in focus  
        LessonObjRef objRef = null;
        GameObject target = HoloToolkit.Unity.InputModule.GazeManager.Instance.HitObject;

        if (target != null)
        {
            TwoHandManipulatableObj manip = target.GetComponent<TwoHandManipulatableObj>();
            if (manip != null)
            {
                objRef = manip.getLessonObjRef();
                logText.println("Object in focus: " + objRef.GetID());
            }
        }

        return objRef;

    }

    public void startAttaching()
    {
        logText.println("Begin attachment...");
        objRefToAttach = getObjRefInFocus();
    }
    
    public void endAttaching()
    {
        logText.println("End attachment...");
        if(objRefToAttach != null)
        {
            // Get object in focus now...
            LessonObjRef parentRef = getObjRefInFocus();

            // Make sure that:
            // - it's not the SAME as the original
            // - it's not already in the list
            // TODO: Need better parent hierarchy check
            if(parentRef != objRefToAttach 
                && parentRef.FindLessonTapAction(objRefToAttach.GetID()) == null
                && !objRefToAttach.HasParent() )
            {
                // Add tap action
                LessonTapAction action = new LessonTapAction(objRefToAttach.GetID());
                parentRef.AddLessonTapAction(action);

                // Make other object disabled
                allCreatedObjects[objRefToAttach.GetID()].SetActive(false);

                // Add as child object
                allCreatedObjects[objRefToAttach.GetID()].transform.parent = allCreatedObjects[parentRef.GetID()].transform;
                allCreatedObjects[objRefToAttach.GetID()].GetComponent<TwoHandManipulatableObj>().updateLessonObjRef();

                // Set parent
                objRefToAttach.SetParent(parentRef.GetID());
            }
            else
            {
                logText.println("ERROR: Cannot attach object!");
            }

            objRefToAttach = null;
        }
    }

    public void NotifyClicked(string lessonObjRefID)
    {
        logText.println(lessonObjRefID + " was clicked!");
        // Loop through all tap actions
        LessonObjRef objRef = currentScene.FindLessonObjRef(lessonObjRefID);
        List<LessonTapAction> allActions = objRef.GetLessonTapActionList();

        // Perform special action for item itself
        GameObject thisObject = allCreatedObjects[objRef.GetID()];
        TapActionResponder thisTapper = thisObject.GetComponent<TapActionResponder>();
        thisTapper.performSpecialAction();

        // Perform tap actions
        foreach (LessonTapAction action in allActions)
        {
            string otherRefID = action.GetRefObjID();
            GameObject otherObject = allCreatedObjects[otherRefID];
            bool currentActiveState = otherObject.activeSelf;
            otherObject.SetActive(!currentActiveState);
            if(!currentActiveState)
            {
                TapActionResponder tapper = otherObject.GetComponent<TapActionResponder>();
                tapper.performSpecialAction();
            }
        }
    }

    public bool isInTeacherMode()
    {
        return inTeacherMode;
    }

    public void resetAll()
    {
        // Reset resources
        allResourcesGlobal.Reset();
        allResourcesLocal.Reset();

        // Clean up spawners
        deleteSpawners();

        // Reset lessons        
        allLessons = new LessonList();
        currentLessonID = "DefaultLesson";
        currentSceneID = "DefaultScene";
        currentLesson = new Lesson(currentLessonID);
        currentScene = new LessonScene(currentSceneID);
        currentLesson.AddLessonScene(currentScene);
        allLessons.AddLesson(currentLesson);

        objectCreationCounter = 0;
        objRefToAttach = null;

        // Clean up all created objects (not spawners)
        foreach (KeyValuePair<string, GameObject> entry in allCreatedObjects)
        {
            Destroy(entry.Value);
        }
        allCreatedObjects.Clear();

        logText.println("EVERYTHING CLEARED.");        
    }

    public void ToggleTeacherMode()
    {
        inTeacherMode = !inTeacherMode;

        foreach(GameObject obj in allSpawners)
        {
            obj.SetActive(inTeacherMode);
        }        
    }
}

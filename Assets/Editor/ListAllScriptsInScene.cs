using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ListAllScriptsInScene
{
    [MenuItem("Tools/List All Scripts In Scene")]
    public static void ListScripts()
    {
        MonoBehaviour[] allBehaviours = GameObject.FindObjectsOfType<MonoBehaviour>(true);
        HashSet<string> scriptNames = new HashSet<string>();

        foreach (var mb in allBehaviours)
        {
            if (mb == null) continue;
            scriptNames.Add(mb.GetType().Name);
        }

        Debug.Log("Scripts in scene:\n" + string.Join("\n", scriptNames));
    }
}
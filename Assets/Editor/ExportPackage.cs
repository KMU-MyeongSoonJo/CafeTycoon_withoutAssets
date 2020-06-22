using UnityEngine;
using System.Collections;
using UnityEditor;

public static class ExportPackage
{


    [MenuItem("Export/Export with tags and layers, Input settings, Physics Settings, EditorBuildSettings")]
    public static void export()
    {
        string[] projectContent = new string[] { "Assets", "ProjectSettings/TagManager.asset", "ProjectSettings/InputManager.asset", "ProjectSettings/ProjectSettings.asset", "ProjectSettings/Physics2DSettings.asset", "ProjectSettings/EditorBuildSettings.asset" };
        AssetDatabase.ExportPackage(projectContent, "Done.unitypackage", ExportPackageOptions.Interactive | ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies);
        Debug.Log("Project Exported");
    }

}
﻿#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
#endif
using UnityEngine;
using System.IO;
using System;
public static class EnvironmentConfigController {

    public static string path = Application.dataPath + "/EnvironmentConfig/";
    public static string editorFile = path + "Editor.json";
    public static string developmentFile=path + "Development.json";
    public static string productionFile=path + "Production.json";

    public static EnvironmentConfig Init(){
        string filename = "";
        if (PlayerPrefs.GetInt("DevelopmentBuild", 0) == 0 ? false : true) {
            filename = developmentFile;
        } else {
            filename = productionFile;
        }
        #if UNITY_EDITOR
        filename = editorFile;
        #endif
        FileInfo file = new FileInfo(filename);
        string text = ReadFile(file.FullName);
        EnvironmentConfig data = JsonUtility.FromJson<EnvironmentConfig>(text);
        Prefs.prefix = data.prefsPrefix;
        return data;
    }

    #if UNITY_EDITOR
    [PostProcessBuildAttribute(1)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) {
        if (Debug.isDebugBuild) {
            SetEnvironment("DevelopmentBuild");
            Debug.Log("Development build");
        } else {
            SetEnvironment("ProductionBuild");
            Debug.Log("Production build");
        }
        if (target == BuildTarget.StandaloneOSXIntel64 || target == BuildTarget.StandaloneOSXIntel) {
            Debug.Log(Directory.GetParent(pathToBuiltProject) + " Enviro Files Copied.");
            DirectoryCopy (Application.dataPath + "/EnvironmentConfig", pathToBuiltProject + "/Contents/EnvironmentConfig", true);
        }
        if (target == BuildTarget.StandaloneWindows64 || target == BuildTarget.StandaloneWindows) {
            Debug.Log(Directory.GetParent(pathToBuiltProject) + " Enviro Files Copied.");
            DirectoryCopy (Application.dataPath + "/EnvironmentConfig", Directory.GetParent(pathToBuiltProject).ToString()+"/Game_Data/EnvironmentConfig", true);
        }

    }
        
    public static void SetEnvironment(string enviro){
        if (enviro == "Development") {
            PlayerPrefs.SetInt("DevelopmentBuild", 1);
        } else {
            PlayerPrefs.SetInt("DevelopmentBuild", 0);
        }
    }
    #endif 
    public static string ReadFile (string file){
        StreamReader reader = File.OpenText(file);
        string text = "";
        string line = "";
        while (line != null) {
            line = reader.ReadLine();
            text += line;
        }
        reader.Close ();
        return text;
    }

    private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
    {
        // Get the subdirectories for the specified directory.
        DirectoryInfo dir = new DirectoryInfo (sourceDirName);

        if (!dir.Exists) {
            throw new DirectoryNotFoundException (
                "Source directory does not exist or could not be found: "
                + sourceDirName);
        }

        DirectoryInfo[] dirs = dir.GetDirectories ();
        // If the destination directory doesn't exist, create it.
        if (!Directory.Exists (destDirName)) {
            Directory.CreateDirectory (destDirName);
        }

        // Get the files in the directory and copy them to the new location.
        FileInfo[] files = dir.GetFiles ();
        foreach (FileInfo file in files) {
            string temppath = Path.Combine (destDirName, file.Name);
            file.CopyTo (temppath, false);
        }

        // If copying subdirectories, copy them and their contents to new location.
        if (copySubDirs) {
            foreach (DirectoryInfo subdir in dirs) {
                string temppath = Path.Combine (destDirName, subdir.Name);
                DirectoryCopy (subdir.FullName, temppath, copySubDirs);
            }
        }
    }
}
[System.Serializable]
public class EnvironmentConfig {
    public string prefsPrefix;
    public string serverHost;
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace BetaJester.EnumGenerator {
    public static class ScriptableObjectUtility {

        internal static T CreateAsset<T>(string savePath, string name, Func<T, bool> SetupCode) where T : ScriptableObject {
#if UNITY_EDITOR
            T newSO = ScriptableObject.CreateInstance<T>();

            if (SetupCode != null)
                SetupCode(newSO);

            string path = savePath + (savePath[savePath.Length - 1] == '/' ? "" : "/") + $"{name}.asset";

            AssetDatabase.DeleteAsset(path);
            AssetDatabaseUtility.CreateAssetAndDirectories(newSO, path);
            //AssetDatabase.CreateAsset(newSO, path);
            //AssetDatabase.SaveAssets();

            T generatedSO = AssetDatabase.LoadAssetAtPath(path, typeof(T)) as T;
            EditorUtility.SetDirty(generatedSO);

            return generatedSO;

#else
        return null;
#endif
        }

        public static void CreateAsset<T>() where T : ScriptableObject {

#if UNITY_EDITOR
            T asset = ScriptableObject.CreateInstance<T>();

            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "") {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != "") {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New " + typeof(T).ToString() + ".asset");

            AssetDatabase.CreateAsset(asset, assetPathAndName);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
#endif
        }



        public static List<T> Load<T>(string loadPath, bool includeSubFolders = true) where T : ScriptableObject {
#if UNITY_EDITOR

            List<T> soObjects = new List<T>();

            string[] assetNames = AssetDatabase.FindAssets($"t:{typeof(T).ToString()}", new[] { loadPath });

            foreach (string SOName in assetNames) {
                var SOpath = AssetDatabase.GUIDToAssetPath(SOName);
                var loadedObj = AssetDatabase.LoadAssetAtPath<T>(SOpath);

                if (loadedObj != null)
                    soObjects.Add(loadedObj);
            }

            if (includeSubFolders) {
                string[] subFolders = AssetDatabase.GetSubFolders(loadPath);

                for (int i = 0; i < subFolders.Length; i++) {
                    Load<T>(loadPath + subFolders[i] + "/", includeSubFolders);
                }
            }

            return soObjects;
#else
        return null;
#endif
        }

        public static void SetNewName(this ScriptableObject so, string newName, bool checkForUniqueName = true) {
#if UNITY_EDITOR
            var pathName = AssetDatabase.GetAssetPath(so);

            if (checkForUniqueName && AssetDatabase.FindAssets("\"" + newName + "\"").Length > 1) {

                //   EditorUtility.DisplayDialog("Error: Scriptable Object Utility", $"Renaming scriptable object {so.name} failed, new name is not unique.", "OK");
                Debug.LogError($"Error: Renaming scriptable object {so.name} failed, new name is not unique.", so);
            }
            else {
                so.name = newName;
                AssetDatabase.RenameAsset(pathName, newName);
            }


#endif
        }

        public static void AutoRename(ScriptableObject so, string response) {
            so.name = GetAutoName(so.GetType().ToString(), response);
        }

        public static string GetAutoName(string so, string response) {
            string newName = $"{so}-{UnityEngine.Networking.UnityWebRequest.EscapeURL(response)}";
            return newName.Substring(0, newName.Length > 140 ? 140 : newName.Length);
        }
        public static string GetAutoName<T>(T so, string response) {
            string newName = $"{so.GetType().ToString()}-{UnityEngine.Networking.UnityWebRequest.EscapeURL(response)}";
            return newName.Substring(0, newName.Length > 140 ? 140 : newName.Length);
        }

        public static string EscapeFileName(string fileName) {
            string newName = UnityEngine.Networking.UnityWebRequest.EscapeURL(fileName);
            return newName.Substring(0, newName.Length > 140 ? 140 : newName.Length);
        }

        public static T GetInstance<T>() where T : ScriptableObject {
            return GetAllInstances<T>().First();
        }

        public static T[] GetAllInstances<T>(params Type[] whichImplement) where T : ScriptableObject {
#if UNITY_EDITOR
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);  //FindAssets uses tags check documentation for more info
            List<T> a = new List<T>();
            for (int i = 0; i < guids.Length; i++)         //probably could get optimized 
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                a.Add(AssetDatabase.LoadAssetAtPath<T>(path));

                for (int j = 0; j < whichImplement.Count(); j++) {

                    Type interfaceType = whichImplement[j];
                    if (a != null && a.Last() != null) {
                        Type objectType = a.Last().GetType();

                        if (!objectType.IsTypeOf(interfaceType)) {
                            a.RemoveAt(a.Count() - 1);
                            break;
                        }
                    }
                    else {
                        a.RemoveAt(a.Count() - 1);
                        break;
                    }
                }
            }

            return a.ToArray();
#else
        return null;
#endif

        }
        public static bool IsTypeOf<T>(this Type type) {
            return typeof(T).IsAssignableFrom(type);
        }

        public static bool IsTypeOf(this Type type, Type checkAgainstType) {
            return checkAgainstType.IsAssignableFrom(type);
        }

    }
}
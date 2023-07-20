using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


[System.Serializable]
public struct EnumInfo {
    public string _name;
    public string[] _stringValues;
    public int[] _intValues;
}

public interface IEnumContainer {
    EnumInfo[] GetEnums();
}

[CreateAssetMenu(menuName = "Enum Generator/Enum Creator")]
public class EnumCreator : SingletonScriptableObject<EnumCreator> {
    public static char whiteSpaceReplacement = '_';

#if UNITY_EDITOR
        [System.Serializable]
        public struct EnumValRef {
            public string enumName;
            public string enumVal;
            public int enumIntVal;
        }

        [SerializeField] string _namespaceName = "";
        [SerializeField] string _filePathOverride = "";
        [SerializeField] List<EnumValRef> _createdValues = new List<EnumValRef>();
        [SerializeField] UnityEngine.Object[] _enumContainers;
        [SerializeField] bool _isInitialised = false;

        public void OnEnable() {
            if (!_isInitialised) {
                _isInitialised = true;
                CreateEnums();
                string definesString = UnityEditor.PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
                List<string> allDefines = definesString.Split(';').ToList();
                allDefines.Add("ENUMS_GENERATED");
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, string.Join(";", allDefines.ToArray()));
            }
        }

        public void CreateEnums() {


            if (!_isInitialised) {
                OnEnable();
            }

            //EnumCreator[] enumCreators = ScriptableObjectUtility.GetAllInstances<EnumCreator>();

            //if (enumCreators.Count() == 0)
            //  return;

            //string saveLocation = AssetDatabase.GetAssetPath(enumCreators.First());
            //saveLocation = saveLocation.Substring(0, saveLocation.Length - 17);

            string fileName = "GeneratedEnums";

            //string generatedFilePath = saveLocation;



            string GetFilePathOverride() {
                if (_filePathOverride.Last() == '/' || _filePathOverride.Last() == '\'')
                    return _filePathOverride;
                else
                    return _filePathOverride + "/";
            }


            string copyPath = _filePathOverride == "" ? "Assets/Generated/" + fileName + ".cs" : GetFilePathOverride() + fileName + ".cs";
            Debug.Log("Creating Classfile: " + copyPath);

            using (StreamWriter enumFile =
                new StreamWriter(copyPath)) {
                enumFile.WriteLine("using UnityEngine;");
                enumFile.WriteLine("using System.Collections;");
                enumFile.WriteLine("");
                enumFile.WriteLine("//This class is auto-generated, please do not edit it as your changes will be lost");

                if (_namespaceName.Count() > 0)
                    enumFile.WriteLine("namespace " + _namespaceName + ".Enums {");

                enumFile.WriteLine(" ");

                List<EnumInfo> enumsToGenerate = new List<EnumInfo>();
                _enumContainers = (ScriptableObjectUtility.GetAllInstances<ScriptableObject>(typeof(IEnumContainer)).Where(x => x != null).Select(x => x as UnityEngine.Object).ToArray());

                for (int i = 0; i < _enumContainers.Length; i++) {

                    ScriptableObject so = _enumContainers[i] as ScriptableObject;

                    IEnumContainer enumContainer = so as IEnumContainer;


                    if (enumContainer == null) {
                        GameObject enumGO = _enumContainers[i] as GameObject;

                        enumsToGenerate.AddRange(enumGO.GetComponent<IEnumContainer>().GetEnums().ToList());
                    }
                    else {
                        enumsToGenerate.AddRange(enumContainer.GetEnums().ToList());
                    }
                }



                for (int i = 0; i < enumsToGenerate.Count; i++) {

                    if (enumsToGenerate[i]._name == "") continue;

                    enumFile.WriteLine("[System.Serializable]");
                    enumFile.WriteLine("public enum " + enumsToGenerate[i]._name.Replace(' ', whiteSpaceReplacement) + " {");

                    string[] uniqueValues = enumsToGenerate[i]._stringValues.Distinct().ToArray();
                    int[] uniqueIntValues = new int[0];

                    if (enumsToGenerate[i]._intValues != null)
                        uniqueIntValues = enumsToGenerate[i]._intValues.Distinct().ToArray();

                    for (int j = 0; j < uniqueValues.Length; j++) {
                        if (ContainsAny(uniqueValues[j], new string[] { "-", "'", "?", "!", "{", "}" })) {
                            Debug.LogError("Cannot have anything other than a-z, A-Z, 0-9, and spaces in value name. Halting generation on: " + uniqueValues[j]);
                            enumFile.WriteLine("}");
                            enumFile.WriteLine(" ");
                            enumFile.WriteLine("}");
                            AssetDatabase.Refresh();
                            return;
                        }

                        if (uniqueValues[j] == " ") {
                            Debug.Log("Value " + j + " is blank, skipping: " + enumsToGenerate[i]._name);
                            continue;
                        }

                        string enumName = uniqueValues[j].Replace(' ', whiteSpaceReplacement);
                        int inEnumIntVal = uniqueIntValues.Length > j ? uniqueIntValues[j] : -1000;


                        if (enumName.Length == 0)
                            continue;

                        if (int.TryParse(enumName[0].ToString(), out int _)) {
                            enumName = "_" + enumName;
                        }

                        EnumValRef enumValRef = new EnumValRef() {
                            enumName = enumsToGenerate[i]._name.Replace(' ', whiteSpaceReplacement),
                            enumVal = enumName,
                            enumIntVal = inEnumIntVal != -1000 ? inEnumIntVal : GetEnumIntMaxVal(enumsToGenerate[i]._name.Replace(' ', whiteSpaceReplacement)) + 1
                        };


                        if (_createdValues.Any(x => x.enumName == enumValRef.enumName && x.enumVal == enumValRef.enumVal))
                            inEnumIntVal = _createdValues.First(x => x.enumName == enumValRef.enumName && x.enumVal == enumValRef.enumVal).enumIntVal;
                        else {
                            inEnumIntVal = enumValRef.enumIntVal;
                            _createdValues.Add(enumValRef);
                        }


                        enumFile.WriteLine(enumName + " = " + inEnumIntVal + ",");
                    }

                    enumFile.WriteLine("}");
                    enumFile.WriteLine(" ");
                }

                if (_namespaceName.Count() > 0)
                    enumFile.WriteLine("}");
            }

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
            AssetDatabase.Refresh();


        }

        [MenuItem("Enum Creator/Regenerate Enums %e")]
        public static void RegenerateEnums() {
            //EnumCreator[] enumCreators = GetAllInstances<EnumCreator>();

            //if (enumCreators.Count() == 0) {
            //    if (!AssetDatabase.IsValidFolder("Assets/Generated")) AssetDatabase.CreateFolder("Assets", "Generated");

            //    ScriptableObjectUtility.CreateAsset<EnumCreator>("Assets/Generated", "EnumCreator", (s) => { s.CreateEnums(); return true; });

            //}
            //else {
            //    enumCreators.First().CreateEnums();
            //}

            EnumCreator.Instance.CreateEnums();
        }

        
        private int GetEnumIntMaxVal(string enumName) {
            if (_createdValues.Any(x => x.enumName == enumName))
                return _createdValues.Where(x => x.enumName == enumName).OrderByDescending(x => x.enumIntVal).First().enumIntVal;
            else
                return -1;
        }


        public static T[] GetAllInstances<T>() where T : ScriptableObject {
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);  //FindAssets uses tags check documentation for more info
            T[] a = new T[guids.Length];
            for (int i = 0; i < guids.Length; i++)         //probably could get optimized 
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                a[i] = AssetDatabase.LoadAssetAtPath<T>(path);
            }

            return a;

        }


#endif

    public static T StringToEnum<T>(string value, T defaultValue) where T : struct, IConvertible {
        if (!typeof(T).IsEnum) throw new ArgumentException("T must be an enumerated type");
        if (string.IsNullOrEmpty(value)) return defaultValue;


        string replacedValue = value.Replace(' ', whiteSpaceReplacement);

        foreach (T item in Enum.GetValues(typeof(T))) {
#if UNITY_2021_1_OR_NEWER
            if (item.ToString().Equals(replacedValue.Trim(), StringComparison.InvariantCultureIgnoreCase)) return item;
#else
                if (item.ToString().ToLower().Equals(replacedValue.Trim().ToLower())) return item;
#endif
        }
        return defaultValue;
    }

    public static string EnumToString<T>(T value) {
        string stringValue = Enum.GetName(typeof(T), value);
        return stringValue.Replace(whiteSpaceReplacement, ' ');
    }

    public static bool ContainsAny(string haystack, params string[] needles) {
        foreach (string needle in needles) {
            if (haystack.Contains(needle))
                return true;
        }

        return false;
    }

}


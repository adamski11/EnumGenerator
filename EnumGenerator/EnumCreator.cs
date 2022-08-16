﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BetaJester.EnumGenerator
{
    [System.Serializable]
    public struct EnumInfo
    {
        public string _name;
        public string[] _values;
    }

    [CreateAssetMenu(menuName = "Enum Generator/Enum Creator")]
    public class EnumCreator : ScriptableObject
    {
        public static char whiteSpaceReplacement = '_';


        public string namespaceName = "";
       
        string filePathOverride = "Assets/";
        UnityEngine.Object[] enumContainers;

        public List<EnumValRef> createdValues = new List<EnumValRef>();

        [System.Serializable]
        public struct EnumValRef
        {
            public string enumName;
            public string enumVal;
            public int enumIntVal;
        }

        public void CreateEnums()
        {
#if UNITY_EDITOR
         
            EnumCreator[] enumCreators = ScriptableObjectUtility.GetAllInstances<EnumCreator>();

            if (enumCreators.Count() == 0)
                return;

            string saveLocation = AssetDatabase.GetAssetPath(enumCreators.First());
            saveLocation = saveLocation.Substring(0, saveLocation.Length - 17);

            string fileName = "GeneratedEnums";
            filePathOverride = saveLocation;

            string GetFilePathOverride()
            {
                if (filePathOverride.Last() == '/' || filePathOverride.Last() == '\'')
                    return filePathOverride;
                else
                    return filePathOverride + "/";
            }

            string copyPath = filePathOverride == "" ? "Assets/" + fileName + ".cs" : GetFilePathOverride() + fileName + ".cs";
            Debug.Log("Creating Classfile: " + copyPath);

            using (StreamWriter enumFile =
                new StreamWriter(copyPath))
            {
                enumFile.WriteLine("using UnityEngine;");
                enumFile.WriteLine("using System.Collections;");
                enumFile.WriteLine("");
                enumFile.WriteLine("//This class is auto-generated, please do not edit it as your changes will be lost");

                if (namespaceName.Count() > 0)
                    enumFile.WriteLine("namespace " + namespaceName + ".Enums {");

                enumFile.WriteLine(" ");

                List<EnumInfo> enumsToGenerate = new List<EnumInfo>();
               
                
                enumContainers = (ScriptableObjectUtility.GetAllInstances<ScriptableObject>(typeof(IEnumContainer)));

                for (int i = 0; i < enumContainers.Length; i++)
                {
                    ScriptableObject so = enumContainers[i] as ScriptableObject;

                    IEnumContainer enumContainer = so as IEnumContainer;


                    if (enumContainer == null)
                    {
                        GameObject enumGO = enumContainers[i] as GameObject;

                        enumsToGenerate.AddRange(enumGO.GetComponent<IEnumContainer>().GetEnums().ToList());
                    }
                    else
                    {
                        enumsToGenerate.AddRange(enumContainer.GetEnums().ToList());
                    }
                }



                for (int i = 0; i < enumsToGenerate.Count; i++)
                {
                    enumFile.WriteLine("[System.Serializable]");
                    enumFile.WriteLine("public enum " + enumsToGenerate[i]._name.Replace(' ', whiteSpaceReplacement) + " {");

                    string[] uniqueValues = enumsToGenerate[i]._values.Distinct().ToArray();

                    for (int j = 0; j < uniqueValues.Length; j++)
                    {
                        if (ContainsAny(uniqueValues[j], new string[] { "-", "'", "?", "!", "{", "}" }))
                        {
                            Debug.LogError("Cannot have anything other than a-z, A-Z, 0-9, and spaces in value name. Halting generation on: " + uniqueValues[j]);
                            enumFile.WriteLine("}");
                            enumFile.WriteLine(" ");
                            enumFile.WriteLine("}");
                            AssetDatabase.Refresh();
                            return;
                        }

                        if (uniqueValues[j] == " ")
                        {
                            Debug.Log("Value " + j + " is blank, skipping: " + enumsToGenerate[i]._name);
                            continue;
                        }

                        string enumName = uniqueValues[j].Replace(' ', whiteSpaceReplacement);

                        if (enumName.Length == 0)
                            continue;

                        if (int.TryParse(enumName[0].ToString(), out int _))
                        {
                            enumName = "_" + enumName;
                        }

                        EnumValRef enumValRef = new EnumValRef()
                        {
                            enumName = enumsToGenerate[i]._name.Replace(' ', whiteSpaceReplacement),
                            enumVal = enumName,
                            enumIntVal = GetEnumIntMaxVal(enumsToGenerate[i]._name.Replace(' ', whiteSpaceReplacement)) + 1
                        };

                        int enumIntVal = enumValRef.enumIntVal;

                        if (createdValues.Any(x => x.enumName == enumValRef.enumName && x.enumVal == enumValRef.enumVal))
                            enumIntVal = createdValues.First(x => x.enumName == enumValRef.enumName && x.enumVal == enumValRef.enumVal).enumIntVal;
                        else
                            createdValues.Add(enumValRef);

                        enumFile.WriteLine(enumName + " = " + enumIntVal + ",");
                    }

                    enumFile.WriteLine("}");
                    enumFile.WriteLine(" ");
                }

                if (namespaceName.Count() > 0)
                    enumFile.WriteLine("}");
            }

            AssetDatabase.Refresh();
#endif

        }

        private int GetEnumIntMaxVal(string enumName)
        {
            if (createdValues.Any(x => x.enumName == enumName))
                return createdValues.Where(x => x.enumName == enumName).OrderByDescending(x => x.enumIntVal).First().enumIntVal;
            else
                return -1;
        }

#if UNITY_EDITOR
        [MenuItem("Enum Creator/Regenerate Enums %e")]
        public static void RegenerateEnums()
        {
            GetAllInstances<EnumCreator>().First().CreateEnums();
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
        public static T StringToEnum<T>(string value, T defaultValue) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum) throw new ArgumentException("T must be an enumerated type");
            if (string.IsNullOrEmpty(value)) return defaultValue;


            string replacedValue = value.Replace(' ', whiteSpaceReplacement);

            foreach (T item in Enum.GetValues(typeof(T)))
            {
                if (item.ToString().Equals(replacedValue.Trim(), StringComparison.InvariantCultureIgnoreCase)) return item;
            }
            return defaultValue;
        }

        public static string EnumToString<T>(T value)
        {
            string stringValue = Enum.GetName(typeof(T), value);
            return stringValue.Replace(whiteSpaceReplacement, ' ');
        }

        public static bool ContainsAny(string haystack, params string[] needles)
        {
            foreach (string needle in needles)
            {
                if (haystack.Contains(needle))
                    return true;
            }

            return false;
        }

    }

    public interface IEnumContainer
    {
        EnumInfo[] GetEnums();
    }


}
﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Adamski11.EnumGenerator
{
    [System.Serializable]
    public struct EnumInfo
    {
        public string _name;
        public string[] _values;
    }

    public class EnumCreator : MonoBehaviour
    {
        public static char whiteSpaceReplacement = '_';

        public string namespaceName = "ExampleTeam";
        [Tooltip("Must start with \"Assets\"")]
        public string filePathOverride = "\"Assets\"";
        public EnumInfo[] enumInfo;

        //public IEnumContainer[] enumContainers;
        public GameObject[] enumContainers;

        public List<EnumValRef> createdValues = new List<EnumValRef>();

        [System.Serializable]
        public struct EnumValRef
        {
            public string enumName;
            public string enumVal;
            public int enumIntVal;
        }

#if UNITY_EDITOR

        public void CreateEnums()
        {
            string fileName = "GeneratedEnums";

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
                enumFile.WriteLine("namespace " + namespaceName + ".Enums {");
                enumFile.WriteLine(" ");

                List<EnumInfo> enumsToGenerate = new List<EnumInfo>();
                enumsToGenerate.AddRange(enumInfo.ToList());

                for (int i = 0; i < enumContainers.Length; i++)
                {
                    enumsToGenerate.AddRange(enumContainers[i].GetComponent<IEnumContainer>().GetEnums().ToList());
                }

                for (int i = 0; i < enumsToGenerate.Count; i++)
                {
                    enumFile.WriteLine("[System.Serializable]");
                    enumFile.WriteLine("public enum " + enumsToGenerate[i]._name.Replace(' ', whiteSpaceReplacement) + " {");
                    for (int j = 0; j < enumsToGenerate[i]._values.Length; j++)
                    {
                        EnumValRef enumValRef = new EnumValRef()
                        {
                            enumName = enumsToGenerate[i]._name.Replace(' ', whiteSpaceReplacement),
                            enumVal = enumsToGenerate[i]._values[j].Replace(' ', whiteSpaceReplacement),
                            enumIntVal = GetEnumIntMaxVal(enumsToGenerate[i]._name.Replace(' ', whiteSpaceReplacement)) + 1
                        };

                        int enumIntVal = enumValRef.enumIntVal;

                        if (createdValues.Any(x => x.enumName == enumValRef.enumName && x.enumVal == enumValRef.enumVal))
                            enumIntVal = createdValues.First(x => x.enumName == enumValRef.enumName && x.enumVal == enumValRef.enumVal).enumIntVal;
                        else
                            createdValues.Add(enumValRef);

                        enumFile.WriteLine(enumsToGenerate[i]._values[j].Replace(' ', whiteSpaceReplacement) + " = " + enumIntVal + ",");
                    }

                    enumFile.WriteLine("}");
                    enumFile.WriteLine(" ");
                }

                enumFile.WriteLine("}");
            }

            AssetDatabase.Refresh();

        }

        private int GetEnumIntMaxVal(string enumName)
        {
            if (createdValues.Any(x => x.enumName == enumName))
                return createdValues.Where(x => x.enumName == enumName).OrderByDescending(x => x.enumIntVal).First().enumIntVal;
            else
                return -1;
        }

        [MenuItem("Enum Creator/Regenerate Enums %e")]
        public static void RegenerateEnums()
        {
            GameObject.FindObjectOfType<EnumCreator>().CreateEnums();
        }
#endif

        public static T StringToEnum<T>(string value, T defaultValue) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum) throw new ArgumentException("T must be an enumerated type");
            if (string.IsNullOrEmpty(value)) return defaultValue;

            foreach (T item in Enum.GetValues(typeof(T)))
            {
                if (item.ToString().ToLower().Equals(value.Trim().ToLower())) return item;
            }
            return defaultValue;
        }

        public static string EnumToString<T>(T value)
        {
            string stringValue = Enum.GetName(typeof(T), value);
            return stringValue.Replace(whiteSpaceReplacement, ' ');
        }


    }

    public interface IEnumContainer
    {
        EnumInfo[] GetEnums();
    }

}
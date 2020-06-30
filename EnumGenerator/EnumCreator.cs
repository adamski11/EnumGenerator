using System;
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
        public EnumContainer[] enumContainers;


#if UNITY_EDITOR

        public void CreateEnums()
        {
            string fileName = "GeneratedEnums";

            string GetfilePathOverride()
            {
                if (filePathOverride.Last() == '/')
                    return filePathOverride;
                else
                    return filePathOverride + "/";
            }

            string copyPath = filePathOverride == "" ? "Assets/" + fileName + ".cs" : GetfilePathOverride() + fileName + ".cs";
            Debug.Log("Creating Classfile: " + copyPath);

            using (StreamWriter enumFile =
                new StreamWriter(copyPath))
            {
                enumFile.WriteLine("using UnityEngine;");
                enumFile.WriteLine("using System.Collections;");
                enumFile.WriteLine("");

                enumFile.WriteLine("namespace " + namespaceName + ".Enums {");
                enumFile.WriteLine(" ");

                List<EnumInfo> enumsToGenerate = new List<EnumInfo>();
                enumsToGenerate.AddRange(enumInfo.ToList());

                for (int i = 0; i < enumContainers.Length; i++)
                {
                    enumsToGenerate.AddRange(enumContainers[i].GetEnums().ToList());
                }
                
                for (int i = 0; i < enumsToGenerate.Count; i++)
                {
                    enumFile.WriteLine("[System.Serializable]");
                    enumFile.WriteLine("public enum " + enumsToGenerate[i]._name.Replace(' ', whiteSpaceReplacement) + " {");
                    for (int j = 0; j < enumsToGenerate[i]._values.Length; j++)
                    {
                        enumFile.WriteLine(enumsToGenerate[i]._values[j].Replace(' ', whiteSpaceReplacement) + ",");
                    }

                    enumFile.WriteLine("}");
                    enumFile.WriteLine(" ");
                }

                enumFile.WriteLine("}");
            }

            AssetDatabase.Refresh();

        }
#endif

        public static T EnumToString<T>(string value, T defaultValue) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum) throw new ArgumentException("T must be an enumerated type");
            if (string.IsNullOrEmpty(value)) return defaultValue;

            foreach (T item in Enum.GetValues(typeof(T)))
            {
                if (item.ToString().ToLower().Equals(value.Trim().ToLower())) return item;
            }
            return defaultValue;
        }

        public static string StringToEnum<T>(string value)
        {
            string stringValue = Enum.GetName(typeof(T), value);
            return stringValue.Replace(whiteSpaceReplacement, ' ');
        }
    }

    public abstract class EnumContainer : MonoBehaviour
    {
        public abstract EnumInfo[] GetEnums();

        
    }

    public abstract class EnumConverter { };


}
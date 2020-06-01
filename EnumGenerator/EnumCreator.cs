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
        public string teamName = "ExampleTeam";
        [Tooltip("Must start with \"Assets\"")]
        public string filePathOverride;

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

                enumFile.WriteLine("namespace " + teamName + ".Enums {");
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
                    enumFile.WriteLine("public enum " + enumsToGenerate[i]._name + " {");
                    for (int j = 0; j < enumsToGenerate[i]._values.Length; j++)
                    {
                        enumFile.WriteLine(enumsToGenerate[i]._values[j] + ",");
                    }

                    enumFile.WriteLine("}");
                    enumFile.WriteLine(" ");
                }

                enumFile.WriteLine("}");
            }

            AssetDatabase.Refresh();

        }
#endif
    }

    public abstract class EnumContainer : MonoBehaviour
    {
        public abstract EnumInfo[] GetEnums();
    }

}
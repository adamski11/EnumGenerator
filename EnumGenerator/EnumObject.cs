using BetaJester.EnumGenerator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using NaughtyAttributes;
#endif

[CreateAssetMenu(menuName = "Enum Generator/Enum Object")]
public class EnumObject : ScriptableObject, IEnumContainer {

    public const uint NONE_CASE_VALUE = 0;
    public const uint SPECIAL_CASE_VALUE = int.MaxValue;


    [Serializable]
    public struct EnumItemInfo {
        public string itemName;
        public int itemValue;
    }

    [SerializeField] string _enumName;
    public List<EnumItemInfo> enumTypes = new List<EnumItemInfo>() {
        new EnumItemInfo(){itemName = "None", itemValue = (int)NONE_CASE_VALUE},
        new EnumItemInfo(){itemName = "Special", itemValue = (int)SPECIAL_CASE_VALUE}};


    [Header("Debug")]
    [SerializeField] string _data;
    [SerializeField] string _splitter = ",";

#if ODIN_INSPECTOR
    [Button("Generate Unique Integer Values")]
#endif
    public void GenerateUniqueIntegerValues() {
        for (int i = 0; i < enumTypes.Count; i++) {
            if (enumTypes[i].itemName == "None") enumTypes[i] = new EnumItemInfo() { itemName = "None", itemValue = (int)NONE_CASE_VALUE };
            else if (enumTypes[i].itemName == "Special") enumTypes[i] = new EnumItemInfo() { itemName = "Special", itemValue = (int)SPECIAL_CASE_VALUE };
            else enumTypes[i] = new EnumItemInfo() { itemName = enumTypes[i].itemName, itemValue = i };


        }
    }


#if ODIN_INSPECTOR
    [Button("Generate From String")]
#endif
    public void GenerateFromString() {
        List<string> enumItems = _data.Split(new string[] { _splitter }, System.StringSplitOptions.RemoveEmptyEntries).ToList();



        for (int i = 0; i < enumItems.Count; i++) {

            string[] enumItemSplit = enumItems[i].Split('=');

            if (enumItemSplit.Count() > 1) {
                enumTypes.Add(new EnumItemInfo() { itemName = enumItemSplit[0].Trim(), itemValue = int.Parse(enumItemSplit[1].Trim()) });
            }
            else {
                enumTypes.Add(new EnumItemInfo() { itemName = enumItems[i].Trim(), itemValue = enumTypes.Count - 1 });
            }

        }

        enumTypes = enumTypes.OrderBy(x => x.itemValue).ToList();

#if UNITY_EDITOR

        if (!UnityEditor.EditorApplication.isUpdating && !UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode && !Application.isPlaying) {
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
            UnityEditor.AssetDatabase.Refresh();
        }


#endif
    }



    public EnumInfo[] GetEnums() {
        return new EnumInfo[] {
            new EnumInfo()
            {
                _name = _enumName,
                _stringValues = enumTypes.Select(x => x.itemName).ToArray(),
                _intValues = enumTypes.Select(x => x.itemValue).ToArray()
            }
        };
    }
}

using BetaJester.EnumGenerator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "Enum Generator/Enum Object")]
public class EnumObject : DynamicScriptableObject, IEnumContainer {

    [Serializable]
    public struct EnumItemInfo {
        public string itemName;
        public int itemValue;
    }

    [SerializeField] string _enumName;
    public List<EnumItemInfo> enumTypes = new List<EnumItemInfo>() {
        new EnumItemInfo(){itemName = "None", itemValue = (int)EnumContext.NONE_CASE_VALUE},
        new EnumItemInfo(){itemName = "Special", itemValue = (int)EnumContext.SPECIAL_CASE_VALUE}};


    [Header("Debug")]
    [SerializeField] string _data;
    [SerializeField] string _splitter = ",";


    [Button("Generate From String")]
    public void GenerateFromString() {
        List<string> enumItems = _data.Split(new string[] { _splitter }, System.StringSplitOptions.RemoveEmptyEntries).ToList();



        for (int i = 0; i < enumItems.Count; i++) {

            string[] enumItemSplit = enumItems[i].Split('=');

            if (enumItemSplit.Count() > 1) {
                enumTypes.Add(new EnumItemInfo() { itemName = enumItemSplit[0].Trim(), itemValue = int.Parse(enumItemSplit[1].Trim()) });
            }
            else {
                enumTypes.Add(new EnumItemInfo() { itemName = enumItems[i].Trim(), itemValue = enumTypes.Count - 1});
            }

        }

        enumTypes = enumTypes.OrderBy(x => x.itemValue).ToList();

        SaveChangesToObject();
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

using BetaJester.EnumGenerator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

[CreateAssetMenu(menuName = "Enum Generator/Enum Object")]
public class EnumObject : ScriptableObject, IEnumContainer {

    [SerializeField] string _enumName;
    public List<string> enumTypes;


    [Header("Debug")]
    [TestButton("Generate From String", "GenerateFromString"), TestButton("Add From String", "AddFromString"), SerializeField] string _data;
    [SerializeField] string _splitter = ",";


    public void GenerateFromString() {
        enumTypes = _data.Split(new string[] { _splitter }, System.StringSplitOptions.RemoveEmptyEntries).ToList();
        for (int i = 0; i < enumTypes.Count; i++) {
            enumTypes[i] = enumTypes[i].Trim();
        }
    }
    
    public void AddFromString() {
        enumTypes.AddRange(_data.Split(new string[] { _splitter }, System.StringSplitOptions.RemoveEmptyEntries));
        for (int i = 0; i < enumTypes.Count; i++) {
            enumTypes[i] = enumTypes[i].Trim();
        }
    }

    public EnumInfo[] GetEnums() {
        return new EnumInfo[] { new EnumInfo() { _name = _enumName, _values = enumTypes.ToArray() } };
    }
}

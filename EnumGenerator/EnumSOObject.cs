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

[CreateAssetMenu(menuName = "Enum Generator/Enum SO Object")]
public class EnumSOObject : ScriptableObject, IEnumContainer {


    public const uint NONE_CASE_VALUE = 0;
    public const uint SPECIAL_CASE_VALUE = int.MaxValue;


    public interface IEnumItemName {
        public string itemName { get; }
    }

    [Serializable]
    public class EnumSOItemInfo {
        public ScriptableObject item;
        public int itemValue;
    }

    [SerializeField] string _enumName;
    public List<EnumSOItemInfo> enumTypes;

    [Header("Debug")]
    [SerializeField] string _data;
    [SerializeField] string _splitter = ",";

#if ODIN_INSPECTOR
    [Button("Generate Unique Integer Values")]
#endif
    public void GenerateUniqueIntegerValues() {
        for (int i = 0; i < enumTypes.Count; i++) {

            IEnumItemName enumName = enumTypes[i].item as IEnumItemName;

            if (enumName.itemName == "None") enumTypes[i].itemValue = (int)NONE_CASE_VALUE ;
            else if (enumName.itemName == "Special") enumTypes[i].itemValue = (int)SPECIAL_CASE_VALUE ;
            else enumTypes[i].itemValue = i;


        }
    }


    public EnumInfo[] GetEnums() {
        return new EnumInfo[] {
            new EnumInfo()
            {
                _name = _enumName,
                _stringValues = enumTypes.Select(x => (x.item as IEnumItemName).itemName).ToArray(),
                _intValues = enumTypes.Select(x => x.itemValue).ToArray()
            }
        };
    }
}

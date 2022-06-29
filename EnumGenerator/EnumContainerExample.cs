using BetaJester.EnumGenerator;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace EnumGenerator {
    public class EnumContainerExample : MonoBehaviour, IEnumContainer {
        public List<ObjectInfo> objectInfos = new List<ObjectInfo>();

        public EnumInfo[] GetEnums() {
            return new EnumInfo[] { new EnumInfo() { _name = "ObjectType", _values = objectInfos.Select(x => x.objectName).ToArray() } };
        }

    }

    [System.Serializable]
    public class ObjectInfo {
        public string objectName;
        public float objectValue;

    }
}
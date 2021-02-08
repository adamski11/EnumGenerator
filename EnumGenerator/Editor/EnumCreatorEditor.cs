using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace BetaJester.EnumGenerator
{
#if UNITY_EDITOR
    [CustomEditor(typeof(EnumCreator))]
    public class EnumCreatorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EnumCreator myTarget = (EnumCreator)target;

            DrawDefaultInspector();

            if (GUILayout.Button("Generate Enums"))
            {
                myTarget.CreateEnums();
            }
        }
    }
#endif
}
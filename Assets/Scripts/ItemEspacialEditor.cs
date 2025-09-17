// Assets/Editor/ItemEspacialEditor.cs
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(ItemEspacial))]
public class ItemEspacialEditor : Editor
{
    SerializedProperty efectosProp;

    void OnEnable()
    {
        efectosProp = serializedObject.FindProperty("efectos");
    }

    public override void OnInspectorGUI()
    {
        
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Efectos (Decorator) – Helper", EditorStyles.boldLabel);

        
        if (GUILayout.Button("Agregar efecto..."))
        {
            var menu = new GenericMenu();

           
            var tipos = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => {
                    try { return a.GetTypes(); } catch { return new Type[0]; }
                })
                .Where(t => typeof(IEfectoDeItem).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .ToList();

            foreach (var t in tipos)
            {
                var nombre = t.Name;
                menu.AddItem(new GUIContent(nombre), false, () =>
                {
                    serializedObject.Update();
                    int index = efectosProp.arraySize;
                    efectosProp.InsertArrayElementAtIndex(index);
                    var elem = efectosProp.GetArrayElementAtIndex(index);
                    elem.managedReferenceValue = Activator.CreateInstance(t);
                    serializedObject.ApplyModifiedProperties();
                });
            }

            menu.ShowAsContext();
        }

        
        if (GUILayout.Button("Quitar entradas nulas"))
        {
            serializedObject.Update();
            for (int i = efectosProp.arraySize - 1; i >= 0; i--)
            {
                var elem = efectosProp.GetArrayElementAtIndex(i);
                if (elem.managedReferenceValue == null)
                    efectosProp.DeleteArrayElementAtIndex(i);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}

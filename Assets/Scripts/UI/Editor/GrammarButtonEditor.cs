using TMPro;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GrammarButton))]
public class GrammarButtonEditor : DictionnaryButtonEditor
{
    [Header("Grammar")]
    SerializedProperty title;
    SerializedProperty subTitle;

    protected override void OnEnable()
    {
        base.OnEnable();

        title = serializedObject.FindProperty("title");
        subTitle = serializedObject.FindProperty("subTitle");
    }

    public override void OnInspectorGUI()
    {
        // Affiche les propriétés de base du Button
        base.OnInspectorGUI();

        // Rafraîchir l'objet sérialisé
        serializedObject.Update();

        // Afficher les propriétés personnalisées de VolumeSlider
        EditorGUILayout.PropertyField(title);
        EditorGUILayout.PropertyField(subTitle);
        
        // Appliquer les changements
        serializedObject.ApplyModifiedProperties();
    }
}

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
        // Affiche les propri�t�s de base du Button
        base.OnInspectorGUI();

        // Rafra�chir l'objet s�rialis�
        serializedObject.Update();

        // Afficher les propri�t�s personnalis�es de VolumeSlider
        EditorGUILayout.PropertyField(title);
        EditorGUILayout.PropertyField(subTitle);
        
        // Appliquer les changements
        serializedObject.ApplyModifiedProperties();
    }
}

using TMPro;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DictionaryButton))]
public class DictionnaryButtonEditor : SelectableButtonEditor
{
    [Header("Color")]
    SerializedProperty baseColor;
    SerializedProperty alternativeColor;

    [Header("Pin")]
    SerializedProperty pinnedSprite;
    SerializedProperty notPinnedSprite;

    protected override void OnEnable()
    {
        base.OnEnable();

        baseColor = serializedObject.FindProperty("baseColor");
        alternativeColor = serializedObject.FindProperty("alternativeColor");

        pinnedSprite = serializedObject.FindProperty("pinnedSprite");
        notPinnedSprite = serializedObject.FindProperty("notPinnedSprite");
    }

    public override void OnInspectorGUI()
    {
        // Affiche les propri�t�s de base du Button
        base.OnInspectorGUI();

        // Rafra�chir l'objet s�rialis�
        serializedObject.Update();

        // Afficher les propri�t�s personnalis�es de VolumeSlider
        EditorGUILayout.PropertyField(baseColor);
        EditorGUILayout.PropertyField(alternativeColor);

        EditorGUILayout.PropertyField(pinnedSprite);
        EditorGUILayout.PropertyField(notPinnedSprite);

        // Appliquer les changements
        serializedObject.ApplyModifiedProperties();
    }
}

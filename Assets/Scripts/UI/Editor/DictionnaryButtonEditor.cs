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
        // Affiche les propriétés de base du Button
        base.OnInspectorGUI();

        // Rafraîchir l'objet sérialisé
        serializedObject.Update();

        // Afficher les propriétés personnalisées de VolumeSlider
        EditorGUILayout.PropertyField(baseColor);
        EditorGUILayout.PropertyField(alternativeColor);

        EditorGUILayout.PropertyField(pinnedSprite);
        EditorGUILayout.PropertyField(notPinnedSprite);

        // Appliquer les changements
        serializedObject.ApplyModifiedProperties();
    }
}

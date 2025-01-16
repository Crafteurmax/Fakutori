using TMPro;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VocabularyButton))]
public class VocabularyButtonEditor : SelectableButtonEditor
{
    [Header("Color")]
    SerializedProperty baseColor;
    SerializedProperty alternativeColor;

    [Header("Vocabulary")]
    SerializedProperty kanji;
    SerializedProperty kana;
    SerializedProperty romaji;
    SerializedProperty traduction;

    [Header("Intern Objects")]
    SerializedProperty kanjiTextMesh;
    SerializedProperty kanaTextMesh;
    SerializedProperty romajiTextMesh;
    SerializedProperty traductionTextMesh;

    protected override void OnEnable()
    {
        base.OnEnable();

        baseColor = serializedObject.FindProperty("baseColor");
        alternativeColor = serializedObject.FindProperty("alternativeColor");

        kanji = serializedObject.FindProperty("kanji");
        kana = serializedObject.FindProperty("kana");
        romaji = serializedObject.FindProperty("romaji");
        traduction = serializedObject.FindProperty("traduction");

        kanjiTextMesh = serializedObject.FindProperty("kanjiTextMesh");
        kanaTextMesh = serializedObject.FindProperty("kanaTextMesh");
        romajiTextMesh = serializedObject.FindProperty("romajiTextMesh");
        traductionTextMesh = serializedObject.FindProperty("traductionTextMesh");
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

        EditorGUILayout.PropertyField(kanji);
        EditorGUILayout.PropertyField(kana);
        EditorGUILayout.PropertyField(romaji);
        EditorGUILayout.PropertyField(traduction);

        EditorGUILayout.PropertyField(kanjiTextMesh);
        EditorGUILayout.PropertyField(kanaTextMesh);
        EditorGUILayout.PropertyField(romajiTextMesh);
        EditorGUILayout.PropertyField(traductionTextMesh);

        // Appliquer les changements
        serializedObject.ApplyModifiedProperties();
    }
}

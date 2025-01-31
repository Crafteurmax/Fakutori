using TMPro;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VocabularyButton))]
public class VocabularyButtonEditor : DictionnaryButtonEditor
{
    [Header("Vocabulary")]
    SerializedProperty kanji;
    SerializedProperty kana;
    SerializedProperty romaji;
    SerializedProperty traduction;

    protected override void OnEnable()
    {
        base.OnEnable();

        kanji = serializedObject.FindProperty("kanji");
        kana = serializedObject.FindProperty("kana");
        romaji = serializedObject.FindProperty("romaji");
        traduction = serializedObject.FindProperty("traduction");
    }

    public override void OnInspectorGUI()
    {
        // Affiche les propriétés de base du Button
        base.OnInspectorGUI();

        // Rafraîchir l'objet sérialisé
        serializedObject.Update();

        // Afficher les propriétés personnalisées de VolumeSlider
        EditorGUILayout.PropertyField(kanji);
        EditorGUILayout.PropertyField(kana);
        EditorGUILayout.PropertyField(romaji);
        EditorGUILayout.PropertyField(traduction);

        // Appliquer les changements
        serializedObject.ApplyModifiedProperties();
    }
}

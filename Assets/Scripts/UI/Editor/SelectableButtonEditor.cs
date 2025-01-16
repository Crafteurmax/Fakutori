using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SelectableButton))]
public class SelectableButtonEditor : MultiLayerButtonEditor
{
    [Header("Selection")]
    SerializedProperty selectedColor;
    SerializedProperty isSelected;

    protected override void OnEnable()
    {
        base.OnEnable();

        selectedColor = serializedObject.FindProperty("selectedColor");
        isSelected = serializedObject.FindProperty("isSelected");
    }

    public override void OnInspectorGUI()
    {
        // Affiche les propriétés de base du Button
        base.OnInspectorGUI();

        // Rafraîchir l'objet sérialisé
        serializedObject.Update();

        // Afficher les propriétés personnalisées de VolumeSlider
        EditorGUILayout.PropertyField(selectedColor);
        EditorGUILayout.PropertyField(isSelected);

        // Appliquer les changements
        serializedObject.ApplyModifiedProperties();
    }
}

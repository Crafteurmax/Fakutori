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
        // Affiche les propri�t�s de base du Button
        base.OnInspectorGUI();

        // Rafra�chir l'objet s�rialis�
        serializedObject.Update();

        // Afficher les propri�t�s personnalis�es de VolumeSlider
        EditorGUILayout.PropertyField(selectedColor);
        EditorGUILayout.PropertyField(isSelected);

        // Appliquer les changements
        serializedObject.ApplyModifiedProperties();
    }
}

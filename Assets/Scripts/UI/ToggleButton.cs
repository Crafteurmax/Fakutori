// Source : https://stackoverflow.com/questions/56498130/how-can-i-make-a-button-to-act-like-a-toggle-or-maybe-using-a-toggle-and-make-th

using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ToggleButton : MonoBehaviour, IPointerClickHandler
{
    public ToggleEvent CheckedChanged = new ToggleEvent();

    Image _image;
    Color _originalColor;

    bool _checked;
    [SerializeField] Color _checkedColor;
    [SerializeField] public ToggleButtonGroup _group;

    [SerializeField]
    public bool Checked
    {
        get
        {
            return _checked;
        }
        set
        {
            if (_checked != value)
            {
                _checked = value;
                UpdateVisual();
                CheckedChanged.Invoke(this);
            }
        }
    }

    void Start()
    {
        _image = GetComponent<Image>();
        _originalColor = _image.color;

        if (_group != null)
            _group.RegisterToggle(this);
    }

    private void UpdateVisual()
    {
        _image.color = Checked ? _checkedColor : _originalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Checked = !Checked;
    }

    [Serializable]
    public class ToggleEvent : UnityEvent<ToggleButton>
    {
    }
}
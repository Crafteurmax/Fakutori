// Source : https://stackoverflow.com/questions/56498130/how-can-i-make-a-button-to-act-like-a-toggle-or-maybe-using-a-toggle-and-make-th

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ToggleButtonGroup : MonoBehaviour
{
    [SerializeField] List<ToggleButton> _toggles = new List<ToggleButton>();
    ToggleButton _currentToggle;
    ToggleButton _lastToggle = null;
    public UnityEvent NewToggledButton = new UnityEvent();

    public void RegisterToggle(ToggleButton toggle)
    {
        _toggles.Add(toggle);
        toggle.CheckedChanged.AddListener(HandleCheckedChanged);
    }

    void HandleCheckedChanged(ToggleButton toggle)
    {
        if (toggle.Checked)
        {
            foreach (var item in _toggles)
            {
                if (item.GetInstanceID() != toggle.GetInstanceID())
                {
                    item.Checked = false;
                }
                else
                {
                    if (_lastToggle == null || _lastToggle.GetInstanceID() != toggle.GetInstanceID())
                    {
                        _lastToggle = item;
                    }
                    _currentToggle = item;
                }
            }
            NewToggledButton.Invoke();
        }
        else
        {
            if (_lastToggle.GetInstanceID() == toggle.GetInstanceID())
            {
                //Debug.Log("Uncheked");
                _currentToggle = null;
                _lastToggle = null;
                NewToggledButton.Invoke();
            }            
        }
    }

    public void ClearList()
    {
        _toggles.Clear();
    }

    public ToggleButton GetCurrentToggledButton()
    {
        return _currentToggle;
    }
}
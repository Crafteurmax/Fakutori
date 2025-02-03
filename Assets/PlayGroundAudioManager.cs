using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayGroundAudioManager : MonoBehaviour
{
    public void PlayEffect(int index)
    {
        AudioManager.instance.PlayEffect(index);
    }

    public void PlayEffect(string effectName) {
        AudioManager.instance.PlayEffect(effectName);
    }
}

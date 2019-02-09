using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundsData : ScriptableObject {

    public SoundsManager.SoundItem[] sounds;

    private void OnValidate()
    {
        if (sounds != null)
        {
            foreach (var sound in sounds)
            {
                sound.name = sound.type.ToString();
                if (sound.isSequence)
                {
                    sound.maxActive = 1;
                }
            }
        }
    }
}

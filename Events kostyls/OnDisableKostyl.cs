using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class OnDisableKostyl : MonoBehaviour {

    public Ugly.Call[] calls = new Ugly.Call[] { new Ugly.Call { delayed = false, FramesDelay = 1 } };

    MonoBehaviour monoBehaviour;

    private void Awake()
    {
        monoBehaviour = this;
    }

    private void OnDisable()
    {
        for (int i = 0; i < calls.Length; i++)
        {
            calls[i].Invoke(monoBehaviour);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (calls != null && calls.Length > 0)
        {
            for (int i = 0; i < calls.Length; i++)
            {
                calls[i].Update();
            }
        }
    }
#endif
}

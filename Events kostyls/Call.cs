using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Ugly
{
    [System.Serializable]
    public class Call
    {
#if UNITY_EDITOR
        [HideInInspector] public string name;
#endif
        public bool delayed = true;

        [SerializeField] private int framesDelay = 1;
        public int FramesDelay
        {
            get
            {
                return framesDelay;
            }
            set
            {
                framesDelay = value;
                if (framesDelay < 1)
                {
                    framesDelay = 1;
                }
            }
        }
        public UnityEvent onEnable;

        Coroutine coroutine;

        public void Invoke(MonoBehaviour monoBehaviour)
        {
            if (delayed && framesDelay > 0)
            {
                if (coroutine != null)
                {
                    monoBehaviour.StopCoroutine(coroutine);
                }
                coroutine = monoBehaviour.StartCoroutine(DelayedIvoke());
            }
            else
            {
                onEnable.Invoke();
            }
        }

        IEnumerator DelayedIvoke()
        {
            for (int i = 0; i < framesDelay; i++)
            {
                yield return null;
            }

            onEnable.Invoke();
            coroutine = null;
        }

#if UNITY_EDITOR
        public void Update()
        {
            System.Text.StringBuilder name = new System.Text.StringBuilder();

            if (delayed && FramesDelay <= 0)
            {
                FramesDelay = 1;
            }

            if (onEnable != null)
            {
                int count = onEnable.GetPersistentEventCount();

                if (count > 0)
                {
                    name.Append(delayed ? "DELAYED: " : "");
                    for (int j = 0; j < count; j++)
                    {
                        if (j > 0)
                        {
                            name.Append(", ");
                        }
                        name.Append(onEnable.GetPersistentMethodName(j));
                    }
                }
                else
                {
                    name.Append("EMPTY");
                }

                this.name = name.ToString();
            }
        }
#endif
    }
}
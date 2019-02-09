using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundsManager : MonoBehaviour
{
    public enum SoundGroup
    {
        music, sound, uI
    }

    [System.Serializable]
    public class SoundItem
    {
        [HideInInspector] public string name;
        public SoundType type;
        public SoundGroup group;
        public bool loop, isSequence;
        public float volume = 1f;
        public int maxActive = 5;
        public AudioClip[] clips;
        [HideInInspector] public int currentClip = -1;
        [HideInInspector] public float startTime, lenth;
    }

    private static SoundsManager instance;
    public static SoundsManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SoundsManager>();
            }
            return instance;
        }
    }

    private static float overallVolume = 1f, soundsVolume = 1f, musicVolume = 1f, uiVolume = 1f;
    public static float Volume
    {
        set
        {
            overallVolume = value;
            PlayerPrefs.SetFloat("OverallVolume", overallVolume);
            Instance.UpdateVolume();
        }
        get
        {
            return overallVolume;
        }
    }

    public static float SoundsVolume
    {
        set
        {
            soundsVolume = value;
            PlayerPrefs.SetFloat("SoundsVolume", soundsVolume);
            Instance.UpdateVolume();
        }
        get
        {
            return soundsVolume;
        }
    }

    public static float MusicVolume
    {
        set
        {
            musicVolume = value;
            PlayerPrefs.SetFloat("MusicVolume", musicVolume);
            Instance.UpdateVolume();
        }
        get
        {
            return musicVolume;
        }
    }

    public static float UiVolume
    {
        set
        {
            uiVolume = value;
            PlayerPrefs.SetFloat("UiVolume", uiVolume);
            Instance.UpdateVolume();
        }
        get
        {
            return uiVolume;
        }
    }

    public SoundsData soundsData;
    public bool logWarnings = false;

    private Dictionary<SoundType, SoundItem> soundDict;
    private Dictionary<SoundType, List<AudioSource>> sourcesDict;
    private static List<AudioSource> tempAudioSources;
    private List<SoundType> sequencesList;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            SoundTypeComparer comparer = new SoundTypeComparer();

            sourcesDict = new Dictionary<SoundType, List<AudioSource>>(comparer);
            soundDict = new Dictionary<SoundType, SoundItem>(comparer);

            tempAudioSources = new List<AudioSource>();
            sequencesList = new List<SoundType>();
                       
            for (int i = 0; i < soundsData.sounds.Length; i++)
            {
                soundDict.Add(soundsData.sounds[i].type, soundsData.sounds[i]);
            }

            Volume = PlayerPrefs.GetFloat("OverallVolume", 1);
            MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1);
            SoundsVolume = PlayerPrefs.GetFloat("SoundsVolume", 1);
            UiVolume = PlayerPrefs.GetFloat("UiVolume", 1);

            DontDestroyOnLoad(this);
        }
    }

    public void UpdateVolume()
    {
        if (sourcesDict != null)
        {
            foreach (var kvp in sourcesDict)
            {
                if (kvp.Value != null && kvp.Value.Count > 0)
                {
                    float vol = GetGroupVolume(soundDict[kvp.Key].group) * soundDict[kvp.Key].volume;

                    for (int i = kvp.Value.Count - 1; i > -1; i--)
                    {
                        kvp.Value[i].volume = vol;
                    }
                }
            }
        }
    }

    float GetGroupVolume(SoundGroup group)
    {
        if (group == SoundGroup.music)
        {
            return overallVolume * musicVolume;
        }
        else if (group == SoundGroup.sound)
        {
            return overallVolume * soundsVolume;
        }
        else if (group == SoundGroup.uI)
        {
            return overallVolume * uiVolume;
        }
        return 0;
    }

    public AudioClip PlaySoundType(SoundType type, float delay = 0)
    {
        if (soundDict != null && soundDict.ContainsKey(type))
        {
            AudioClip clip;
            var soundItem = soundDict[type];

            if (soundItem.isSequence)
            {
                soundItem.startTime = Time.unscaledTime;

                if (!sequencesList.Contains(type))
                {
                    clip = soundItem.clips[0];

                    soundItem.currentClip = 0;
                    soundItem.lenth = clip.length;

                    sequencesList.Add(type);
                }
                else
                {
                    soundItem.currentClip++;
                    if (soundItem.currentClip >= soundItem.clips.Length)
                    {
                        soundItem.currentClip = 0;
                    }
                    clip = soundItem.clips[soundItem.currentClip];
                    soundItem.lenth = clip.length;
                }
            }
            else
            {
                clip = soundItem.clips.RandomItem();
            }

            Play(clip, soundItem, delay);

            return clip;
        }
        if (logWarnings)
        {
            Debug.LogWarning("<color=yellow>SoundManager is not inited.</color>\nChange order", this);
        }
        return null;
    }

    private void Play(AudioClip clip, SoundItem item, float delay = 0)
    {
        if (clip != null)
        {
            if (!sourcesDict.ContainsKey(item.type))
            {
                sourcesDict.Add(item.type, new List<AudioSource>());
            }

            var typeSources = sourcesDict[item.type];

            if (typeSources.Count >= item.maxActive)
            {
                AudioSource source;
                for (int i = typeSources.Count - 1; i > -1; i--)
                {
                    if (!typeSources[i].isPlaying || typeSources[i].clip == null)
                    {
                        source = typeSources[i];
                        typeSources.RemoveAt(i);
                        tempAudioSources.Add(source);
                    }
                }
            }
            if (typeSources.Count < item.maxActive)
            {
                var source = GetIdleSource();
                typeSources.Add(source);

                source.volume = GetGroupVolume(item.group) * item.volume;
                source.clip = clip;
                source.loop = item.isSequence ? false : item.loop;

                delay = Mathf.Abs(delay);
                if (delay == 0)
                {
                    source.Play();
                }
                else
                {
                    source.PlayDelayed(delay);
                }
            }
        }
        else if (logWarnings)
        {
            Debug.LogWarning("<color=yellow>AudioClip is null</color>", this);
        }
    }

    public void PlayOneShot(string _type)
    {
        SoundType type;
        try
        {
            type = (SoundType)System.Enum.Parse(typeof(SoundType), _type);
            PlaySoundType(type);
        }
        catch (System.Exception ex)
        {
            if (logWarnings)
            {
                Debug.LogWarning("<color=yellow>" + ex + "</color>", this);
            }
        }
    }

    public void SwapSounds(SoundType _from, SoundType _to, float time, System.Action onSwaped)
    {
        AudioSource from, to;
        if (!CheckPlayingType(_from))
        {
            PlaySoundType(_from);
        }

        from = sourcesDict[_from][0];
        sourcesDict[_from].RemoveAt(0);

        if (!CheckPlayingType(_to))
        {
            PlaySoundType(_to);
        }

        to = sourcesDict[_to][0];
        sourcesDict[_to].RemoveAt(0);

        StartCoroutine(SwapCoroutine(from, to, _to, time, onSwaped));
    }

    private IEnumerator SwapCoroutine(AudioSource sourceFrom, AudioSource sourceTo, SoundType _to, float time, System.Action onSwaped)
    {
        float remainingTime = time;
        float volume_1 = sourceFrom.volume;
        sourceTo.volume = 0;

        while (remainingTime > 0)
        {
            remainingTime -= Time.unscaledDeltaTime;

            sourceFrom.volume = (remainingTime / time) * volume_1;
            sourceTo.volume = (1 - remainingTime / time) * volume_1;

            yield return null;
        }

        tempAudioSources.Add(sourceFrom);
        sourcesDict[_to].Add(sourceTo);

        if (onSwaped != null)
        {
            onSwaped();
        }
    }

    AudioSource GetIdleSource()
    {
        AudioSource source;
        if (tempAudioSources.Count > 0)
        {
            source = tempAudioSources[0];
            tempAudioSources.RemoveAt(0);
            return source;
        }

        foreach (var kvp in sourcesDict)
        {
            if (kvp.Value != null && kvp.Value.Count > 0)
            {
                for (int i = kvp.Value.Count - 1; i > -1; i--)
                {
                    if ((!kvp.Value[i].isPlaying && !soundDict[kvp.Key].isSequence) || kvp.Value[i].clip == null)
                    {
                        source = kvp.Value[i];
                        kvp.Value.RemoveAt(i);
                        return source;
                    }
                }
            }
        }

        source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        return source;
    }

    public bool CheckPlayingType(SoundType type)
    {
        if (soundDict != null && sourcesDict.ContainsKey(type))
        {
            var typeSources = sourcesDict[type];

            for (int i = 0; i < typeSources.Count; i++)
            {
                if (typeSources[i].isPlaying)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void StopTypes(params SoundType[] types)
    {
        AudioSource source;
        for (int i = 0; i < types.Length; i++)
        {
            if (sourcesDict != null && sourcesDict.ContainsKey(types[i]))
            {
                if (sourcesDict[types[i]] != null && sourcesDict[types[i]].Count > 0)
                {
                    sequencesList.Remove(types[i]);
                    for (int j = sourcesDict[types[i]].Count - 1; j > -1; j--)
                    {
                        source = sourcesDict[types[i]][j];
                        source.Stop();
                        source.clip = null;

                        tempAudioSources.Add(source);
                        sourcesDict[types[i]].RemoveAt(j);
                    }
                }
            }
        }
    }

    public void StopAll()
    {
        AudioSource source;
        foreach (var kvp in sourcesDict)
        {
            if (kvp.Value != null && kvp.Value.Count > 0)
            {
                for (int i = kvp.Value.Count - 1; i > -1; i--)
                {
                    source = kvp.Value[i];
                    source.Stop();
                    source.clip = null;

                    tempAudioSources.Add(source);
                    kvp.Value.RemoveAt(i);
                }
            }
        }

        sequencesList.Clear();
    }

    private void Update()
    {
        if (sequencesList.Count > 0)
        {
            SoundItem item;
            AudioSource source;
            for (int i = sequencesList.Count - 1; i > -1; i--)
            {
                if (soundDict.ContainsKey(sequencesList[i]) && sourcesDict.ContainsKey(sequencesList[i]))
                {
                    item = soundDict[sequencesList[i]];
                    if (sourcesDict[sequencesList[i]].Count > 0)
                    {
                        source = sourcesDict[sequencesList[i]][0];
                        if (!source.isPlaying && Time.unscaledTime >= item.lenth + item.startTime)
                        {
                            source.Stop();
                            tempAudioSources.Add(source);
                            sourcesDict[sequencesList[i]].Clear();

                            if (item.currentClip + 1 == item.clips.Length && !item.loop)
                            {
                                sequencesList.RemoveAt(i);
                            }
                            else
                            {
                                PlaySoundType(sequencesList[i]);
                            }
                        }
                    }
                }
            }
        }
    }
}
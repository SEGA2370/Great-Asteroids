using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[CreateAssetMenu(fileName = "Assets/_Project/Music/Music Clip Groups/MusicClipGroup", menuName = "Music/Music Clip Group", order = 3)]
public class MusicClipGroup : ScriptableObject
{
    [SerializeField] string _groupName;
    [SerializeField] AudioClip[] _audioClips;
    readonly Queue<AudioClip> _trackQueue = new();
    public AudioClip GetNextMusicClip()
    {
        if (_trackQueue.Count == 0) ShuffleTrackQueue();
        var audioClip = _trackQueue.Dequeue();
        return audioClip;
    }
    void ShuffleTrackQueue()
    {
        if (_audioClips == null || _audioClips.Length == 0) return;

        var list = new List<AudioClip>(_audioClips);
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            (list[n], list[k]) = (list[k], list[n]);
        }

        foreach (var clip in list)
            _trackQueue.Enqueue(clip);
    }
}
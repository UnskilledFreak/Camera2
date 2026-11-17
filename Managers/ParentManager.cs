using System.Collections.Generic;
using UnityEngine;

namespace Camera2.Managers;

internal static class ParentManager
{
    private const float MinLookupDelay = 0.01f;
    private const float MaxLookupDelay = 5f;

    private sealed class Entry
    {
        public GameObject Target;
        public float LastLookup = float.NegativeInfinity;
    }

    private static readonly Dictionary<string, Entry> Entries = new();

    public static GameObject Get(string path, float lookupDelay)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        if (!Entries.TryGetValue(path, out var entry))
        {
            entry = new Entry();
            Entries.Add(path, entry);
        }

        if (entry.Target != null && entry.Target.activeInHierarchy)
        {
            return entry.Target;
        }

        if (entry.Target is not null && entry.Target == null)
        {
            entry.Target = null;
            entry.LastLookup = float.NegativeInfinity;
        }

        if (Time.unscaledTime - entry.LastLookup < Mathf.Clamp(lookupDelay, MinLookupDelay, MaxLookupDelay))
        {
            return null;
        }

        entry.LastLookup = Time.unscaledTime;
        entry.Target = GameObject.Find(path);

        return entry.Target;
    }

    public static void Clear() => Entries.Clear();
}
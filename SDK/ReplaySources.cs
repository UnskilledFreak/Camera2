using System.Collections.Generic;
using Camera2.Interfaces;
using JetBrains.Annotations;
using UnityEngine;

namespace Camera2.SDK
{
    public static class ReplaySources
    {
        internal static readonly HashSet<ISource> Sources = [];

        public static void Register(ISource source) => Sources.Add(source);

        public static void Unregister(ISource source) => Sources.Remove(source);

        // this needs to be here because BeatLeader adapted Kinsi55's spaghetti code
        [UsedImplicitly]
        public class GenericSource : ISource
        {
            public string Name { get; }
            public bool IsInReplay { get; private set; }
            public Vector3 LocalHeadPosition { get; private set; }
            public Quaternion LocalHeadRotation { get; private set; }

            // other mods hook in here... ugly but does the c# compiler magic
            [UsedImplicitly]
            public Vector3 localHeadPosition { get => LocalHeadPosition; set => LocalHeadPosition = value; }

            [UsedImplicitly]
            public Quaternion localHeadRotation { get => LocalHeadRotation; set => LocalHeadRotation = value; }

            public GenericSource(string name)
            {
                Name = name;
            }

            [UsedImplicitly]
            public void Update(Vector3 headPosition, Quaternion headRotation)
            {
                LocalHeadPosition = headPosition;
                LocalHeadRotation = headRotation;
            }

            [UsedImplicitly]
            public void SetActive(bool isInReplay)
            {
                IsInReplay = isInReplay;
            }
        }
    }
}
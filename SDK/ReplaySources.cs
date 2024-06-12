using System.Collections.Generic;
using Camera2.Interfaces;
using JetBrains.Annotations;
using UnityEngine;

namespace Camera2.SDK
{
    public static class ReplaySources
    {
        internal static readonly HashSet<ISource> Sources = new HashSet<ISource>();

        public static void Register(ISource source) => Sources.Add(source);

        public static void Unregister(ISource source) => Sources.Remove(source);

        // this needs to be here because BeatLeader adapted Kinsi55's spaghetti code
        // also funny that they used reflections and not the SDK, talking about not working SDK.... yikes
        [UsedImplicitly]
        public class GenericSource : ISource
        {
            public string Name { get; }
            public bool IsInReplay { get; private set; }

            // another things thank to spaghetti... lower case public members urgh
            [UsedImplicitly]
            public Vector3 localHeadPosition { get; private set; }

            // another things thank to spaghetti... lower case public members urgh
            [UsedImplicitly]
            public Quaternion localHeadRotation { get; private set; }

            public GenericSource(string name)
            {
                Name = name;
            }

            [UsedImplicitly]
            public void Update(ref Vector3 localHeadPosition, ref Quaternion localHeadRotation)
            {
                this.localHeadPosition = localHeadPosition;
                this.localHeadRotation = localHeadRotation;
            }

            [UsedImplicitly]
            public void SetActive(bool isInReplay)
            {
                IsInReplay = isInReplay;
            }
        }
    }
}
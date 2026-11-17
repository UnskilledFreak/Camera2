using System.Runtime.CompilerServices;
using IPA.Config.Stores;
using JetBrains.Annotations;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]

namespace Camera2.Configuration;

[UsedImplicitly]
internal class PluginConfig
{
    public static PluginConfig Instance { get; set; }

    public virtual float NoodleParentLookupDelay { get; set; } = 0.1f;
    public virtual float TargetParentLookupDelay { get; set; } = 0.5f;
}
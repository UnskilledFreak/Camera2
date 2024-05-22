using System;

namespace Camera2.Enums
{
    [Flags]
    internal enum VisibilityMasks
    {
        ThirdPersonAvatar = 1 << VisibilityLayers.ThirdPerson,
        FirstPersonAvatar = 1 << VisibilityLayers.FirstPerson,
        Floor = 1 << VisibilityLayers.Floor, // Called "Water" ingame
        UI = 1 << VisibilityLayers.UI,
        Notes = 1 << VisibilityLayers.Notes,
        Debris = 1 << VisibilityLayers.Debris,
        Avatar = 1 << VisibilityLayers.Avatar,
        Walls = 1 << VisibilityLayers.Walls,
        Sabers = 1 << VisibilityLayers.Sabers,
        CutParticles = 1 << VisibilityLayers.CutParticles,
        CustomNotes = 1 << VisibilityLayers.CustomNotes,
        WallTextures = 1 << VisibilityLayers.WallTextures,
        PlayerPlatform = 1 << VisibilityLayers.PlayerPlatform
    }
}
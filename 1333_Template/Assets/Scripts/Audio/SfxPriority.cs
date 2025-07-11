/// <summary>
/// Priority levels for SFX playback arbitration.
/// Higher value means higher importance.
/// </summary>
public enum SfxPriority : byte
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}
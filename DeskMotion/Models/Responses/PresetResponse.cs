namespace DeskMotion.Models.Responses;

public class PresetResponse
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public DeskPreset? Preset { get; set; }

    public static PresetResponse FromSuccess(DeskPreset preset)
    {
        return new PresetResponse
        {
            Success = true,
            Preset = preset
        };
    }

    public static PresetResponse FromSuccess()
    {
        return new PresetResponse
        {
            Success = true
        };
    }

    public static PresetResponse FromError(string error)
    {
        return new PresetResponse
        {
            Success = false,
            Error = error
        };
    }
}

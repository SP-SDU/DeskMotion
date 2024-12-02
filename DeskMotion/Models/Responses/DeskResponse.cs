namespace DeskMotion.Models.Responses
{
    public class DeskResponse
    {
        public bool Success { get; set; }
        public DeskInfo? Desk { get; set; }
        public string? Error { get; set; }

        public static DeskResponse FromSuccess(DeskInfo desk) => new() { Success = true, Desk = desk };
        public static DeskResponse FromError(string error) => new() { Success = false, Error = error };
    }
}

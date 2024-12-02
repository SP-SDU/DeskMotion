using System.Collections.Generic;

namespace DeskMotion.Models.Responses
{
    public class SearchResponse
    {
        public bool Success { get; set; }
        public IEnumerable<DeskInfo>? Results { get; set; }
        public string? Error { get; set; }

        public static SearchResponse FromSuccess(IEnumerable<DeskInfo> results) => new() { Success = true, Results = results };
        public static SearchResponse FromError(string error) => new() { Success = false, Error = error };
    }
}

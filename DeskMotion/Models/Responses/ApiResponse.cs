namespace DeskMotion.Models.Responses
{
    /// <summary>
    /// Generic API response wrapper for consistent response format
    /// </summary>
    /// <typeparam name="T">Type of the response data</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Whether the operation was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Optional error message if operation failed
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Optional response data
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Creates a successful response with data
        /// </summary>
        public static ApiResponse<T> Ok(T data) => new() { Success = true, Data = data };

        /// <summary>
        /// Creates a successful response without data
        /// </summary>
        public static ApiResponse<T> Ok() => new() { Success = true };

        /// <summary>
        /// Creates an error response with message
        /// </summary>
        public static ApiResponse<T> Error(string message) => new() { Success = false, Message = message };
    }
}

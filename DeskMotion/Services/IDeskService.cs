using DeskMotion.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DeskMotion.Services
{
    public interface IDeskService
    {
        /// <summary>
        /// Moves the desk up by 5cm.
        /// </summary>
        /// <param name="deskId">The ID of the desk to move.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>True if successful, false if desk not found.</returns>
        Task<bool> MoveUpAsync(Guid deskId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Moves the desk down by 5cm.
        /// </summary>
        /// <param name="deskId">The ID of the desk to move.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>True if successful, false if desk not found.</returns>
        Task<bool> MoveDownAsync(Guid deskId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Stops the desk movement.
        /// </summary>
        /// <param name="deskId">The ID of the desk to stop.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>True if successful, false if desk not found.</returns>
        Task<bool> StopMovementAsync(Guid deskId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets information about a specific desk.
        /// </summary>
        /// <param name="deskId">The ID of the desk to query.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>Desk information if found, null otherwise.</returns>
        Task<DeskInfo?> GetDeskInfoAsync(Guid deskId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets information about all desks.
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>Collection of desk information.</returns>
        Task<IEnumerable<DeskInfo>> GetAllDesksAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Searches for desks matching the query.
        /// </summary>
        /// <param name="query">The search query.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>Collection of matching desk information.</returns>
        Task<IEnumerable<DeskInfo>> SearchDesksAsync(string query, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets the desk height to a specific value.
        /// </summary>
        /// <param name="deskId">The ID of the desk to adjust.</param>
        /// <param name="targetHeight">The target height in centimeters.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>True if successful, false if desk not found or height invalid.</returns>
        Task<bool> SetDeskHeightAsync(Guid deskId, double targetHeight, CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves a preset height for a desk.
        /// </summary>
        /// <param name="deskId">The ID of the desk.</param>
        /// <param name="name">The name of the preset.</param>
        /// <param name="height">The height in centimeters.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The created preset.</returns>
        Task<DeskPreset> SavePresetAsync(Guid deskId, string name, double height, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a preset height for a desk.
        /// </summary>
        /// <param name="deskId">The ID of the desk.</param>
        /// <param name="presetName">The name of the preset to delete.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>True if successful, false if preset not found.</returns>
        Task<bool> DeletePresetAsync(Guid deskId, string presetName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Moves the desk to a saved preset height.
        /// </summary>
        /// <param name="deskId">The ID of the desk.</param>
        /// <param name="presetName">The name of the preset to move to.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>True if successful, false if preset not found.</returns>
        Task<bool> MoveToPresetAsync(Guid deskId, string presetName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adjusts the desk height to a target value.
        /// </summary>
        /// <param name="deskId">The ID of the desk.</param>
        /// <param name="targetHeight">The target height in centimeters.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>True if successful, false if height invalid.</returns>
        Task<bool> AdjustHeightAsync(Guid deskId, double targetHeight, CancellationToken cancellationToken = default);

        /// <summary>
        /// Records the start of desk usage by a user.
        /// </summary>
        /// <param name="deskId">The ID of the desk.</param>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        Task RecordDeskUsageStartAsync(Guid deskId, Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Records the end of desk usage by a user.
        /// </summary>
        /// <param name="deskId">The ID of the desk.</param>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        Task RecordDeskUsageEndAsync(Guid deskId, Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Records a height adjustment for a desk.
        /// </summary>
        /// <param name="deskId">The ID of the desk.</param>
        /// <param name="oldHeight">The previous height.</param>
        /// <param name="newHeight">The new height.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        Task RecordHeightAdjustmentAsync(Guid deskId, double oldHeight, double newHeight, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets usage statistics for a desk within a date range.
        /// </summary>
        /// <param name="deskId">The ID of the desk.</param>
        /// <param name="startDate">The start date of the range.</param>
        /// <param name="endDate">The end date of the range.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>Usage statistics for the desk.</returns>
        Task<DeskUsageStatistics> GetDeskUsageStatisticsAsync(Guid deskId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets user usage statistics for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="startDate">The start date for the statistics.</param>
        /// <param name="endDate">The end date for the statistics.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>User usage statistics.</returns>
        Task<UserUsageStatistics> GetUserUsageStatisticsAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the desk information.
        /// </summary>
        /// <param name="desk">The desk information to update.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>True if successful, false if desk not found or update failed.</returns>
        Task<bool> UpdateDeskAsync(DeskInfo desk, CancellationToken cancellationToken = default);
    }
}

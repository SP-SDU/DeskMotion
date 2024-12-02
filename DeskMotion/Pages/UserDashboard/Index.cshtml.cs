using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DeskMotion.Services;
using DeskMotion.Models;
using DeskMotion.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using DeskMotion.Models.Requests;

namespace DeskMotion.Pages.UserDashboard
{
    // Temporarily commented out for development
    [Authorize]
    [IgnoreAntiforgeryToken]
    public class Index : PageModel
    {
        private readonly IDeskService _deskService;
        private readonly ILogger<Index> _logger;

        public Index(
            IDeskService deskService,
            ILogger<Index> logger)
        {
            _deskService = deskService;
            _logger = logger;
        }

        [BindProperty]
        public string? SearchQuery { get; set; }

        [BindProperty(SupportsGet = true)]
        public Guid? DeskId { get; set; }

        public List<DeskInfo>? Desks { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Loading desks...");
                Desks = (await _deskService.GetAllDesksAsync(cancellationToken)).ToList();
                _logger.LogInformation("Loaded {DeskCount} desks.", Desks.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading desks");
                ErrorMessage = ex.Message;
            }
        }

        public async Task<IActionResult> OnGetGetAllDesksAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting all desks...");
                var desks = await _deskService.GetAllDesksAsync(cancellationToken);
                _logger.LogInformation("Retrieved {Count} desks", desks.Count());
                
                return new JsonResult(new
                {
                    success = true,
                    desks = desks.Select(d => new
                    {
                        d.Id,
                        d.Name,
                        d.CurrentHeight,
                        d.MinHeight,
                        d.MaxHeight,
                        d.IsMoving,
                        d.Status,
                        d.LastUpdated,
                        d.IsAssigned,
                        d.AssignedUserId,
                        d.AssignedUserName,
                        d.AssignmentStart
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all desks");
                return new JsonResult(new { success = false, error = ex.Message });
            }
        }

        public async Task<IActionResult> OnGetDeskUsageStatisticsAsync(Guid deskId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            try
            {
                var statistics = await _deskService.GetDeskUsageStatisticsAsync(deskId, startDate, endDate, cancellationToken);
                if (statistics == null)
                {
                    return new JsonResult(StatisticsResponse.FromError("Failed to retrieve desk usage statistics"));
                }
                return new JsonResult(StatisticsResponse.FromSuccess(statistics));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting desk usage statistics for desk {DeskId}", deskId);
                return new JsonResult(StatisticsResponse.FromError(ex.Message));
            }
        }

        public async Task<IActionResult> OnPostRecordDeskUsageStartAsync(Guid deskId, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("User not authenticated"));
                await _deskService.RecordDeskUsageStartAsync(deskId, userId, cancellationToken);
                return new JsonResult(UsageRecordResponse.FromSuccess());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording desk usage start for desk {DeskId}", deskId);
                return new JsonResult(UsageRecordResponse.FromError(ex.Message));
            }
        }

        public async Task<IActionResult> OnPostRecordDeskUsageEndAsync(Guid deskId, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("User not authenticated"));
                await _deskService.RecordDeskUsageEndAsync(deskId, userId, cancellationToken);
                return new JsonResult(UsageRecordResponse.FromSuccess());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording desk usage end for desk {DeskId}", deskId);
                return new JsonResult(UsageRecordResponse.FromError(ex.Message));
            }
        }

        public async Task<IActionResult> OnPostRecordHeightAdjustmentAsync(Guid deskId, int oldHeight, int newHeight, CancellationToken cancellationToken = default)
        {
            try
            {
                await _deskService.RecordHeightAdjustmentAsync(deskId, oldHeight, newHeight, cancellationToken);
                return new JsonResult(UsageRecordResponse.FromSuccess());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording height adjustment for desk {DeskId}", deskId);
                return new JsonResult(UsageRecordResponse.FromError(ex.Message));
            }
        }

        public async Task<IActionResult> OnPostSearchAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SearchQuery))
                {
                    var allDesks = (await _deskService.GetAllDesksAsync(cancellationToken)).ToList();
                    if (allDesks == null)
                    {
                        return new JsonResult(SearchResponse.FromError("Failed to retrieve desks"));
                    }
                    return new JsonResult(SearchResponse.FromSuccess(allDesks));
                }

                var results = await _deskService.SearchDesksAsync(SearchQuery, cancellationToken);
                if (results == null)
                {
                    return new JsonResult(SearchResponse.FromError("Failed to search desks"));
                }
                return new JsonResult(SearchResponse.FromSuccess(results));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching desks: {ErrorMessage}", ex.Message);
                return new JsonResult(SearchResponse.FromError($"Failed to search desks: {ex.Message}"));
            }
        }

        public async Task<IActionResult> OnPostMoveUpAsync(Guid deskId, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _deskService.MoveUpAsync(deskId, cancellationToken);
                if (!result)
                {
                    return new JsonResult(DeskResponse.FromError("Failed to move desk up"));
                }

                var desk = await _deskService.GetDeskInfoAsync(deskId, cancellationToken);
                if (desk == null)
                {
                    return new JsonResult(DeskResponse.FromError("Desk not found"));
                }
                return new JsonResult(DeskResponse.FromSuccess(desk));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moving desk up");
                return new JsonResult(DeskResponse.FromError(ex.Message));
            }
        }

        public async Task<IActionResult> OnPostMoveDownAsync(Guid deskId, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _deskService.MoveDownAsync(deskId, cancellationToken);
                if (!result)
                {
                    return new JsonResult(DeskResponse.FromError("Failed to move desk down"));
                }

                var desk = await _deskService.GetDeskInfoAsync(deskId, cancellationToken);
                if (desk == null)
                {
                    return new JsonResult(DeskResponse.FromError("Desk not found"));
                }
                return new JsonResult(DeskResponse.FromSuccess(desk));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moving desk down");
                return new JsonResult(DeskResponse.FromError(ex.Message));
            }
        }

        public async Task<IActionResult> OnPostStopMovementAsync(Guid deskId, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _deskService.StopMovementAsync(deskId, cancellationToken);
                if (!result)
                {
                    return new JsonResult(DeskResponse.FromError("Failed to stop desk movement"));
                }

                var desk = await _deskService.GetDeskInfoAsync(deskId, cancellationToken);
                if (desk == null)
                {
                    return new JsonResult(DeskResponse.FromError("Desk not found"));
                }
                return new JsonResult(DeskResponse.FromSuccess(desk));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping desk movement");
                return new JsonResult(DeskResponse.FromError(ex.Message));
            }
        }

        public async Task<IActionResult> OnGetDeskStatusAsync(Guid deskId, CancellationToken cancellationToken = default)
        {
            try
            {
                var desk = await _deskService.GetDeskInfoAsync(deskId, cancellationToken);
                if (desk == null)
                {
                    return new JsonResult(DeskResponse.FromError("Desk not found"));
                }
                return new JsonResult(DeskResponse.FromSuccess(desk));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting desk status for desk {DeskId}", deskId);
                return new JsonResult(DeskResponse.FromError(ex.Message));
            }
        }

        public async Task<IActionResult> OnGetCurrentHeightAsync(Guid deskId, CancellationToken cancellationToken = default)
        {
            try
            {
                var desk = await _deskService.GetDeskInfoAsync(deskId, cancellationToken);
                if (desk == null)
                {
                    return new JsonResult(DeskResponse.FromError("Desk not found"));
                }
                return new JsonResult(DeskResponse.FromSuccess(desk));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current height for desk {DeskId}", deskId);
                return new JsonResult(DeskResponse.FromError(ex.Message));
            }
        }

        public async Task<IActionResult> OnPostAdjustHeightAsync(Guid deskId, int targetHeight, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Adjusting height for desk {DeskId} to {TargetHeight}cm", deskId, targetHeight);

            // Validate height range
            if (targetHeight < 60 || targetHeight > 120)
            {
                _logger.LogWarning("Invalid height {Height}cm requested for desk {DeskId}", targetHeight, deskId);
                return new JsonResult(DeskResponse.FromError("Target height must be between 60 and 120 cm"));
            }

            try
            {
                // Attempt to adjust height
                var success = await _deskService.AdjustHeightAsync(deskId, targetHeight, cancellationToken);
                if (!success)
                {
                    _logger.LogWarning("Failed to adjust height for desk {DeskId}", deskId);
                    return new JsonResult(DeskResponse.FromError("Failed to adjust desk height"));
                }

                // Get updated desk info
                var deskInfo = await _deskService.GetDeskInfoAsync(deskId, cancellationToken);
                if (deskInfo == null)
                {
                    _logger.LogError("Desk {DeskId} not found after height adjustment", deskId);
                    return new JsonResult(DeskResponse.FromError("Failed to get desk status"));
                }

                _logger.LogInformation("Successfully adjusted height for desk {DeskId} to {Height}cm", deskId, deskInfo.CurrentHeight);
                return new JsonResult(DeskResponse.FromSuccess(deskInfo));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adjusting height for desk {DeskId}", deskId);
                return new JsonResult(DeskResponse.FromError("Failed to adjust desk height"));
            }
        }

        public async Task<IActionResult> OnPostSavePresetAsync(Guid deskId, string presetName, int height, CancellationToken cancellationToken = default)
        {
            try
            {
                if (height < 60 || height > 120)
                {
                    return new JsonResult(PresetResponse.FromError("Height must be between 60 and 120 cm"));
                }

                var preset = await _deskService.SavePresetAsync(deskId, presetName, height, cancellationToken);
                return new JsonResult(PresetResponse.FromSuccess(preset));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving preset: {Message}", ex.Message);
                return new JsonResult(PresetResponse.FromError(ex.Message));
            }
        }

        public async Task<IActionResult> OnPostMoveToPresetAsync(Guid deskId, string presetName, CancellationToken cancellationToken = default)
        {
            try
            {
                var success = await _deskService.MoveToPresetAsync(deskId, presetName, cancellationToken);
                if (!success)
                {
                    return new JsonResult(DeskResponse.FromError("Failed to move desk to preset position"));
                }

                var desk = await _deskService.GetDeskInfoAsync(deskId, cancellationToken);
                if (desk == null)
                {
                    return new JsonResult(DeskResponse.FromError("Desk not found"));
                }
                return new JsonResult(DeskResponse.FromSuccess(desk));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moving to preset: {Message}", ex.Message);
                return new JsonResult(DeskResponse.FromError(ex.Message));
            }
        }

        public async Task<IActionResult> OnPostDeletePresetAsync(Guid deskId, string presetName, CancellationToken cancellationToken = default)
        {
            try
            {
                var success = await _deskService.DeletePresetAsync(deskId, presetName, cancellationToken);
                if (!success)
                {
                    return new JsonResult(PresetResponse.FromError("Failed to delete preset"));
                }

                return new JsonResult(PresetResponse.FromSuccess());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting preset: {Message}", ex.Message);
                return new JsonResult(PresetResponse.FromError(ex.Message));
            }
        }

        public async Task<IActionResult> OnPostAssignDeskAsync([FromBody] AssignDeskRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("AssignDeskAsync called with DeskId: {DeskId}", request?.DeskId);

                if (request?.DeskId == null)
                {
                    _logger.LogWarning("AssignDeskAsync failed: Desk ID is null");
                    return new JsonResult(DeskResponse.FromError("Desk ID is required"));
                }

                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("User not authenticated"));
                var userName = User.FindFirstValue(ClaimTypes.Name) ?? "Unknown User";
                _logger.LogInformation("User attempting to assign desk - UserId: {UserId}, UserName: {UserName}", userId, userName);

                var desk = await _deskService.GetDeskInfoAsync(request.DeskId.Value, cancellationToken);
                if (desk == null)
                {
                    _logger.LogWarning("AssignDeskAsync failed: Desk not found with ID: {DeskId}", request.DeskId);
                    return new JsonResult(DeskResponse.FromError("Desk not found"));
                }

                _logger.LogInformation("Found desk - Name: {DeskName}, IsAssigned: {IsAssigned}", desk.Name, desk.IsAssigned);

                if (desk.IsAssigned)
                {
                    _logger.LogWarning("AssignDeskAsync failed: Desk {DeskId} is already assigned to user {AssignedUser}", 
                        desk.Id, desk.AssignedUserName);
                    return new JsonResult(DeskResponse.FromError("Desk is already assigned"));
                }

                desk.IsAssigned = true;
                desk.AssignedUserId = userId;
                desk.AssignedUserName = userName;
                desk.AssignmentStart = DateTime.UtcNow;
                desk.IsActive = true;

                var success = await _deskService.UpdateDeskAsync(desk, cancellationToken);
                if (!success)
                {
                    _logger.LogError("AssignDeskAsync failed: Unable to update desk {DeskId}", desk.Id);
                    return new JsonResult(DeskResponse.FromError("Failed to update desk"));
                }

                _logger.LogInformation("Successfully assigned desk {DeskId} to user {UserId}", desk.Id, userId);
                return new JsonResult(DeskResponse.FromSuccess(desk));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning desk: {Message}", ex.Message);
                return new JsonResult(DeskResponse.FromError(ex.Message));
            }
        }

        public async Task<IActionResult> OnPostUnassignDeskAsync([FromBody] UnassignDeskRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                if (request?.DeskId == null)
                {
                    return new JsonResult(DeskResponse.FromError("Desk ID is required"));
                }

                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("User not authenticated"));
                var desk = await _deskService.GetDeskInfoAsync(request.DeskId.Value, cancellationToken);
                
                if (desk == null)
                {
                    return new JsonResult(DeskResponse.FromError("Desk not found"));
                }

                if (!desk.IsAssigned)
                {
                    return new JsonResult(DeskResponse.FromError("Desk is not assigned"));
                }

                if (desk.AssignedUserId != userId)
                {
                    return new JsonResult(DeskResponse.FromError("You can only unassign your own desk"));
                }

                desk.IsAssigned = false;
                desk.AssignedUserId = null;
                desk.AssignedUserName = null;
                desk.AssignmentEnd = DateTime.UtcNow;
                desk.IsActive = false;

                var success = await _deskService.UpdateDeskAsync(desk, cancellationToken);
                if (!success)
                {
                    return new JsonResult(DeskResponse.FromError("Failed to unassign desk"));
                }

                return new JsonResult(DeskResponse.FromSuccess(desk));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unassigning desk {DeskId}", request?.DeskId);
                return new JsonResult(DeskResponse.FromError(ex.Message));
            }
        }
    }
}

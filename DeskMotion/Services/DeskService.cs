using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DeskMotion.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DeskMotion.Data;

namespace DeskMotion.Services
{
    public class DeskService : IDeskService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DeskService> _logger;

        public DeskService(ApplicationDbContext context, ILogger<DeskService> logger)
        {
            _context = context;
            _logger = logger;

            // Ensure desks exist in database
            EnsureDesksExist().Wait();
        }

        private async Task EnsureDesksExist()
        {
            if (!await _context.DeskInfos.AnyAsync())
            {
                var desk1Id = Guid.Parse("dd78459c-fa77-4c35-ba20-cb7596fcd86b");
                var desk2Id = Guid.Parse("36905219-7f1b-4483-8bcf-4f9f43dd74f4");

                _context.DeskInfos.AddRange(
                    new DeskInfo
                    {
                        Id = desk1Id,
                        Name = "Standing Desk 1",
                        CurrentHeight = 75,
                        MinHeight = 60,
                        MaxHeight = 120,
                        IsMoving = false,
                        Status = "Available",
                        LastUpdated = DateTime.UtcNow
                    },
                    new DeskInfo
                    {
                        Id = desk2Id,
                        Name = "Standing Desk 2",
                        CurrentHeight = 85,
                        MinHeight = 60,
                        MaxHeight = 120,
                        IsMoving = false,
                        Status = "Available",
                        LastUpdated = DateTime.UtcNow
                    }
                );

                await _context.SaveChangesAsync();
                _logger.LogInformation("Created initial desk records");
            }
        }

        public async Task<IEnumerable<DeskInfo>> GetAllDesksAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting all desks");
            return await _context.DeskInfos
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<DeskInfo?> GetDeskInfoAsync(Guid deskId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting desk info for {DeskId}", deskId);
            return await _context.DeskInfos.FindAsync(new object[] { deskId }, cancellationToken);
        }

        public async Task<bool> UpdateDeskAsync(DeskInfo desk, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Updating desk {DeskId}. IsAssigned: {IsAssigned}, AssignedUserId: {AssignedUserId}", 
                    desk.Id, desk.IsAssigned, desk.AssignedUserId);

                var existingDesk = await _context.DeskInfos.FindAsync(new object[] { desk.Id }, cancellationToken);
                if (existingDesk == null)
                {
                    _logger.LogWarning("Desk not found in database: {DeskId}", desk.Id);
                    return false;
                }

                // Update all properties
                existingDesk.Name = desk.Name;
                existingDesk.IsAssigned = desk.IsAssigned;
                existingDesk.AssignedUserId = desk.AssignedUserId;
                existingDesk.AssignedUserName = desk.AssignedUserName;
                existingDesk.AssignmentStart = desk.AssignmentStart;
                existingDesk.AssignmentEnd = desk.AssignmentEnd;
                existingDesk.IsActive = desk.IsActive;
                existingDesk.CurrentHeight = desk.CurrentHeight;
                existingDesk.IsMoving = desk.IsMoving;
                existingDesk.Status = desk.Status;
                existingDesk.LastUpdated = DateTime.UtcNow;

                // Save changes to database
                await _context.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation("Successfully updated desk {DeskId} in database", desk.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating desk {DeskId}", desk.Id);
                return false;
            }
        }

        public async Task<bool> UpdateDeskHeightAsync(Guid deskId, double newHeight, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Updating height for desk {DeskId} to {NewHeight}", deskId, newHeight);

                var desk = await _context.DeskInfos.FindAsync(new object[] { deskId }, cancellationToken);
                if (desk == null)
                {
                    _logger.LogWarning("Desk not found: {DeskId}", deskId);
                    return false;
                }

                desk.CurrentHeight = newHeight;
                desk.LastUpdated = DateTime.UtcNow;

                await _context.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation("Successfully updated height for desk {DeskId}", deskId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating height for desk {DeskId}", deskId);
                return false;
            }
        }

        public async Task<bool> UpdateDeskMovementStatusAsync(Guid deskId, bool isMoving, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Updating movement status for desk {DeskId} to {IsMoving}", deskId, isMoving);

                var desk = await _context.DeskInfos.FindAsync(new object[] { deskId }, cancellationToken);
                if (desk == null)
                {
                    _logger.LogWarning("Desk not found: {DeskId}", deskId);
                    return false;
                }

                desk.IsMoving = isMoving;
                desk.LastUpdated = DateTime.UtcNow;

                await _context.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation("Successfully updated movement status for desk {DeskId}", deskId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating movement status for desk {DeskId}", deskId);
                return false;
            }
        }

        public async Task<bool> MoveUpAsync(Guid deskId, CancellationToken cancellationToken = default)
        {
            var desk = await _context.DeskInfos.FindAsync(new object[] { deskId }, cancellationToken);
            if (desk == null)
            {
                _logger.LogWarning("Desk not found: {DeskId}", deskId);
                return false;
            }

            if (desk.CurrentHeight < desk.MaxHeight)
            {
                var oldHeight = desk.CurrentHeight ?? desk.MinHeight;
                desk.CurrentHeight = Math.Min(oldHeight + 5, desk.MaxHeight);
                desk.IsMoving = true;
                desk.LastUpdated = DateTime.UtcNow;

                // Record height adjustment
                await RecordHeightAdjustmentAsync(deskId, oldHeight, desk.CurrentHeight.Value, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
            return false;
        }

        public async Task<bool> MoveDownAsync(Guid deskId, CancellationToken cancellationToken = default)
        {
            var desk = await _context.DeskInfos.FindAsync(new object[] { deskId }, cancellationToken);
            if (desk == null)
            {
                _logger.LogWarning("Desk not found: {DeskId}", deskId);
                return false;
            }

            if (desk.CurrentHeight > desk.MinHeight)
            {
                var oldHeight = desk.CurrentHeight ?? desk.MaxHeight;
                desk.CurrentHeight = Math.Max(oldHeight - 5, desk.MinHeight);
                desk.IsMoving = true;
                desk.LastUpdated = DateTime.UtcNow;

                // Record height adjustment
                await RecordHeightAdjustmentAsync(deskId, oldHeight, desk.CurrentHeight.Value, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
            return false;
        }

        public async Task<bool> StopMovementAsync(Guid deskId, CancellationToken cancellationToken = default)
        {
            var desk = await _context.DeskInfos.FindAsync(new object[] { deskId }, cancellationToken);
            if (desk == null)
            {
                _logger.LogWarning("Desk not found: {DeskId}", deskId);
                return false;
            }

            desk.IsMoving = false;
            desk.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<IEnumerable<DeskInfo>> SearchDesksAsync(string query, CancellationToken cancellationToken = default)
        {
            return await _context.DeskInfos
                .AsNoTracking()
                .Where(d =>
                    d.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    d.Id.ToString().Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> SetDeskHeightAsync(Guid deskId, double targetHeight, CancellationToken cancellationToken = default)
        {
            var desk = await _context.DeskInfos.FindAsync(new object[] { deskId }, cancellationToken);
            if (desk == null)
            {
                _logger.LogWarning("Desk not found: {DeskId}", deskId);
                return false;
            }

            if (targetHeight >= desk.MinHeight && targetHeight <= desk.MaxHeight)
            {
                var oldHeight = desk.CurrentHeight ?? desk.MinHeight;
                desk.CurrentHeight = targetHeight;
                desk.LastUpdated = DateTime.UtcNow;

                // Record height adjustment
                await RecordHeightAdjustmentAsync(deskId, oldHeight, targetHeight, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
            return false;
        }

        public async Task<DeskPreset> SavePresetAsync(Guid deskId, string name, double height, CancellationToken cancellationToken = default)
        {
            var preset = new DeskPreset { DeskId = deskId.ToString(), Name = name, Height = height };
            _context.DeskPresets.Add(preset);
            await _context.SaveChangesAsync(cancellationToken);
            return preset;
        }

        public async Task<bool> DeletePresetAsync(Guid deskId, string presetName, CancellationToken cancellationToken = default)
        {
            var preset = await _context.DeskPresets
                .FirstOrDefaultAsync(p => p.DeskId == deskId.ToString() && p.Name == presetName, cancellationToken);
            if (preset != null)
            {
                _context.DeskPresets.Remove(preset);
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
            return false;
        }

        public async Task<bool> MoveToPresetAsync(Guid deskId, string presetName, CancellationToken cancellationToken = default)
        {
            var preset = await _context.DeskPresets
                .FirstOrDefaultAsync(p => p.DeskId == deskId.ToString() && p.Name == presetName, cancellationToken);
            if (preset != null)
            {
                return await SetDeskHeightAsync(deskId, preset.Height, cancellationToken);
            }
            return false;
        }

        public async Task<bool> AdjustHeightAsync(Guid deskId, double targetHeight, CancellationToken cancellationToken = default)
            => await SetDeskHeightAsync(deskId, targetHeight, cancellationToken);

        public async Task RecordDeskUsageStartAsync(Guid deskId, Guid userId, CancellationToken cancellationToken = default)
        {
            var session = new UsageSession
            {
                UserId = userId,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1)
            };
            _context.UsageSessions.Add(session);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task RecordDeskUsageEndAsync(Guid deskId, Guid userId, CancellationToken cancellationToken = default)
        {
            var session = await _context.UsageSessions
                .FirstOrDefaultAsync(s => s.UserId == userId && s.EndTime > DateTime.UtcNow, cancellationToken);
            if (session != null)
            {
                session.EndTime = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task RecordHeightAdjustmentAsync(Guid deskId, double oldHeight, double newHeight, CancellationToken cancellationToken = default)
        {
            var adjustment = new HeightAdjustment
            {
                Id = Guid.NewGuid(),
                DeskId = deskId,
                Timestamp = DateTime.UtcNow,
                OldHeight = oldHeight,
                NewHeight = newHeight
            };
            _context.HeightAdjustments.Add(adjustment);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<DeskUsageStatistics> GetDeskUsageStatisticsAsync(Guid deskId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            var desk = await _context.DeskInfos.FindAsync(new object[] { deskId }, cancellationToken);
            if (desk == null)
            {
                return new DeskUsageStatistics
                {
                    DeskId = deskId.ToString(),
                    TotalUsageTime = TimeSpan.Zero,
                    NumberOfAdjustments = new int[24],
                    UsageSessions = new List<UsageSession>()
                };
            }

            var sessions = await _context.UsageSessions
                .Where(s => s.StartTime >= startDate && s.EndTime <= endDate)
                .ToListAsync(cancellationToken);

            var adjustmentsPerHour = new int[24];
            var adjustments = await _context.HeightAdjustments
                .Where(a => a.DeskId == deskId && a.Timestamp >= startDate && a.Timestamp <= endDate)
                .ToListAsync(cancellationToken);
            foreach (var adjustment in adjustments)
            {
                adjustmentsPerHour[adjustment.Timestamp.Hour]++;
            }

            return new DeskUsageStatistics
            {
                DeskId = deskId.ToString(),
                TotalUsageTime = TimeSpan.FromHours(sessions.Sum(s => (s.EndTime - s.StartTime).TotalHours)),
                NumberOfAdjustments = adjustmentsPerHour,
                UsageSessions = sessions
            };
        }

        public async Task<UserUsageStatistics> GetUserUsageStatisticsAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            var sessions = await _context.UsageSessions
                .Where(s => s.UserId == userId && s.StartTime >= startDate && s.EndTime <= endDate)
                .ToListAsync(cancellationToken);

            return new UserUsageStatistics
            {
                UserId = userId,
                TotalUsageTime = TimeSpan.FromHours(sessions.Sum(s => (s.EndTime - s.StartTime).TotalHours)),
                NumberOfAdjustments = 0,
                DeskUsageSessions = sessions.Select(s => new DeskUsageSession
                {
                    DeskId = s.DeskId.ToString(),
                    StartTime = s.StartTime,
                    EndTime = s.EndTime
                }).ToList()
            };
        }
    }
}

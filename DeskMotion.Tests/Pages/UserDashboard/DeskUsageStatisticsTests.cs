using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using DeskMotion.Models;
using DeskMotion.Models.Responses;
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using System.Collections.Generic;

namespace DeskMotion.Tests.Pages.UserDashboard
{
    public class DeskUsageStatisticsTests : UserDashboardTestBase
    {
        [Fact]
        public async Task GetDeskUsageStatistics_ValidDeskId_ReturnsStatistics()
        {
            // Arrange
            var deskId = "DESK001";
            var startDate = DateTime.UtcNow.AddDays(-7);
            var endDate = DateTime.UtcNow;

            var expectedStats = new DeskUsageStatistics
            {
                DeskId = deskId,
                TotalUsageTime = TimeSpan.FromHours(40),
                NumberOfAdjustments = 15,
                UsageSessions = new List<UsageSession>
                {
                    new UsageSession
                    {
                        UserId = Guid.NewGuid(),
                        StartTime = startDate.AddHours(1),
                        EndTime = startDate.AddHours(9)
                    }
                }
            };

            _deskServiceMock.Setup(s => s.GetDeskUsageStatisticsAsync(
                deskId,
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedStats);

            // Act
            var result = await _pageModel.OnGetDeskUsageStatisticsAsync(deskId, startDate, endDate) as JsonResult;

            // Assert
            result.Should().NotBeNull();
            var response = result!.Value.Should().BeOfType<StatisticsResponse>().Subject;
            response.Success.Should().BeTrue();
            response.Statistics.Should().BeEquivalentTo(expectedStats);
        }

        [Fact]
        public async Task GetDeskUsageStatistics_InvalidDeskId_ReturnsError()
        {
            // Arrange
            var deskId = "INVALID";
            var startDate = DateTime.UtcNow.AddDays(-7);
            var endDate = DateTime.UtcNow;

            _deskServiceMock.Setup(s => s.GetDeskUsageStatisticsAsync(
                deskId,
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Desk not found"));

            // Act
            var result = await _pageModel.OnGetDeskUsageStatisticsAsync(deskId, startDate, endDate) as JsonResult;

            // Assert
            result.Should().NotBeNull();
            var response = result!.Value.Should().BeOfType<StatisticsResponse>().Subject;
            response.Success.Should().BeFalse();
            response.Error.Should().Be("Desk not found");
        }

        [Fact]
        public async Task RecordDeskUsage_ValidData_RecordsSuccessfully()
        {
            // Arrange
            var deskId = "DESK001";
            var userId = Guid.NewGuid();

            _deskServiceMock.Setup(s => s.RecordDeskUsageStartAsync(
                deskId,
                userId,
                It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _pageModel.OnPostRecordDeskUsageStartAsync(deskId) as JsonResult;

            // Assert
            result.Should().NotBeNull();
            var response = result!.Value.Should().BeOfType<UsageRecordResponse>().Subject;
            response.Success.Should().BeTrue();

            _deskServiceMock.Verify(s => s.RecordDeskUsageStartAsync(
                deskId,
                userId,
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RecordHeightAdjustment_ValidData_RecordsSuccessfully()
        {
            // Arrange
            var deskId = "DESK001";
            var oldHeight = 75;
            var newHeight = 85;

            _deskServiceMock.Setup(s => s.RecordHeightAdjustmentAsync(
                deskId,
                oldHeight,
                newHeight,
                It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _pageModel.OnPostRecordHeightAdjustmentAsync(deskId, oldHeight, newHeight) as JsonResult;

            // Assert
            result.Should().NotBeNull();
            var response = result!.Value.Should().BeOfType<UsageRecordResponse>().Subject;
            response.Success.Should().BeTrue();

            _deskServiceMock.Verify(s => s.RecordHeightAdjustmentAsync(
                deskId,
                oldHeight,
                newHeight,
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

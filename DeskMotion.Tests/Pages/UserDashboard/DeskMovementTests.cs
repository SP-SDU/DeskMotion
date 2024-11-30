using Xunit;
using Moq;
using Moq.Language.Flow;
using Microsoft.AspNetCore.Mvc;
using DeskMotion.Models;
using DeskMotion.Models.Responses;
using System.Threading.Tasks;
using System;
using System.Threading;
using FluentAssertions;

namespace DeskMotion.Tests.Pages.UserDashboard
{
    public class DeskMovementTests : UserDashboardTestBase
    {
        [Fact]
        public async Task OnPostMoveUpAsync_ValidDeskId_ReturnsSuccessResponse()
        {
            // Arrange
            var deskId = "DESK001";
            var expectedDesk = new DeskInfo { DeskId = deskId, CurrentHeight = 80, IsMoving = true };
            _deskServiceMock.Setup(s => s.MoveUpAsync(deskId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            SetupMockForSuccess(deskId, expectedDesk);

            // Act
            var result = await _pageModel.OnPostMoveUpAsync(deskId, CancellationToken.None) as JsonResult;

            // Assert
            AssertSuccessResponse(result, expectedDesk);
            _deskServiceMock.Verify(s => s.MoveUpAsync(deskId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task OnPostMoveUpAsync_InvalidDeskId_ReturnsErrorResponse()
        {
            // Arrange
            var deskId = "INVALID";
            var expectedError = "Failed to move desk up";
            _deskServiceMock.Setup(s => s.MoveUpAsync(deskId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            // Act
            var result = await _pageModel.OnPostMoveUpAsync(deskId, CancellationToken.None) as JsonResult;

            // Assert
            AssertErrorResponse(result, expectedError);
            _deskServiceMock.Verify(s => s.MoveUpAsync(deskId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task OnPostMoveDownAsync_ValidDeskId_ReturnsSuccessResponse()
        {
            // Arrange
            var deskId = "DESK001";
            var expectedDesk = new DeskInfo { DeskId = deskId, CurrentHeight = 70, IsMoving = true };
            _deskServiceMock.Setup(s => s.MoveDownAsync(deskId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            SetupMockForSuccess(deskId, expectedDesk);

            // Act
            var result = await _pageModel.OnPostMoveDownAsync(deskId, CancellationToken.None) as JsonResult;

            // Assert
            AssertSuccessResponse(result, expectedDesk);
            _deskServiceMock.Verify(s => s.MoveDownAsync(deskId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task OnPostMoveDownAsync_InvalidDeskId_ReturnsErrorResponse()
        {
            // Arrange
            var deskId = "INVALID";
            var expectedError = "Failed to move desk down";
            _deskServiceMock.Setup(s => s.MoveDownAsync(deskId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            // Act
            var result = await _pageModel.OnPostMoveDownAsync(deskId, CancellationToken.None) as JsonResult;

            // Assert
            AssertErrorResponse(result, expectedError);
            _deskServiceMock.Verify(s => s.MoveDownAsync(deskId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task OnPostStopMovementAsync_ValidDeskId_ReturnsSuccessResponse()
        {
            // Arrange
            var deskId = "DESK001";
            var expectedDesk = new DeskInfo { DeskId = deskId, CurrentHeight = 75, IsMoving = false };
            _deskServiceMock.Setup(s => s.StopMovementAsync(deskId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            SetupMockForSuccess(deskId, expectedDesk);

            // Act
            var result = await _pageModel.OnPostStopMovementAsync(deskId, CancellationToken.None) as JsonResult;

            // Assert
            AssertSuccessResponse(result, expectedDesk);
            _deskServiceMock.Verify(s => s.StopMovementAsync(deskId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task OnPostStopMovementAsync_InvalidDeskId_ReturnsErrorResponse()
        {
            // Arrange
            var deskId = "INVALID";
            var expectedError = "Failed to stop desk movement";
            _deskServiceMock.Setup(s => s.StopMovementAsync(deskId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            // Act
            var result = await _pageModel.OnPostStopMovementAsync(deskId, CancellationToken.None) as JsonResult;

            // Assert
            AssertErrorResponse(result, expectedError);
            _deskServiceMock.Verify(s => s.StopMovementAsync(deskId, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

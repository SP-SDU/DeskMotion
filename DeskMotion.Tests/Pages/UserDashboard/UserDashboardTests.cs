using Microsoft.AspNetCore.Mvc;
using DeskMotion.Models;
using DeskMotion.Models.Responses;
using Xunit;
using Moq;
using System.Threading.Tasks;
using System;
using FluentAssertions;
using System.Threading;

namespace DeskMotion.Tests.Pages.UserDashboard
{
    public class UserDashboardTests : UserDashboardTestBase
    {
        [Fact]
        public async Task OnGetAsync_LoadsDesks()
        {
            // Arrange
            var testDesks = new List<DeskInfo>
            {
                new DeskInfo { DeskId = "DESK001", Name = "Test Desk 1", CurrentHeight = 75 },
                new DeskInfo { DeskId = "DESK002", Name = "Test Desk 2", CurrentHeight = 65 }
            };
            _deskServiceMock.Setup(s => s.GetAllDesksAsync(It.IsAny<CancellationToken>())).ReturnsAsync(testDesks);

            // Act
            await _pageModel.OnGetAsync(CancellationToken.None);

            // Assert
            _pageModel.Desks.Should().NotBeNull()
                .And.BeEquivalentTo(testDesks);
            _pageModel.ErrorMessage.Should().BeNull();
        }

        [Fact]
        public async Task OnGetAsync_Error_SetsErrorMessage()
        {
            // Arrange
            var expectedError = "Failed to load desks";
            _deskServiceMock.Setup(s => s.GetAllDesksAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception(expectedError));

            // Act
            await _pageModel.OnGetAsync(CancellationToken.None);

            // Assert
            _pageModel.Desks.Should().BeNull();
            _pageModel.ErrorMessage.Should().Be(expectedError);
        }

        [Fact]
        public async Task OnPostSearchAsync_ReturnsMatchingDesks()
        {
            // Arrange
            var searchQuery = "test";
            var expectedDesks = new List<DeskInfo>
            {
                new DeskInfo { DeskId = "DESK001", Name = "Test Desk 1" },
                new DeskInfo { DeskId = "DESK002", Name = "Test Desk 2" }
            };
            _deskServiceMock.Setup(s => s.SearchDesksAsync(searchQuery, It.IsAny<CancellationToken>())).ReturnsAsync(expectedDesks);
            _pageModel.SearchQuery = searchQuery;

            // Act
            var result = await _pageModel.OnPostSearchAsync(CancellationToken.None) as JsonResult;

            // Assert
            AssertSearchSuccessResponse(result, expectedDesks);
        }

        [Fact]
        public async Task OnPostSearchAsync_EmptyQuery_ReturnsError()
        {
            // Arrange
            _pageModel.SearchQuery = "";

            // Act
            var result = await _pageModel.OnPostSearchAsync(CancellationToken.None) as JsonResult;

            // Assert
            AssertSearchErrorResponse(result, "Search query cannot be empty");
        }

        [Fact]
        public async Task OnPostSearchAsync_ServiceError_ReturnsError()
        {
            // Arrange
            var searchQuery = "test";
            var expectedError = "Search failed";
            _deskServiceMock.Setup(s => s.SearchDesksAsync(searchQuery, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception(expectedError));
            _pageModel.SearchQuery = searchQuery;

            // Act
            var result = await _pageModel.OnPostSearchAsync(CancellationToken.None) as JsonResult;

            // Assert
            AssertSearchErrorResponse(result, expectedError);
        }

        [Fact]
        public async Task OnPostMoveUpAsync_Success()
        {
            // Arrange
            var deskId = "DESK001";
            var expectedDesk = new DeskInfo { DeskId = deskId, IsMoving = true, CurrentHeight = 75 };
            _deskServiceMock.Setup(s => s.MoveUpAsync(deskId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _deskServiceMock.Setup(s => s.GetDeskInfoAsync(deskId, It.IsAny<CancellationToken>())).ReturnsAsync(expectedDesk);

            // Act
            var result = await _pageModel.OnPostMoveUpAsync(deskId, CancellationToken.None) as JsonResult;

            // Assert
            AssertDeskSuccessResponse(result, expectedDesk);
        }

        [Fact]
        public async Task OnPostMoveDownAsync_Success()
        {
            // Arrange
            var deskId = "DESK001";
            var expectedDesk = new DeskInfo { DeskId = deskId, IsMoving = true, CurrentHeight = 75 };
            _deskServiceMock.Setup(s => s.MoveDownAsync(deskId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _deskServiceMock.Setup(s => s.GetDeskInfoAsync(deskId, It.IsAny<CancellationToken>())).ReturnsAsync(expectedDesk);

            // Act
            var result = await _pageModel.OnPostMoveDownAsync(deskId, CancellationToken.None) as JsonResult;

            // Assert
            AssertDeskSuccessResponse(result, expectedDesk);
        }

        [Fact]
        public async Task OnPostStopMovementAsync_Success()
        {
            // Arrange
            var deskId = "DESK001";
            var expectedDesk = new DeskInfo { DeskId = deskId, IsMoving = false, CurrentHeight = 75 };
            _deskServiceMock.Setup(s => s.StopMovementAsync(deskId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _deskServiceMock.Setup(s => s.GetDeskInfoAsync(deskId, It.IsAny<CancellationToken>())).ReturnsAsync(expectedDesk);

            // Act
            var result = await _pageModel.OnPostStopMovementAsync(deskId, CancellationToken.None) as JsonResult;

            // Assert
            AssertDeskSuccessResponse(result, expectedDesk);
        }

        [Fact]
        public async Task OnPostMoveUpAsync_Error()
        {
            // Arrange
            var deskId = "DESK001";
            _deskServiceMock.Setup(s => s.MoveUpAsync(deskId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            // Act
            var result = await _pageModel.OnPostMoveUpAsync(deskId, CancellationToken.None) as JsonResult;

            // Assert
            AssertDeskErrorResponse(result, "Failed to move desk up");
        }

        [Fact]
        public async Task OnPostMoveDownAsync_Error()
        {
            // Arrange
            var deskId = "DESK001";
            _deskServiceMock.Setup(s => s.MoveDownAsync(deskId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            // Act
            var result = await _pageModel.OnPostMoveDownAsync(deskId, CancellationToken.None) as JsonResult;

            // Assert
            AssertDeskErrorResponse(result, "Failed to move desk down");
        }

        [Fact]
        public async Task OnPostStopMovementAsync_Error()
        {
            // Arrange
            var deskId = "DESK001";
            _deskServiceMock.Setup(s => s.StopMovementAsync(deskId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            // Act
            var result = await _pageModel.OnPostStopMovementAsync(deskId, CancellationToken.None) as JsonResult;

            // Assert
            AssertDeskErrorResponse(result, "Failed to stop desk movement");
        }

        [Fact]
        public async Task OnPostAdjustHeightAsync_ValidHeight_ReturnsSuccessResult()
        {
            // Arrange
            var deskId = "DESK001";
            var targetHeight = 75;
            var expectedDesk = new DeskInfo { DeskId = deskId, CurrentHeight = targetHeight, IsMoving = false };
            _deskServiceMock.Setup(s => s.SetDeskHeightAsync(deskId, targetHeight, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _deskServiceMock.Setup(s => s.GetDeskInfoAsync(deskId, It.IsAny<CancellationToken>())).ReturnsAsync(expectedDesk);

            // Act
            var result = await _pageModel.OnPostAdjustHeightAsync(deskId, targetHeight, CancellationToken.None);

            // Assert
            AssertDeskSuccessResponse(result as JsonResult, expectedDesk);
        }

        [Fact]
        public async Task OnPostAdjustHeightAsync_InvalidHeight_ReturnsErrorResult()
        {
            // Arrange
            var deskId = "DESK001";
            var targetHeight = 150;

            // Act
            var result = await _pageModel.OnPostAdjustHeightAsync(deskId, targetHeight, CancellationToken.None);

            // Assert
            AssertDeskErrorResponse(result as JsonResult, "Target height must be between 60 and 120 cm");
        }

        [Fact]
        public async Task OnPostAdjustHeightAsync_ServiceError_ReturnsErrorResult()
        {
            // Arrange
            var deskId = "DESK001";
            var targetHeight = 75;
            _deskServiceMock.Setup(s => s.SetDeskHeightAsync(deskId, targetHeight, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            // Act
            var result = await _pageModel.OnPostAdjustHeightAsync(deskId, targetHeight, CancellationToken.None);

            // Assert
            AssertDeskErrorResponse(result as JsonResult, "Failed to adjust desk height");
        }

        [Fact]
        public async Task OnGetDeskStatusAsync_Success()
        {
            // Arrange
            var deskId = "DESK001";
            var deskInfo = new DeskInfo
            {
                DeskId = deskId,
                CurrentHeight = 75,
                IsMoving = false
            };
            _deskServiceMock.Setup(s => s.GetDeskInfoAsync(deskId, It.IsAny<CancellationToken>())).ReturnsAsync(deskInfo);

            // Act
            var result = await _pageModel.OnGetDeskStatusAsync(deskId, CancellationToken.None);

            // Assert
            AssertDeskSuccessResponse(result as JsonResult, deskInfo);
        }

        [Fact]
        public async Task OnGetDeskStatusAsync_NotFound()
        {
            // Arrange
            var deskId = "NONEXISTENT";
            _deskServiceMock.Setup(s => s.GetDeskInfoAsync(deskId, It.IsAny<CancellationToken>())).ReturnsAsync((DeskInfo)null);

            // Act
            var result = await _pageModel.OnGetDeskStatusAsync(deskId, CancellationToken.None);

            // Assert
            AssertDeskErrorResponse(result as JsonResult, "Desk not found");
        }

        [Fact]
        public async Task OnPostSavePresetAsync_ValidHeight_Success()
        {
            // Arrange
            var deskId = "DESK001";
            var presetName = "Standing";
            var height = 100;
            var expectedPreset = new HeightPreset { Name = presetName, Height = height };
            _deskServiceMock.Setup(s => s.SavePresetAsync(deskId, presetName, height, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedPreset);

            // Act
            var result = await _pageModel.OnPostSavePresetAsync(deskId, presetName, height, CancellationToken.None) as JsonResult;

            // Assert
            AssertPresetSuccessResponse(result, expectedPreset);
        }

        [Fact]
        public async Task OnPostSavePresetAsync_InvalidHeight_ReturnsError()
        {
            // Arrange
            var deskId = "DESK001";
            var presetName = "Standing";
            var height = 150;

            // Act
            var result = await _pageModel.OnPostSavePresetAsync(deskId, presetName, height, CancellationToken.None) as JsonResult;

            // Assert
            AssertPresetErrorResponse(result, "Height must be between 60 and 120 cm");
        }

        [Fact]
        public async Task OnPostSavePresetAsync_ServiceError_ReturnsError()
        {
            // Arrange
            var deskId = "DESK001";
            var presetName = "Standing";
            var height = 100;
            var expectedError = "Failed to save preset";
            _deskServiceMock.Setup(s => s.SavePresetAsync(deskId, presetName, height, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception(expectedError));

            // Act
            var result = await _pageModel.OnPostSavePresetAsync(deskId, presetName, height, CancellationToken.None) as JsonResult;

            // Assert
            AssertPresetErrorResponse(result, expectedError);
        }

        [Fact]
        public async Task OnPostSavePresetAsync_EmptyName_ReturnsError()
        {
            // Arrange
            var deskId = "DESK001";
            var presetName = "";
            var height = 100;

            // Act
            var result = await _pageModel.OnPostSavePresetAsync(deskId, presetName, height, CancellationToken.None) as JsonResult;

            // Assert
            AssertPresetErrorResponse(result, "Preset name cannot be empty");
        }

        [Fact]
        public async Task OnPostSavePresetAsync_InvalidDeskId_ReturnsError()
        {
            // Arrange
            var deskId = "";
            var presetName = "Standing";
            var height = 100;

            // Act
            var result = await _pageModel.OnPostSavePresetAsync(deskId, presetName, height, CancellationToken.None) as JsonResult;

            // Assert
            AssertPresetErrorResponse(result, "Invalid desk ID");
        }

        [Fact]
        public async Task OnPostDeletePresetAsync_Success()
        {
            // Arrange
            var deskId = "DESK001";
            var presetName = "Standing";
            _deskServiceMock.Setup(s => s.DeletePresetAsync(deskId, presetName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _pageModel.OnPostDeletePresetAsync(deskId, presetName, CancellationToken.None) as JsonResult;

            // Assert
            AssertPresetSuccessResponse(result);
        }

        [Fact]
        public async Task OnPostDeletePresetAsync_NotFound_ReturnsError()
        {
            // Arrange
            var deskId = "DESK001";
            var presetName = "Standing";
            _deskServiceMock.Setup(s => s.DeletePresetAsync(deskId, presetName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _pageModel.OnPostDeletePresetAsync(deskId, presetName, CancellationToken.None) as JsonResult;

            // Assert
            AssertPresetErrorResponse(result, "Failed to delete preset");
        }

        [Fact]
        public async Task OnPostDeletePresetAsync_EmptyName_ReturnsError()
        {
            // Arrange
            var deskId = "DESK001";
            var presetName = "";

            // Act
            var result = await _pageModel.OnPostDeletePresetAsync(deskId, presetName, CancellationToken.None) as JsonResult;

            // Assert
            AssertPresetErrorResponse(result, "Preset name cannot be empty");
        }

        [Fact]
        public async Task OnPostMoveToPresetAsync_Success()
        {
            // Arrange
            var deskId = "DESK001";
            var presetName = "Standing";
            var expectedDesk = new DeskInfo { DeskId = deskId, CurrentHeight = 100, IsMoving = true };
            _deskServiceMock.Setup(s => s.MoveToPresetAsync(deskId, presetName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _deskServiceMock.Setup(s => s.GetDeskInfoAsync(deskId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDesk);

            // Act
            var result = await _pageModel.OnPostMoveToPresetAsync(deskId, presetName, CancellationToken.None) as JsonResult;

            // Assert
            AssertDeskSuccessResponse(result, expectedDesk);
        }

        [Fact]
        public async Task OnPostMoveToPresetAsync_NotFound_ReturnsError()
        {
            // Arrange
            var deskId = "DESK001";
            var presetName = "Standing";
            _deskServiceMock.Setup(s => s.MoveToPresetAsync(deskId, presetName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _pageModel.OnPostMoveToPresetAsync(deskId, presetName, CancellationToken.None) as JsonResult;

            // Assert
            AssertDeskErrorResponse(result, "Failed to move desk to preset position");
        }

        [Fact]
        public async Task OnPostMoveToPresetAsync_EmptyName_ReturnsError()
        {
            // Arrange
            var deskId = "DESK001";
            var presetName = "";

            // Act
            var result = await _pageModel.OnPostMoveToPresetAsync(deskId, presetName, CancellationToken.None) as JsonResult;

            // Assert
            AssertDeskErrorResponse(result, "Preset name cannot be empty");
        }

        private new void AssertSearchSuccessResponse(JsonResult result, IEnumerable<DeskInfo> expectedResponse)
        {
            base.AssertSearchSuccessResponse(result, expectedResponse);
        }

        private new void AssertSearchErrorResponse(JsonResult result, string expectedError)
        {
            base.AssertSearchErrorResponse(result, expectedError);
        }

        private new void AssertDeskSuccessResponse(JsonResult result, DeskInfo expectedResponse)
        {
            base.AssertDeskSuccessResponse(result, expectedResponse);
        }

        private new void AssertDeskErrorResponse(JsonResult result, string expectedError)
        {
            base.AssertDeskErrorResponse(result, expectedError);
        }

        private new void AssertPresetSuccessResponse(JsonResult result, HeightPreset? expectedPreset = null)
        {
            base.AssertPresetSuccessResponse(result, expectedPreset);
        }

        private new void AssertPresetErrorResponse(JsonResult result, string expectedError)
        {
            base.AssertPresetErrorResponse(result, expectedError);
        }
    }
}

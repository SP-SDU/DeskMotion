using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using DeskMotion.Models;
using DeskMotion.Models.Responses;
using System.Threading.Tasks;
using System;
using System.Threading;
using FluentAssertions;

namespace DeskMotion.Tests.Pages.UserDashboard
{
    public class DeskPresetTests : UserDashboardTestBase
    {
        [Fact]
        public async Task OnPostSavePresetAsync_ValidInput_ReturnsSuccessResponse()
        {
            // Arrange
            var deskId = "DESK001";
            var presetName = "Standing";
            var height = 110;
            var preset = new DeskPreset
            {
                DeskId = deskId,
                Name = presetName,
                Height = height
            };
            _deskServiceMock.Setup(s => s.SavePresetAsync(deskId, presetName, height, It.IsAny<CancellationToken>()))
                .ReturnsAsync(preset);

            // Act
            var result = await _pageModel.OnPostSavePresetAsync(deskId, presetName, height, CancellationToken.None) as JsonResult;

            // Assert
            AssertSuccessResponse(result, preset);
            _deskServiceMock.Verify(s => s.SavePresetAsync(deskId, presetName, height, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task OnPostSavePresetAsync_InvalidInput_ReturnsErrorResponse()
        {
            // Arrange
            var deskId = "INVALID";
            var presetName = "Standing";
            var height = 110;
            var expectedError = "Failed to save preset";
            _deskServiceMock.Setup(s => s.SavePresetAsync(deskId, presetName, height, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception(expectedError));

            // Act
            var result = await _pageModel.OnPostSavePresetAsync(deskId, presetName, height, CancellationToken.None) as JsonResult;

            // Assert
            AssertErrorResponse(result, expectedError);
            _deskServiceMock.Verify(s => s.SavePresetAsync(deskId, presetName, height, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task OnPostMoveToPresetAsync_ValidInput_ReturnsSuccessResponse()
        {
            // Arrange
            var deskId = "DESK001";
            var presetName = "Standing";
            var expectedDesk = new DeskInfo
            {
                DeskId = deskId,
                CurrentHeight = 110,
                IsMoving = true
            };
            _deskServiceMock.Setup(s => s.MoveToPresetAsync(deskId, presetName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            SetupMockForSuccess(deskId, expectedDesk);

            // Act
            var result = await _pageModel.OnPostMoveToPresetAsync(deskId, presetName, CancellationToken.None) as JsonResult;

            // Assert
            AssertSuccessResponse(result, expectedDesk);
            _deskServiceMock.Verify(s => s.MoveToPresetAsync(deskId, presetName, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task OnPostMoveToPresetAsync_InvalidInput_ReturnsErrorResponse()
        {
            // Arrange
            var deskId = "INVALID";
            var presetName = "Standing";
            var expectedError = "Failed to move to preset";
            _deskServiceMock.Setup(s => s.MoveToPresetAsync(deskId, presetName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _pageModel.OnPostMoveToPresetAsync(deskId, presetName, CancellationToken.None) as JsonResult;

            // Assert
            AssertErrorResponse(result, expectedError);
            _deskServiceMock.Verify(s => s.MoveToPresetAsync(deskId, presetName, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task OnPostDeletePresetAsync_ValidInput_ReturnsSuccessResponse()
        {
            // Arrange
            var deskId = "DESK001";
            var presetName = "Standing";
            _deskServiceMock.Setup(s => s.DeletePresetAsync(deskId, presetName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _pageModel.OnPostDeletePresetAsync(deskId, presetName, CancellationToken.None) as JsonResult;

            // Assert
            AssertSuccessResponse(result, true);
            _deskServiceMock.Verify(s => s.DeletePresetAsync(deskId, presetName, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task OnPostDeletePresetAsync_InvalidInput_ReturnsErrorResponse()
        {
            // Arrange
            var deskId = "INVALID";
            var presetName = "Standing";
            var expectedError = "Failed to delete preset";
            _deskServiceMock.Setup(s => s.DeletePresetAsync(deskId, presetName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _pageModel.OnPostDeletePresetAsync(deskId, presetName, CancellationToken.None) as JsonResult;

            // Assert
            AssertErrorResponse(result, expectedError);
            _deskServiceMock.Verify(s => s.DeletePresetAsync(deskId, presetName, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

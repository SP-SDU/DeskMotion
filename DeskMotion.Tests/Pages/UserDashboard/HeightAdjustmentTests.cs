using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using DeskMotion.Models;
using DeskMotion.Models.Responses;
using System.Threading.Tasks;
using System.Threading;
using FluentAssertions;

namespace DeskMotion.Tests.Pages.UserDashboard
{
    public class HeightAdjustmentTests : UserDashboardTestBase
    {
        [Theory]
        [InlineData(75)]  // Mid-range height
        [InlineData(60)]  // Minimum height
        [InlineData(120)] // Maximum height
        public async Task OnPostAdjustHeightAsync_ValidHeight_ReturnsSuccessResponse(int targetHeight)
        {
            // Arrange
            var deskId = "DESK001";
            var expectedDesk = new DeskInfo { DeskId = deskId, CurrentHeight = targetHeight, IsMoving = false };
            _deskServiceMock.Setup(s => s.AdjustHeightAsync(deskId, targetHeight, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            SetupMockForSuccess(deskId, expectedDesk);

            // Act
            var result = await _pageModel.OnPostAdjustHeightAsync(deskId, targetHeight, CancellationToken.None) as JsonResult;

            // Assert
            AssertSuccessResponse(result, expectedDesk);
            _deskServiceMock.Verify(s => s.AdjustHeightAsync(deskId, targetHeight, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Theory]
        [InlineData(59)]   // Below minimum
        [InlineData(121)]  // Above maximum
        [InlineData(-10)]  // Negative value
        public async Task OnPostAdjustHeightAsync_InvalidHeight_ReturnsErrorResponse(int targetHeight)
        {
            // Arrange
            var deskId = "DESK001";
            var expectedError = "Target height must be between 60 and 120 cm";

            // Act
            var result = await _pageModel.OnPostAdjustHeightAsync(deskId, targetHeight, CancellationToken.None) as JsonResult;

            // Assert
            AssertErrorResponse(result, expectedError);
            _deskServiceMock.Verify(s => s.AdjustHeightAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task OnPostAdjustHeightAsync_ServiceError_ReturnsErrorResponse()
        {
            // Arrange
            var deskId = "DESK001";
            var targetHeight = 80;
            var expectedError = "Failed to adjust desk height";
            _deskServiceMock.Setup(s => s.AdjustHeightAsync(deskId, targetHeight, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _pageModel.OnPostAdjustHeightAsync(deskId, targetHeight, CancellationToken.None) as JsonResult;

            // Assert
            AssertErrorResponse(result, expectedError);
            _deskServiceMock.Verify(s => s.AdjustHeightAsync(deskId, targetHeight, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task OnPostAdjustHeightAsync_InvalidDeskId_ReturnsErrorResponse()
        {
            // Arrange
            var deskId = "INVALID";
            var targetHeight = 80;
            var expectedError = "Failed to adjust desk height";
            _deskServiceMock.Setup(s => s.AdjustHeightAsync(deskId, targetHeight, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _pageModel.OnPostAdjustHeightAsync(deskId, targetHeight, CancellationToken.None) as JsonResult;

            // Assert
            AssertErrorResponse(result, expectedError);
            _deskServiceMock.Verify(s => s.AdjustHeightAsync(deskId, targetHeight, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

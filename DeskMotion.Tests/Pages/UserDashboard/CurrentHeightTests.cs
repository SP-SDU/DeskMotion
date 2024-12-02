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
    public class CurrentHeightTests : UserDashboardTestBase
    {
        [Fact]
        public async Task OnGetCurrentHeightAsync_ValidDesk_ReturnsSuccess()
        {
            // Arrange
            var deskId = "DESK001";
            var expectedHeight = 75;
            var expectedDesk = new DeskInfo
            {
                DeskId = deskId,
                CurrentHeight = expectedHeight,
                IsMoving = false
            };
            _deskServiceMock.Setup(s => s.GetDeskInfoAsync(deskId, It.IsAny<CancellationToken>())).ReturnsAsync(expectedDesk);

            // Act
            var result = await _pageModel.OnGetCurrentHeightAsync(deskId, CancellationToken.None);

            // Assert
            AssertSuccessResponse(result as JsonResult, expectedDesk);
        }

        [Fact]
        public async Task OnGetCurrentHeightAsync_DeskNotFound_ReturnsError()
        {
            // Arrange
            var deskId = "NONEXISTENT";
            _deskServiceMock.Setup(s => s.GetDeskInfoAsync(deskId, It.IsAny<CancellationToken>())).ReturnsAsync((DeskInfo?)null);

            // Act
            var result = await _pageModel.OnGetCurrentHeightAsync(deskId, CancellationToken.None);

            // Assert
            AssertErrorResponse(result as JsonResult, "Desk not found");
        }

        [Fact]
        public async Task OnGetCurrentHeightAsync_ServiceError_ReturnsError()
        {
            // Arrange
            var deskId = "DESK001";
            var expectedError = "Failed to get desk height";
            _deskServiceMock.Setup(s => s.GetDeskInfoAsync(deskId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception(expectedError));

            // Act
            var result = await _pageModel.OnGetCurrentHeightAsync(deskId, CancellationToken.None);

            // Assert
            AssertErrorResponse(result as JsonResult, expectedError);
        }
    }
}

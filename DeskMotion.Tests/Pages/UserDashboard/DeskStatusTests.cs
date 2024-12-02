using Xunit;
using Moq;
using Moq.Language.Flow;
using Microsoft.AspNetCore.Mvc;
using DeskMotion.Models;
using DeskMotion.Models.Responses;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using FluentAssertions;
using System.Threading;

namespace DeskMotion.Tests.Pages.UserDashboard
{
    public class DeskStatusTests : UserDashboardTestBase
    {
        [Fact]
        public async Task GetDeskStatus_ValidDeskId_ReturnsStatus()
        {
            // Arrange
            var deskId = "DESK001";
            var expectedDesk = new DeskInfo 
            { 
                DeskId = deskId, 
                CurrentHeight = 75, 
                IsMoving = false,
                Name = "Test Desk"
            };
            SetupMockForSuccess(deskId, expectedDesk);

            // Act
            var result = await _pageModel.OnGetDeskStatusAsync(deskId, CancellationToken.None) as JsonResult;

            // Assert
            AssertSuccessResponse(result, expectedDesk);
        }

        [Fact]
        public async Task GetDeskStatus_InvalidDeskId_ReturnsError()
        {
            // Arrange
            var deskId = "INVALID";
            var expectedError = "Desk not found";
            SetupMockForError(deskId, expectedError);

            // Act
            var result = await _pageModel.OnGetDeskStatusAsync(deskId, CancellationToken.None) as JsonResult;

            // Assert
            AssertErrorResponse(result, expectedError);
        }

        [Fact]
        public async Task GetAllDesks_ReturnsListOfDesks()
        {
            // Arrange
            var expectedDesks = new List<DeskInfo>
            {
                new DeskInfo { DeskId = "DESK001", Name = "Desk 1", CurrentHeight = 75 },
                new DeskInfo { DeskId = "DESK002", Name = "Desk 2", CurrentHeight = 65 }
            };
            _deskServiceMock.Setup(s => s.GetAllDesksAsync(It.IsAny<CancellationToken>())).ReturnsAsync(expectedDesks);

            // Act
            await _pageModel.OnGetAsync(CancellationToken.None);

            // Assert
            _pageModel.Desks.Should().NotBeNull();
            _pageModel.Desks.Should().BeEquivalentTo(expectedDesks);
            _pageModel.ErrorMessage.Should().BeNull();
        }

        [Fact]
        public async Task GetAllDesks_ServiceError_SetsErrorMessage()
        {
            // Arrange
            var expectedError = "Failed to retrieve desks";
            _deskServiceMock.Setup(s => s.GetAllDesksAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception(expectedError));

            // Act
            await _pageModel.OnGetAsync(CancellationToken.None);

            // Assert
            _pageModel.Desks.Should().BeNull();
            _pageModel.ErrorMessage.Should().NotBeNull();
            _pageModel.ErrorMessage.Should().Be(expectedError);
        }

        [Fact]
        public async Task GetDeskStatus_MovingDesk_ReturnsCorrectStatus()
        {
            // Arrange
            var deskId = "DESK001";
            var expectedDesk = new DeskInfo 
            { 
                DeskId = deskId, 
                CurrentHeight = 75, 
                IsMoving = true,
                Name = "Moving Desk"
            };
            SetupMockForSuccess(deskId, expectedDesk);

            // Act
            var result = await _pageModel.OnGetDeskStatusAsync(deskId, CancellationToken.None) as JsonResult;

            // Assert
            AssertSuccessResponse(result, expectedDesk);
            var response = result.Value as DeskResponse;
            response.Desk.IsMoving.Should().BeTrue();
        }
    }
}

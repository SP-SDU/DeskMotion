using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DeskMotion.Models;
using DeskMotion.Models.Responses;
using DeskMotion.Pages.UserDashboard;
using DeskMotion.Services;
using Moq;
using FluentAssertions;
using FluentAssertions.Execution;
using System.Threading;
using System.Collections.Generic;

namespace DeskMotion.Tests.Pages.UserDashboard
{
    public abstract class UserDashboardTestBase
    {
        protected readonly Mock<IDeskService> _deskServiceMock;
        protected readonly global::DeskMotion.Pages.UserDashboard.Index _pageModel;

        protected UserDashboardTestBase()
        {
            _deskServiceMock = new Mock<IDeskService>();
            _pageModel = new global::DeskMotion.Pages.UserDashboard.Index(_deskServiceMock.Object, Mock.Of<ILogger<global::DeskMotion.Pages.UserDashboard.Index>>());
        }

        protected void SetupMockForSuccess(string deskId, DeskInfo expectedDesk)
        {
            _deskServiceMock.Setup(s => s.GetDeskInfoAsync(deskId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDesk);
        }

        protected void SetupMockForError(string deskId, string errorMessage)
        {
            _deskServiceMock.Setup(s => s.GetDeskInfoAsync(deskId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException(errorMessage));
        }

        protected void AssertDeskSuccessResponse(JsonResult result, DeskInfo expectedDesk)
        {
            result.Should().NotBeNull();
            var response = result!.Value.Should().BeOfType<DeskResponse>().Subject;
            response.Success.Should().BeTrue();
            response.Desk.Should().BeEquivalentTo(expectedDesk);
            response.Error.Should().BeNull();
        }

        protected void AssertDeskErrorResponse(JsonResult result, string expectedError)
        {
            result.Should().NotBeNull();
            var response = result!.Value.Should().BeOfType<DeskResponse>().Subject;
            response.Success.Should().BeFalse();
            response.Error.Should().Be(expectedError);
            response.Desk.Should().BeNull();
        }

        protected void AssertSearchSuccessResponse(JsonResult result, IEnumerable<DeskInfo> expectedDesks)
        {
            result.Should().NotBeNull();
            var response = result!.Value.Should().BeOfType<SearchResponse>().Subject;
            response.Success.Should().BeTrue();
            response.Results.Should().BeEquivalentTo(expectedDesks);
            response.Error.Should().BeNull();
        }

        protected void AssertSearchErrorResponse(JsonResult result, string expectedError)
        {
            result.Should().NotBeNull();
            var response = result!.Value.Should().BeOfType<SearchResponse>().Subject;
            response.Success.Should().BeFalse();
            response.Error.Should().Be(expectedError);
            response.Results.Should().BeNull();
        }

        protected void AssertPresetSuccessResponse(JsonResult result, HeightPreset? expectedPreset = null)
        {
            result.Should().NotBeNull();
            var response = result!.Value.Should().BeOfType<PresetResponse>().Subject;
            response.Success.Should().BeTrue();
            response.Error.Should().BeNull();
            
            if (expectedPreset != null)
            {
                response.Preset.Should().BeEquivalentTo(expectedPreset);
            }
        }

        protected void AssertPresetErrorResponse(JsonResult result, string expectedError)
        {
            result.Should().NotBeNull();
            var response = result!.Value.Should().BeOfType<PresetResponse>().Subject;
            response.Success.Should().BeFalse();
            response.Error.Should().Be(expectedError);
            response.Preset.Should().BeNull();
        }
    }
}

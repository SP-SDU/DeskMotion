using Xunit;
using Moq;
using Moq.Language.Flow;
using Microsoft.AspNetCore.Mvc;
using DeskMotion.Models;
using DeskMotion.Models.Responses;
using FluentAssertions;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions.Execution;

namespace DeskMotion.Tests.Pages.UserDashboard
{
    public class DeskSearchTests : UserDashboardTestBase
    {
        [Fact]
        public async Task Search_ValidQuery_ReturnsMatchingDesks()
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
            AssertSearchResponse(result, expectedDesks);
            _deskServiceMock.Verify(s => s.SearchDesksAsync(searchQuery, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Search_NoResults_ReturnsEmptyList()
        {
            // Arrange
            var searchQuery = "nonexistent";
            var expectedDesks = new List<DeskInfo>();
            _deskServiceMock.Setup(s => s.SearchDesksAsync(searchQuery, It.IsAny<CancellationToken>())).ReturnsAsync(expectedDesks);
            _pageModel.SearchQuery = searchQuery;

            // Act
            var result = await _pageModel.OnPostSearchAsync(CancellationToken.None) as JsonResult;

            // Assert
            AssertSearchResponse(result, expectedDesks);
        }

        [Fact]
        public async Task Search_NullQuery_ReturnsError()
        {
            // Arrange
            string? searchQuery = null;
            _pageModel.SearchQuery = searchQuery;

            // Act
            var result = await _pageModel.OnPostSearchAsync(CancellationToken.None) as JsonResult;

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                var response = result!.Value.Should().BeOfType<SearchResponse>().Subject;
                response.Success.Should().BeFalse();
                response.Error.Should().Be("Search query cannot be empty");
                response.Results.Should().BeNull();
            }
        }

        [Fact]
        public async Task Search_ServiceError_ReturnsError()
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
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                var response = result!.Value.Should().BeOfType<SearchResponse>().Subject;
                response.Success.Should().BeFalse();
                response.Error.Should().Be(expectedError);
                response.Results.Should().BeNull();
            }
        }

        private void AssertSearchResponse(JsonResult? result, IEnumerable<DeskInfo> expectedResults)
        {
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                var response = result!.Value.Should().BeOfType<SearchResponse>().Subject;
                response.Success.Should().BeTrue();
                response.Error.Should().BeNull();
                response.Results.Should().BeEquivalentTo(expectedResults);
            }
        }
    }
}

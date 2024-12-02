using Xunit;
using FluentAssertions;
using DeskMotion.Services;
using DeskMotion.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace DeskMotion.Tests.Services
{
    public class DeskServiceTests
    {
        private readonly DeskService _service;
        private readonly Mock<ILogger<DeskService>> _loggerMock;
        private const string ValidDeskId = "DESK001";
        private const string InvalidDeskId = "INVALID_DESK";

        public DeskServiceTests()
        {
            _loggerMock = new Mock<ILogger<DeskService>>();
            _service = new DeskService(_loggerMock.Object);
        }

        [Fact]
        public async Task GetDeskInfo_ValidId_ReturnsDeskInfo()
        {
            // Act
            var result = await _service.GetDeskInfoAsync(ValidDeskId);

            // Assert
            result.Should().NotBeNull();
            result!.DeskId.Should().Be(ValidDeskId);
            result.CurrentHeight.Should().BeInRange(60, 120);
        }

        [Fact]
        public async Task GetDeskInfo_InvalidId_ReturnsNull()
        {
            // Act
            var result = await _service.GetDeskInfoAsync(InvalidDeskId);

            // Assert
            result.Should().BeNull();
        }

        [Theory]
        [InlineData(70, true)]  // Valid height
        [InlineData(50, false)] // Too low
        [InlineData(130, false)] // Too high
        public async Task SetDeskHeight_ValidatesTargetHeight(int targetHeight, bool expectedSuccess)
        {
            // Act
            var result = await _service.SetDeskHeightAsync(ValidDeskId, targetHeight);

            // Assert
            result.Should().Be(expectedSuccess);
            if (expectedSuccess)
            {
                var desk = await _service.GetDeskInfoAsync(ValidDeskId);
                desk.Should().NotBeNull();
                desk!.CurrentHeight.Should().Be(targetHeight);
            }
        }

        [Fact]
        public async Task MoveUp_ValidDeskId_IncreasesHeight()
        {
            // Arrange
            var initialDesk = await _service.GetDeskInfoAsync(ValidDeskId);
            var initialHeight = initialDesk!.CurrentHeight;

            // Act
            var result = await _service.MoveUpAsync(ValidDeskId);

            // Assert
            result.Should().BeTrue();
            var updatedDesk = await _service.GetDeskInfoAsync(ValidDeskId);
            updatedDesk.Should().NotBeNull();
            updatedDesk!.CurrentHeight.Should().BeGreaterThan(initialHeight);
            updatedDesk.CurrentHeight.Should().BeLessOrEqualTo(120);
        }

        [Fact]
        public async Task MoveDown_ValidDeskId_DecreasesHeight()
        {
            // Arrange
            var initialDesk = await _service.GetDeskInfoAsync(ValidDeskId);
            var initialHeight = initialDesk!.CurrentHeight;

            // Act
            var result = await _service.MoveDownAsync(ValidDeskId);

            // Assert
            result.Should().BeTrue();
            var updatedDesk = await _service.GetDeskInfoAsync(ValidDeskId);
            updatedDesk.Should().NotBeNull();
            updatedDesk!.CurrentHeight.Should().BeLessThan(initialHeight);
            updatedDesk.CurrentHeight.Should().BeGreaterOrEqualTo(60);
        }

        [Fact]
        public async Task StopMovement_ValidDeskId_StopsMovement()
        {
            // Arrange
            await _service.MoveUpAsync(ValidDeskId);

            // Act
            var result = await _service.StopMovementAsync(ValidDeskId);

            // Assert
            result.Should().BeTrue();
            var desk = await _service.GetDeskInfoAsync(ValidDeskId);
            desk.Should().NotBeNull();
            desk!.IsMoving.Should().BeFalse();
        }

        [Fact]
        public async Task GetAllDesks_ReturnsAllDesks()
        {
            // Act
            var desks = await _service.GetAllDesksAsync();

            // Assert
            desks.Should().NotBeNull();
            desks.Should().HaveCountGreaterThan(0);
            desks.Should().OnlyContain(d => d != null);
        }

        [Fact]
        public async Task SearchDesks_WithValidQuery_ReturnsMatchingDesks()
        {
            // Act
            var results = await _service.SearchDesksAsync("DESK");

            // Assert
            results.Should().NotBeNull();
            results.Should().OnlyContain(d => d.DeskId.Contains("DESK", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task SearchDesks_WithEmptyQuery_ReturnsEmptyList()
        {
            // Act
            var results = await _service.SearchDesksAsync("");

            // Assert
            results.Should().NotBeNull();
            results.Should().BeEmpty();
        }
    }
}

// Copyright 2024 PET Group16
//
// Licensed under the Apache License, Version 2.0 (the "License"):
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using DeskMotion.Data;
using DeskMotion.Models;
using DeskMotion.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace DeskMotion.Tests.Services;

public class DeskDataUpdaterTests
{
    private readonly Mock<HttpClient> _httpClientMock;
    private readonly Mock<DeskService> _deskServiceMock;
    private readonly Mock<ILogger<DeskDataUpdater>> _loggerMock;
    private readonly IServiceProvider _serviceProvider;

    public DeskDataUpdaterTests()
    {
        _httpClientMock = new Mock<HttpClient>();
        _deskServiceMock = new Mock<DeskService>(_httpClientMock.Object);
        _loggerMock = new Mock<ILogger<DeskDataUpdater>>();

        var services = new ServiceCollection();
        _ = services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase("TestDatabase"));
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesDeskData()
    {
        // Arrange
        var deskIds = new List<string> { "desk1", "desk2" };
        var deskData = new Desk { MacAddress = "desk1", IsLatest = true };

        _ = _deskServiceMock.Setup(ds => ds.GetDeskIdsAsync())
            .ReturnsAsync(deskIds);

        _ = _deskServiceMock.Setup(ds => ds.GetDeskAsync("desk1"))
            .ReturnsAsync(deskData);

        _ = _deskServiceMock.Setup(ds => ds.GetDeskAsync("desk2"))
            .ReturnsAsync(new Desk { MacAddress = "desk2", IsLatest = true });

        var deskDataUpdater = new DeskDataUpdater(
            _deskServiceMock.Object,
            _serviceProvider,
            _loggerMock.Object);

        // Act
        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(1));
            await deskDataUpdater.StartAsync(cancellationTokenSource.Token);
        }

        // Assert
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var metadataList = await dbContext.DeskMetadata.ToListAsync();
        var desks = await dbContext.Desks.ToListAsync();

        Assert.Equal(2, metadataList.Count);
        Assert.Equal(2, desks.Count);
        Assert.All(desks, d => Assert.True(d.IsLatest));
    }

    [Fact]
    public async Task ExecuteAsync_HandlesExceptionGracefully()
    {
        // Arrange
        _ = _deskServiceMock.Setup(ds => ds.GetDeskIdsAsync())
            .ThrowsAsync(new Exception("Test Exception"));

        var deskDataUpdater = new DeskDataUpdater(
            _deskServiceMock.Object,
            _serviceProvider,
            _loggerMock.Object);

        // Act
        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(1));
            await deskDataUpdater.StartAsync(cancellationTokenSource.Token);
        }

        // Assert
        _loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) => state.ToString()!.Contains("Error updating desk data")),
                It.Is<Exception>(ex => ex.Message.Contains("Test Exception")),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}

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
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;

namespace DeskMotion.Tests.Services;

public class DeskDataUpdaterTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DeskDataUpdater> _logger;

    public DeskDataUpdaterTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost")
        };

        _logger = Mock.Of<ILogger<DeskDataUpdater>>();

        var services = new ServiceCollection();
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase("TestDatabase"));
        services.AddSingleton(new RestRepository<List<string>>(httpClient));
        services.AddSingleton(new RestRepository<Desk>(httpClient));
        services.AddSingleton(_logger);

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesDeskData()
    {
        // Arrange
        var deskIds = new List<string> { "desk1", "desk2" };
        var deskData = new Desk { MacAddress = "desk1", IsLatest = true };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().EndsWith("desks")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(deskIds)
            });

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().EndsWith("desks/desk1")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(deskData)
            });

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().EndsWith("desks/desk2")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(new Desk { MacAddress = "desk2", IsLatest = true })
            });

        var deskDataUpdater = new DeskDataUpdater(_serviceProvider, _logger);

        // Act
        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(2));
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
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new Exception("Test Exception"));

        var deskDataUpdater = new DeskDataUpdater(_serviceProvider, _logger);

        // Act
        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(1));
            await deskDataUpdater.StartAsync(cancellationTokenSource.Token);
        }

        // Assert
        Mock.Get(_logger).Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) => state.ToString()!.Contains("Error updating desk data")),
                It.Is<Exception>(ex => ex.Message.Contains("Test Exception")),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}

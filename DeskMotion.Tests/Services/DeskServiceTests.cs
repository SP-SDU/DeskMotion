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

using DeskMotion.Models;
using DeskMotion.Services;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;

namespace DeskMotion.Tests.Services;

public class DeskServiceTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly DeskService _deskService;

    public DeskServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost")
        };
        _deskService = new DeskService(_httpClient);
    }

    [Fact]
    public async Task GetDeskIdsAsync_ReturnsDeskIds()
    {
        // Arrange
        var deskIds = new List<string> { "desk1", "desk2" };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(deskIds)
        };
        _ = _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        var result = await _deskService.GetDeskIdsAsync();

        // Assert
        Assert.Equal(deskIds, result);
    }

    [Fact]
    public async Task GetDeskAsync_ReturnsDesk()
    {
        // Arrange
        const string deskId = "desk1";
        var desk = new Desk { MacAddress = deskId };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(desk)
        };
        _ = _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        var result = await _deskService.GetDeskAsync(deskId);

        // Assert
        Assert.Equal(deskId, result.MacAddress);
    }

    [Fact]
    public async Task UpdateDeskPositionAsync_SendsCorrectRequest()
    {
        // Arrange
        const string deskId = "desk1";
        const int newPositionMm = 1000;
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        _ = _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        await _deskService.UpdateDeskPositionAsync(deskId, newPositionMm);

        // Assert
        _httpMessageHandlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Put &&
                    req.RequestUri == new Uri($"http://localhost/desks/{deskId}/state") &&
                    req.Content!.ReadAsStringAsync().Result.Contains($"\"position_mm\":{newPositionMm}")
                ),
                ItExpr.IsAny<CancellationToken>());
    }
}

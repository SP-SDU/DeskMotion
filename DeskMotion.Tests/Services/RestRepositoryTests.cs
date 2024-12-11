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
using System.Text.Json;

namespace DeskMotion.Tests.Services;

public class RestRepositoryTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly RestRepository<Desk> _restRepository;

    public RestRepositoryTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost")
        };
        _restRepository = new RestRepository<Desk>(_httpClient);
    }

    [Fact]
    public async Task PostAsync_SendsCorrectRequest()
    {
        // Arrange
        var desk = new Desk
        {
            Config = new Config { Name = "Desk A", Manufacturer = "DeskCorp" }
        };
        var response = new HttpResponseMessage(HttpStatusCode.OK);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) =>
            {
                Assert.Equal(HttpMethod.Post, req.Method);
                Assert.Equal(new Uri("http://localhost/desks"), req.RequestUri);

                // Deserialize and verify content
                var content = req.Content!.ReadAsStringAsync().Result;
                var deserializedContent = JsonSerializer.Deserialize<Desk>(content);
                Assert.NotNull(deserializedContent);
                Assert.Equal("Desk A", deserializedContent!.Config.Name);
                Assert.Equal("DeskCorp", deserializedContent.Config.Manufacturer);
            })
            .ReturnsAsync(response);

        // Act
        await _restRepository.PostAsync("/desks", desk);
    }

    [Fact]
    public async Task PutAsync_SendsCorrectRequest()
    {
        // Arrange
        const string deskId = "desk1";
        var updatedDesk = new { position_mm = 1000 };
        var response = new HttpResponseMessage(HttpStatusCode.OK);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) =>
            {
                Assert.Equal(HttpMethod.Put, req.Method);
                Assert.Equal(new Uri($"http://localhost/desks/{deskId}/state"), req.RequestUri);
                Assert.Contains("\"position_mm\":1000", req.Content!.ReadAsStringAsync().Result);
            })
            .ReturnsAsync(response);

        // Act
        await _restRepository.PutAsync($"/desks/{deskId}/state", updatedDesk);
    }
}

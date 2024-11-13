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
using DeskMotion.Pages.Account;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace DeskMotion.Tests.Pages.Account;

public class LogoutModelTests
{
    private readonly Mock<SignInManager<User>> _mockSignInManager;
    private readonly Mock<ILogger<LogoutModel>> _mockLogger;
    private readonly LogoutModel _logoutModel;

    public LogoutModelTests()
    {
        var userStoreMock = new Mock<IUserStore<User>>();
        var userManagerMock = new Mock<UserManager<User>>(
            userStoreMock.Object,
            null!, null!, null!, null!, null!, null!, null!, null!);

        _mockSignInManager = new Mock<SignInManager<User>>(
            userManagerMock.Object,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<User>>(),
            null!, null!, null!, null!);

        _mockLogger = new Mock<ILogger<LogoutModel>>();
        _logoutModel = new LogoutModel(_mockSignInManager.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task OnPost_NoReturnUrl_RedirectsToPage()
    {
        // Act
        var result = await _logoutModel.OnPost();

        // Assert
        _mockSignInManager.Verify(s => s.SignOutAsync(), Times.Once);

        _mockLogger.Verify(
            l => l.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("User logged out.")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);

        Assert.IsType<RedirectToPageResult>(result);
    }
}

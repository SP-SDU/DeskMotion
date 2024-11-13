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

public class LoginModelTests
{
    private readonly Mock<SignInManager<User>> _mockSignInManager;
    private readonly Mock<ILogger<LoginModel>> _mockLogger;
    private readonly LoginModel _loginModel;

    public LoginModelTests()
    {
        _mockSignInManager = MockSignInManager();
        _mockLogger = new Mock<ILogger<LoginModel>>();
        _loginModel = new LoginModel(_mockSignInManager.Object, _mockLogger.Object);
    }

    private static Mock<SignInManager<User>> MockSignInManager()
    {
        var userManager = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(), null!, null!, null!, null!, null!, null!, null!, null!);

        return new Mock<SignInManager<User>>(
            userManager.Object,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<User>>(),
            null!, null!, null!, null!);
    }

    [Fact]
    public async Task OnPostAsync_ValidCredentials_RedirectsToReturnUrl()
    {
        // Arrange
        _loginModel.Input = new LoginModel.InputModel
        {
            Email = "test@example.com",
            Password = "Password123!",
            RememberMe = false
        };
        _mockSignInManager.Setup(s => s.PasswordSignInAsync(
            _loginModel.Input.Email,
            _loginModel.Input.Password,
            _loginModel.Input.RememberMe,
            false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        // Act
        var result = await _loginModel.OnPostAsync("~/");

        // Assert
        var redirectResult = Assert.IsType<LocalRedirectResult>(result);
        Assert.Equal("~/", redirectResult.Url);
        _mockLogger.Verify(
            l => l.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("User logged in.")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((_, __) => true)), Times.Once);
    }

    [Fact]
    public async Task OnPostAsync_InvalidCredentials_ReturnsPageWithModelError()
    {
        // Arrange
        _loginModel.Input = new LoginModel.InputModel
        {
            Email = "test@example.com",
            Password = "WrongPassword!",
            RememberMe = false
        };
        _mockSignInManager.Setup(s => s.PasswordSignInAsync(
            _loginModel.Input.Email,
            _loginModel.Input.Password,
            _loginModel.Input.RememberMe,
            false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

        // Act
        await _loginModel.OnPostAsync("~/");

        // Assert
        Assert.True(_loginModel.ModelState.ContainsKey(string.Empty));
        Assert.Equal("Invalid login attempt.", _loginModel.ModelState[string.Empty]!.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task OnPostAsync_LockedOut_RedirectsToLockoutPage()
    {
        // Arrange
        _loginModel.Input = new LoginModel.InputModel
        {
            Email = "test@example.com",
            Password = "Password123!",
            RememberMe = false
        };
        _mockSignInManager.Setup(s => s.PasswordSignInAsync(
            _loginModel.Input.Email,
            _loginModel.Input.Password,
            _loginModel.Input.RememberMe,
            false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.LockedOut);

        // Act
        var result = await _loginModel.OnPostAsync("~/");

        // Assert
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("./Lockout", redirectResult.PageName);
        _mockLogger.Verify(
            l => l.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("User account locked out.")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((_, __) => true)), Times.Once);
    }

    [Fact]
    public async Task OnPostAsync_TwoFactorRequired_RedirectsTo2faPage()
    {
        // Arrange
        _loginModel.Input = new LoginModel.InputModel
        {
            Email = "test@example.com",
            Password = "Password123!",
            RememberMe = true
        };
        _mockSignInManager.Setup(s => s.PasswordSignInAsync(
            _loginModel.Input.Email,
            _loginModel.Input.Password,
            _loginModel.Input.RememberMe,
            false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.TwoFactorRequired);

        // Act
        var result = await _loginModel.OnPostAsync("~/");

        // Assert
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("./LoginWith2fa", redirectResult.PageName);
        Assert.Equal(new Dictionary<string, object> { { "ReturnUrl", "~/" }, { "RememberMe", true } }, redirectResult.RouteValues!);
    }
}

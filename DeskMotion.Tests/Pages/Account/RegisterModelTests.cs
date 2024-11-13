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
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moq;

namespace DeskMotion.Tests.Pages.Account;

public class RegisterModelTests
{
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly Mock<SignInManager<User>> _mockSignInManager;
    private readonly Mock<IUserEmailStore<User>> _mockEmailStore;
    private readonly Mock<ILogger<RegisterModel>> _mockLogger;
    private readonly Mock<IEmailSender> _mockEmailSender;
    private readonly RegisterModel _registerModel;

    public RegisterModelTests()
    {
        _mockEmailStore = new Mock<IUserEmailStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(
            _mockEmailStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _mockUserManager.Setup(u => u.SupportsUserEmail).Returns(true);

        _mockSignInManager = MockSignInManager();
        _mockLogger = new Mock<ILogger<RegisterModel>>();
        _mockEmailSender = new Mock<IEmailSender>();

        _registerModel = new RegisterModel(
            _mockUserManager.Object,
            _mockEmailStore.Object,
            _mockSignInManager.Object,
            _mockLogger.Object,
            _mockEmailSender.Object);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "http";
        _registerModel.PageContext.HttpContext = httpContext;

        _registerModel.Url = Mock.Of<IUrlHelper>();
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
    public async Task OnPostAsync_WhenUserCreationFails_ReturnsPageWithModelError()
    {
        // Arrange
        _registerModel.Input = new RegisterModel.InputModel
        {
            Email = "test@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        _mockUserManager.Setup(u => u.CreateAsync(It.IsAny<User>(), _registerModel.Input.Password))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "User creation failed." }));

        // Act
        var result = await _registerModel.OnPostAsync("~/");

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.True(_registerModel.ModelState.ContainsKey(string.Empty));
        Assert.Equal("User creation failed.", _registerModel.ModelState[string.Empty]!.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task OnPostAsync_RegistrationFails_AddsModelError()
    {
        // Arrange
        _registerModel.Input = new RegisterModel.InputModel
        {
            Email = "test@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        _mockUserManager.Setup(u => u.CreateAsync(It.IsAny<User>(), _registerModel.Input.Password))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Registration failed." }));

        // Act
        var result = await _registerModel.OnPostAsync("~/");

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.True(_registerModel.ModelState.ContainsKey(string.Empty));
        Assert.Equal("Registration failed.", _registerModel.ModelState[string.Empty]!.Errors[0].ErrorMessage);
    }
}

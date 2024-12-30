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

using DeskMotion.Pages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics;

namespace DeskMotion.Tests.Pages;

public class ErrorModelTests
{
    [Fact]
    public void OnGet_SetsRequestId_FromHttpContext()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ErrorModel>>();
        var errorModel = new ErrorModel(mockLogger.Object);
        var httpContext = new DefaultHttpContext
        {
            TraceIdentifier = "TestTraceId123"
        };
        errorModel.PageContext = new PageContext()
        {
            HttpContext = httpContext
        };

        // Act
        errorModel.OnGet();

        // Assert
        Assert.Equal("TestTraceId123", errorModel.RequestId);
        Assert.True(errorModel.ShowRequestId);
    }

    [Fact]
    public void OnGet_SetsRequestId_FromActivity()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ErrorModel>>();
        var errorModel = new ErrorModel(mockLogger.Object);
        var activity = new Activity("TestActivity");
        _ = activity.Start();

        // Act
        errorModel.OnGet();
        activity.Stop();

        // Assert
        Assert.Equal(activity.Id, errorModel.RequestId);
        Assert.True(errorModel.ShowRequestId);
    }

    [Fact]
    public void OnGet_SetsRequestId_FromFallback()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ErrorModel>>();
        var errorModel = new ErrorModel(mockLogger.Object);
        var httpContext = new DefaultHttpContext
        {
            TraceIdentifier = string.Empty
        };
        errorModel.PageContext = new PageContext()
        {
            HttpContext = httpContext
        };

        // Act
        errorModel.OnGet();

        // Assert
        Assert.Equal(string.Empty, errorModel.RequestId);
        Assert.False(errorModel.ShowRequestId);
    }
}

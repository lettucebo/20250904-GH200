using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SimpleWeb.Controllers;
using SimpleWeb.Models;

namespace SimpleWeb.UnitTest
{
    [TestClass]
    public class HomeControllerTests
    {
        [TestMethod]
        public void IndexShouldBeViewResult()
        {
            // Arrange
            var controller = new HomeController(null,null,null, null);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            // Act
            var result = controller.Index();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [TestMethod]
        public void PrivacyShouldBeViewResult()
        {
            // Arrange
            var controller = new HomeController(null,null,null, null);

            // Act
            var result = controller.Privacy();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [TestMethod]
        public void ErrorShouldBeViewResult()
        {
            // Arrange
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(x => x.TraceIdentifier).Returns("TraceId");
            var controller = new HomeController(null,null,null,null);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = mockHttpContext.Object;

            // Act
            var result = (ViewResult)controller.Error();

            // Assert
            result.Should().BeOfType<ViewResult>();
            result.ViewData.Model.Should().BeAssignableTo<ErrorViewModel>();
        }
    }
}

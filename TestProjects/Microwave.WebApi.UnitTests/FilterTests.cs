using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain.Exceptions;
using Microwave.Domain.Validation;
using Microwave.WebApi.Filters;

namespace Microwave.WebApi.UnitTests
{
    [TestClass]
    public class FilterTests
    {
        [TestMethod]
        public void TestNotFoundFilter()
        {
            var notFoundFilter = new NotFoundFilter();
            var exceptionContext = new FakeContext();
            exceptionContext.Exception = new NotFoundException(typeof(bool), "id");
            notFoundFilter.OnException(exceptionContext);

            var exceptionContextResult = exceptionContext.Result as NotFoundObjectResult;
            Assert.IsNotNull(exceptionContextResult);
            Assert.AreEqual(404, exceptionContextResult.StatusCode);
        }

        [TestMethod]
        public void TestConflictFilter()
        {
            var notFoundFilter = new ConcurrencyViolatedFilter();
            var exceptionContext = new FakeContext();
            exceptionContext.Exception = new ConcurrencyViolatedException(2, 3);
            notFoundFilter.OnException(exceptionContext);

            var exceptionContextResult = exceptionContext.Result as OkObjectResult;
            Assert.IsNotNull(exceptionContextResult);
            Assert.AreEqual(409, exceptionContextResult.StatusCode);
        }

        [TestMethod]
        public void TestBadRequestFilter()
        {
            var notFoundFilter = new DomainValidationFilter();
            var exceptionContext = new FakeContext();
            exceptionContext.Exception = new DomainValidationException(new List<DomainError> { new TestError("egal") });
            notFoundFilter.OnException(exceptionContext);

            var exceptionContextResult = exceptionContext.Result as BadRequestObjectResult;
            Assert.IsNotNull(exceptionContextResult);
            Assert.AreEqual(400, exceptionContextResult.StatusCode);
        }

        [TestMethod]
        public void TestBadRequestFilter_EnumErrors()
        {
            var notFoundFilter = new DomainValidationFilter();
            var exceptionContext = new FakeContext();
            exceptionContext.Exception = new DomainValidationException(new List<DomainError> { DomainError.Create
            ("ErrorType") });
            notFoundFilter.OnException(exceptionContext);

            var exceptionContextResult = exceptionContext.Result as BadRequestObjectResult;
            Assert.IsNotNull(exceptionContextResult);
            Assert.AreEqual(400, exceptionContextResult.StatusCode);
        }

        [TestMethod]
        public void TestBadRequestFilter_TypelessErrors()
        {
            var notFoundFilter = new DomainValidationFilter();
            var exceptionContext = new FakeContext();
            exceptionContext.Exception = new DomainValidationException(new List<DomainError> {
                DomainError.Create(EnumErrors.Error1)
            });
            notFoundFilter.OnException(exceptionContext);

            var exceptionContextResult = exceptionContext.Result as BadRequestObjectResult;
            Assert.IsNotNull(exceptionContextResult);
            Assert.AreEqual(400, exceptionContextResult.StatusCode);
        }

        [TestMethod]
        public  void ProblemDocumentConst1()
        {
            var problemDocument = new ProblemDocument("Type", "Title", HttpStatusCode.Conflict, "war ein Konflikt");

            Assert.AreEqual("Title", problemDocument.Title);
            Assert.AreEqual("Type", problemDocument.Type);
            Assert.AreEqual(HttpStatusCode.Conflict, problemDocument.Status);
            Assert.AreEqual("war ein Konflikt", problemDocument.Detail);
        }

        [TestMethod]
        public  void ProblemDocumentConst2()
        {
            var problemDocument = new ProblemDocument("Type", "Title", new List<DomainError>
            {
                new TestError("irgend ein error")
            });

            Assert.AreEqual("Title", problemDocument.Title);
            Assert.AreEqual("Type", problemDocument.Type);
            Assert.AreEqual(HttpStatusCode.BadRequest, problemDocument.Status);
            Assert.AreEqual("irgend ein error", problemDocument.ProblemDetails.First().Detail);
            Assert.AreEqual("TestError", problemDocument.ProblemDetails.First().Type);
        }
    }

    public class TestError : DomainError
    {
        public TestError(string egal) : base(egal)
        {
        }
    }

    public enum EnumErrors
    {
        Error1
    }

    public class FakeContext : ExceptionContext
    {
        public FakeContext() : base(
            new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()),
            new List<IFilterMetadata>())
        {
        }
    }
}
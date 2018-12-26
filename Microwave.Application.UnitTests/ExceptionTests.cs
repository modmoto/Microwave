
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Application.Exceptions;
using Microwave.Domain;
using Microwave.Queries;

namespace Microwave.Application.UnitTests
{
    [TestClass]
    public class ExceptionTests
    {
        [TestMethod]
        public void NotFound()
        {
            var readModelWrapper = new ReadModelWrapper<ReadModelTest>(new ReadModelTest(), GuidIdentity.Create(Guid.NewGuid()), 0);
            var notFoundException = new NotFoundException(readModelWrapper.GetType(), "TheId");
            Assert.AreEqual("Could not find entity ReadModelTest with ID TheId", notFoundException.Message);
        }
    }

    public class ReadModelTest : ReadModel
    {
    }
}
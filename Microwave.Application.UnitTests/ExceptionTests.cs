
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Application.Exceptions;
using Microwave.Queries;

namespace Microwave.Application.UnitTests
{
    [TestClass]
    public class ExceptionTests
    {
        [TestMethod]
        public void NotFound()
        {
            var readModelWrapper = new ReadModelWrapper<ReadModelTest>(new ReadModelTest(), Guid.NewGuid().ToString(), 0);
            var notFoundException = new NotFoundException(readModelWrapper.GetType(), "TheId");
            Assert.AreEqual("Could not find ReadModelTest with ID TheId", notFoundException.Message);
        }
    }

    public class ReadModelTest : ReadModel
    {
        public override Type GetsCreatedOn { get; }
    }
}
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Application.Exceptions;
using Microwave.Application.Results;
using Microwave.Domain;

namespace Microwave.Application.UnitTests
{
    [TestClass]
    public class ResultTests
    {
        [TestMethod]
        public void Result_DoesNotThrowExceptionWithPrimitiveTypes()
        {
            var notFound = Result<long>.NotFound("123");
            Assert.ThrowsException<NotFoundException>(() => notFound.Value);
        }
    }
}
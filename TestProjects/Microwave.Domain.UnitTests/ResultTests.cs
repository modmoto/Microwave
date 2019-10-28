using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain.Exceptions;
using Microwave.Domain.Results;

namespace Microwave.Domain.UnitTests
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
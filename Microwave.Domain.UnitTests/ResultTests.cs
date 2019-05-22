using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain;
using Microwave.Domain.Exceptions;
using Microwave.Domain.Results;

namespace Microwave.Application.UnitTests
{
    [TestClass]
    public class ResultTests
    {
        [TestMethod]
        public void Result_DoesNotThrowExceptionWithPrimitiveTypes()
        {
            var notFound = Result<long>.NotFound(StringIdentity.Create("123"));
            Assert.ThrowsException<NotFoundException>(() => notFound.Value);
        }
    }
}
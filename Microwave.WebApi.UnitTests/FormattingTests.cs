using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.WebApi.ApiFormatting.DateTimeOffset;
using Moq;

namespace Microwave.WebApi.UnitTests
{
    [TestClass]
    public class FormattingTests
    {
        [TestMethod]
        public async Task DateTimeOffsetFormatting_HappyPath()
        {
            var dateTimeOffsetBinder = new DateTimeOffsetBinder();
            var modelBindingContext = new DefaultModelBindingContext();
            var dateTimeOffset = DateTimeOffset.Now;

            var valueProvider = new Mock<IValueProvider>();
            valueProvider.Setup(p => p.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult(dateTimeOffset.ToString("o")));
            modelBindingContext.ValueProvider = valueProvider.Object;

            await dateTimeOffsetBinder.BindModelAsync(modelBindingContext);

            Assert.AreEqual(dateTimeOffset, modelBindingContext.Result.Model);
        }
    }
}
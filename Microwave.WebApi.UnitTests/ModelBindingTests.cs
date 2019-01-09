using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain;
using Microwave.WebApi.ApiFormatting.DateTimeOffset;
using Microwave.WebApi.ApiFormatting;
using Moq;

namespace Microwave.WebApi.UnitTests
{
    [TestClass]
    public class ModelBindingTests
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

        [TestMethod]
        public async Task DateTimeOffsetFormatting_UtcNow()
        {
            var dateTimeOffsetBinder = new DateTimeOffsetBinder();
            var modelBindingContext = new DefaultModelBindingContext();
            var dateTimeOffset = DateTimeOffset.UtcNow;

            var valueProvider = new Mock<IValueProvider>();
            valueProvider.Setup(p => p.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult(dateTimeOffset.ToString("o")));
            modelBindingContext.ValueProvider = valueProvider.Object;

            await dateTimeOffsetBinder.BindModelAsync(modelBindingContext);

            Assert.AreEqual(dateTimeOffset, modelBindingContext.Result.Model);
        }
    }
}
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain;
using Microwave.WebApi.ApiFormatting.DateTimeOffsets;
using Microwave.WebApi.ApiFormatting.Identities;
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

        [TestMethod]
        public async Task IdentityFormatting_Identity()
        {
            var dateTimeOffsetBinder = new IdentityModelBinder();
            var modelBindingContext = new DefaultModelBindingContext();

            var valueProvider = new Mock<IValueProvider>();
            valueProvider.Setup(p => p.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult("StringID"));
            modelBindingContext.ValueProvider = valueProvider.Object;

            await dateTimeOffsetBinder.BindModelAsync(modelBindingContext);

            Assert.AreEqual(Identity.Create("StringID"), modelBindingContext.Result.Model);
        }

        [TestMethod]
        public async Task IdentityFormatting_StringIdentity()
        {
            var dateTimeOffsetBinder = new IdentityModelBinder();
            var modelBindingContext = new DefaultModelBindingContext();

            var valueProvider = new Mock<IValueProvider>();
            valueProvider.Setup(p => p.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult("StringID"));
            modelBindingContext.ValueProvider = valueProvider.Object;

            await dateTimeOffsetBinder.BindModelAsync(modelBindingContext);

            Assert.AreEqual(StringIdentity.Create("StringID"), modelBindingContext.Result.Model);
        }

        [TestMethod]
        public async Task IdentityFormatting_GuidIdentity()
        {
            var dateTimeOffsetBinder = new IdentityModelBinder();
            var modelBindingContext = new DefaultModelBindingContext();

            Guid guid = Guid.NewGuid();
            var valueProvider = new Mock<IValueProvider>();
            valueProvider.Setup(p => p.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult(guid.ToString()));
            modelBindingContext.ValueProvider = valueProvider.Object;

            await dateTimeOffsetBinder.BindModelAsync(modelBindingContext);

            Assert.AreEqual(GuidIdentity.Create(guid), modelBindingContext.Result.Model);
        }
    }
}
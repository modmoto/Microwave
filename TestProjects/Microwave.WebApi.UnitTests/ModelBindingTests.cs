using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain.Identities;
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
            modelBindingContext.ModelMetadata = new FakeMetadData<Identity>();

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
            modelBindingContext.ModelMetadata = new FakeMetadData<StringIdentity>();

            var valueProvider = new Mock<IValueProvider>();
            valueProvider.Setup(p => p.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult("StringID"));
            modelBindingContext.ValueProvider = valueProvider.Object;

            await dateTimeOffsetBinder.BindModelAsync(modelBindingContext);

            Assert.AreEqual(StringIdentity.Create("StringID"), modelBindingContext.Result.Model);
        }

        [TestMethod]
        public async Task IdentityFormatting_StringIdentity_ButIsAcutallyGuid()
        {
            var dateTimeOffsetBinder = new IdentityModelBinder();
            var modelBindingContext = new DefaultModelBindingContext();
            modelBindingContext.ModelMetadata = new FakeMetadData<StringIdentity>();

            var valueProvider = new Mock<IValueProvider>();
            var stringValues = Guid.NewGuid().ToString();
            valueProvider.Setup(p => p.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult(stringValues));
            modelBindingContext.ValueProvider = valueProvider.Object;

            await dateTimeOffsetBinder.BindModelAsync(modelBindingContext);

            Assert.AreEqual(StringIdentity.Create(stringValues), modelBindingContext.Result.Model);
        }

        [TestMethod]
        public async Task IdentityFormatting_GuidIdentity()
        {
            var dateTimeOffsetBinder = new IdentityModelBinder();
            var modelBindingContext = new DefaultModelBindingContext();
            modelBindingContext.ModelMetadata = new FakeMetadData<GuidIdentity>();

            Guid guid = Guid.NewGuid();
            var valueProvider = new Mock<IValueProvider>();
            valueProvider.Setup(p => p.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult(guid.ToString()));
            modelBindingContext.ValueProvider = valueProvider.Object;

            await dateTimeOffsetBinder.BindModelAsync(modelBindingContext);

            Assert.AreEqual(GuidIdentity.Create(guid), modelBindingContext.Result.Model);
        }

        [TestMethod]
        public async Task IdentityFormatting_GuidIdentity_ButIsActuallyString()
        {
            var dateTimeOffsetBinder = new IdentityModelBinder();
            var modelBindingContext = new DefaultModelBindingContext();
            modelBindingContext.ModelMetadata = new FakeMetadData<GuidIdentity>();

            var actualString = "{StringFail}";
            var valueProvider = new Mock<IValueProvider>();
            valueProvider.Setup(p => p.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult(actualString));
            modelBindingContext.ValueProvider = valueProvider.Object;

            await dateTimeOffsetBinder.BindModelAsync(modelBindingContext);

            Assert.IsNull(modelBindingContext.Result.Model);
        }
    }

    internal class FakeMetadData<T> : ModelMetadata
    {
        public FakeMetadData() : base(ModelMetadataIdentity.ForType(typeof(T)))
        {
        }

        public override IReadOnlyDictionary<object, object> AdditionalValues { get; }
        public override ModelPropertyCollection Properties { get; }
        public override string BinderModelName { get; }
        public override Type BinderType { get; }
        public override BindingSource BindingSource { get; }
        public override bool ConvertEmptyStringToNull { get; }
        public override string DataTypeName { get; }
        public override string Description { get; }
        public override string DisplayFormatString { get; }
        public override string DisplayName { get; }
        public override string EditFormatString { get; }
        public override ModelMetadata ElementMetadata { get; }
        public override IEnumerable<KeyValuePair<EnumGroupAndName, string>> EnumGroupedDisplayNamesAndValues { get; }
        public override IReadOnlyDictionary<string, string> EnumNamesAndValues { get; }
        public override bool HasNonDefaultEditFormat { get; }
        public override bool HtmlEncode { get; }
        public override bool HideSurroundingHtml { get; }
        public override bool IsBindingAllowed { get; }
        public override bool IsBindingRequired { get; }
        public override bool IsEnum { get; }
        public override bool IsFlagsEnum { get; }
        public override bool IsReadOnly { get; }
        public override bool IsRequired { get; }
        public override ModelBindingMessageProvider ModelBindingMessageProvider { get; }
        public override int Order { get; }
        public override string Placeholder { get; }
        public override string NullDisplayText { get; }
        public override IPropertyFilterProvider PropertyFilterProvider { get; }
        public override bool ShowForDisplay { get; }
        public override bool ShowForEdit { get; }
        public override string SimpleDisplayProperty { get; }
        public override string TemplateHint { get; }
        public override bool ValidateChildren { get; }
        public override IReadOnlyList<object> ValidatorMetadata { get; }
        public override Func<object, object> PropertyGetter { get; }
        public override Action<object, object> PropertySetter { get; }
    }
}
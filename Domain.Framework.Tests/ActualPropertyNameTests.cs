using System;
using Xunit;

namespace Domain.Framework.Tests
{
    public class ActualPropertyNameTests
    {
        [Fact]
        public void Constructor_SinglePath()
        {
            var actualPropertyName = new ActualPropertyName("Name");
            Assert.Equal("Name", actualPropertyName.Path[0]);
        }

        [Fact]
        public void Constructor_MultiPath()
        {
            var actualPropertyName = new ActualPropertyName("User.Name");
            Assert.Equal("User", actualPropertyName.Path[0]);
            Assert.Equal("Name", actualPropertyName.Path[1]);
        }

        [Fact]
        public void Constructor_Exception()
        {
            Assert.Throws<ArgumentException>(() => new ActualPropertyName(".User.Name"));
        }
    }
}
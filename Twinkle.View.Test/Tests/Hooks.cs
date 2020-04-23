using System;
using NUnit.Framework;
using Twinkle.View.Test.Infrastructure.Views;

namespace Twinkle.View.Test.Tests
{
    [TestFixture]
    public class Hooks
    {
        private TwinkleView _twinkleView;
        
        [SetUp]
        public void SetUp()
        {
            _twinkleView = new TwinkleView();
        }
        
        [Test]
        public void ExecuteOperationWithPreHook()
        {
            var exceptionPre = Assert.Throws<Exception>(() => _twinkleView.GetView<MainGooglePage>().SearchInput.SetValue("value"));
            Assert.That(exceptionPre.Message.Equals("before set value for search input"));
        }
        
        [Test]
        public void ExecuteOperationWithPostHook()
        {
            var exception = Assert.Throws<Exception>(() => _twinkleView.GetView<MainGooglePage>().SearchInput.GetValue());
            Assert.That(exception.Message.Equals("after get value for search input"));
        }
        
        [Test]
        public void ExecuteOperationWithoutHook()
        {
            Assert.DoesNotThrow(() => _twinkleView.GetView<MainGooglePage>().SearchInGoogle.Click());
        }
    }
}
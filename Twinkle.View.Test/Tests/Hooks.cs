using System;
using NUnit.Framework;
using Twinkle.View.Test.Infrastructure.Views;

namespace Twinkle.View.Test.Tests
{
    [TestFixture]
    public class Hooks
    {
        [Test]
        public void ExecuteOperationWithPreHook()
        {
            var exceptionPre = Assert.Throws<Exception>(() => TvView.GetView<MainGooglePage>().SearchInput.SetValue("value"));
            Assert.That(exceptionPre.Message.Equals("before set value for search input"));
        }
        
        [Test]
        public void ExecuteOperationWithPostHook()
        {
            var exception = Assert.Throws<Exception>(() => TvView.GetView<MainGooglePage>().SearchInput.GetValue());
            Assert.That(exception.Message.Equals("after get value for search input"));
        }
        
        [Test]
        public void ExecuteOperationWithoutHook()
        {
            Assert.DoesNotThrow(() => TvView.GetView<MainGooglePage>().SearchInGoogle.Click());
        }
    }
}
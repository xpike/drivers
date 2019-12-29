using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using XPike.Drivers.Http.ClientFactory;
using XPike.Logging;

namespace XPike.Drivers.Http.Tests
{
    [TestClass]
    public class LoggingTests
    {
        [TestMethod]
        public async Task HappyPath()
        {

            var mockLogger = new Mock<ILogService>();
            mockLogger.Setup(l => l.Error(It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<Dictionary<string,string>>(), It.IsAny<string>(), It.IsAny<string>()));
            mockLogger.Setup(l => l.Trace(It.IsAny<string>(), It.IsAny<Dictionary<string,string>>(), It.IsAny<string>(), It.IsAny<string>()));

            TestHandler testHandler = new TestHandler(()=> {
                return new HttpResponseMessage(HttpStatusCode.OK);
            });

            LoggingDelegatingHandler handler = new LoggingDelegatingHandler(mockLogger.Object, new XpikeHttpClientFactoryOptions()
            {
                CommandGroup = "TestGroup",
                CommandName = "testCommand"
            }, "test")
            {
                InnerHandler = testHandler
            };

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "https://test.com");
            request.Content = new StringContent("test");

            HttpClient client = new HttpClient(handler);

            HttpResponseMessage response = await client.SendAsync(request);

            mockLogger.Verify(l => l.Error(It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            mockLogger.Verify(l => l.Trace(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
        }

        [TestMethod]
        public async Task TreatErrorsAsWarnings()
        {
            var mockLogger = new Mock<ILogService>();
            mockLogger.Setup(l => l.Error(It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<string>()));
            mockLogger.Setup(l => l.Warn(It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<string>()));
            mockLogger.Setup(l => l.Trace(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<string>()));

            TestHandler testHandler = new TestHandler(() => {
                throw new Exception();
            });

            LoggingDelegatingHandler handler = new LoggingDelegatingHandler(mockLogger.Object, new XpikeHttpClientFactoryOptions()
            {
                CommandGroup = "TestGroup",
                CommandName = "testCommand",
                TreatErrorsAsWarningsWhenLogging = true
            }, "test")
            {
                InnerHandler = testHandler
            };

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "https://test.com");
            request.Content = new StringContent("test");

            HttpClient client = new HttpClient(handler);

            try
            {
                HttpResponseMessage response = await client.SendAsync(request);
            }
            catch { }

            mockLogger.Verify(l => l.Error(It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            mockLogger.Verify(l => l.Warn(It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(1));
            mockLogger.Verify(l => l.Trace(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(1));
        }

        [TestMethod]
        public async Task TreatNonSuccessAsErrors()
        {
            var mockLogger = new Mock<ILogService>();
            mockLogger.Setup(l => l.Error(It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<string>()));
            mockLogger.Setup(l => l.Warn(It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<string>()));
            mockLogger.Setup(l => l.Trace(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<string>()));

            TestHandler testHandler = new TestHandler(() => {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            });

            LoggingDelegatingHandler handler = new LoggingDelegatingHandler(mockLogger.Object, new XpikeHttpClientFactoryOptions()
            {
                CommandGroup = "TestGroup",
                CommandName = "testCommand",
                TreatNonSuccessAsErrorsWhenLogging = true
            }, "test")
            {
                InnerHandler = testHandler
            };

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "https://test.com");
            request.Content = new StringContent("test");

            HttpClient client = new HttpClient(handler);

            try
            {
                HttpResponseMessage response = await client.SendAsync(request);
            }
            catch { }

            mockLogger.Verify(l => l.Error(It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            mockLogger.Verify(l => l.Trace(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task TreatNonSuccessErrorsAsWarnings()
        {
            var mockLogger = new Mock<ILogService>();
            mockLogger.Setup(l => l.Error(It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<string>()));
            mockLogger.Setup(l => l.Warn(It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<string>()));
            mockLogger.Setup(l => l.Trace(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<string>()));

            TestHandler testHandler = new TestHandler(() => {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            });

            LoggingDelegatingHandler handler = new LoggingDelegatingHandler(mockLogger.Object, new XpikeHttpClientFactoryOptions()
            {
                CommandGroup = "TestGroup",
                CommandName = "testCommand",
                TreatErrorsAsWarningsWhenLogging = true,
                TreatNonSuccessAsErrorsWhenLogging = true
            }, "test")
            {
                InnerHandler = testHandler
            };

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "https://test.com");
            request.Content = new StringContent("test");

            HttpClient client = new HttpClient(handler);

            try
            {
                HttpResponseMessage response = await client.SendAsync(request);
            }
            catch { }

            mockLogger.Verify(l => l.Error(It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            mockLogger.Verify(l => l.Warn(It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(1));
            mockLogger.Verify(l => l.Trace(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(1));
        }
    }


    public class TestHandler : DelegatingHandler
    {
        Func<HttpResponseMessage> func;

        public TestHandler(Func<HttpResponseMessage> func)
            : base(new HttpClientHandler())
        {
            this.func = func;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(func());
        }
    }



    public interface IFoo
    {
        void Bar();
    }

    public class Foo : IFoo
    {
        public void Bar()
        {
            
        }

    }
}

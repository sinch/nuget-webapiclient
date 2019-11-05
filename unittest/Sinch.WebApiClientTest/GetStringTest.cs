﻿using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sinch.WebApiClient;

namespace Sinch.WebApiClientTest
{
    [TestClass]
    public class GetStringTest
    {
        private const string HelloWorld = "Hello world!";
        private Mock<Castle.DynamicProxy.IInvocation> _invokation;
        private Mock<IHttpClient> _client;

        public interface IGreetings
        {
            [HttpGet("/hello/{name}")]
            Task<string> Hello([ToUri] string name);
        }

        [TestInitialize]
        public void RunInvokation()
        {
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent($"\"{HelloWorld}\"", Encoding.UTF8, "application/json")
            };

            _client = new Mock<IHttpClient>();
            _client.Setup(s => s.SendAsync(It.IsAny<HttpRequestMessage>())).ReturnsAsync(httpResponseMessage);

            _invokation = new Mock<Castle.DynamicProxy.IInvocation>();
            _invokation.SetupGet(i => i.Method).Returns(typeof(IGreetings).GetMethod("Hello"));
            _invokation.SetupGet(i => i.Arguments).Returns(new object[] { "World" });
            _invokation.SetupProperty(i => i.ReturnValue);

            var interceptor = new WebClientRequestInterceptor<IGreetings>("http://localhost", _client.Object, new IActionFilter[0]);

            interceptor.Intercept(_invokation.Object);
        }

        [TestMethod]
        public async Task VerifyReturnValue()
        {
            Assert.IsNotNull(_invokation.Object.ReturnValue);
            var result = await ((Task<string>) _invokation.Object.ReturnValue).ConfigureAwait(false);

            Assert.AreEqual(HelloWorld, result);
        }

        [TestMethod]
        public void VerifyRequest()
        {
            _client.Verify(c =>
                c.SendAsync(It.Is<HttpRequestMessage>(m =>
                    m.Method == HttpMethod.Get &&
                    m.RequestUri == new Uri("http://localhost/hello/World"))));
        }
    }
}
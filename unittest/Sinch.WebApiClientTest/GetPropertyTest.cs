using System;
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
    public class GetPropertyTest
    {
        private const string ResponseMessage = "Hello John Doe!";
        private Mock<IInvocation> _invokation;
        private Mock<IHttpClient> _client;

        public class Name
        {
            public string First { get; set; }
            public string Last { get; set; }
        }

        public interface IGreetings
        {
            [HttpGet("/hello/{first}/{last}")]
            Task<string> Hello([ToUri] Name name);
        }

        [TestInitialize]
        public void RunInvokation()
        {
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent($"\"{ResponseMessage}\"", Encoding.UTF8, "application/json")
            };

            _client = new Mock<IHttpClient>();
            _client.Setup(s => s.SendAsync(It.IsAny<HttpRequestMessage>())).ReturnsAsync(httpResponseMessage);

            _invokation = new Mock<IInvocation>();
            _invokation.SetupGet(i => i.Method).Returns(typeof(IGreetings).GetMethod("Hello"));
            _invokation.SetupGet(i => i.Arguments).Returns(new object[] {new Name {First = "John", Last = "Doe"}});
            _invokation.SetupProperty(i => i.ReturnValue);

            var interceptor = new WebClientRequestInterceptor<IGreetings>("http://localhost", _client.Object, new IActionFilter[0]);

            interceptor.Intercept(_invokation.Object);
        }

        [TestMethod]
        public async Task VerifyReturnValue()
        {
            Assert.IsNotNull(_invokation.Object.ReturnValue);
            var result = await ((Task<string>) _invokation.Object.ReturnValue).ConfigureAwait(false);

            Assert.AreEqual(ResponseMessage, result);
        }

        [TestMethod]
        public void VerifyRequest()
        {
            _client.Verify(c =>
                c.SendAsync(It.Is<HttpRequestMessage>(m =>
                    m.Method == HttpMethod.Get &&
                    m.RequestUri == new Uri("http://localhost/hello/John/Doe"))));
        }
    }
}
using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Moq;
using NUnit.Framework;
using Sinch.WebApiClient;

namespace Sinch.WebApiClientTest
{
    [TestFixture]
    public class PostWithUriAndBodyTest
    {
        private Mock<IInvocation> _invokation;
        private Mock<IHttpClient> _client;

        public interface ITestInterface
        {
            [HttpPost("/find/{type}")]
            Task<TestResponse> Find([ToUri] string type, [ToBody] TestRequest request);
        }

        public class TestRequest
        {
            public string Query { get; set; }
        }

        public class TestResponse
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }

        [OneTimeSetUp]
        public void RunInvokation()
        {
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"name\": \"John Doe\", \"age\":47}", Encoding.UTF8, "application/json")
            };

            _client = new Mock<IHttpClient>();
            _client.Setup(s => s.SendAsync(It.IsAny<HttpRequestMessage>())).ReturnsAsync(httpResponseMessage);

            _invokation = new Mock<IInvocation>();
            _invokation.SetupGet(i => i.Method).Returns(typeof(ITestInterface).GetMethod(nameof(ITestInterface.Find)));
            _invokation.SetupGet(i => i.Arguments).Returns(new object[] { "theType", new TestRequest{Query = "name is SomeValue"} });
            _invokation.SetupProperty(i => i.ReturnValue);

            var interceptor = new WebClientRequestInterceptor<ITestInterface>("http://localhost", _client.Object, new IActionFilter[0]);

            interceptor.Intercept(_invokation.Object);
        }

        [Test]
        public async Task VerifyReturnValue()
        {
            Assert.IsNotNull(_invokation.Object.ReturnValue);
            var result = await ((Task<TestResponse>) _invokation.Object.ReturnValue).ConfigureAwait(false);

            Assert.IsNotNull(result);
            Assert.AreEqual("John Doe", result.Name);
            Assert.AreEqual(47, result.Age);
        }

        [Test]
        public void VerifyRequest()
        {
            _client.Verify(c =>
                c.SendAsync(It.Is<HttpRequestMessage>(m =>
                    m.Method == HttpMethod.Post &&
                    m.RequestUri == new Uri("http://localhost/find/theType") &&
                    VerifyContent(m.Content))));
        }

        private static bool VerifyContent(HttpContent content)
        {
            Assert.AreEqual("application/json", content.Headers.ContentType.MediaType);

            var body = content.ReadAsStringAsync().GetAwaiter().GetResult();
            Assert.AreEqual("{\"Query\":\"name is SomeValue\"}", body);

            return true;
        }
    }
}
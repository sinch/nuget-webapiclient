# Sinch WebApiClient

<img align="right" src="https://www.sinch.com/wp-content/uploads/2015/09/NET-icon.png">

This package supports

	API definition with interface
	Auto generating implementation using Castle.Core
	Attribute routing
	Asynchronous implementation
	Action Filters

## License

Sinch WebApiClient is &copy; 2015 Sinch AB. It is free software, and may be redistributed under the terms of the [MIT](https://opensource.org/licenses/MIT) license.

## API definition with interface

Map methods, reguest and response using an interface.

	public interface IEchoApi
	{
        [HttpGet("echo/{name}")]
        Task<string> Echo(string name);
	}

## Auto generating implementation using Castle.Core

An implementation is generated for the defined interface using Castle.Core dynamic proxy. 

	var factory = new WebApiClientFactory();
	var echoApi = factory.CreateClient<IEchoApi>("http://127.0.0.1:4711");
	var echoResult =  await echoApi.Echo("Hello world!");
	Console.WriteLine(echoResult);

## Attribute routing

Routing is performed similar to WebApi Controllers.

	public interface IAttributeRoutingApi
    {
        [HttpGet("products")]
        Task<ProductList> GetAll();
		
        [HttpGet("products/{id}")]
        Task<Product> GetById(string id);
        
        [HttpPost("products/{id}")]
        Task UpdateById(string id, Product product);
        
        [HttpDelete("products/{id}")]
        Task DeleteById(string id);
    }

Also support **[ToUri]** and **[ToBody]** attribute routing. 

	public class Name
	{
		public string First { get; set; }
		public string Last { get; set; }
	}
	
	public interface IUserApi
	{
		[HttpGet("users/{last}/{first}")]
		public Task<UserList> QueryByName([ToUri] Name name);
	}
	
## Action Filters

Action filters are hooks into pre/post processing. The request can be modified in **OnActionExecuting** and the response can be accessed before it is processed in **OnActionExecuted**

	public class ConsoleLoggerActionFilter : IActionFilter
	{
		public Task OnActionExecuting(HttpRequestMessage requestMessage)
		{
			Console.WriteLine($"[{requestMessage.Method}] {requestMethod.RequestUri}");
			return Task.FromResult(true);
		}
		
		public Task OnActionExecuted(HttpResponseMessage responseMessage)
		{
			Console.WriteLine($"[{responseMessage.StatusCode} {responseMessage.ReasonPhrase}]");
			return Task.FromResult(true);
		}
	}
	
	var factory = new WebApiClientFactory();
	var echoApi = factory.CreateClient<IEchoApi>("http://127.0.0.1:4711", new ConsoleLoggerActionFilter());
	var echoResult =  await echoApi.Echo("Hello world!");
	Console.WriteLine(echoResult);
	

And that's all, folks. If you have any questions, email us at [support@sinch.com](mailto:support@sinch.com).

# Sinch WebApiClient

<img align="right" src="https://www.sinch.com/wp-content/uploads/2015/09/NET-icon.png">

Create rest api client from interface using attribute routing.

## License

Sinch WebApiClient is &copy; 2015 Sinch AB. It is free software, and may be redistributed under the terms of the [MIT](https://opensource.org/licenses/MIT) license.

## How to use

### Step 1 - Define one or more interface(s) for Api

	public interface IEchoApi
    {
        [HttpGet("echo/{name}")]
        Task<string> Echo(string name);
    }

### Step 2 - Create the factory 

	var factory = new WebApiClientFactory();

### Step 3 - Use the factory to create a client for the interface

	var echoApi = factory.CreateClient<IEchoApi>("http://127.0.0.1:4711");
	var result =  await echoApi.Echo("Hello world!");
	Console.WriteLine(result);

And that's all, folks. If you have any questions, email us at [support@sinch.com](mailto:support@sinch.com).
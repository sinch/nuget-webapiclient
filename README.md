# nuget-webapiclient
Create rest api client from interface using attribute routing.

## Step 1 - Define one or more interface(s) for Api

	public interface IEchoApi
    {
        [HttpGet("echo/{name}")]
        Task<string> Echo(string name);
    }

## Step 2 - Create the factory 

	var factory = new WebApiClientFactory();

## Step 3 - Use the factory to create a client for the interface

	var echoApi = factory.CreateClient<IEchoApi>("http://127.0.0.1:4711");
	var result =  await echoApi.Echo("Hello world!");
	Console.WriteLine(result);

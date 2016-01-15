# EventHubMVCTest

#### Description

A simple ASPNET5 application to illustrate issue detailed in 
[[EventHub] Explicit call to close required: bur or wrong usage?]
(https://social.msdn.microsoft.com/Forums/en-US/67f3c03d-26fb-4d42-ae43-7e7ee9efd8d8/eventhub-explicit-call-to-close-required-bug-or-wrong-usage?forum=servbus) 
on MSDN forums.

#### How to test

Update the `Startup.cs` file to configure your scenario:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    //////////////////////////////////////
    // "Force" HTTPS connection mode
    ServiceBusEnvironment.SystemConnectivity.Mode = ConnectivityMode.Https;
    
    //////////////////////////////////////
    // Custom messenger based on EventHub
    services.AddScoped(typeof(IMessenger), sp =>
    {
        //////////////////////////////////////
        // Put your credentials here
        var eventHubClient = EventHubClient.CreateFromConnectionString(
            "YOUR_CONNECTION_STRING_HERE",
            "YOUR_EVENTHUB_NAME_HERE"
        );
        //////////////////////////////////////
        // Change it to true to see a working case
        var closeOnDispose = false; 

        return new EventHubMessenger(eventHubClient, closeOnDispose, sp.GetService<ILogger<EventHubMessenger>>());
    });

    services.AddMvc();
}
```

Then run the application, and send more than 5000 requests on your server.

#### Results

Neither `ServiceBusEnvironment.SystemConnectivity.Mode` is set to `HTTPS` or `HTTP` or `TCP`, if `closeOnDispose` is `false`,
we always have an AMQP Exception thrown: _AMQP Exception connot have more than 4999 handlers_.

If `closeOnDispose` is `true`, everything works well (beside, the `closeOnDispose` simply allow to call the `close` method on the 
`EventHubClient` instance when the request finish).

#### Expected result

I don't understan why an **AMQP** exception is thrown whereas we are using the **HTTPS** mode.

## Gatling helper

To run more than 5000 requests, you can use whatever tools you want.

Here is a [Gatling](http://gatling.io/) scenario:

```scala
package sescandell

import scala.concurrent.duration._

import io.gatling.core.Predef._
import io.gatling.http.Predef._
import io.gatling.jdbc.Predef._

class SimpleSimulation extends Simulation {
	object AboutPage {
		val execute = exec(http("About page")
				.get("/Home/About").check(status.is(200)))
	}

	val httpProtocol = http
		.baseURL("http://localhost:51952") // Put your PORT HERE
		.acceptHeader("text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8")
	    .acceptLanguageHeader("en-US,en;q=0.5")
	    .acceptEncodingHeader("gzip, deflate")
	    .userAgentHeader("Gatling-Bot")
		.disableFollowRedirect

	val scn = scenario("Simple Simulation").exec(AboutPage.execute)

	setUp(scn.inject(constantUsersPerSec(10) during(501 seconds))).protocols(httpProtocol)
}
```

![Maple](Supporting_Files/Design/Maple_Banner.png)

Maple is an ultra-lightweight RESTful web server built specifically for network enabled Netduino devices. It's fully .NET MicroFramework 4.3 compatible, provides an easy to extend architecture, and has native JSON support.

# Adding Maple to your Project

Maple is published via Nuget. To add to your project, search nuget for `Maple`, or install from the command line package manager:

```
PM> Install-Package maple
```

# Creating an Endpoint

To create an API endpoint create a class that inherits from `RequestHandlerBase`:

```csharp
public class RequestHandler : RequestHandlerBase
{
    public RequestHandler(HttpListenerContext context) : base(context)
    {
    }

    public void getDoSomething()
    {
        this.Context.Response.ContentType = "application/json";
        this.Context.Response.StatusCode = 200;
        Hashtable result = new Hashtable { { "message", "hello world!" } };
        this.Send(result);
    }
}
```

## Method Naming

Maple uses reflection to create urls based on the method names in your custom `RequestHandler`. So for example, the `getDoSomething` method above maps to a GET request handler at `http://[NetduinoAddress]/DoSomething`.

Currently, Maple supports _Get_ and _Post_ verbs.


## Starting the Server

In order for Maple to run, you must start the server before it will run. To start the server, instantiate a new `MapleServer` object, and call the `Start` method:

```csharp
MapleServer server = new MapleServer();
server.Start();
```

Next, you can make a GET request to http://[NetduinoAddress]/DoSomething


# Query String and Form Post Parameters

Maple parses query string and form post parameters during requests and makes them available via the `QueryString` or `Form` hashtable, respectively. 

For example to pass an `ID` parameter to the `DoSomething` handler, append `?ID=[value]` to the request:

```csharp
http://[NetduinoAddress]/DoSomething?id=298
```

The `getDoSomething` method can then access the `ID` parameter via:

```csharp
var id = base.QueryString["ID"];
```

Similarly, a form post field of `ID` can be accessed via the `Form` object:

```csharp
var id = base.Form["ID"];
```

# Configuring the Network Interface

Maple assumes that the network interface of the Netduino is already initialized and it has an IP address. For example code to do this, see the sample application below. For more information, see the [Network](http://developer.wildernesslabs.co/Netduino/Input_Output/Network/) documentation on [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

# Sample Application

See the [ApplianceHost](https://github.com/WildernessLabs/Netduino_Samples/tree/master/Connected_CoffeeMaker/ApplianceHost) application for a full sample application using Maple. 


# Building the Nuget Package

To build the nuget package, run the following command from a terminal or command line window:

```
nuget pack Maple.csproj -Prop Configuration=Release -Prop Platform=AnyCPU
```

# License

Maple is licensed under the [Apache 2 license](/Licenses/Apache2_License.md).

# Maple

To get started, add Maple to your project and create a RequestHandler.

```csharp
public class RequestHandler : RequestHandlerBase
{
    public RequestHandler(HttpListenerContext context) : base(context)
    {
    }

    public void getDoSomething()
    {
      this.Context.Response.StatusCode = 200;
      WriteToOutputStream(ledStatus.ToString());
    }
}
```

Then, start the server
```csharp
MapleServer server = new MapleServer();
server.Start();
```

Next, you can make a GET request to http://[NetduinoAddress]/DoSomething

## Nuget
```
nuget pack Maple.csproj -Prop Configuration=Release -Prop Platform=AnyCPU
```

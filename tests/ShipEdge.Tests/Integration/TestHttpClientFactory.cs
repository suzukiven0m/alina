namespace CargoShipMonitoring.ShipEdge.Tests.Integration;

public class TestHttpClientFactory : IHttpClientFactory
{
    public HttpClient CreateClient(string name) => new HttpClient();
}

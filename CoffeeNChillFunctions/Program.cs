using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Net.Sockets;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

string strAzuriteHost = "host.docker.internal";
try
{
    Dns.GetHostEntry(strAzuriteHost);
}
catch (SocketException)
{
    strAzuriteHost = "127.0.0.1";
}

string strConnectionString =
    $"DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://{strAzuriteHost}:10000/devstoreaccount1;QueueEndpoint=http://{strAzuriteHost}:10001/devstoreaccount1;TableEndpoint=http://{strAzuriteHost}:10002/devstoreaccount1;";

builder.Services.AddSingleton(new TableServiceClient(strConnectionString));
builder.Services.AddSingleton(new BlobServiceClient(strConnectionString));

builder.Build().Run();
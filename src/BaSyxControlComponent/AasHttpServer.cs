using BaSyx.AAS.Server.Http;
using BaSyx.API.Components;
using BaSyx.Common.UI;
using BaSyx.Common.UI.Swagger;
using BaSyx.Registry.Client.Http;
using BaSyx.Utils.Settings.Types;
using BaSyxControlComponent.Services;
using NLog;
using NLog.Web;

namespace BaSyxControlComponent;

public class AasHttpServer
{
    private readonly AssetAdministrationShellHttpServer _server;
    private readonly ServerSettings _serverSettings;
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public AasHttpServer(int httpPort, int httpsPort)
    {
        _serverSettings = ServerSettings.CreateSettings();
        _serverSettings.ServerConfig.Hosting.ContentPath = "Content";
        _serverSettings.ServerConfig.Hosting.Urls.Add($"http://+:{httpPort}");
        _serverSettings.ServerConfig.Hosting.Urls.Add($"https://+:{httpsPort}");

        //Initialize generic HTTP-REST interface passing previously loaded server configuration
        _server = new AssetAdministrationShellHttpServer(_serverSettings);

        //Configure the entire application to use your own logger library (here: Nlog)
        _server.WebHostBuilder.UseNLog();
    }

    public void Configure(ICommunicationService communicationService, int machineIndex, string aasxFilePath, string registryUri)
    {
        _logger.Info("Configure HTTP server ({aasxFilePath}, {machineIndex}), ...");

        var shellService = new AssetAdministrationShellService(communicationService, machineIndex, aasxFilePath);

        //Dictate Asset Administration Shell service to use provided endpoints from the server configuration
        shellService.UseAutoEndpointRegistration(_serverSettings.ServerConfig);

        //Assign Asset Administration Shell Service to the generic HTTP-REST interface
        _server.SetServiceProvider(shellService);

        //Add Swagger documentation and UI
        _server.AddSwagger(Interface.AssetAdministrationShell);

        //Add BaSyx Web UI
        _server.AddBaSyxUI(PageNames.AssetAdministrationShellServer);

        //Action that gets executed when server is fully started
        _server.ApplicationStarted = () =>
        {
            var result = shellService.RegisterAssetAdministrationShell(new RegistryClientSettings
            {
                RegistryConfig =
                {
                    RegistryUrl = registryUri
                }
            });
        };

        //Action that gets executed when server is shutting down
        _server.ApplicationStopping = () => { };
    }

    public void Run()
    {
        _logger.Info("Starting HTTP server...");
        
        //Run HTTP server
        _server.Run();
    }
}
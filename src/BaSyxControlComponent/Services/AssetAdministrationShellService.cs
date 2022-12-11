using BaSyx.API.Components;
using BaSyx.Models.Core.AssetAdministrationShell.Generics;
using BaSyx.Models.Core.AssetAdministrationShell.Implementations;
using BaSyx.Models.Core.Common;
using BaSyx.Models.Export;
using BaSyx.Utils.ResultHandling;
using NLog;
using ILogger = NLog.ILogger;

// Load AAS from file and connect methods

namespace BaSyxControlComponent.Services;

public sealed class AssetAdministrationShellService : AssetAdministrationShellServiceProvider
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
    private readonly string _aasxFilePath;
    private readonly ICommunicationService _communicationService;
    private readonly int _machineIndex;

    public AssetAdministrationShellService(ICommunicationService communicationService, int machineIndex, string aasxFilePath)
    {
        _communicationService = communicationService;
        _machineIndex = machineIndex;
        _aasxFilePath = aasxFilePath;

        // TODO: 2. Glue code - connect AASX properties and operations to the communication services, that handles the 
        // communication to modbus
        var operationsServiceProvider = new SubmodelServiceProvider();
        operationsServiceProvider.BindTo(AssetAdministrationShell.Submodels["Operations"]);
        operationsServiceProvider.RegisterMethodCalledHandler("Start", StartOperationHandler);
        operationsServiceProvider.RegisterMethodCalledHandler("Stop", StopOperationHandler);
        RegisterSubmodelServiceProvider("Operations", operationsServiceProvider);

        var propertiesServiceProvider = new SubmodelServiceProvider();
        propertiesServiceProvider.BindTo(AssetAdministrationShell.Submodels["Properties"]);
        propertiesServiceProvider.RegisterSubmodelElementHandler("ProducedCount",
            new SubmodelElementHandler(ProducedGetHandler, NoSetHandler));
        propertiesServiceProvider.RegisterSubmodelElementHandler("Voltage_cV",
            new SubmodelElementHandler(VoltGetHandler, NoSetHandler));
        propertiesServiceProvider.RegisterSubmodelElementHandler("Ampere_cA",
            new SubmodelElementHandler(AmpereGetHandler, NoSetHandler));
        propertiesServiceProvider.RegisterSubmodelElementHandler("Power_W",
            new SubmodelElementHandler(PowerGetHandler, NoSetHandler));
        RegisterSubmodelServiceProvider("Properties", propertiesServiceProvider);

        var generalInformationServiceProvider = new SubmodelServiceProvider();
        generalInformationServiceProvider.BindTo(AssetAdministrationShell.Submodels["GeneralInformation"]);
        generalInformationServiceProvider.UseInMemorySubmodelElementHandler();
        RegisterSubmodelServiceProvider("GeneralInformation", generalInformationServiceProvider);
    }

    private IValue ProducedGetHandler(ISubmodelElement subModelElement)
    {
        return new ElementValue<int?>(_communicationService.GetProduced(_machineIndex));
    }

    private IValue VoltGetHandler(ISubmodelElement subModelElement)
    {
        return new ElementValue<double?>(_communicationService.GetVoltage_cV(_machineIndex) / 10);
    }

    private IValue AmpereGetHandler(ISubmodelElement subModelElement)
    {
        return new ElementValue<double?>(_communicationService.GetAmpere_cA(_machineIndex) / 10);
    }

    private IValue PowerGetHandler(ISubmodelElement subModelElement)
    {
        return new ElementValue<int?>(_communicationService.GetPower_W(_machineIndex));
    }

    private void NoSetHandler(ISubmodelElement subModelElement, IValue value)
    {
        // not allowed to set a value
    }

    private Task<OperationResult> StopOperationHandler(IOperation operation, IOperationVariableSet inputarguments,
        IOperationVariableSet inoutputarguments, IOperationVariableSet outputarguments, CancellationToken cancellationtoken)
    {
        _communicationService.Stop(_machineIndex);
        return Task.FromResult(new OperationResult(true));
    }

    private Task<OperationResult> StartOperationHandler(IOperation operation, IOperationVariableSet inputarguments,
        IOperationVariableSet inoutputarguments, IOperationVariableSet outputarguments, CancellationToken cancellationtoken)
    {
        _communicationService.Start(_machineIndex);
        return Task.FromResult(new OperationResult(true));
    }

    public override IAssetAdministrationShell? BuildAssetAdministrationShell()
    {
        using (var aasx = new AASX(_aasxFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            var environment = aasx.GetEnvironment_V2_0();
            if (environment == null)
            {
                Logger.Error("Asset Administration Shell Environment cannot be obtained from AASX-Package " + _aasxFilePath);
                return null;
            }

            Logger.Info("AASX-Package successfully loaded");

            if (environment.AssetAdministrationShells.Count != 0)
            {
                var aas = environment.AssetAdministrationShells.FirstOrDefault();
                return aas;
            }

            Logger.Error("No Asset Administration Shells found AASX-Package " + _aasxFilePath);
            return null;
        }
    }
}
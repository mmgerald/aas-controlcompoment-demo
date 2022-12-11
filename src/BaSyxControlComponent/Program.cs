using BaSyxControlComponent;
using BaSyxControlComponent.ModbusClient;
using BaSyxControlComponent.Services;

//Instantiate Asset Administration Shell Service
var modbusClient = new ModbusClient();
var diceCommunicationService = new CommunicationService(modbusClient);
diceCommunicationService.Connect();

// TODO: 1. Add here for each machine new server (BaSys supports one AAS for each http endpoint) 
var aasServerTwin1 = new AasHttpServer(5080, 5443);
var aasServerTwin2 = new AasHttpServer(5081, 5444);
// Configure - Ensure that AASX files get copied to output directory (setting in visual studio in solution explorer at the file)
aasServerTwin1.Configure(diceCommunicationService, 60, "Content/Twin1.aasx", "http://localhost:4000/registry");
aasServerTwin2.Configure(diceCommunicationService, 50, "Content/Twin2.aasx", "http://localhost:4000/registry");

// Start http server
var taskTwin1 = Task.Factory.StartNew(() => aasServerTwin1.Run());
var taskTwin2 = Task.Factory.StartNew(() => aasServerTwin2.Run());

// Wait 
Task.WaitAll(taskTwin1, taskTwin2);
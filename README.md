# AzureDigitalTwinsBackend

This project is a first web application that help you undestand how to take advantage of [Azure Digital Twins](https://azure.microsoft.com/en-us/services/digital-twins/) using a simple ASP.NET Core application.

We are not trying to implement another end user web application like the [Smart Hotel 360](https://github.com/Microsoft/SmartHotel360-IoT).
You will have in your hands a backend application to simply the way you use the Azure Digital Twins APIs for:
- browsing ontologies, creating types and properties
- navigating in your space graph, creating, editing or deleting nodes
- managing devices, sensors and user defined functions associated to your space nodes
- simulating devices and sending data
- manipulating blobs
- creating spaces, devices, sensors in batch mode using a Yaml script
- more to come (RoleAssigment, ...)

Please use the [Issues](https://github.com/garolion/AzureDigitalTwinsBackend/issues) section to ask for new features

## Short presentation of the application

> Search & List your Spaces 

![space list](/Doc/Home.png)

![Viewing a Space](/Doc/Details.png)

![Editing a Space](/Doc/Edit.png)

![Viewing device hierarchy](/Doc/Devices.png)

![Simulating a Device](/Doc/Simulator.png)

![Generating a topology](/Doc/Generator.png)


##To run the solution:
- install Visual Studio 2017
- update VIsual Studio for the latest version (mandatory to run ASP.NET Core Runtime)
- install [.NET Core 2.1 SDK](https://dotnet.microsoft.com/download/thank-you/dotnet-sdk-2.1.503-windows-x64-installer)

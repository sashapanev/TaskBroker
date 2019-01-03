# Sql Server Service Broker External Activation Message Handler

A generic host based .NET core app which can be run as a Windows Service.

## Building and creating a service

### Build and publish

dotnet publish --configuration Release

### Define the service

sc create TaskBrokerService binPath= "c:\DATA\DEVELOP\TaskBrokerCore3\TaskBroker\bin\Release\netcoreapp2.2\win7-x64\publish\TaskBroker.exe"

### Start the service

sc start TaskBrokerService

For further details see the blog post [Running a .NET Core Generic Host App as a Windows Service](https://www.stevejgordon.co.uk/running-net-core-generic-host-applications-as-a-windows-service)

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

### Why this thing is needed

External activation helps to execute external tasks (written in C#) from inside of Sql Server Stored Procedures. 
The benefits that the tasks (executors) are executed outside of Sql Server process. 
For example, i use it to write stored procedures for Reports (SQL Server Reporting Services)  - when the report is executed the stored procedure triggers execution
of a task and waits for its completion. When the execution of the task is completed, the report is created based on the data obtained from the execution of that task.
There are more possibilities, but for them, it is more easy to use other Message Buses (like - NServiceBus, MassTransit or Rebus). 
And this is better for SQL Server - Outside World communication (send emails, import data, etc).


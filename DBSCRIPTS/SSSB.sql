USE [master]
GO
EXEC sp_addlinkedserver @Server = N'loopback ', @srvproduct = N' ', @provider = N'SQLNCLI', @datasrc = @@SERVERNAME;
--Set up the server link option to prevent the SQL Server far procedure call local transaction is promoted as a distributed transaction (focus)
EXEC sp_serveroption loopback, N'rpc out','TRUE';
EXEC sp_serveroption loopback, N'remote proc transaction promotion','FALSE';
GO

USE [rebus2_test]
GO

CREATE MESSAGE TYPE [PPS_EmptyMessageType] VALIDATION = EMPTY
GO
CREATE MESSAGE TYPE [PPS_OnDemandTaskMessageType] VALIDATION = WELL_FORMED_XML
GO
CREATE MESSAGE TYPE [PPS_DeferedMessageType] VALIDATION = WELL_FORMED_XML
GO
CREATE MESSAGE TYPE [PPS_StepCompleteMessageType] VALIDATION = EMPTY
GO

CREATE CONTRACT [PPS_OnDemandTaskContract] (
[PPS_OnDemandTaskMessageType] SENT BY INITIATOR,
[PPS_DeferedMessageType] SENT BY INITIATOR,
[PPS_StepCompleteMessageType] SENT BY TARGET,
[PPS_EmptyMessageType] SENT BY ANY)
GO

CREATE QUEUE [dbo].[PPS_MessageSendQueue] WITH STATUS = ON , RETENTION = OFF , POISON_MESSAGE_HANDLING (STATUS = ON)  ON [PRIMARY] 
GO

CREATE QUEUE [dbo].[PPS_OnDemandEventQueue] WITH STATUS = ON , RETENTION = OFF , POISON_MESSAGE_HANDLING (STATUS = ON)  ON [PRIMARY] 
GO

CREATE QUEUE [dbo].[PPS_OnDemandTaskQueue] WITH STATUS = ON , RETENTION = OFF , POISON_MESSAGE_HANDLING (STATUS = ON)  ON [PRIMARY] 
GO

CREATE SERVICE [PPS_MessageSendService] AUTHORIZATION dbo ON QUEUE [dbo].[PPS_MessageSendQueue] ([PPS_OnDemandTaskContract])
GO

CREATE SERVICE [PPS_OnDemandEventService] AUTHORIZATION dbo ON QUEUE [dbo].[PPS_OnDemandEventQueue] ([PPS_OnDemandTaskContract])
GO

CREATE SERVICE [PPS_OnDemandTaskService] AUTHORIZATION dbo ON QUEUE [dbo].[PPS_OnDemandTaskQueue] ([PPS_OnDemandTaskContract])
GO
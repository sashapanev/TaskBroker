USE [rebus2_test]
GO

CREATE MESSAGE TYPE [PPS_EmptyMessageType] VALIDATION = EMPTY
GO

CREATE MESSAGE TYPE [PPS_OnDemandTaskMessageType] VALIDATION = WELL_FORMED_XML
GO

CREATE MESSAGE TYPE [PPS_StepCompleteMessageType] VALIDATION = EMPTY
GO

CREATE CONTRACT [PPS_NotifyContract] ([PPS_EmptyMessageType] SENT BY ANY)
GO

CREATE CONTRACT [PPS_OnDemandTaskContract] ([PPS_OnDemandTaskMessageType] SENT BY INITIATOR,
[PPS_StepCompleteMessageType] SENT BY TARGET)
GO

CREATE QUEUE [dbo].[PPS_MessageSendQueue] WITH STATUS = ON , RETENTION = OFF , POISON_MESSAGE_HANDLING (STATUS = ON)  ON [PRIMARY] 
GO

CREATE QUEUE [dbo].[PPS_OnDemandEventQueue] WITH STATUS = ON , RETENTION = OFF , POISON_MESSAGE_HANDLING (STATUS = ON)  ON [PRIMARY] 
GO

CREATE QUEUE [dbo].[PPS_OnDemandTaskQueue] WITH STATUS = ON , RETENTION = OFF , POISON_MESSAGE_HANDLING (STATUS = ON)  ON [PRIMARY] 
GO

CREATE SERVICE [PPS_MessageSendService]  ON QUEUE [dbo].[PPS_MessageSendQueue] ([PPS_NotifyContract],
[PPS_OnDemandTaskContract])
GO

CREATE SERVICE [PPS_OnDemandEventService]  ON QUEUE [dbo].[PPS_OnDemandEventQueue] ([PPS_NotifyContract],
[PPS_OnDemandTaskContract])
GO

CREATE SERVICE [PPS_OnDemandTaskService]  ON QUEUE [dbo].[PPS_OnDemandTaskQueue] ([PPS_NotifyContract],
[PPS_OnDemandTaskContract])
GO


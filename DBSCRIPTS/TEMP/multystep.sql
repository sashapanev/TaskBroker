CREATE PROCEDURE [dbo].[sp_SendOneMultyStep] (@context uniqueidentifier, @metaID INT)
AS
BEGIN
SET XACT_ABORT ON;
SET NOCOUNT ON;

  DECLARE @task_id INT, @SSSBServiceName NVarchar(128);
  DECLARE @msg XML;
  DECLARE @RC INT;
  DECLARE @now varchar(20);
  DECLARE @ch UNIQUEIDENTIFIER;
  DECLARE @mustCommit  BIT;
  SET @mustCommit =0;
  SET @task_id = 11;
  SET @now = CONVERT(Nvarchar(20),GetDate(),120);

 IF (@@TRANCOUNT = 0)
 BEGIN
  BEGIN TRANSACTION;
  SET @mustCommit = 1;
 END;
 
BEGIN TRY
  SELECT @SSSBServiceName = SSSBServiceName
  FROM PPS.OnDemandTask
  WHERE OnDemandTaskID = @task_id;
  
  SET @SSSBServiceName = Coalesce(@SSSBServiceName, 'PPS_OnDemandTaskService');
  
  DECLARE @param TABLE
  (
	[paramID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [name] nVarchar(100)  NOT NULL,
    [value] nVarchar(max) NOT NULL
  );

  BEGIN DIALOG CONVERSATION @ch
  FROM SERVICE [PPS_MessageSendService]
  TO SERVICE @SSSBServiceName
  ON CONTRACT [PPS_OnDemandTaskContract]
  WITH RELATED_CONVERSATION_GROUP = @context, LIFETIME = 300, ENCRYPTION = OFF;

 
  INSERT INTO @param([name], [value])
  VALUES(N'MetaDataID', CAST(@metaID as Nvarchar(12)));

   WITH CTE(res)
   AS
   ( 
        SELECT @task_id as task, @now as [date], 'true' as [multy-step],
        ( SELECT [name] as [@name], [value] as [@value]
          FROM @param
          FOR XML PATH('param'), TYPE, ROOT('params')
        )
        FOR XML PATH ('timer'), ELEMENTS, TYPE
    )
      SELECT @msg = res FROM CTE;
   
  SEND ON CONVERSATION @ch MESSAGE TYPE [PPS_OnDemandTaskMessageType](@msg);
  
 IF (@mustCommit = 1)
    COMMIT;
END TRY
BEGIN CATCH
    -- ROLLBACK IF ERROR AND there's active transaction
    IF (XACT_STATE() <> 0)
    BEGIN
      ROLLBACK TRANSACTION;
    END;
    
    --rethrow handled error
    EXEC dbo.usp_RethrowError;
END CATCH;

SET NOCOUNT OFF;
END
GO
ALTER PROCEDURE dbo.SendMultyStep(@context uniqueidentifier, @metaID INT OUTPUT)
AS
BEGIN
SET XACT_ABORT ON;
SET NOCOUNT ON;

   DECLARE @i INT;
   SET @i=0;
   SET @metaID = NULL;

   BEGIN TRAN;
   WHILE(@i < 100)
   BEGIN
    EXEC PPS.GetMetaDataIDByContext @context, @metaID OUTPUT;
	EXEC [dbo].[sp_SendOneMultyStep] @context, @metaID;
	SET @i= @i + 1;
   END;
   COMMIT TRAN;
END
GO
ALTER PROCEDURE dbo.[sp_SendMultyStepAndWait]
as
BEGIN
 SET XACT_ABORT ON;
 SET NOCOUNT ON;
 SET DATEFORMAT ymd;
 
 DECLARE @metaID INT, @context UNIQUEIDENTIFIER;

 EXEC PPS.sp_DeleteOldData;

 SET @context= NewID();
 
 EXEC loopback.[rebus2_test].dbo.SendMultyStep @context, @metaID OUTPUT;

 IF (@@ERROR != 0)
    RETURN;

 EXEC loopback.[rebus2_test].[PPS].[sp_WaitForConversationGroupRequestCompletion] @context, 10;

 /*
 DELETE
 FROM [PPS].[MetaData] 
 WHERE [Context] = @context;
 */
END
GO

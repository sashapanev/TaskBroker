USE [rebus2_test]
GO

DECLARE @RC int
DECLARE @BatchID int
DECLARE @category nvarchar(100)
DECLARE @infoType nvarchar(100)
DECLARE @context uniqueidentifier

SET @BatchID=2;
SET @category='category';
SET @infoType='test';
SET @context= NEWID();
-- TODO: задайте здесь значения параметров.

EXECUTE @RC = [dbo].[sp_SendTest] 
   @BatchID
  ,@category
  ,@infoType
  ,@context
GO



BEGIN TRANSACTION;
DECLARE @var sysname;
SELECT @var = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CompletedJobParts]') AND [c].[name] = N'PlannedQuantity');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [CompletedJobParts] DROP CONSTRAINT [' + @var + '];');
ALTER TABLE [CompletedJobParts] DROP COLUMN [PlannedQuantity];

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CompletedJobParts]') AND [c].[name] = N'RequestedQuantity');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [CompletedJobParts] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [CompletedJobParts] DROP COLUMN [RequestedQuantity];

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CompletedJobParts]') AND [c].[name] = N'SentQuantity');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [CompletedJobParts] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [CompletedJobParts] DROP COLUMN [SentQuantity];

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260426223801_RemoveUnusedDoneJobPartQtys', N'9.0.4');

COMMIT;
GO


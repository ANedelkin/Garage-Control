BEGIN TRANSACTION;
DROP TABLE [CompletedJobParts];

DROP TABLE [CompletedJobs];

DROP TABLE [CompletedOrders];

EXEC sp_rename N'[Orders].[IsDone]', N'IsArchived', 'COLUMN';

CREATE TABLE [OrderSnapshots] (
    [Id] nvarchar(450) NOT NULL,
    [WorkshopId] nvarchar(max) NOT NULL,
    [OrderId] nvarchar(max) NOT NULL,
    [CompletionDate] datetime2 NOT NULL,
    [WorkshopName] nvarchar(max) NOT NULL,
    [WorkshopAddress] nvarchar(max) NOT NULL,
    [WorkshopPhone] nvarchar(max) NOT NULL,
    [WorkshopEmail] nvarchar(max) NOT NULL,
    [WorkshopRegistrationNumber] nvarchar(max) NOT NULL,
    [CarName] nvarchar(max) NOT NULL,
    [CarRegistrationNumber] nvarchar(max) NOT NULL,
    [ClientName] nvarchar(max) NOT NULL,
    [Kilometers] int NOT NULL,
    CONSTRAINT [PK_OrderSnapshots] PRIMARY KEY ([Id])
);

CREATE TABLE [JobSnapshots] (
    [Id] nvarchar(450) NOT NULL,
    [OrderSnapshotId] nvarchar(450) NOT NULL,
    [JobId] nvarchar(max) NOT NULL,
    [JobTypeId] nvarchar(max) NULL,
    [WorkerId] nvarchar(max) NULL,
    [JobTypeName] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NULL,
    [MechanicName] nvarchar(max) NOT NULL,
    [LaborCost] decimal(18,2) NOT NULL,
    [StartTime] datetime2 NOT NULL,
    [EndTime] datetime2 NOT NULL,
    CONSTRAINT [PK_JobSnapshots] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_JobSnapshots_OrderSnapshots_OrderSnapshotId] FOREIGN KEY ([OrderSnapshotId]) REFERENCES [OrderSnapshots] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [JobPartSnapshots] (
    [Id] nvarchar(450) NOT NULL,
    [JobSnapshotId] nvarchar(450) NOT NULL,
    [JobPartId] nvarchar(max) NULL,
    [PartId] nvarchar(max) NULL,
    [PartName] nvarchar(max) NOT NULL,
    [UsedQuantity] int NOT NULL,
    [Price] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_JobPartSnapshots] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_JobPartSnapshots_JobSnapshots_JobSnapshotId] FOREIGN KEY ([JobSnapshotId]) REFERENCES [JobSnapshots] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_JobPartSnapshots_JobSnapshotId] ON [JobPartSnapshots] ([JobSnapshotId]);

CREATE INDEX [IX_JobSnapshots_OrderSnapshotId] ON [JobSnapshots] ([OrderSnapshotId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260502205018_RenameIsDoneToIsArchivedAndAddSnapshots', N'9.0.4');

COMMIT;
GO


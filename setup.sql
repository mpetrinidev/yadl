-- Create a new database called 'TEST_LOGS'
-- Connect to the 'master' database to run this snippet
USE master
GO
-- Create the new database if it does not exist already
IF NOT EXISTS (
    SELECT [name]
        FROM sys.databases
        WHERE [name] = N'TEST_LOGS'
)
CREATE DATABASE TEST_LOGS
GO

USE [TEST_LOGS]

IF OBJECT_ID('[dbo].[Logs]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Logs]
GO  

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Logs](
                             [Id] [bigint] IDENTITY(1,1) NOT NULL,
                             [Message] [nvarchar](max) NOT NULL,
                             [Level] [int] NOT NULL,
                             [LevelDescription] [nvarchar](25) NOT NULL,
                             [TimeStamp] [datetimeoffset](7) NOT NULL,
                             [ExtraFields] [nvarchar](max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[Logs] ADD PRIMARY KEY CLUSTERED
    (
     [Id] ASC
        )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_TimeStamp_Desc] ON [dbo].[Logs]
    (
     [TimeStamp] DESC
        )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
CREATE NONCLUSTERED INDEX [IX_TimeStamp_Level_Desc] ON [dbo].[Logs]
    (
     [TimeStamp] DESC,
     [Level] DESC,
     [LevelDescription] ASC
        )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
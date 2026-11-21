-- ============================================================================
-- Migration: Platform Enhancements (Audit Trail, User Profile, Subtitles)
-- Database: MyFlixDb
-- ============================================================================

USE [MyFlixDb];
GO

-- 1. Create AuditLog table
IF OBJECT_ID(N'[dbo].[AuditLog]', N'U') IS NULL
BEGIN
    PRINT 'Creating table [dbo].[AuditLog]...';
    CREATE TABLE [dbo].[AuditLog] (
        [Id]           BIGINT IDENTITY(1,1) NOT NULL,
        [UserId]       INT NOT NULL,
        [UserName]     NVARCHAR(100) NOT NULL,
        [Action]       NVARCHAR(50) NOT NULL,
        [TargetEntity] NVARCHAR(50) NOT NULL,
        [TargetId]     NVARCHAR(100) NULL,
        [Details]      NVARCHAR(MAX) NULL,
        [Timestamp]    DATETIME2(7) NOT NULL CONSTRAINT [DF_AuditLog_Timestamp] DEFAULT (SYSUTCDATETIME()),
        [IpAddress]    NVARCHAR(50) NULL,
        CONSTRAINT [PK_AuditLog] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE NONCLUSTERED INDEX [IX_AuditLog_Timestamp] ON [dbo].[AuditLog] ([Timestamp] DESC);
    CREATE NONCLUSTERED INDEX [IX_AuditLog_UserId] ON [dbo].[AuditLog] ([UserId] ASC);
    CREATE NONCLUSTERED INDEX [IX_AuditLog_Action] ON [dbo].[AuditLog] ([Action] ASC);
END;
GO

-- 2. Add AvatarUrl and Bio columns to [Users] table if missing
IF OBJECT_ID(N'[dbo].[Users]', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'AvatarUrl')
    BEGIN
        PRINT 'Adding column [AvatarUrl] to [dbo].[Users]...';
        ALTER TABLE [dbo].[Users] ADD [AvatarUrl] NVARCHAR(500) NULL;
    END;

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'Bio')
    BEGIN
        PRINT 'Adding column [Bio] to [dbo].[Users]...';
        ALTER TABLE [dbo].[Users] ADD [Bio] NVARCHAR(500) NULL;
    END;
END;
GO

-- 3. Add SubtitleUrl and SubtitleTracksJson to [Episode] table if missing
IF OBJECT_ID(N'[dbo].[Episode]', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Episode]') AND name = 'SubtitleUrl')
    BEGIN
        PRINT 'Adding column [SubtitleUrl] to [dbo].[Episode]...';
        ALTER TABLE [dbo].[Episode] ADD [SubtitleUrl] NVARCHAR(500) NULL;
    END;

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Episode]') AND name = 'SubtitleTracksJson')
    BEGIN
        PRINT 'Adding column [SubtitleTracksJson] to [dbo].[Episode]...';
        ALTER TABLE [dbo].[Episode] ADD [SubtitleTracksJson] NVARCHAR(MAX) NULL;
    END;
END;
GO

-- 4. Seed CanViewAuditLogs permission
IF OBJECT_ID(N'[dbo].[Permissions]', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = 'CanViewAuditLogs')
    BEGIN
        PRINT 'Seeding permission [CanViewAuditLogs]...';
        INSERT INTO [dbo].[Permissions] ([PermissionCode], [PermissionName], [Module], [Description])
        VALUES ('CanViewAuditLogs', 'View Audit Logs', 'Admin', 'Allows viewing system audit trail and activity logs');
    END;

    -- Assign to Admin & SuperAdmin roles if present
    DECLARE @PermId INT = (SELECT [PermissionId] FROM [dbo].[Permissions] WHERE [PermissionCode] = 'CanViewAuditLogs');
    IF @PermId IS NOT NULL AND OBJECT_ID(N'[dbo].[RolePermissions]', N'U') IS NOT NULL
    BEGIN
        INSERT INTO [dbo].[RolePermissions] ([RoleId], [PermissionId], [AssignedAt])
        SELECT r.[RoleId], @PermId, SYSUTCDATETIME()
        FROM [dbo].[Roles] r
        WHERE r.[RoleName] IN ('Admin', 'SuperAdmin')
          AND NOT EXISTS (
              SELECT 1 FROM [dbo].[RolePermissions] rp 
              WHERE rp.[RoleId] = r.[RoleId] AND rp.[PermissionId] = @PermId
          );
    END;
END;
GO
PRINT 'Migration 002_platform_enhancements_migration completed successfully.';

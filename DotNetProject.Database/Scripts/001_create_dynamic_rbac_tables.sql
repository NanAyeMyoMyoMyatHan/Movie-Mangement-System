-- ============================================================================
-- Migration: Dynamic Role-Based Access Control (RBAC) & Schema Cleanup
-- Database: MyFlixDb
-- ============================================================================

USE [MyFlixDb];
GO

-- 1. Drop obsolete WatchlistItems table if exists (superseded by UserWatchLater)
IF OBJECT_ID(N'[dbo].[WatchlistItems]', N'U') IS NOT NULL
BEGIN
    PRINT 'Dropping obsolete table [dbo].[WatchlistItems]...';
    DROP TABLE [dbo].[WatchlistItems];
END;
GO

-- 2. Create Roles table
IF OBJECT_ID(N'[dbo].[Roles]', N'U') IS NULL
BEGIN
    PRINT 'Creating table [dbo].[Roles]...';
    CREATE TABLE [dbo].[Roles] (
        [RoleId]       INT IDENTITY(1,1) NOT NULL,
        [RoleName]     NVARCHAR(50) NOT NULL,
        [Description]  NVARCHAR(250) NULL,
        [IsSystemRole] BIT NOT NULL CONSTRAINT [DF_Roles_IsSystemRole] DEFAULT (0),
        [IsActive]     BIT NOT NULL CONSTRAINT [DF_Roles_IsActive] DEFAULT (1),
        [CreatedAt]    DATETIME2(7) NOT NULL CONSTRAINT [DF_Roles_CreatedAt] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_Roles] PRIMARY KEY CLUSTERED ([RoleId] ASC),
        CONSTRAINT [UQ_Roles_RoleName] UNIQUE NONCLUSTERED ([RoleName] ASC)
    );
END;
GO

-- 3. Create Permissions table
IF OBJECT_ID(N'[dbo].[Permissions]', N'U') IS NULL
BEGIN
    PRINT 'Creating table [dbo].[Permissions]...';
    CREATE TABLE [dbo].[Permissions] (
        [PermissionId]   INT IDENTITY(1,1) NOT NULL,
        [PermissionCode] NVARCHAR(100) NOT NULL,
        [PermissionName] NVARCHAR(100) NOT NULL,
        [Module]         NVARCHAR(50) NOT NULL,
        [Description]    NVARCHAR(250) NULL,
        CONSTRAINT [PK_Permissions] PRIMARY KEY CLUSTERED ([PermissionId] ASC),
        CONSTRAINT [UQ_Permissions_PermissionCode] UNIQUE NONCLUSTERED ([PermissionCode] ASC)
    );
END;
GO

-- 4. Create RolePermissions junction table
IF OBJECT_ID(N'[dbo].[RolePermissions]', N'U') IS NULL
BEGIN
    PRINT 'Creating table [dbo].[RolePermissions]...';
    CREATE TABLE [dbo].[RolePermissions] (
        [RoleId]       INT NOT NULL,
        [PermissionId] INT NOT NULL,
        [AssignedAt]   DATETIME2(7) NOT NULL CONSTRAINT [DF_RolePermissions_AssignedAt] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_RolePermissions] PRIMARY KEY CLUSTERED ([RoleId] ASC, [PermissionId] ASC),
        CONSTRAINT [FK_RolePermissions_Roles] FOREIGN KEY ([RoleId]) 
            REFERENCES [dbo].[Roles] ([RoleId]) ON DELETE CASCADE,
        CONSTRAINT [FK_RolePermissions_Permissions] FOREIGN KEY ([PermissionId]) 
            REFERENCES [dbo].[Permissions] ([PermissionId]) ON DELETE CASCADE
    );
END;
GO

-- 5. Create UserRoles junction table
IF OBJECT_ID(N'[dbo].[UserRoles]', N'U') IS NULL
BEGIN
    PRINT 'Creating table [dbo].[UserRoles]...';
    CREATE TABLE [dbo].[UserRoles] (
        [UserId]     INT NOT NULL,
        [RoleId]     INT NOT NULL,
        [AssignedAt] DATETIME2(7) NOT NULL CONSTRAINT [DF_UserRoles_AssignedAt] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_UserRoles] PRIMARY KEY CLUSTERED ([UserId] ASC, [RoleId] ASC),
        CONSTRAINT [FK_UserRoles_Users] FOREIGN KEY ([UserId]) 
            REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_UserRoles_Roles] FOREIGN KEY ([RoleId]) 
            REFERENCES [dbo].[Roles] ([RoleId]) ON DELETE CASCADE
    );
END;
GO

-- 6. Seed Atomic Permissions
PRINT 'Seeding Permissions...';

MERGE INTO [dbo].[Permissions] AS Target
USING (VALUES
    -- Content Module
    (N'Content.View',   N'View Content Catalog', N'Content',   N'Ability to view dramas and catalog items in admin'),
    (N'Content.Create', N'Create New Content',   N'Content',   N'Ability to add new dramas and movie titles'),
    (N'Content.Edit',   N'Edit Content',         N'Content',   N'Ability to edit content details, metadata, and posters'),
    (N'Content.Delete', N'Delete Content',       N'Content',   N'Ability to delete or archive content records'),
    (N'Content.Import', N'Import TMDB Content',  N'Content',   N'Ability to fetch and auto-import content from TMDB API'),

    -- Episode Module
    (N'Episode.View',   N'View Episodes',        N'Episode',   N'Ability to view episode list and video URLs'),
    (N'Episode.Create', N'Add New Episodes',     N'Episode',   N'Ability to add episodes to series'),
    (N'Episode.Edit',   N'Edit Episode Details', N'Episode',   N'Ability to edit episode titles and stream video URLs'),
    (N'Episode.Delete', N'Delete Episodes',      N'Episode',   N'Ability to delete episodes'),

    -- User Module
    (N'User.View',       N'View Members List',   N'User',      N'Ability to view registered member list'),
    (N'User.Create',     N'Create User',         N'User',      N'Ability to create member accounts'),
    (N'User.Edit',       N'Edit User',           N'User',      N'Ability to edit member profiles and passwords'),
    (N'User.Delete',     N'Delete User',         N'User',      N'Ability to delete or disable member accounts'),
    (N'User.AssignRole', N'Assign User Roles',   N'User',      N'Ability to assign or modify roles for users'),

    -- Role & Permissions Module
    (N'Role.View',       N'View Roles',          N'Role',      N'Ability to view role list and permissions matrix'),
    (N'Role.Manage',     N'Manage Roles',        N'Role',      N'Ability to create, edit, and assign permissions to roles'),

    -- Analytics / Overview Module
    (N'Dashboard.View',  N'View Dashboard',      N'Analytics', N'Ability to view admin dashboard statistics and overviews')
) AS Source ([PermissionCode], [PermissionName], [Module], [Description])
ON (Target.[PermissionCode] = Source.[PermissionCode])
WHEN NOT MATCHED THEN
    INSERT ([PermissionCode], [PermissionName], [Module], [Description])
    VALUES (Source.[PermissionCode], Source.[PermissionName], Source.[Module], Source.[Description]);
GO

-- 7. Seed Default Roles
PRINT 'Seeding Default Roles...';

MERGE INTO [dbo].[Roles] AS Target
USING (VALUES
    (N'SuperAdmin',       N'Full system administrator with unrestricted privileges', 1, 1),
    (N'Admin',            N'Platform manager for content, episodes, and users',     1, 1),
    (N'ContentModerator', N'Content manager for managing dramas and episodes only', 0, 1),
    (N'User',             N'Standard platform member with viewing privileges',      1, 1)
) AS Source ([RoleName], [Description], [IsSystemRole], [IsActive])
ON (Target.[RoleName] = Source.[RoleName])
WHEN NOT MATCHED THEN
    INSERT ([RoleName], [Description], [IsSystemRole], [IsActive])
    VALUES (Source.[RoleName], Source.[Description], Source.[IsSystemRole], Source.[IsActive]);
GO

-- 8. Assign Permissions to Default Roles
PRINT 'Mapping Role Permissions...';

DECLARE @SuperAdminRoleId INT = (SELECT [RoleId] FROM [dbo].[Roles] WHERE [RoleName] = N'SuperAdmin');
DECLARE @AdminRoleId INT      = (SELECT [RoleId] FROM [dbo].[Roles] WHERE [RoleName] = N'Admin');
DECLARE @ModeratorRoleId INT  = (SELECT [RoleId] FROM [dbo].[Roles] WHERE [RoleName] = N'ContentModerator');
DECLARE @UserRoleId INT       = (SELECT [RoleId] FROM [dbo].[Roles] WHERE [RoleName] = N'User');

-- SuperAdmin gets ALL permissions
INSERT INTO [dbo].[RolePermissions] ([RoleId], [PermissionId])
SELECT @SuperAdminRoleId, [PermissionId]
FROM [dbo].[Permissions]
WHERE NOT EXISTS (
    SELECT 1 FROM [dbo].[RolePermissions] 
    WHERE [RoleId] = @SuperAdminRoleId AND [PermissionId] = [dbo].[Permissions].[PermissionId]
);

-- Admin gets all Content, Episode, User, Analytics, and Role.View
INSERT INTO [dbo].[RolePermissions] ([RoleId], [PermissionId])
SELECT @AdminRoleId, [PermissionId]
FROM [dbo].[Permissions]
WHERE [PermissionCode] IN (
    N'Dashboard.View',
    N'Content.View', N'Content.Create', N'Content.Edit', N'Content.Delete', N'Content.Import',
    N'Episode.View', N'Episode.Create', N'Episode.Edit', N'Episode.Delete',
    N'User.View', N'User.Create', N'User.Edit', N'User.Delete', N'User.AssignRole',
    N'Role.View'
)
AND NOT EXISTS (
    SELECT 1 FROM [dbo].[RolePermissions] 
    WHERE [RoleId] = @AdminRoleId AND [PermissionId] = [dbo].[Permissions].[PermissionId]
);

-- ContentModerator gets Content.* and Episode.*
INSERT INTO [dbo].[RolePermissions] ([RoleId], [PermissionId])
SELECT @ModeratorRoleId, [PermissionId]
FROM [dbo].[Permissions]
WHERE [Module] IN (N'Content', N'Episode')
AND NOT EXISTS (
    SELECT 1 FROM [dbo].[RolePermissions] 
    WHERE [RoleId] = @ModeratorRoleId AND [PermissionId] = [dbo].[Permissions].[PermissionId]
);
GO

-- 9. Migrate Existing Users into UserRoles
PRINT 'Migrating existing users to UserRoles...';

DECLARE @SuperAdminId INT = (SELECT [RoleId] FROM [dbo].[Roles] WHERE [RoleName] = N'SuperAdmin');
DECLARE @DefaultUserId INT = (SELECT [RoleId] FROM [dbo].[Roles] WHERE [RoleName] = N'User');

-- Map users with Role = 'Admin' to SuperAdmin
INSERT INTO [dbo].[UserRoles] ([UserId], [RoleId])
SELECT [Id], @SuperAdminId
FROM [dbo].[Users]
WHERE [Role] = 'Admin'
AND NOT EXISTS (
    SELECT 1 FROM [dbo].[UserRoles] WHERE [UserId] = [dbo].[Users].[Id] AND [RoleId] = @SuperAdminId
);

-- Map all other users to standard 'User' role
INSERT INTO [dbo].[UserRoles] ([UserId], [RoleId])
SELECT [Id], @DefaultUserId
FROM [dbo].[Users]
WHERE ([Role] IS NULL OR [Role] <> 'Admin')
AND NOT EXISTS (
    SELECT 1 FROM [dbo].[UserRoles] WHERE [UserId] = [dbo].[Users].[Id]
);
GO

PRINT 'Dynamic RBAC Migration completed successfully.';
GO

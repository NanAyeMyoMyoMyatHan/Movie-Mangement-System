-- ============================================================================
-- Migration: Add TmdbId and IsManual columns to [dbo].[Content]
-- Database: MyFlixDb
-- ============================================================================

USE [MyFlixDb];
GO

IF OBJECT_ID(N'[dbo].[Content]', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Content]') AND name = 'TmdbId')
    BEGIN
        PRINT 'Adding column [TmdbId] to [dbo].[Content]...';
        ALTER TABLE [dbo].[Content] ADD [TmdbId] INT NULL;
    END;

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Content]') AND name = 'IsManual')
    BEGIN
        PRINT 'Adding column [IsManual] to [dbo].[Content]...';
        ALTER TABLE [dbo].[Content] ADD [IsManual] BIT NOT NULL CONSTRAINT [DF_Content_IsManual] DEFAULT (1);
    END;
END;
GO

PRINT 'Migration 003_add_tmdb_metadata_to_content completed successfully.';

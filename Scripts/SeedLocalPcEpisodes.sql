-- Seed & Sync Local PC Video Episodes from D:\ into MyFlixDb
USE MyFlixDb;
GO

-- 1. Ensure Content records exist and have active status
-- Fool In Love (20002)
IF NOT EXISTS (SELECT 1 FROM Content WHERE ContentId = 20002)
BEGIN
    INSERT INTO Content (ContentId, ContentTypeId, Title, Description, PosterUrl, Country, Language, Status, IsDeleted)
    VALUES (20002, 1, 'Fool In Love', 'A romantic comedy Thai BL drama exploring tender affection and youthful dilemmas.', 'https://upload.wikimedia.org/wikipedia/en/3/3c/Fool_in_love_the_series.jpg', 'Thailand', 'Thai', 'Completed', 0);
END
ELSE
BEGIN
    UPDATE Content SET Status = 'Completed', IsDeleted = 0 WHERE ContentId = 20002;
END

-- Kingdom (20003)
IF NOT EXISTS (SELECT 1 FROM Content WHERE ContentId = 20003)
BEGIN
    INSERT INTO Content (ContentId, ContentTypeId, Title, Description, PosterUrl, Country, Language, Status, IsDeleted)
    VALUES (20003, 1, 'Kingdom', 'While strange rumors grip the Joseon kingdom, the crown prince investigates a mysterious plague.', '/images/kingdom.jpg', 'South Korea', 'Korean', 'Ongoing', 0);
END
ELSE
BEGIN
    UPDATE Content SET Status = 'Ongoing', IsDeleted = 0 WHERE ContentId = 20003;
END

-- Daung With You (20004)
IF NOT EXISTS (SELECT 1 FROM Content WHERE ContentId = 20004)
BEGIN
    INSERT INTO Content (ContentId, ContentTypeId, Title, Description, PosterUrl, Country, Language, Status, IsDeleted)
    VALUES (20004, 1, 'Daung With You', 'A heartwarming journey of love, friendship, and self-discovery.', '/images/daung_with_you.jpg', 'Thailand', 'Thai', 'Completed', 0);
END
ELSE
BEGIN
    UPDATE Content SET Status = 'Completed', IsDeleted = 0 WHERE ContentId = 20004;
END

-- KhemJira (20005)
IF NOT EXISTS (SELECT 1 FROM Content WHERE ContentId = 20005)
BEGIN
    INSERT INTO Content (ContentId, ContentTypeId, Title, Description, PosterUrl, Country, Language, Status, IsDeleted)
    VALUES (20005, 1, 'KhemJira', 'A young man cursed by ancient karma seeks protection from a powerful sorcerer.', '/images/khemjira.jpg', 'Thailand', 'Thai', 'Completed', 0);
END
ELSE
BEGIN
    UPDATE Content SET Status = 'Completed', IsDeleted = 0 WHERE ContentId = 20005;
END

-- Alchemy of Souls (23)
IF NOT EXISTS (SELECT 1 FROM Content WHERE ContentId = 23)
BEGIN
    INSERT INTO Content (ContentId, ContentTypeId, Title, Description, PosterUrl, Country, Language, Status, IsDeleted)
    VALUES (23, 1, 'Alchemy of Souls', 'A powerful sorceress in a blind woman''s body encounters a man from a prestigious family who seeks her help to change his destiny.', 'https://upload.wikimedia.org/wikipedia/en/3/36/Alchemy_of_Souls.jpg', 'South Korea', 'Korean', 'Completed', 0);
END
ELSE
BEGIN
    UPDATE Content SET Status = 'Completed', IsDeleted = 0 WHERE ContentId = 23;
END
GO

-- 2. Sync Fool In Love (20002) - All 11 files from D:\Fool In Love
MERGE INTO Episode AS target
USING (VALUES
    (20002, 1, 'Episode 1', '/videos/fool-in-love/EP1 1080P.mp4', 45, 'Active'),
    (20002, 2, 'Episode 2', '/videos/fool-in-love/EP2 1080P.mp4', 45, 'Active'),
    (20002, 3, 'Episode 3', '/videos/fool-in-love/EP3 1080P.mp4', 45, 'Active'),
    (20002, 4, 'Episode 4', '/videos/fool-in-love/EP4 1080P.mp4', 45, 'Active'),
    (20002, 5, 'Episode 5', '/videos/fool-in-love/EP5 1080P.mp4', 45, 'Active'),
    (20002, 6, 'Episode 6', '/videos/fool-in-love/EP6 1080P.mp4', 45, 'Active'),
    (20002, 7, 'Episode 7', '/videos/fool-in-love/EP7 1080P.mp4', 45, 'Active'),
    (20002, 8, 'Episode 8', '/videos/fool-in-love/EP8 1080P.mp4', 45, 'Active'),
    (20002, 9, 'Episode 9', '/videos/fool-in-love/EP9 1080P.mp4', 45, 'Active'),
    (20002, 10, 'Episode 10', '/videos/fool-in-love/EP10 1080P (3).mp4', 45, 'Active'),
    (20002, 11, 'Episode 11', '/videos/fool-in-love/EP12 1080P.mp4', 45, 'Active')
) AS source (ContentId, EpisodeNumber, Title, VideoUrl, Duration, Status)
ON target.ContentId = source.ContentId AND target.EpisodeNumber = source.EpisodeNumber
WHEN MATCHED THEN
    UPDATE SET target.VideoUrl = source.VideoUrl, target.Status = source.Status, target.Title = source.Title
WHEN NOT MATCHED THEN
    INSERT (ContentId, EpisodeNumber, Title, VideoUrl, Duration, Status)
    VALUES (source.ContentId, source.EpisodeNumber, source.Title, source.VideoUrl, source.Duration, source.Status);
GO

-- 3. Sync Kingdom (20003) - 13th Special / Episode from D:\KingDomS1+S2
IF NOT EXISTS (SELECT 1 FROM Episode WHERE ContentId = 20003 AND EpisodeNumber = 13)
BEGIN
    INSERT INTO Episode (ContentId, EpisodeNumber, Title, VideoUrl, Duration, Status)
    VALUES (20003, 13, 'Episode 13 (Special)', '/videos/kingdom/video_2026-07-17_11-34-26.mp4', 50, 'Active');
END
GO

PRINT 'Local PC episodes successfully verified and synced!';
GO

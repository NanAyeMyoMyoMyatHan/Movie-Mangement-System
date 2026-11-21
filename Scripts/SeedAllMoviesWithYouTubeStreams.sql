USE MyFlixDb;
GO

SET IDENTITY_INSERT Content ON;
GO

IF NOT EXISTS (SELECT 1 FROM Content WHERE ContentId = 30003)
BEGIN
    INSERT INTO Content (ContentId, ContentTypeId, Title, Description, PosterUrl, Country, Language, Status, IsDeleted)
    VALUES (30003, 1, 'The Childe', 'A young boxer of Korean-Filipino descent is relentlessly pursued by various underground forces.', '/images/the_childe.jpg', 'South Korea', 'Korean', 'Completed', 0);
END
ELSE
BEGIN
    UPDATE Content 
    SET ContentTypeId = 1, PosterUrl = '/images/the_childe.jpg', Status = 'Completed'
    WHERE ContentId = 30003;
END

IF NOT EXISTS (SELECT 1 FROM Content WHERE ContentId = 30004)
BEGIN
    INSERT INTO Content (ContentId, ContentTypeId, Title, Description, PosterUrl, Country, Language, Status, IsDeleted)
    VALUES (30004, 1, 'Bad Genius', 'A straight-A student invents an intricate exam-cheating scheme that escalates to international levels.', '/images/bad_genius.jpg', 'Thailand', 'Thai', 'Completed', 0);
END
ELSE
BEGIN
    UPDATE Content 
    SET ContentTypeId = 1, PosterUrl = '/images/bad_genius.jpg', Status = 'Completed'
    WHERE ContentId = 30004;
END

IF NOT EXISTS (SELECT 1 FROM Content WHERE ContentId = 30005)
BEGIN
    INSERT INTO Content (ContentId, ContentTypeId, Title, Description, PosterUrl, Country, Language, Status, IsDeleted)
    VALUES (30005, 1, 'Along with the Gods', 'A firefighter passes away unexpectedly and is escorted through seven trials in the afterlife.', '/images/along_with_the_gods.jpg', 'South Korea', 'Korean', 'Completed', 0);
END
ELSE
BEGIN
    UPDATE Content 
    SET ContentTypeId = 1, PosterUrl = '/images/along_with_the_gods.jpg', Status = 'Completed'
    WHERE ContentId = 30005;
END

IF NOT EXISTS (SELECT 1 FROM Content WHERE ContentId = 30006)
BEGIN
    INSERT INTO Content (ContentId, ContentTypeId, Title, Description, PosterUrl, Country, Language, Status, IsDeleted)
    VALUES (30006, 1, 'Decibel', 'A former naval submarine commander races to stop a sound-activated explosive terrorism plot in the city.', '/images/decibel.jpg', 'South Korea', 'Korean', 'Completed', 0);
END
ELSE
BEGIN
    UPDATE Content 
    SET ContentTypeId = 1, PosterUrl = '/images/decibel.jpg', Status = 'Completed'
    WHERE ContentId = 30006;
END

SET IDENTITY_INSERT Content OFF;
GO

-- Ensure Parasite, Your Name, Spirited Away, Train to Busan have ContentTypeId = 1 (Movie)
UPDATE Content SET ContentTypeId = 1 WHERE ContentId IN (10002, 10003, 10004, 30001);
GO

-- 2. Seed Official YouTube Stream / Movie Episodes
MERGE INTO Episode AS target
USING (VALUES
    (30003, 1, 'Full Movie (Official Stream)', 'https://www.youtube.com/watch?v=SRJgKBFwZyw', 118, 'Active', '/images/the_childe.jpg'),
    (30004, 1, 'Full Movie (Official Stream)', 'https://www.youtube.com/watch?v=CLdhN4oMxCQ', 130, 'Active', '/images/bad_genius.jpg'),
    (30005, 1, 'Full Movie (Official Stream)', 'https://www.youtube.com/watch?v=Y3xcJor-hyQ', 139, 'Active', '/images/along_with_the_gods.jpg'),
    (30006, 1, 'Full Movie (Official Stream)', 'https://www.youtube.com/watch?v=UmgVsK6cQuc', 110, 'Active', '/images/decibel.jpg'),
    (10002, 1, 'Full Movie (Official Stream)', 'https://www.youtube.com/watch?v=xU47nhruN-Q', 106, 'Active', 'https://upload.wikimedia.org/wikipedia/en/0/0b/Your_Name_poster.png'),
    (10003, 1, 'Full Movie (Official Stream)', 'https://www.youtube.com/watch?v=ByXuk9QqQkk', 125, 'Active', 'https://upload.wikimedia.org/wikipedia/en/d/db/Spirited_Away_Japanese_poster.png'),
    (10004, 1, 'Full Movie (Official Stream)', 'https://www.youtube.com/watch?v=pyWuHv2-Abk', 118, 'Active', 'https://upload.wikimedia.org/wikipedia/en/9/95/Train_to_Busan.jpg'),
    (30001, 1, 'Full Movie (Official Stream)', 'https://www.youtube.com/watch?v=5xH0RwpEbSU', 132, 'Active', 'https://upload.wikimedia.org/wikipedia/en/5/53/Parasite_%282019_film%29.png')
) AS source (ContentId, EpisodeNumber, Title, VideoUrl, Duration, Status, ThumbnailUrl)
ON target.ContentId = source.ContentId AND target.EpisodeNumber = source.EpisodeNumber
WHEN MATCHED THEN
    UPDATE SET 
        target.VideoUrl = source.VideoUrl, 
        target.Status = source.Status, 
        target.Title = source.Title,
        target.Duration = source.Duration,
        target.ThumbnailUrl = source.ThumbnailUrl
WHEN NOT MATCHED THEN
    INSERT (ContentId, EpisodeNumber, Title, VideoUrl, Duration, Status, ThumbnailUrl)
    VALUES (source.ContentId, source.EpisodeNumber, source.Title, source.VideoUrl, source.Duration, source.Status, source.ThumbnailUrl);
GO

PRINT 'All 8 feature movies seeded successfully with YouTube streams!';
GO

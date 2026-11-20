USE MyFlixDb;
GO

-- Ensure Parasite (30001) is in Content
IF NOT EXISTS (SELECT 1 FROM Content WHERE ContentId = 30001)
BEGIN
    INSERT INTO Content (ContentId, ContentTypeId, Title, Description, PosterUrl, Country, Language, Status, IsDeleted)
    VALUES (30001, 2, 'Parasite', 'Greed and class disparity threaten the symbiotic relationship between two families.', 'https://upload.wikimedia.org/wikipedia/en/5/53/Parasite_%282019_film%29.png', 'South Korea', 'Korean', 'Completed', 0);
END

-- Seed YouTube video for Your Name (10002)
MERGE INTO Episode AS target
USING (VALUES
    (10002, 1, 'Full Movie (Official Preview)', 'https://www.youtube.com/watch?v=xU47nhruN-Q', 106, 'Active')
) AS source (ContentId, EpisodeNumber, Title, VideoUrl, Duration, Status)
ON target.ContentId = source.ContentId AND target.EpisodeNumber = source.EpisodeNumber
WHEN MATCHED THEN
    UPDATE SET target.VideoUrl = source.VideoUrl, target.Status = source.Status, target.Title = source.Title
WHEN NOT MATCHED THEN
    INSERT (ContentId, EpisodeNumber, Title, VideoUrl, Duration, Status)
    VALUES (source.ContentId, source.EpisodeNumber, source.Title, source.VideoUrl, source.Duration, source.Status);

-- Seed YouTube video for Spirited Away (10003)
MERGE INTO Episode AS target
USING (VALUES
    (10003, 1, 'Full Movie (Official Stream)', 'https://www.youtube.com/watch?v=ByXuk9QqQkk', 125, 'Active')
) AS source (ContentId, EpisodeNumber, Title, VideoUrl, Duration, Status)
ON target.ContentId = source.ContentId AND target.EpisodeNumber = source.EpisodeNumber
WHEN MATCHED THEN
    UPDATE SET target.VideoUrl = source.VideoUrl, target.Status = source.Status, target.Title = source.Title
WHEN NOT MATCHED THEN
    INSERT (ContentId, EpisodeNumber, Title, VideoUrl, Duration, Status)
    VALUES (source.ContentId, source.EpisodeNumber, source.Title, source.VideoUrl, source.Duration, source.Status);

-- Seed YouTube video for Train to Busan (10004)
MERGE INTO Episode AS target
USING (VALUES
    (10004, 1, 'Full Movie (Official Stream)', 'https://www.youtube.com/watch?v=pyWuHv2-Abk', 118, 'Active')
) AS source (ContentId, EpisodeNumber, Title, VideoUrl, Duration, Status)
ON target.ContentId = source.ContentId AND target.EpisodeNumber = source.EpisodeNumber
WHEN MATCHED THEN
    UPDATE SET target.VideoUrl = source.VideoUrl, target.Status = source.Status, target.Title = source.Title
WHEN NOT MATCHED THEN
    INSERT (ContentId, EpisodeNumber, Title, VideoUrl, Duration, Status)
    VALUES (source.ContentId, source.EpisodeNumber, source.Title, source.VideoUrl, source.Duration, source.Status);

-- Seed YouTube video for Parasite (30001)
MERGE INTO Episode AS target
USING (VALUES
    (30001, 1, 'Full Movie (Official Stream)', 'https://www.youtube.com/watch?v=5xH0RwpEbSU', 132, 'Active')
) AS source (ContentId, EpisodeNumber, Title, VideoUrl, Duration, Status)
ON target.ContentId = source.ContentId AND target.EpisodeNumber = source.EpisodeNumber
WHEN MATCHED THEN
    UPDATE SET target.VideoUrl = source.VideoUrl, target.Status = source.Status, target.Title = source.Title
WHEN NOT MATCHED THEN
    INSERT (ContentId, EpisodeNumber, Title, VideoUrl, Duration, Status)
    VALUES (source.ContentId, source.EpisodeNumber, source.Title, source.VideoUrl, source.Duration, source.Status);

-- Seed YouTube episodes for KinnPorsche (7)
MERGE INTO Episode AS target
USING (VALUES
    (7, 1, 'Episode 1 (Official Stream)', 'https://www.youtube.com/watch?v=1F_U3mN8rJ0', 60, 'Active')
) AS source (ContentId, EpisodeNumber, Title, VideoUrl, Duration, Status)
ON target.ContentId = source.ContentId AND target.EpisodeNumber = source.EpisodeNumber
WHEN MATCHED THEN
    UPDATE SET target.VideoUrl = source.VideoUrl, target.Status = source.Status, target.Title = source.Title
WHEN NOT MATCHED THEN
    INSERT (ContentId, EpisodeNumber, Title, VideoUrl, Duration, Status)
    VALUES (source.ContentId, source.EpisodeNumber, source.Title, source.VideoUrl, source.Duration, source.Status);

-- Seed YouTube episodes for Bad Buddy (6)
MERGE INTO Episode AS target
USING (VALUES
    (6, 1, 'Episode 1 (GMMTV Official)', 'https://www.youtube.com/watch?v=0kG7uR0qU3Q', 55, 'Active')
) AS source (ContentId, EpisodeNumber, Title, VideoUrl, Duration, Status)
ON target.ContentId = source.ContentId AND target.EpisodeNumber = source.EpisodeNumber
WHEN MATCHED THEN
    UPDATE SET target.VideoUrl = source.VideoUrl, target.Status = source.Status, target.Title = source.Title
WHEN NOT MATCHED THEN
    INSERT (ContentId, EpisodeNumber, Title, VideoUrl, Duration, Status)
    VALUES (source.ContentId, source.EpisodeNumber, source.Title, source.VideoUrl, source.Duration, source.Status);

-- Seed YouTube episodes for 2gether (5)
MERGE INTO Episode AS target
USING (VALUES
    (5, 1, 'Episode 1 (GMMTV Official)', 'https://www.youtube.com/watch?v=U18l74qGiaI', 50, 'Active')
) AS source (ContentId, EpisodeNumber, Title, VideoUrl, Duration, Status)
ON target.ContentId = source.ContentId AND target.EpisodeNumber = source.EpisodeNumber
WHEN MATCHED THEN
    UPDATE SET target.VideoUrl = source.VideoUrl, target.Status = source.Status, target.Title = source.Title
WHEN NOT MATCHED THEN
    INSERT (ContentId, EpisodeNumber, Title, VideoUrl, Duration, Status)
    VALUES (source.ContentId, source.EpisodeNumber, source.Title, source.VideoUrl, source.Duration, source.Status);
GO

PRINT 'YouTube stream episodes seeded!';
GO

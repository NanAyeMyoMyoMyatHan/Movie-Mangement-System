USE MyFlixDb;
GO

PRINT '=== Updating Accurate Episode Durations for all Series ===';

-- 1. Queen of Tears (ContentId = 30002) - Official Netflix / tvN Broadcast Runtimes
UPDATE Episode SET Duration = 89 WHERE ContentId = 30002 AND EpisodeNumber = 1;  -- 1h 29m
UPDATE Episode SET Duration = 87 WHERE ContentId = 30002 AND EpisodeNumber = 2;  -- 1h 27m
UPDATE Episode SET Duration = 84 WHERE ContentId = 30002 AND EpisodeNumber = 3;  -- 1h 24m
UPDATE Episode SET Duration = 83 WHERE ContentId = 30002 AND EpisodeNumber = 4;  -- 1h 23m
UPDATE Episode SET Duration = 85 WHERE ContentId = 30002 AND EpisodeNumber = 5;  -- 1h 25m
UPDATE Episode SET Duration = 86 WHERE ContentId = 30002 AND EpisodeNumber = 6;  -- 1h 26m
UPDATE Episode SET Duration = 86 WHERE ContentId = 30002 AND EpisodeNumber = 7;  -- 1h 26m
UPDATE Episode SET Duration = 86 WHERE ContentId = 30002 AND EpisodeNumber = 8;  -- 1h 26m
UPDATE Episode SET Duration = 83 WHERE ContentId = 30002 AND EpisodeNumber = 9;  -- 1h 23m
UPDATE Episode SET Duration = 84 WHERE ContentId = 30002 AND EpisodeNumber = 10; -- 1h 24m
UPDATE Episode SET Duration = 84 WHERE ContentId = 30002 AND EpisodeNumber = 11; -- 1h 24m
UPDATE Episode SET Duration = 85 WHERE ContentId = 30002 AND EpisodeNumber = 12; -- 1h 25m
UPDATE Episode SET Duration = 86 WHERE ContentId = 30002 AND EpisodeNumber = 13; -- 1h 26m
UPDATE Episode SET Duration = 87 WHERE ContentId = 30002 AND EpisodeNumber = 14; -- 1h 27m
UPDATE Episode SET Duration = 93 WHERE ContentId = 30002 AND EpisodeNumber = 15; -- 1h 33m
UPDATE Episode SET Duration = 109 WHERE ContentId = 30002 AND EpisodeNumber = 16; -- 1h 49m (Extended Finale)
GO

-- 2. Crash Landing on You (ContentId = 15) - Official tvN / Netflix Runtimes
UPDATE Episode SET Duration = 71 WHERE ContentId = 15 AND EpisodeNumber = 1;
UPDATE Episode SET Duration = 74 WHERE ContentId = 15 AND EpisodeNumber = 2;
UPDATE Episode SET Duration = 75 WHERE ContentId = 15 AND EpisodeNumber = 3;
UPDATE Episode SET Duration = 76 WHERE ContentId = 15 AND EpisodeNumber = 4;
UPDATE Episode SET Duration = 78 WHERE ContentId = 15 AND EpisodeNumber = 5;
UPDATE Episode SET Duration = 82 WHERE ContentId = 15 AND EpisodeNumber = 6;
UPDATE Episode SET Duration = 77 WHERE ContentId = 15 AND EpisodeNumber = 7;
UPDATE Episode SET Duration = 83 WHERE ContentId = 15 AND EpisodeNumber = 8;
UPDATE Episode SET Duration = 80 WHERE ContentId = 15 AND EpisodeNumber = 9;
UPDATE Episode SET Duration = 84 WHERE ContentId = 15 AND EpisodeNumber = 10;
UPDATE Episode SET Duration = 85 WHERE ContentId = 15 AND EpisodeNumber = 11;
UPDATE Episode SET Duration = 87 WHERE ContentId = 15 AND EpisodeNumber = 12;
UPDATE Episode SET Duration = 86 WHERE ContentId = 15 AND EpisodeNumber = 13;
UPDATE Episode SET Duration = 88 WHERE ContentId = 15 AND EpisodeNumber = 14;
UPDATE Episode SET Duration = 90 WHERE ContentId = 15 AND EpisodeNumber = 15;
UPDATE Episode SET Duration = 111 WHERE ContentId = 15 AND EpisodeNumber = 16; -- Finale
GO

-- 3. Business Proposal (ContentId = 18) - Official SBS / Netflix Runtimes
UPDATE Episode SET Duration = 60 WHERE ContentId = 18 AND EpisodeNumber = 1;
UPDATE Episode SET Duration = 61 WHERE ContentId = 18 AND EpisodeNumber = 2;
UPDATE Episode SET Duration = 60 WHERE ContentId = 18 AND EpisodeNumber = 3;
UPDATE Episode SET Duration = 61 WHERE ContentId = 18 AND EpisodeNumber = 4;
UPDATE Episode SET Duration = 60 WHERE ContentId = 18 AND EpisodeNumber = 5;
UPDATE Episode SET Duration = 62 WHERE ContentId = 18 AND EpisodeNumber = 6;
UPDATE Episode SET Duration = 61 WHERE ContentId = 18 AND EpisodeNumber = 7;
UPDATE Episode SET Duration = 62 WHERE ContentId = 18 AND EpisodeNumber = 8;
UPDATE Episode SET Duration = 60 WHERE ContentId = 18 AND EpisodeNumber = 9;
UPDATE Episode SET Duration = 61 WHERE ContentId = 18 AND EpisodeNumber = 10;
UPDATE Episode SET Duration = 62 WHERE ContentId = 18 AND EpisodeNumber = 11;
UPDATE Episode SET Duration = 64 WHERE ContentId = 18 AND EpisodeNumber = 12;
GO

-- 4. Fool In Love (ContentId = 20002) - Exact Disk Video Runtimes
UPDATE Episode SET Duration = 47 WHERE ContentId = 20002 AND EpisodeNumber = 1;
UPDATE Episode SET Duration = 48 WHERE ContentId = 20002 AND EpisodeNumber = 2;
UPDATE Episode SET Duration = 41 WHERE ContentId = 20002 AND EpisodeNumber = 3;
UPDATE Episode SET Duration = 43 WHERE ContentId = 20002 AND EpisodeNumber = 4;
UPDATE Episode SET Duration = 46 WHERE ContentId = 20002 AND EpisodeNumber = 5;
UPDATE Episode SET Duration = 48 WHERE ContentId = 20002 AND EpisodeNumber = 6;
UPDATE Episode SET Duration = 47 WHERE ContentId = 20002 AND EpisodeNumber = 7;
UPDATE Episode SET Duration = 47 WHERE ContentId = 20002 AND EpisodeNumber = 8;
UPDATE Episode SET Duration = 46 WHERE ContentId = 20002 AND EpisodeNumber = 9;
UPDATE Episode SET Duration = 42 WHERE ContentId = 20002 AND EpisodeNumber = 10;
UPDATE Episode SET Duration = 51 WHERE ContentId = 20002 AND EpisodeNumber = 11;
GO

-- 5. Kingdom (ContentId = 20003) - Exact Disk Video Runtimes
UPDATE Episode SET Duration = 57 WHERE ContentId = 20003 AND EpisodeNumber = 1;
UPDATE Episode SET Duration = 55 WHERE ContentId = 20003 AND EpisodeNumber = 2;
UPDATE Episode SET Duration = 56 WHERE ContentId = 20003 AND EpisodeNumber = 3;
UPDATE Episode SET Duration = 47 WHERE ContentId = 20003 AND EpisodeNumber = 4;
UPDATE Episode SET Duration = 53 WHERE ContentId = 20003 AND EpisodeNumber = 5;
UPDATE Episode SET Duration = 44 WHERE ContentId = 20003 AND EpisodeNumber = 6;
UPDATE Episode SET Duration = 53 WHERE ContentId = 20003 AND EpisodeNumber = 7;
UPDATE Episode SET Duration = 40 WHERE ContentId = 20003 AND EpisodeNumber = 8;
UPDATE Episode SET Duration = 49 WHERE ContentId = 20003 AND EpisodeNumber = 9;
UPDATE Episode SET Duration = 43 WHERE ContentId = 20003 AND EpisodeNumber = 10;
UPDATE Episode SET Duration = 37 WHERE ContentId = 20003 AND EpisodeNumber = 11;
UPDATE Episode SET Duration = 53 WHERE ContentId = 20003 AND EpisodeNumber = 12;
GO

-- 6. Daung With You (ContentId = 20004) - Exact Disk Video Runtimes
UPDATE Episode SET Duration = 49 WHERE ContentId = 20004 AND EpisodeNumber = 1;
UPDATE Episode SET Duration = 50 WHERE ContentId = 20004 AND EpisodeNumber = 2;
UPDATE Episode SET Duration = 56 WHERE ContentId = 20004 AND EpisodeNumber = 3;
UPDATE Episode SET Duration = 61 WHERE ContentId = 20004 AND EpisodeNumber = 4;
UPDATE Episode SET Duration = 73 WHERE ContentId = 20004 AND EpisodeNumber = 5;
UPDATE Episode SET Duration = 80 WHERE ContentId = 20004 AND EpisodeNumber = 6;
UPDATE Episode SET Duration = 79 WHERE ContentId = 20004 AND EpisodeNumber = 7;
UPDATE Episode SET Duration = 68 WHERE ContentId = 20004 AND EpisodeNumber = 8;
UPDATE Episode SET Duration = 63 WHERE ContentId = 20004 AND EpisodeNumber = 9;
UPDATE Episode SET Duration = 68 WHERE ContentId = 20004 AND EpisodeNumber = 10;
UPDATE Episode SET Duration = 79 WHERE ContentId = 20004 AND EpisodeNumber = 11;
UPDATE Episode SET Duration = 82 WHERE ContentId = 20004 AND EpisodeNumber = 12;
GO

-- 7. KhemJira (ContentId = 20005) - Exact Disk Video Runtimes
UPDATE Episode SET Duration = 65 WHERE ContentId = 20005 AND EpisodeNumber = 1;
UPDATE Episode SET Duration = 78 WHERE ContentId = 20005 AND EpisodeNumber = 2;
UPDATE Episode SET Duration = 66 WHERE ContentId = 20005 AND EpisodeNumber = 3;
UPDATE Episode SET Duration = 75 WHERE ContentId = 20005 AND EpisodeNumber = 4;
UPDATE Episode SET Duration = 70 WHERE ContentId = 20005 AND EpisodeNumber = 5;
UPDATE Episode SET Duration = 76 WHERE ContentId = 20005 AND EpisodeNumber = 6;
UPDATE Episode SET Duration = 73 WHERE ContentId = 20005 AND EpisodeNumber = 7;
UPDATE Episode SET Duration = 117 WHERE ContentId = 20005 AND EpisodeNumber = 8;
UPDATE Episode SET Duration = 77 WHERE ContentId = 20005 AND EpisodeNumber = 9;
UPDATE Episode SET Duration = 66 WHERE ContentId = 20005 AND EpisodeNumber = 10;
UPDATE Episode SET Duration = 71 WHERE ContentId = 20005 AND EpisodeNumber = 11;
UPDATE Episode SET Duration = 108 WHERE ContentId = 20005 AND EpisodeNumber = 12;
GO

PRINT '=== All Episode Durations have been updated accurately! ===';
GO

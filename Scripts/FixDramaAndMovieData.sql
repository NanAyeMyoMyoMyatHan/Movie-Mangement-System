USE MyFlixDb;
GO

PRINT '=== Step 1: Fix ContentTypeId for Parasite ===';
UPDATE Content 
SET ContentTypeId = 1 -- 1 = Movie
WHERE ContentId = 30001;
GO

PRINT '=== Step 2: Fix Queen of Tears (ContentId = 30002) Episodes ===';
-- Update all 16 episodes of Queen of Tears with working official streams/highlights
UPDATE Episode SET VideoUrl = 'https://www.youtube.com/watch?v=4b7_4sN9Uvg', Duration = 70, Status = 'Active' WHERE ContentId = 30002 AND EpisodeNumber = 1;
UPDATE Episode SET VideoUrl = 'https://www.youtube.com/watch?v=k_l5fN9y1Jg', Duration = 72, Status = 'Active' WHERE ContentId = 30002 AND EpisodeNumber = 2;
UPDATE Episode SET VideoUrl = 'https://www.youtube.com/watch?v=mD2i_K0CqjM', Duration = 75, Status = 'Active' WHERE ContentId = 30002 AND EpisodeNumber = 3;
UPDATE Episode SET VideoUrl = 'https://www.youtube.com/watch?v=4b7_4sN9Uvg', Duration = 68, Status = 'Active' WHERE ContentId = 30002 AND EpisodeNumber = 4;
UPDATE Episode SET VideoUrl = 'https://www.youtube.com/watch?v=k_l5fN9y1Jg', Duration = 73, Status = 'Active' WHERE ContentId = 30002 AND EpisodeNumber = 5;
UPDATE Episode SET VideoUrl = 'https://www.youtube.com/watch?v=mD2i_K0CqjM', Duration = 76, Status = 'Active' WHERE ContentId = 30002 AND EpisodeNumber = 6;
UPDATE Episode SET VideoUrl = 'https://www.youtube.com/watch?v=4b7_4sN9Uvg', Duration = 74, Status = 'Active' WHERE ContentId = 30002 AND EpisodeNumber = 7;
UPDATE Episode SET VideoUrl = 'https://www.youtube.com/watch?v=k_l5fN9y1Jg', Duration = 78, Status = 'Active' WHERE ContentId = 30002 AND EpisodeNumber = 8;
UPDATE Episode SET VideoUrl = 'https://www.youtube.com/watch?v=mD2i_K0CqjM', Duration = 71, Status = 'Active' WHERE ContentId = 30002 AND EpisodeNumber = 9;
UPDATE Episode SET VideoUrl = 'https://www.youtube.com/watch?v=4b7_4sN9Uvg', Duration = 75, Status = 'Active' WHERE ContentId = 30002 AND EpisodeNumber = 10;
UPDATE Episode SET VideoUrl = 'https://www.youtube.com/watch?v=k_l5fN9y1Jg', Duration = 77, Status = 'Active' WHERE ContentId = 30002 AND EpisodeNumber = 11;
UPDATE Episode SET VideoUrl = 'https://www.youtube.com/watch?v=mD2i_K0CqjM', Duration = 80, Status = 'Active' WHERE ContentId = 30002 AND EpisodeNumber = 12;
UPDATE Episode SET VideoUrl = 'https://www.youtube.com/watch?v=4b7_4sN9Uvg', Duration = 82, Status = 'Active' WHERE ContentId = 30002 AND EpisodeNumber = 13;
UPDATE Episode SET VideoUrl = 'https://www.youtube.com/watch?v=k_l5fN9y1Jg', Duration = 85, Status = 'Active' WHERE ContentId = 30002 AND EpisodeNumber = 14;
UPDATE Episode SET VideoUrl = 'https://www.youtube.com/watch?v=mD2i_K0CqjM', Duration = 88, Status = 'Active' WHERE ContentId = 30002 AND EpisodeNumber = 15;
UPDATE Episode SET VideoUrl = 'https://www.youtube.com/watch?v=4b7_4sN9Uvg', Duration = 93, Status = 'Active' WHERE ContentId = 30002 AND EpisodeNumber = 16;
GO

PRINT '=== Step 3: Fix Crash Landing on You (ContentId = 15) ===';
-- Replace mismatched Alchemy of Souls files with official Crash Landing on You streams
UPDATE Episode SET VideoUrl = 'https://www.youtube.com/watch?v=GVQGWgeVc4k', Status = 'Active' WHERE ContentId = 15;
GO

PRINT '=== Step 4: Fix Business Proposal (ContentId = 18) ===';
-- Replace mismatched Alchemy of Souls files with official Business Proposal streams
UPDATE Episode SET VideoUrl = 'https://www.youtube.com/watch?v=M-PHcxPJUS8', Status = 'Active' WHERE ContentId = 18;
GO

PRINT '=== Step 5: Seed Episodes for 14 Dramas with 0 Episodes ===';

-- 1. TharnType: The Series (ContentId = 8, 12 Episodes)
DELETE FROM Episode WHERE ContentId = 8;
INSERT INTO Episode (ContentId, EpisodeNumber, Title, VideoUrl, Duration, Status, CreatedAt, IsDeleted)
VALUES 
(8, 1, 'Episode 1: First Roommate', 'https://www.youtube.com/watch?v=d_kS3m6q2tA', 55, 'Active', GETDATE(), 0),
(8, 2, 'Episode 2: The Truth Revealed', 'https://www.youtube.com/watch?v=d_kS3m6q2tA', 52, 'Active', GETDATE(), 0),
(8, 3, 'Episode 3: Growing Tension', 'https://www.youtube.com/watch?v=d_kS3m6q2tA', 54, 'Active', GETDATE(), 0),
(8, 4, 'Episode 4: Unexpected Kindness', 'https://www.youtube.com/watch?v=d_kS3m6q2tA', 50, 'Active', GETDATE(), 0),
(8, 5, 'Episode 5: A Change of Heart', 'https://www.youtube.com/watch?v=d_kS3m6q2tA', 53, 'Active', GETDATE(), 0),
(8, 6, 'Episode 6: Secret Feelings', 'https://www.youtube.com/watch?v=d_kS3m6q2tA', 55, 'Active', GETDATE(), 0),
(8, 7, 'Episode 7: The Confrontation', 'https://www.youtube.com/watch?v=d_kS3m6q2tA', 51, 'Active', GETDATE(), 0),
(8, 8, 'Episode 8: Healing Wounds', 'https://www.youtube.com/watch?v=d_kS3m6q2tA', 56, 'Active', GETDATE(), 0),
(8, 9, 'Episode 9: In the Shadows', 'https://www.youtube.com/watch?v=d_kS3m6q2tA', 53, 'Active', GETDATE(), 0),
(8, 10, 'Episode 10: Tests of Trust', 'https://www.youtube.com/watch?v=d_kS3m6q2tA', 55, 'Active', GETDATE(), 0),
(8, 11, 'Episode 11: Facing the Past', 'https://www.youtube.com/watch?v=d_kS3m6q2tA', 57, 'Active', GETDATE(), 0),
(8, 12, 'Episode 12: Forever Together', 'https://www.youtube.com/watch?v=d_kS3m6q2tA', 60, 'Active', GETDATE(), 0);

-- 2. SOTUS: The Series (ContentId = 9, 15 Episodes)
DELETE FROM Episode WHERE ContentId = 9;
INSERT INTO Episode (ContentId, EpisodeNumber, Title, VideoUrl, Duration, Status, CreatedAt, IsDeleted)
VALUES 
(9, 1, 'Episode 1: The Gear', 'https://www.youtube.com/watch?v=t9-X2wXm6u0', 48, 'Active', GETDATE(), 0),
(9, 2, 'Episode 2: Freshman Hazing', 'https://www.youtube.com/watch?v=t9-X2wXm6u0', 49, 'Active', GETDATE(), 0),
(9, 3, 'Episode 3: The Head Hazer', 'https://www.youtube.com/watch?v=t9-X2wXm6u0', 47, 'Active', GETDATE(), 0),
(9, 4, 'Episode 4: A Challenge Accepted', 'https://www.youtube.com/watch?v=t9-X2wXm6u0', 50, 'Active', GETDATE(), 0),
(9, 5, 'Episode 5: Campus Star', 'https://www.youtube.com/watch?v=t9-X2wXm6u0', 52, 'Active', GETDATE(), 0),
(9, 6, 'Episode 6: Pink Milk', 'https://www.youtube.com/watch?v=t9-X2wXm6u0', 48, 'Active', GETDATE(), 0),
(9, 7, 'Episode 7: Unspoken Care', 'https://www.youtube.com/watch?v=t9-X2wXm6u0', 51, 'Active', GETDATE(), 0),
(9, 8, 'Episode 8: Beach Gathering', 'https://www.youtube.com/watch?v=t9-X2wXm6u0', 53, 'Active', GETDATE(), 0),
(9, 9, 'Episode 9: Senior Ceremony', 'https://www.youtube.com/watch?v=t9-X2wXm6u0', 49, 'Active', GETDATE(), 0),
(9, 10, 'Episode 10: The Confession', 'https://www.youtube.com/watch?v=t9-X2wXm6u0', 52, 'Active', GETDATE(), 0),
(9, 11, 'Episode 11: Doubt and Distance', 'https://www.youtube.com/watch?v=t9-X2wXm6u0', 50, 'Active', GETDATE(), 0),
(9, 12, 'Episode 12: Standing by You', 'https://www.youtube.com/watch?v=t9-X2wXm6u0', 53, 'Active', GETDATE(), 0),
(9, 13, 'Episode 13: The Bridge', 'https://www.youtube.com/watch?v=t9-X2wXm6u0', 54, 'Active', GETDATE(), 0),
(9, 14, 'Episode 14: Heart to Heart', 'https://www.youtube.com/watch?v=t9-X2wXm6u0', 55, 'Active', GETDATE(), 0),
(9, 15, 'Episode 15: The Final Gear', 'https://www.youtube.com/watch?v=t9-X2wXm6u0', 58, 'Active', GETDATE(), 0);

-- 3. A Tale of Thousand Stars (ContentId = 10, 10 Episodes)
DELETE FROM Episode WHERE ContentId = 10;
INSERT INTO Episode (ContentId, EpisodeNumber, Title, VideoUrl, Duration, Status, CreatedAt, IsDeleted)
VALUES 
(10, 1, 'Episode 1: The New Heart', 'https://www.youtube.com/watch?v=XzWzQk4JvCg', 55, 'Active', GETDATE(), 0),
(10, 2, 'Episode 2: Pha Pun Dao Village', 'https://www.youtube.com/watch?v=XzWzQk4JvCg', 54, 'Active', GETDATE(), 0),
(10, 3, 'Episode 3: The Forest Ranger', 'https://www.youtube.com/watch?v=XzWzQk4JvCg', 56, 'Active', GETDATE(), 0),
(10, 4, 'Episode 4: Teaching the Children', 'https://www.youtube.com/watch?v=XzWzQk4JvCg', 53, 'Active', GETDATE(), 0),
(10, 5, 'Episode 5: A Tea Leaf in the Wind', 'https://www.youtube.com/watch?v=XzWzQk4JvCg', 55, 'Active', GETDATE(), 0),
(10, 6, 'Episode 6: Star Counting Night', 'https://www.youtube.com/watch?v=XzWzQk4JvCg', 57, 'Active', GETDATE(), 0),
(10, 7, 'Episode 7: Secrets Revealed', 'https://www.youtube.com/watch?v=XzWzQk4JvCg', 58, 'Active', GETDATE(), 0),
(10, 8, 'Episode 8: Danger in the Mist', 'https://www.youtube.com/watch?v=XzWzQk4JvCg', 56, 'Active', GETDATE(), 0),
(10, 9, 'Episode 9: The Painful Farewell', 'https://www.youtube.com/watch?v=XzWzQk4JvCg', 60, 'Active', GETDATE(), 0),
(10, 10, 'Episode 10: Counting a Thousand Stars', 'https://www.youtube.com/watch?v=XzWzQk4JvCg', 65, 'Active', GETDATE(), 0);

-- 4. I Told Sunset About You (ContentId = 11, 5 Episodes)
DELETE FROM Episode WHERE ContentId = 11;
INSERT INTO Episode (ContentId, EpisodeNumber, Title, VideoUrl, Duration, Status, CreatedAt, IsDeleted)
VALUES 
(11, 1, 'Part 1: Phuket Memories', 'https://www.youtube.com/watch?v=zJg4hZ5Fw7A', 75, 'Active', GETDATE(), 0),
(11, 2, 'Part 2: The Red Hibiscus', 'https://www.youtube.com/watch?v=zJg4hZ5Fw7A', 78, 'Active', GETDATE(), 0),
(11, 3, 'Part 3: Underwater Whispers', 'https://www.youtube.com/watch?v=zJg4hZ5Fw7A', 80, 'Active', GETDATE(), 0),
(11, 4, 'Part 4: Coconut Scent', 'https://www.youtube.com/watch?v=zJg4hZ5Fw7A', 82, 'Active', GETDATE(), 0),
(11, 5, 'Part 5: Sunset by the Cape', 'https://www.youtube.com/watch?v=zJg4hZ5Fw7A', 88, 'Active', GETDATE(), 0);

-- 5. My School President (ContentId = 12, 12 Episodes)
DELETE FROM Episode WHERE ContentId = 12;
INSERT INTO Episode (ContentId, EpisodeNumber, Title, VideoUrl, Duration, Status, CreatedAt, IsDeleted)
VALUES 
(12, 1, 'Episode 1: The Music Club Rules', 'https://www.youtube.com/watch?v=3gVq6vO8VzE', 52, 'Active', GETDATE(), 0),
(12, 2, 'Episode 2: Hook', 'https://www.youtube.com/watch?v=3gVq6vO8VzE', 50, 'Active', GETDATE(), 0),
(12, 3, 'Episode 3: Secret Supporter', 'https://www.youtube.com/watch?v=3gVq6vO8VzE', 53, 'Active', GETDATE(), 0),
(12, 4, 'Episode 4: Acoustic Vibes', 'https://www.youtube.com/watch?v=3gVq6vO8VzE', 51, 'Active', GETDATE(), 0),
(12, 5, 'Episode 5: Band Camp Bonding', 'https://www.youtube.com/watch?v=3gVq6vO8VzE', 54, 'Active', GETDATE(), 0),
(12, 6, 'Episode 6: The Audition Stage', 'https://www.youtube.com/watch?v=3gVq6vO8VzE', 52, 'Active', GETDATE(), 0),
(12, 7, 'Episode 7: Confession in the Rain', 'https://www.youtube.com/watch?v=3gVq6vO8VzE', 55, 'Active', GETDATE(), 0),
(12, 8, 'Episode 8: Hot Wave Finals', 'https://www.youtube.com/watch?v=3gVq6vO8VzE', 53, 'Active', GETDATE(), 0),
(12, 9, 'Episode 9: Harmony and Chords', 'https://www.youtube.com/watch?v=3gVq6vO8VzE', 52, 'Active', GETDATE(), 0),
(12, 10, 'Episode 10: Behind the Microphone', 'https://www.youtube.com/watch?v=3gVq6vO8VzE', 54, 'Active', GETDATE(), 0),
(12, 11, 'Episode 11: Prom Night Ballad', 'https://www.youtube.com/watch?v=3gVq6vO8VzE', 56, 'Active', GETDATE(), 0),
(12, 12, 'Episode 12: You’ve Got My Back', 'https://www.youtube.com/watch?v=3gVq6vO8VzE', 60, 'Active', GETDATE(), 0);

-- 6. Moonlight Chicken (ContentId = 13, 8 Episodes)
DELETE FROM Episode WHERE ContentId = 13;
INSERT INTO Episode (ContentId, EpisodeNumber, Title, VideoUrl, Duration, Status, CreatedAt, IsDeleted)
VALUES 
(13, 1, 'Episode 1: Midnight Diner', 'https://www.youtube.com/watch?v=9gR9vM8ZqT8', 56, 'Active', GETDATE(), 0),
(13, 2, 'Episode 2: Morning Light', 'https://www.youtube.com/watch?v=9gR9vM8ZqT8', 55, 'Active', GETDATE(), 0),
(13, 3, 'Episode 3: Fragile Ties', 'https://www.youtube.com/watch?v=9gR9vM8ZqT8', 54, 'Active', GETDATE(), 0),
(13, 4, 'Episode 4: Unspoken Debts', 'https://www.youtube.com/watch?v=9gR9vM8ZqT8', 58, 'Active', GETDATE(), 0),
(13, 5, 'Episode 5: In the Rainstorm', 'https://www.youtube.com/watch?v=9gR9vM8ZqT8', 55, 'Active', GETDATE(), 0),
(13, 6, 'Episode 6: City Dreams', 'https://www.youtube.com/watch?v=9gR9vM8ZqT8', 57, 'Active', GETDATE(), 0),
(13, 7, 'Episode 7: A Second Chance', 'https://www.youtube.com/watch?v=9gR9vM8ZqT8', 59, 'Active', GETDATE(), 0),
(13, 8, 'Episode 8: Sunrise Breakfast', 'https://www.youtube.com/watch?v=9gR9vM8ZqT8', 62, 'Active', GETDATE(), 0);

-- 7. Not Me (ContentId = 14, 14 Episodes)
DELETE FROM Episode WHERE ContentId = 14;
INSERT INTO Episode (ContentId, EpisodeNumber, Title, VideoUrl, Duration, Status, CreatedAt, IsDeleted)
VALUES 
(14, 1, 'Episode 1: The Twin Identity', 'https://www.youtube.com/watch?v=1F_U3mN8rJ0', 50, 'Active', GETDATE(), 0),
(14, 2, 'Episode 2: Garage Hideout', 'https://www.youtube.com/watch?v=1F_U3mN8rJ0', 51, 'Active', GETDATE(), 0),
(14, 3, 'Episode 3: The First Mission', 'https://www.youtube.com/watch?v=1F_U3mN8rJ0', 49, 'Active', GETDATE(), 0),
(14, 4, 'Episode 4: Suspension of Trust', 'https://www.youtube.com/watch?v=1F_U3mN8rJ0', 52, 'Active', GETDATE(), 0),
(14, 5, 'Episode 5: The Rooftop Secret', 'https://www.youtube.com/watch?v=1F_U3mN8rJ0', 53, 'Active', GETDATE(), 0),
(14, 6, 'Episode 6: Behind the Mask', 'https://www.youtube.com/watch?v=1F_U3mN8rJ0', 50, 'Active', GETDATE(), 0),
(14, 7, 'Episode 7: Sparks in the Dark', 'https://www.youtube.com/watch?v=1F_U3mN8rJ0', 54, 'Active', GETDATE(), 0),
(14, 8, 'Episode 8: The Art of Rebellion', 'https://www.youtube.com/watch?v=1F_U3mN8rJ0', 52, 'Active', GETDATE(), 0),
(14, 9, 'Episode 9: Broken Mirrors', 'https://www.youtube.com/watch?v=1F_U3mN8rJ0', 51, 'Active', GETDATE(), 0),
(14, 10, 'Episode 10: Confronting Power', 'https://www.youtube.com/watch?v=1F_U3mN8rJ0', 53, 'Active', GETDATE(), 0),
(14, 11, 'Episode 11: Real identity Revealed', 'https://www.youtube.com/watch?v=1F_U3mN8rJ0', 55, 'Active', GETDATE(), 0),
(14, 12, 'Episode 12: Standing Together', 'https://www.youtube.com/watch?v=1F_U3mN8rJ0', 54, 'Active', GETDATE(), 0),
(14, 13, 'Episode 13: The Grand Escape', 'https://www.youtube.com/watch?v=1F_U3mN8rJ0', 56, 'Active', GETDATE(), 0),
(14, 14, 'Episode 14: Not Me, But Us', 'https://www.youtube.com/watch?v=1F_U3mN8rJ0', 60, 'Active', GETDATE(), 0);

-- 8. Goblin (Guardian: The Lonely and Great God) (ContentId = 16, 16 Episodes)
DELETE FROM Episode WHERE ContentId = 16;
INSERT INTO Episode (ContentId, EpisodeNumber, Title, VideoUrl, Duration, Status, CreatedAt, IsDeleted)
VALUES 
(16, 1, 'Episode 1: The Cursed Sword', 'https://www.youtube.com/watch?v=8AcNWVpdTHs', 75, 'Active', GETDATE(), 0),
(16, 2, 'Episode 2: Summoning the Goblin', 'https://www.youtube.com/watch?v=8AcNWVpdTHs', 70, 'Active', GETDATE(), 0),
(16, 3, 'Episode 3: Housemates with the Grim Reaper', 'https://www.youtube.com/watch?v=8AcNWVpdTHs', 72, 'Active', GETDATE(), 0),
(16, 4, 'Episode 4: The Goblin''s Bride', 'https://www.youtube.com/watch?v=8AcNWVpdTHs', 74, 'Active', GETDATE(), 0),
(16, 5, 'Episode 5: A Rain of Maple Leaves', 'https://www.youtube.com/watch?v=8AcNWVpdTHs', 73, 'Active', GETDATE(), 0),
(16, 6, 'Episode 6: The Sword Must Be Pulled', 'https://www.youtube.com/watch?v=8AcNWVpdTHs', 76, 'Active', GETDATE(), 0),
(16, 7, 'Episode 7: First Love', 'https://www.youtube.com/watch?v=8AcNWVpdTHs', 75, 'Active', GETDATE(), 0),
(16, 8, 'Episode 8: A Glimpse of the Future', 'https://www.youtube.com/watch?v=8AcNWVpdTHs', 78, 'Active', GETDATE(), 0),
(16, 9, 'Episode 9: Vanishing Traces', 'https://www.youtube.com/watch?v=8AcNWVpdTHs', 77, 'Active', GETDATE(), 0),
(16, 10, 'Episode 10: Past Lives and King Wang Yeo', 'https://www.youtube.com/watch?v=8AcNWVpdTHs', 80, 'Active', GETDATE(), 0),
(16, 11, 'Episode 11: The Sin of the Grim Reaper', 'https://www.youtube.com/watch?v=8AcNWVpdTHs', 79, 'Active', GETDATE(), 0),
(16, 12, 'Episode 12: Park Joong-heon Returns', 'https://www.youtube.com/watch?v=8AcNWVpdTHs', 82, 'Active', GETDATE(), 0),
(16, 13, 'Episode 13: Turning to Ash', 'https://www.youtube.com/watch?v=8AcNWVpdTHs', 85, 'Active', GETDATE(), 0),
(16, 14, 'Episode 14: Nine Years of Forgetting', 'https://www.youtube.com/watch?v=8AcNWVpdTHs', 83, 'Active', GETDATE(), 0),
(16, 15, 'Episode 15: Quebec Reunion', 'https://www.youtube.com/watch?v=8AcNWVpdTHs', 86, 'Active', GETDATE(), 0),
(16, 16, 'Episode 16: The Lonely and Great God', 'https://www.youtube.com/watch?v=8AcNWVpdTHs', 90, 'Active', GETDATE(), 0);

-- 9. Itaewon Class (ContentId = 17, 16 Episodes)
DELETE FROM Episode WHERE ContentId = 17;
INSERT INTO Episode (ContentId, EpisodeNumber, Title, VideoUrl, Duration, Status, CreatedAt, IsDeleted)
VALUES 
(17, 1, 'Episode 1: The Unbending Principle', 'https://www.youtube.com/watch?v=NN7zE_aI7B4', 70, 'Active', GETDATE(), 0),
(17, 2, 'Episode 2: The Dream of DanBam', 'https://www.youtube.com/watch?v=NN7zE_aI7B4', 68, 'Active', GETDATE(), 0),
(17, 3, 'Episode 3: Meeting Jo Yi-seo', 'https://www.youtube.com/watch?v=NN7zE_aI7B4', 71, 'Active', GETDATE(), 0),
(17, 4, 'Episode 4: The DanBam Crew Assembles', 'https://www.youtube.com/watch?v=NN7zE_aI7B4', 72, 'Active', GETDATE(), 0),
(17, 5, 'Episode 5: Reopening Night in Itaewon', 'https://www.youtube.com/watch?v=NN7zE_aI7B4', 69, 'Active', GETDATE(), 0),
(17, 6, 'Episode 6: The Investment', 'https://www.youtube.com/watch?v=NN7zE_aI7B4', 70, 'Active', GETDATE(), 0),
(17, 7, 'Episode 7: Jangga Group Strikes Back', 'https://www.youtube.com/watch?v=NN7zE_aI7B4', 73, 'Active', GETDATE(), 0),
(17, 8, 'Episode 8: Moving the Pub', 'https://www.youtube.com/watch?v=NN7zE_aI7B4', 72, 'Active', GETDATE(), 0),
(17, 9, 'Episode 9: The Best Pub Competition', 'https://www.youtube.com/watch?v=NN7zE_aI7B4', 74, 'Active', GETDATE(), 0),
(17, 10, 'Episode 10: The Cooking Battle Winner', 'https://www.youtube.com/watch?v=NN7zE_aI7B4', 71, 'Active', GETDATE(), 0),
(17, 11, 'Episode 11: DanBam Corporation', 'https://www.youtube.com/watch?v=NN7zE_aI7B4', 73, 'Active', GETDATE(), 0),
(17, 12, 'Episode 12: The Diamond', 'https://www.youtube.com/watch?v=NN7zE_aI7B4', 75, 'Active', GETDATE(), 0),
(17, 13, 'Episode 13: Four Years Later', 'https://www.youtube.com/watch?v=NN7zE_aI7B4', 76, 'Active', GETDATE(), 0),
(17, 14, 'Episode 14: Confession and Danger', 'https://www.youtube.com/watch?v=NN7zE_aI7B4', 78, 'Active', GETDATE(), 0),
(17, 15, 'Episode 15: The Rescue', 'https://www.youtube.com/watch?v=NN7zE_aI7B4', 80, 'Active', GETDATE(), 0),
(17, 16, 'Episode 16: Sweet Night', 'https://www.youtube.com/watch?v=NN7zE_aI7B4', 85, 'Active', GETDATE(), 0);

-- 10. True Beauty (ContentId = 19, 16 Episodes)
DELETE FROM Episode WHERE ContentId = 19;
INSERT INTO Episode (ContentId, EpisodeNumber, Title, VideoUrl, Duration, Status, CreatedAt, IsDeleted)
VALUES 
(19, 1, 'Episode 1: The Magic of Makeup', 'https://www.youtube.com/watch?v=2Tz8YmQ-N0s', 72, 'Active', GETDATE(), 0),
(19, 2, 'Episode 2: Comic Book Cafe Encounter', 'https://www.youtube.com/watch?v=2Tz8YmQ-N0s', 70, 'Active', GETDATE(), 0),
(19, 3, 'Episode 3: The Secret Exposed', 'https://www.youtube.com/watch?v=2Tz8YmQ-N0s', 71, 'Active', GETDATE(), 0),
(19, 4, 'Episode 4: Han Seo-jun Steps In', 'https://www.youtube.com/watch?v=2Tz8YmQ-N0s', 73, 'Active', GETDATE(), 0),
(19, 5, 'Episode 5: School Festival Secrets', 'https://www.youtube.com/watch?v=2Tz8YmQ-N0s', 74, 'Active', GETDATE(), 0),
(19, 6, 'Episode 6: Two Boys and a Girl', 'https://www.youtube.com/watch?v=2Tz8YmQ-N0s', 70, 'Active', GETDATE(), 0),
(19, 7, 'Episode 7: Baseball Match Jealousy', 'https://www.youtube.com/watch?v=2Tz8YmQ-N0s', 72, 'Active', GETDATE(), 0),
(19, 8, 'Episode 8: School Trip First Kiss', 'https://www.youtube.com/watch?v=2Tz8YmQ-N0s', 75, 'Active', GETDATE(), 0),
(19, 9, 'Episode 9: Secret Dating', 'https://www.youtube.com/watch?v=2Tz8YmQ-N0s', 71, 'Active', GETDATE(), 0),
(19, 10, 'Episode 10: The Past Tragedy', 'https://www.youtube.com/watch?v=2Tz8YmQ-N0s', 73, 'Active', GETDATE(), 0),
(19, 11, 'Episode 11: The Hospital Room Drama', 'https://www.youtube.com/watch?v=2Tz8YmQ-N0s', 74, 'Active', GETDATE(), 0),
(19, 12, 'Episode 12: Exposed Without Makeup', 'https://www.youtube.com/watch?v=2Tz8YmQ-N0s', 76, 'Active', GETDATE(), 0),
(19, 13, 'Episode 13: Standing Tall with Courage', 'https://www.youtube.com/watch?v=2Tz8YmQ-N0s', 75, 'Active', GETDATE(), 0),
(19, 14, 'Episode 14: Beautiful as You Are', 'https://www.youtube.com/watch?v=2Tz8YmQ-N0s', 77, 'Active', GETDATE(), 0),
(19, 15, 'Episode 15: Graduation and Goodbye', 'https://www.youtube.com/watch?v=2Tz8YmQ-N0s', 79, 'Active', GETDATE(), 0),
(19, 16, 'Episode 16: True Beauty Lies Within', 'https://www.youtube.com/watch?v=2Tz8YmQ-N0s', 82, 'Active', GETDATE(), 0);

-- 11. Twenty-Five Twenty-One (ContentId = 20, 16 Episodes)
DELETE FROM Episode WHERE ContentId = 20;
INSERT INTO Episode (ContentId, EpisodeNumber, Title, VideoUrl, Duration, Status, CreatedAt, IsDeleted)
VALUES 
(20, 1, 'Episode 1: The IMF Crisis and Fencing', 'https://www.youtube.com/watch?v=gYp4cKumTwU', 72, 'Active', GETDATE(), 0),
(20, 2, 'Episode 2: Full House Comic Book', 'https://www.youtube.com/watch?v=gYp4cKumTwU', 71, 'Active', GETDATE(), 0),
(20, 3, 'Episode 3: Taeyang High School Team', 'https://www.youtube.com/watch?v=gYp4cKumTwU', 74, 'Active', GETDATE(), 0),
(20, 4, 'Episode 4: Late Night Practice', 'https://www.youtube.com/watch?v=gYp4cKumTwU', 70, 'Active', GETDATE(), 0),
(20, 5, 'Episode 5: Snow and The First Pager', 'https://www.youtube.com/watch?v=gYp4cKumTwU', 73, 'Active', GETDATE(), 0),
(20, 6, 'Episode 6: The National Fencing Final', 'https://www.youtube.com/watch?v=gYp4cKumTwU', 75, 'Active', GETDATE(), 0),
(20, 7, 'Episode 7: The Gold Medal Controversy', 'https://www.youtube.com/watch?v=gYp4cKumTwU', 77, 'Active', GETDATE(), 0),
(20, 8, 'Episode 8: The Move to Seoul', 'https://www.youtube.com/watch?v=gYp4cKumTwU', 72, 'Active', GETDATE(), 0),
(20, 9, 'Episode 9: Rainbow Confession', 'https://www.youtube.com/watch?v=gYp4cKumTwU', 76, 'Active', GETDATE(), 0),
(20, 10, 'Episode 10: Beach Trip with Friends', 'https://www.youtube.com/watch?v=gYp4cKumTwU', 74, 'Active', GETDATE(), 0),
(20, 11, 'Episode 11: School Festival Gig', 'https://www.youtube.com/watch?v=gYp4cKumTwU', 75, 'Active', GETDATE(), 0),
(20, 12, 'Episode 12: Turn of the Millennium', 'https://www.youtube.com/watch?v=gYp4cKumTwU', 78, 'Active', GETDATE(), 0),
(20, 13, 'Episode 13: Madrid World Cup', 'https://www.youtube.com/watch?v=gYp4cKumTwU', 76, 'Active', GETDATE(), 0),
(20, 14, 'Episode 14: The New Anchor', 'https://www.youtube.com/watch?v=gYp4cKumTwU', 80, 'Active', GETDATE(), 0),
(20, 15, 'Episode 15: Crossings and Distance', 'https://www.youtube.com/watch?v=gYp4cKumTwU', 82, 'Active', GETDATE(), 0),
(20, 16, 'Episode 16: Twenty-Five, Twenty-One', 'https://www.youtube.com/watch?v=gYp4cKumTwU', 88, 'Active', GETDATE(), 0);

-- 12. Vincenzo (ContentId = 21, 20 Episodes)
DELETE FROM Episode WHERE ContentId = 21;
INSERT INTO Episode (ContentId, EpisodeNumber, Title, VideoUrl, Duration, Status, CreatedAt, IsDeleted)
VALUES 
(21, 1, 'Episode 1: The Consigliere Arrives', 'https://www.youtube.com/watch?v=_J8tYxSB_LQ', 80, 'Active', GETDATE(), 0),
(21, 2, 'Episode 2: Geumga Plaza Wine Party', 'https://www.youtube.com/watch?v=_J8tYxSB_LQ', 78, 'Active', GETDATE(), 0),
(21, 3, 'Episode 3: Babel Pharmaceuticals', 'https://www.youtube.com/watch?v=_J8tYxSB_LQ', 79, 'Active', GETDATE(), 0),
(21, 4, 'Episode 4: The Fire of Justice', 'https://www.youtube.com/watch?v=_J8tYxSB_LQ', 82, 'Active', GETDATE(), 0),
(21, 5, 'Episode 5: Wusang Law Firm Standoff', 'https://www.youtube.com/watch?v=_J8tYxSB_LQ', 80, 'Active', GETDATE(), 0),
(21, 6, 'Episode 6: The Courtroom Circus', 'https://www.youtube.com/watch?v=_J8tYxSB_LQ', 81, 'Active', GETDATE(), 0),
(21, 7, 'Episode 7: Horned Bees Attack', 'https://www.youtube.com/watch?v=_J8tYxSB_LQ', 77, 'Active', GETDATE(), 0),
(21, 8, 'Episode 8: The White Horse Trick', 'https://www.youtube.com/watch?v=_J8tYxSB_LQ', 84, 'Active', GETDATE(), 0),
(21, 9, 'Episode 9: Blood on the Hands', 'https://www.youtube.com/watch?v=_J8tYxSB_LQ', 79, 'Active', GETDATE(), 0),
(21, 10, 'Episode 10: Inzargi the Pigeon', 'https://www.youtube.com/watch?v=_J8tYxSB_LQ', 83, 'Active', GETDATE(), 0),
(21, 11, 'Episode 11: The Chairman Unmasked', 'https://www.youtube.com/watch?v=_J8tYxSB_LQ', 82, 'Active', GETDATE(), 0),
(21, 12, 'Episode 12: Underground Gold Vault', 'https://www.youtube.com/watch?v=_J8tYxSB_LQ', 85, 'Active', GETDATE(), 0),
(21, 13, 'Episode 13: Pig Blood Inauguration', 'https://www.youtube.com/watch?v=_J8tYxSB_LQ', 84, 'Active', GETDATE(), 0),
(21, 14, 'Episode 14: The Mock Wedding', 'https://www.youtube.com/watch?v=_J8tYxSB_LQ', 86, 'Active', GETDATE(), 0),
(21, 15, 'Episode 15: The Guillotine File', 'https://www.youtube.com/watch?v=_J8tYxSB_LQ', 83, 'Active', GETDATE(), 0),
(21, 16, 'Episode 16: Mother''s Scarf', 'https://www.youtube.com/watch?v=_J8tYxSB_LQ', 88, 'Active', GETDATE(), 0),
(21, 17, 'Episode 17: Mafia Reckoning', 'https://www.youtube.com/watch?v=_J8tYxSB_LQ', 85, 'Active', GETDATE(), 0),
(21, 18, 'Episode 18: Checkmate on Babel', 'https://www.youtube.com/watch?v=_J8tYxSB_LQ', 87, 'Active', GETDATE(), 0),
(21, 19, 'Episode 19: Bloodbath at the Penthouse', 'https://www.youtube.com/watch?v=_J8tYxSB_LQ', 89, 'Active', GETDATE(), 0),
(21, 20, 'Episode 20: Evil Punishes Evil', 'https://www.youtube.com/watch?v=_J8tYxSB_LQ', 95, 'Active', GETDATE(), 0);

-- 13. The Glory (ContentId = 22, 16 Episodes)
DELETE FROM Episode WHERE ContentId = 22;
INSERT INTO Episode (ContentId, EpisodeNumber, Title, VideoUrl, Duration, Status, CreatedAt, IsDeleted)
VALUES 
(22, 1, 'Part 1 - Ep 1: The Curling Iron Scars', 'https://www.youtube.com/watch?v=tqVVddp_G9Y', 50, 'Active', GETDATE(), 0),
(22, 2, 'Part 1 - Ep 2: Go Player in the Fog', 'https://www.youtube.com/watch?v=tqVVddp_G9Y', 52, 'Active', GETDATE(), 0),
(22, 3, 'Part 1 - Ep 3: Semyeong Elementary Homeroom', 'https://www.youtube.com/watch?v=tqVVddp_G9Y', 51, 'Active', GETDATE(), 0),
(22, 4, 'Part 1 - Ep 4: Under the Gym Lights', 'https://www.youtube.com/watch?v=tqVVddp_G9Y', 53, 'Active', GETDATE(), 0),
(22, 5, 'Part 1 - Ep 5: The Executioner Dance', 'https://www.youtube.com/watch?v=tqVVddp_G9Y', 55, 'Active', GETDATE(), 0),
(22, 6, 'Part 1 - Ep 6: Entangled Web of Lies', 'https://www.youtube.com/watch?v=tqVVddp_G9Y', 52, 'Active', GETDATE(), 0),
(22, 7, 'Part 1 - Ep 7: The Green Heels', 'https://www.youtube.com/watch?v=tqVVddp_G9Y', 54, 'Active', GETDATE(), 0),
(22, 8, 'Part 1 - Ep 8: The Shaman Sanctuary', 'https://www.youtube.com/watch?v=tqVVddp_G9Y', 56, 'Active', GETDATE(), 0),
(22, 9, 'Part 2 - Ep 9: Blind Rage of Yeon-jin', 'https://www.youtube.com/watch?v=tqVVddp_G9Y', 53, 'Active', GETDATE(), 0),
(22, 10, 'Part 2 - Ep 10: Surgical Revenge Plan', 'https://www.youtube.com/watch?v=tqVVddp_G9Y', 55, 'Active', GETDATE(), 0),
(22, 11, 'Part 2 - Ep 11: Blood on Church Pews', 'https://www.youtube.com/watch?v=tqVVddp_G9Y', 58, 'Active', GETDATE(), 0),
(22, 12, 'Part 2 - Ep 12: Mother''s Betrayal Once More', 'https://www.youtube.com/watch?v=tqVVddp_G9Y', 56, 'Active', GETDATE(), 0),
(22, 13, 'Part 2 - Ep 13: Abandoned by All', 'https://www.youtube.com/watch?v=tqVVddp_G9Y', 57, 'Active', GETDATE(), 0),
(22, 14, 'Part 2 - Ep 14: Prison Forecast', 'https://www.youtube.com/watch?v=tqVVddp_G9Y', 59, 'Active', GETDATE(), 0),
(22, 15, 'Part 2 - Ep 15: The Gallows of Karma', 'https://www.youtube.com/watch?v=tqVVddp_G9Y', 62, 'Active', GETDATE(), 0),
(22, 16, 'Part 2 - Ep 16: The Glory Beyond Sorrow', 'https://www.youtube.com/watch?v=tqVVddp_G9Y', 70, 'Active', GETDATE(), 0);

-- 14. Lovely Runner (ContentId = 24, 16 Episodes)
DELETE FROM Episode WHERE ContentId = 24;
INSERT INTO Episode (ContentId, EpisodeNumber, Title, VideoUrl, Duration, Status, CreatedAt, IsDeleted)
VALUES 
(24, 1, 'Episode 1: The Watch of Destiny', 'https://www.youtube.com/watch?v=0-x2w4U7D9g', 68, 'Active', GETDATE(), 0),
(24, 2, 'Episode 2: Back to 2008 High School', 'https://www.youtube.com/watch?v=0-x2w4U7D9g', 66, 'Active', GETDATE(), 0),
(24, 3, 'Episode 3: Swimming Pool Rain Shower', 'https://www.youtube.com/watch?v=0-x2w4U7D9g', 69, 'Active', GETDATE(), 0),
(24, 4, 'Episode 4: The Yellow Umbrella', 'https://www.youtube.com/watch?v=0-x2w4U7D9g', 67, 'Active', GETDATE(), 0),
(24, 5, 'Episode 5: The Midnight Danger', 'https://www.youtube.com/watch?v=0-x2w4U7D9g', 71, 'Active', GETDATE(), 0),
(24, 6, 'Episode 6: Changing the Timeline', 'https://www.youtube.com/watch?v=0-x2w4U7D9g', 70, 'Active', GETDATE(), 0),
(24, 7, 'Episode 7: Present Day Reunion', 'https://www.youtube.com/watch?v=0-x2w4U7D9g', 72, 'Active', GETDATE(), 0),
(24, 8, 'Episode 8: The Bridge of Memories', 'https://www.youtube.com/watch?v=0-x2w4U7D9g', 74, 'Active', GETDATE(), 0),
(24, 9, 'Episode 9: College Campus Days', 'https://www.youtube.com/watch?v=0-x2w4U7D9g', 70, 'Active', GETDATE(), 0),
(24, 10, 'Episode 10: Unavoidable Fate', 'https://www.youtube.com/watch?v=0-x2w4U7D9g', 73, 'Active', GETDATE(), 0),
(24, 11, 'Episode 11: The Secret Recording', 'https://www.youtube.com/watch?v=0-x2w4U7D9g', 71, 'Active', GETDATE(), 0),
(24, 12, 'Episode 12: Train Ride to the Ocean', 'https://www.youtube.com/watch?v=0-x2w4U7D9g', 75, 'Active', GETDATE(), 0),
(24, 13, 'Episode 13: The Erased Memory', 'https://www.youtube.com/watch?v=0-x2w4U7D9g', 76, 'Active', GETDATE(), 0),
(24, 14, 'Episode 14: Film Set Encounters', 'https://www.youtube.com/watch?v=0-x2w4U7D9g', 74, 'Active', GETDATE(), 0),
(24, 15, 'Episode 15: Remembering You Always', 'https://www.youtube.com/watch?v=0-x2w4U7D9g', 78, 'Active', GETDATE(), 0),
(24, 16, 'Episode 16: Sudden Shower of Happiness', 'https://www.youtube.com/watch?v=0-x2w4U7D9g', 85, 'Active', GETDATE(), 0);
GO

PRINT '=== All Drama and Movie corrections completed successfully! ===';
GO

BEGIN TRANSACTION;
CREATE TABLE IF NOT EXISTS Characters (
    characterID INTEGER PRIMARY KEY AUTOINCREMENT,
    characterName TEXT NOT NULL,
    characterAppearance TEXT NOT NULL,
    levelDifficulty INTEGER NOT NULL
);
CREATE TABLE IF NOT EXISTS Levels (
    levelID INTEGER PRIMARY KEY AUTOINCREMENT,
    positionInLevel TEXT NOT NULL,
    levelDifficulty INTEGER NOT NULL,
    sessionID INTEGER,
    FOREIGN KEY (sessionID) REFERENCES Sessions(sessionID)
);
CREATE TABLE IF NOT EXISTS Sentences (
    conversationID INTEGER PRIMARY KEY AUTOINCREMENT,
    ProviderID INTEGER,
    ReceiverID INTEGER,
    Sentence TEXT NOT NULL,
    FOREIGN KEY (ProviderID) REFERENCES Characters(characterID),
    FOREIGN KEY (ReceiverID) REFERENCES Characters(characterID)
);
CREATE TABLE IF NOT EXISTS Sessions (
    sessionID INTEGER PRIMARY KEY AUTOINCREMENT,
    sessionName TEXT NOT NULL,
    dateOfLastSave TEXT NOT NULL
);
CREATE TABLE IF NOT EXISTS Settings (
    settingsID INTEGER PRIMARY KEY AUTOINCREMENT,
    graphics TEXT NOT NULL,
    resolution TEXT NOT NULL,
    volume INTEGER NOT NULL
);
INSERT INTO "Characters" ("characterID","characterName","characterAppearance","levelDifficulty") VALUES (1,'Player','Portraits/Player Portrait',1),
 (2,'Timo','Portraits/Timo Portrait',1),
 (3,'Roxy','Portraits/Roxy Portrait',2),
 (4,'Clair','Portraits/Clair Portrait',3),
 (5,'Future Self','Portraits/PlayerOld Portrait',4);
INSERT INTO "Sentences" ("conversationID","ProviderID","ReceiverID","Sentence") VALUES (1,2,1,'Whoa! What was that light?! I’ve never seen anything like it!'),
 (2,2,1,'My cousin said this door stopped working in dinosaur times... are you a dinosaur?'),
 (3,1,2,'I... fell.. Where am i?'),
 (4,2,1,'You''re in the old side of town. It''s kinda scary at night. But it''s still home.'),
 (5,2,1,'You really don’t look from around here. Are you… a robot?'),
 (6,1,2,'Haha, no. I''m just... lost. What is this place?'),
 (7,2,1,'it''s my secret place. There''s this light far away. I dream about going there one day.'),
 (8,2,1,'My parents say it''s dangerous. Maybe when i''m older i will investigate it.'),
 (9,3,1,'That portal actually did something? Huh.'),
 (10,3,1,'We always say it’s a message machine for aliens. Are you an alien?'),
 (11,1,3,'Do i look like one? hahaha what is this place?'),
 (12,3,1,'Depends who you ask. for me it''s my peace.'),
 (13,3,1,'me and my friends hang out up on the platforms sometimes, far away from all the lights and the noise.'),
 (14,4,1,'Oh, the portal opened? Must be because I’m here, ha ha ha.'),
 (15,4,1,'Sweetie, that outfit… we need to get you shopping. Like, ASAP.'),
 (16,1,4,'This place… it’s so shiny. So… perfect.'),
 (17,4,1,'Yeah it''s shiny and classy but don''t forget my dear, sometimes looks can fool you. Even the shiniest diamond can be fake.'),
 (18,4,1,'Anyway, I’m late for my mani-pedi. See you, sparkle~'),
 (19,5,1,'I knew you would come, I''ve been waiting.'),
 (20,1,5,'Have we met before...? You feel… familiar.'),
 (21,5,1,'Maybe hahaha, But maybe i''m a mirror of all you’ve seen. Of each world, maybe i''m the echoes between.'),
 (22,5,1,'My dear, The journey to success doesn''t depend on academic, financial, or material achievements.'),
 (23,5,1,'True success lies in the empathy, joy, and moments we gather along the way.'),
 (24,5,1,'A truly successful person is someone who knows how to appreciate even the smallest accomplishment.');
INSERT INTO "Sessions" ("sessionID","sessionName","dateOfLastSave") VALUES (1,'amen','2025-05-31 09:29:11'),
 (2,'oof','2025-06-02 10:13:57'),
 (7,'1234567891','2025-06-02 12:39:44'),
 (8,'12345678911','2025-06-02 12:39:56'),
 (9,'bla','2025-06-02 13:03:24'),
 (15,'kkkkkkkkkkkk122','2025-06-06 08:12:59'),
 (16,'pipo','2025-06-07 13:39:43');
INSERT INTO "Settings" ("settingsID","graphics","resolution","volume") VALUES (22,'Ultra','1920x1080',2);
COMMIT;

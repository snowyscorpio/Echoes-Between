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
INSERT INTO "Sessions" ("sessionID","sessionName","dateOfLastSave") VALUES (1,'amen','2025-05-31 09:29:11'),
 (2,'oof','2025-06-02 10:13:57'),
 (7,'1234567891','2025-06-02 12:39:44'),
 (8,'12345678911','2025-06-02 12:39:56'),
 (9,'bla','2025-06-02 13:03:24'),
 (15,'kkkkkkkkkkkk122','2025-06-06 08:12:59'),
 (16,'pipo','2025-06-07 13:39:43');
INSERT INTO "Settings" ("settingsID","graphics","resolution","volume") VALUES (22,'Ultra','1920x1080',2);
COMMIT;

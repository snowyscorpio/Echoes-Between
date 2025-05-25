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
COMMIT;

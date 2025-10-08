﻿CREATE TABLE Users
(
	UserId INT IDENTITY(1,1),
    UserName NVARCHAR(50) NOT NULL UNIQUE,
    Email NVARCHAR(256) NULL,
    EmailConfirmed BIT NOT NULL DEFAULT 0,
    PasswordHash NVARCHAR(200) NOT NULL,
    PRIMARY KEY (UserId),
);


CREATE TABLE Games
(
	GameId INT IDENTITY(1,1),
	ConnectionCode NVARCHAR(10) NOT NULL,
	LeaderSeatId INT NOT NULL,
	Status NVARCHAR(50) NOT NULL,
	GameWinner NVARCHAR(100),
	RoundCounter INT NOT NULL DEFAULT 0,
	CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
	UpdatedAt DATETIME NOT NULL DEFAULT GETDATE(),
	PRIMARY KEY (GameId)
);

CREATE TABLE GamePlayers
(
	GamePlayerId INT IDENTITY(1,1),
	GameId INT NOT NULL,
	UserId INT NOT NULL,
	Seat INT NOT NULL,
	IsReady BIT NOT NULL DEFAULT 0,
	IsConnected BIT NOT NULL DEFAULT 1,
	CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
	UpdatedAt DATETIME NOT NULL DEFAULT GETDATE(),
	PRIMARY KEY (GamePlayerId),
	FOREIGN KEY (GameId) REFERENCES Games(GameId),
	FOREIGN KEY (UserId) REFERENCES USERS(UserId)
);

CREATE TABLE Rounds
(
	RoundId INT IDENTITY(1,1),
	GameId INT NOT NULL,
	RoundNumber INT NOT NULL,
	LeaderSeat INT NOT NULL,
	Phase NVARCHAR(50) NOT NULL,
	Result NVARCHAR(50) NOT NULL,
	TeamSize INT NOT NULL,
	SabotageCounter INT NOT NULL DEFAULT 0,
	TeamVoteCounter INT NOT NULL DEFAULT 0,
	CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
	UpdatedAt DATETIME NOT NULL DEFAULT GETDATE(),
	PRIMARY KEY (RoundId),
	FOREIGN KEY (GameId) REFERENCES Games(GameId)
);

CREATE TABLE MissionVote
(
	MissionVoteId INT IDENTITY(1,1),
	RoundId INT NOT NULL,
	Seat INT NOT NULL,
	IsSabotage BIT NOT NULL,
	PRIMARY KEY (MissionVoteId),
	FOREIGN KEY (RoundId) REFERENCES Rounds(RoundId)
);

CREATE TABLE TeamProposals
(
	TeamProposalId INT IDENTITY(1,1),
	RoundId INT NOT NULL,
	AttemptNumber INT NOT NULL,
	IsActive BIT NOT NULL DEFAULT 1,
	IsApproved BIT,
	PRIMARY KEY (TeamProposalId),
	FOREIGN KEY (RoundId) REFERENCES Rounds(RoundId)
);

CREATE TABLE TeamProposalMembers
(
	TeamProposalMemberId INT IDENTITY(1,1),
	TeamProposalId INT NOT NULL,
	Seat INT NOT NULL,
	PRIMARY KEY (TeamProposalMemberId),
	FOREIGN KEY (TeamProposalId) REFERENCES TeamProposals(TeamProposalId)
);

CREATE TABLE TeamProposalVotes
(
	TeamProposalVoteId INT IDENTITY(1,1),
	TeamProposalId INT NOT NULL,
	Seat INT NOT NULL,
	IsApproved BIT NOT NULL,
	PRIMARY KEY (TeamProposalVoteId),
	FOREIGN KEY (TeamProposalId) REFERENCES TeamProposals(TeamProposalId)
);


DROP TABLE TeamProposalMembers
DROP TABLE TeamProposalVotes
DROP TABLE TeamProposals
DROP TABLE MissionVote
DROP TABLE Rounds

DROP TABLE GamePlayers
DROP TABLE Users

DROP TABLE Games


SELECT * FROM Users
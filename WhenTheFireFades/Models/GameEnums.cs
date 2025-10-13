namespace WhenTheFireFades.Models;

public enum GameStatus { Lobby = 0, InProgress = 1, Finished = 2 }
public enum GameResult { Unknown = 0, Human = 1 , Shapeshifter = 2}
public enum RoundStatus { TeamSelection = 0, VoteOnTeam = 1, SecretChoices = 2, MissionResults = 3 }
public enum RoundResult { Unknown = 0, Success = 1, Sabotage = 2 }
public enum PlayerRole { Human = 0, Shapeshifter = 1 }

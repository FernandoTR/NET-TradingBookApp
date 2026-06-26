namespace Domain.Enums;

public enum ValidationSource
{
    UserInput = 1,
    AiVision = 2,
    DeterministicRule = 3,
    HistoricalEvidence = 4,
    UserConfirmation = 5
}

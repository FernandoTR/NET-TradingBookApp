namespace Domain.Enums;

// Ejemplo:
// Canal descendente + LARGO → Reversal
// Canal ascendente + CORTO → Reversal
// Todo lo demás → Continuation
//--------------------
// Canal descendente + LARGO + break : ReversalBreak
// Canal descendente + LARGO + retest: ReversalRetest
// Canal descendente + CORTO: ContinuationRetest
public enum ConfirmationType : byte
{
    None = 0,
    // Continuación
    ContinuationBreak = 1,
    ContinuationRetest = 2,

    // Reversal (lo importante)
    ReversalBreak = 3,
    ReversalRetest = 4
}

namespace Intertwine.Services.DTOs.UserAnswers
{
    public class SubmitAnswerRequest
    {
        public int AnswerId { get; set; }
        /// <summary>Explicit consent to spend 10 Sparks if the free non-daily allowance is exhausted.</summary>
        public bool SpendSparks { get; set; }
    }
}

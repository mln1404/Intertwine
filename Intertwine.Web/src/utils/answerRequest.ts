// Use local calendar components, never a UTC ISO date.
export function localDate(date = new Date()): string {
  return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`
}
export function answerRequest(
  questionId: number,
  answerId: number,
  date: string,
  key: string,
  spendSparks = false,
) {
  return {
    path: `/api/questions/${questionId}/answer?localDate=${encodeURIComponent(date)}`,
    options: {
      method: 'POST',
      headers: { 'Idempotency-Key': key },
      body: JSON.stringify({ answerId, ...(spendSparks ? { spendSparks: true } : {}) }),
    },
  }
}

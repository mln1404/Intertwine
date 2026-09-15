import type { Category, CreditPackage, Profile, Question } from '../models/question'
const categories: Category[] = [
  { categoryId: 1, categoryName: 'Relationships', color: '#6a7e63' },
  { categoryId: 2, categoryName: 'Everyday life', color: '#b59160' },
  { categoryId: 3, categoryName: 'Values & beliefs', color: '#9786ad' },
  { categoryId: 4, categoryName: 'Dreams & ambitions', color: '#6c9096' },
]
function question(
  id: number,
  title: string,
  full: string,
  category: number,
  answers: string[],
): Question {
  return {
    questionId: id,
    questionTitle: title,
    fullQuestion: full,
    categories: [categories[category]!],
    answers: answers.map((answerText, i) => ({ answerId: id * 10 + i, answerText })),
  }
}
export const demoQuestions: Question[] = [
  question(
    1,
    'The little things that matter',
    'What makes you feel most connected to someone?',
    0,
    [
      'An honest, unhurried conversation',
      'Sharing an experience together',
      'The small, thoughtful gestures',
      'Feeling understood without saying a word',
    ],
  ),
  question(2, 'Your kind of Sunday', 'A whole day with no plans. How would you spend it?', 1, [
    'A slow morning and a good book',
    'Outside, somewhere new',
    'Catching up with my favourite people',
    'Following whatever feels right',
  ]),
  question(
    3,
    'What you stand for',
    'Which quality do you value most in the people around you?',
    2,
    ['Kindness', 'Honesty', 'Curiosity', 'Loyalty'],
  ),
  question(
    4,
    'A little leap of faith',
    'If you could start something new tomorrow, what would it be?',
    3,
    [
      'A creative project',
      'An adventure far from home',
      'A new skill to get lost in',
      'Something that helps other people',
    ],
  ),
  question(
    5,
    'Making room for each other',
    'When someone you care about has a difficult day, what comes naturally to you?',
    0,
    [
      'Listening without trying to fix it',
      'Helping them find a solution',
      'Doing something thoughtful',
      'Giving them space to recharge',
    ],
  ),
  question(6, 'The place you recharge', 'Where do you feel most like yourself?', 1, [
    'At home in my own space',
    'Somewhere surrounded by nature',
    'With my closest people',
    'Exploring somewhere unfamiliar',
  ]),
]
export const demoProfile: Profile = {
  userProfileId: 1,
  identityUserId: 'demo',
  firstName: 'Alex',
  middleName: '',
  lastName: 'Morgan',
  avatarName: 'alexm',
  creditBalance: 120,
}
export const demoPackages: CreditPackage[] = [
  { creditPackageId: 1, currencyCode: 'AUD', amount: 4.99, credits: 50 },
  { creditPackageId: 2, currencyCode: 'AUD', amount: 9.99, credits: 120 },
  { creditPackageId: 3, currencyCode: 'AUD', amount: 19.99, credits: 260 },
]

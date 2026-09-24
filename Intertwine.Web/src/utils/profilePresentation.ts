import type { Category } from '../models/question'

const fallbackAccent = '#3E5947'

export function avatarInitials(avatarName: string): string {
  const words = avatarName.trim().split(/\s+/).filter(Boolean)
  if (!words.length) return 'I'
  return words
    .slice(0, 2)
    .map((word) => word[0]?.toUpperCase() ?? '')
    .join('')
}

export function categoryAccent(categories: Category[]): string {
  const color = categories[0]?.color
  return color && /^#[0-9a-f]{6}$/i.test(color) ? color : fallbackAccent
}

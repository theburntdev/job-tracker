import { cn } from './cn'

describe('cn', () => {
  it('returns empty string for no inputs', () => {
    expect(cn()).toBe('')
  })

  it('concatenates class names', () => {
    expect(cn('foo', 'bar')).toBe('foo bar')
  })

  it('filters falsy values', () => {
    expect(cn('foo', false, undefined, null, 'bar')).toBe('foo bar')
  })

  it('merges conflicting Tailwind classes — last wins', () => {
    expect(cn('px-2', 'px-4')).toBe('px-4')
  })

  it('applies conditional object syntax', () => {
    expect(cn({ 'font-bold': true, 'font-normal': false })).toBe('font-bold')
  })
})

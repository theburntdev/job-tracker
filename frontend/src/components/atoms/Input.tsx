import React from 'react'
import { cn } from '../../lib/cn'

interface InputProps extends React.InputHTMLAttributes<HTMLInputElement> {
  error?: string
}

export const Input = React.forwardRef<HTMLInputElement, InputProps>(
  ({ error, className, ...rest }, ref) => (
    <input
      ref={ref}
      aria-invalid={error ? true : undefined}
      className={cn(
        'flex h-10 w-full rounded border border-border bg-surface-elevated px-3 py-2 text-sm text-text-primary placeholder:text-text-muted focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-primary disabled:cursor-not-allowed disabled:opacity-50',
        error && 'border-status-rejected focus-visible:ring-status-rejected',
        className,
      )}
      {...rest}
    />
  ),
)
Input.displayName = 'Input'

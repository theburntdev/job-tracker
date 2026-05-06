import React from 'react'
import { cn } from '../../lib/cn'

type BadgeVariant = 'default' | 'info' | 'warning' | 'success' | 'error'

interface BadgeProps extends React.HTMLAttributes<HTMLSpanElement> {
  variant?: BadgeVariant
  children: React.ReactNode
}

export const Badge = React.forwardRef<HTMLSpanElement, BadgeProps>(
  ({ variant = 'default', className, children, ...rest }, ref) => (
    <span
      ref={ref}
      className={cn(
        'inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium',
        {
          default: 'border border-border bg-surface text-text-secondary',
          info: 'bg-status-applied/15 text-status-applied',
          warning: 'bg-status-interviewing/15 text-status-interviewing',
          success: 'bg-status-offer/15 text-status-offer',
          error: 'bg-status-rejected/15 text-status-rejected',
        }[variant],
        className,
      )}
      {...rest}
    >
      {children}
    </span>
  ),
)
Badge.displayName = 'Badge'

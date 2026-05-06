import React from 'react'
import { cn } from '../../lib/cn'

interface LabelProps extends React.LabelHTMLAttributes<HTMLLabelElement> {
  children: React.ReactNode
  required?: boolean
}

export const Label = React.forwardRef<HTMLLabelElement, LabelProps>(
  ({ children, required, className, ...rest }, ref) => (
    <label ref={ref} className={cn('text-sm font-medium text-text-primary', className)} {...rest}>
      {children}
      {required && (
        <span className="ml-1 text-status-rejected" aria-hidden="true">
          *
        </span>
      )}
    </label>
  ),
)
Label.displayName = 'Label'

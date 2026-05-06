import React from 'react'
import { Label } from '../atoms/Label'

interface FormFieldProps {
  label: string
  htmlFor: string
  error?: string
  required?: boolean
  children: React.ReactNode
}

export function FormField({ label, htmlFor, error, required, children }: FormFieldProps) {
  return (
    <div className="flex flex-col gap-1.5">
      <Label htmlFor={htmlFor} required={required}>
        {label}
      </Label>
      {children}
      {error && (
        <p role="alert" className="text-xs text-status-rejected">
          {error}
        </p>
      )}
    </div>
  )
}

# ADR-0001: Activity Log for Job-Application Tracking

## Status
Accepted

## Context
Users need to log job search activities against Job-Applications for EDD unemployment reporting. The existing Stage field on JobApplication tracks pipeline status but does not record a history of what actually happened.

## Decision
Introduce an **Activity** entity as a child of JobApplication. The Activity feed is exposed as a new tab alongside the existing Applications tab — it does not replace it.

### Activity entity fields
- `ActivityType` (required, enum): `Applied | PhoneScreen | Interview | Offer | Rejected | Withdrew | EmailedRecruiter | CalledRecruiter`
- `OccurredAt` (required): date the activity happened; defaults to today in the UI
- `ContactName` (optional string)
- `ContactEmail` (optional string)
- `Notes` (optional string)

### Stage independence
Stage on JobApplication and Activity are independent at the domain level. No Activity automatically updates Stage; no Stage change auto-creates an Activity. The "Add Activity" form includes an optional Stage update field as a UI convenience only.

### Auto-create on application creation
When a JobApplication is created with a non-null `AppliedAt`, the system automatically creates an `Applied` Activity with `OccurredAt = AppliedAt`. If `AppliedAt` is null, no Activity is auto-created.

### Mutability
Activities can be deleted but not edited. Corrections require delete-and-re-log.

### Entry point
Activities are logged from the JobApplication detail modal only. The Activity tab is read-only (view + delete).

## Alternatives considered
- **Stage derives from Activities**: Rejected — accidental Stage changes would corrupt the EDD record; EmailedRecruiter/CalledRecruiter have no Stage equivalent.
- **Activity auto-creates on Stage change**: Rejected — same noise problem; Stage changes during data-entry cleanup would pollute the activity log.
- **Free-text ActivityType**: Rejected — inconsistent terminology breaks EDD grouping/reporting.
- **Contact FK instead of ContactName/ContactEmail strings**: Deferred — Contact entity not yet built; strings satisfy EDD reporting needs without the dependency.

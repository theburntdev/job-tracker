# Glossary

## Job-Application
A submitted job application the user is tracking. Has a title, company, location, URL, description, and a Stage. One Job-Application has many Activities.

**Avoid:** "job", "posting", "listing"

---

## Stage
The current status of a Job-Application (e.g. Applied, Screening, Interviewing, Offer, Rejected, Withdrawn). A Stage is a property of the Job-Application, not an Activity.

**Avoid:** "status", "state"

---

## Activity
A record of something that happened in relation to a Job-Application — logged as a job search activity (e.g. for EDD unemployment reporting). Each Activity has a type (ActivityType), a required OccurredAt date (defaults to today), optional contact fields (ContactName, ContactEmail), and optional Notes. Activities can be deleted but not edited. One Job-Application has many Activities.

When a Job-Application is created with a non-null AppliedAt, the system automatically creates an Activity of type Applied with OccurredAt = AppliedAt. If AppliedAt is null at creation time, no Activity is auto-created.

Stage and Activity are independent at the domain level: an Activity does not automatically drive Stage transitions, and advancing Stage does not create an Activity. The "Add Activity" form may optionally include a Stage update field as a UI convenience — the user decides whether to update Stage when logging an Activity.

**Avoid:** "event", "action", "log entry", "contact log"

---

## ActivityType
The kind of action an Activity records. Fixed enum:

`Applied | PhoneScreen | Interview | Offer | Rejected | Withdrew | EmailedRecruiter | CalledRecruiter`

**Avoid:** "event type", "action type"

---

## Contact
A person associated with a Job-Application (e.g. recruiter, hiring manager). Not to be confused with "contacting" someone — that is an Activity.

**Avoid:** "connection", "person"

---

## Note
Free-text record attached to a Job-Application or Contact.

**Avoid:** "comment", "memo"

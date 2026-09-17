# ZMC Academy — engineering notes

This document describes the project as it actually exists: a 2021 C# WinForms academy portal built around SQL Server, a long form-by-form registration flow, and a dashboard made from embedded user controls.

It is intentionally not presented as a modern LMS architecture. The useful part of revisiting the project is understanding what the original design was doing, where the risks were, and which boundaries became obvious only after the app grew.

## 1. system shape

At a high level the application is one desktop process talking directly to one SQL Server database.

```text
Windows Forms UI
      │
      ├── signup / sign-in / password reset forms
      ├── dashboard shell
      └── dashboard user controls
              │
              ▼
      System.Data.SqlClient
              │
              ▼
         ZMC_Academy
```

There is no HTTP API, backend service, ORM, dependency-injection container, or separate domain layer. Form event handlers are responsible for most of the orchestration.

That is very normal for the size and era of the coursework project, but it also explains most of the coupling discussed below.

## 2. the nine-screen signup workflow

Registration is split over `Signup1` through `Signup9`.

Each screen captures one slice of state and stores values in static members that the next form can read. The rough flow is:

```text
Signup1  personal details
   ↓
Signup2  O/L results
   ↓
Signup3  A/L results
   ↓
Signup4  qualification #1
   ↓
Signup5  qualification #2
   ↓
Signup6  selected course
   ↓
Signup7  payment-form prototype
   ↓
Signup8  review / transition
   ↓
Signup9  email verification + persistence
```

This made it easy to build one screen at a time, but it means the registration state lives in UI classes rather than in an explicit model.

A modern equivalent would use something like:

```text
RegistrationDraft
 ├── Profile
 ├── OrdinaryLevelResults
 ├── AdvancedLevelResults
 ├── Qualifications[]
 ├── Course
 └── PaymentDisplayMetadata
```

The screens would edit that object while one application service owns validation and final persistence.

## 3. registration is now one database transaction

The most important correctness improvement in the current signup flow is the transaction in `Signup9`.

The historical design inserted related records separately. That creates an obvious partial-write problem:

```text
Registration inserted     ✓
O/L results inserted      ✓
A/L results inserted      ✓
Course insert             ✗

=> half-created account
```

The current path instead does:

```text
begin SQL transaction
      │
      ├── Registration
      ├── Ol_Results
      ├── Al_Results
      ├── Other_Qualifications
      ├── Other_Qualifications1
      ├── Course
      └── Payment
      │
      ├── all succeed → COMMIT
      └── anything fails → ROLLBACK
```

That is a much more important improvement than simply rearranging the UI because it protects the invariant that a completed signup is stored as one coherent unit.

## 4. password storage

New passwords no longer go into SQL as plaintext.

`Security/PasswordHasher.cs` uses PBKDF2-HMAC-SHA256 with a random salt and stores a self-describing value containing the algorithm, iteration count, salt and derived key.

Conceptually:

```text
password
   +
random salt
   │
   ▼
PBKDF2-SHA256
100,000 iterations
   │
   ▼
encoded password record
```

The current `Registration.Password` column in `Database.sql` is therefore `varchar(255)` rather than the old ten-character field.

### legacy development database migration

The sign-in code has a deliberately small compatibility path for databases produced by the old project.

If the stored password is not in the PBKDF2 format, a successful plaintext comparison is treated as a legacy login and the record is immediately replaced with a new hash.

That lets an old local development database migrate gradually without requiring the original password values to be known ahead of time.

This is useful for a historical repo, but a real production identity migration would need much more deliberate auditing, session management, password policy, reset/token handling, rate limiting, and account-protection controls.

## 5. database configuration

Database access now converges on `Infrastructure/Database.cs`.

The helper looks for:

```text
ZMC_DB_CONNECTION_STRING
```

and keeps the earlier `ZMC_DB_CONNECTION` name as a compatibility fallback.

If neither exists, the development default is a named LocalDB database:

```text
(LocalDB)\MSSQLLocalDB
        ↓
ZMC_Academy
```

The important part is that the repository no longer needs a checked-in `.mdf` and `.ldf` pair tied to one machine. `Database.sql` is the portable source for recreating the development schema.

## 6. SQL injection cleanup

One recurring pattern in the original code was string-built SQL:

```text
"... WHERE Id = '" + value + "'"
```

The current paths touched by the cleanup use SQL parameters for user-controlled values.

For example, the library selects a book with a query shaped like:

```text
SELECT B_id, B_name, B_author
FROM Books
WHERE B_id = @BookId
```

and the current signed-in student ID is passed separately as `@StudentId` when a book is added to the list.

The same principle is used in sign-in, attendance, support reports, past-paper data access, registration, and dashboard profile loading.

One small exception is the attendance table name. `Attendence1` chooses between two fixed, code-owned table names based on the combo-box selection. Table names cannot be passed as ordinary SQL parameters, so the safe boundary there is the closed mapping:

```text
index 0 → Attendence1
otherwise → Attendence2
```

No user string is interpolated into the identifier.

## 7. email verification boundary

Signup and password reset need to send verification mail.

Historically SMTP configuration lived too close to the forms. The current project moves that responsibility into `Services/EmailService.cs` and reads configuration from environment variables:

```text
ZMC_SMTP_USERNAME
ZMC_SMTP_PASSWORD
ZMC_SMTP_FROM
ZMC_SMTP_HOST
ZMC_SMTP_PORT
```

The forms ask the service to generate/send a code and handle configuration errors as UI feedback.

This is still client-side SMTP from a desktop application, which is not how I would design a deployed identity flow today. A current system would normally ask a trusted backend or identity provider to send verification/reset messages so long-lived mail credentials never need to exist on client machines.

## 8. payment form: what it is and what it is not

One signup screen is a historical payment-card form prototype.

That should not be confused with payment processing.

The old schema modeled card number and CVC persistence. The current cleanup intentionally removes that storage behavior. The final signup write keeps only:

```text
method
card type
last 4 digits
expiry date
student id
```

and the in-memory full card/CVC strings are cleared after the signup attempt.

A real payment product should not build its own card-storage flow at all. A hosted/tokenized payment provider should own sensitive card entry, and the application should keep only provider tokens plus safe display metadata when necessary.

## 9. dashboard composition

`Dash1` acts as a shell and swaps embedded WinForms user controls in and out.

```text
Dash1
  │
  ├── News1
  ├── library1
  ├── Pastpapers1
  ├── Attendence1
  ├── Courses1
  └── Calander1
```

`ShowSection(Control active)` hides the other controls and shows the requested one.

This is effectively a tiny hand-built navigation system. The positive side is that the main dashboard remains one window. The downside is that the shell directly knows every feature control, so adding/removing features increases central coupling.

## 10. signed-in state

The project uses static values such as `Signin1.signinID` as lightweight session state.

That ID is then used when loading the dashboard display name, borrowing books, listing attendance, adding past papers, and submitting support requests.

For a single-user desktop process this works, but it has the usual weaknesses of global mutable state:

- feature code can read/change session data from anywhere;
- tests cannot easily create isolated sessions;
- sign-out depends on manually clearing the static value;
- state lifetime is implicit rather than modeled.

A cleaner desktop architecture would pass an immutable `UserSession` or expose it through one session service.

## 11. library flow

The library feature is a nice example of the app doing more than static coursework forms.

On load it fetches the book catalog. Typing in the search box switches to a parameterized title-prefix query. Selecting a row loads the full selected-book details. The user can then choose a return date and create a `Booklist` record tied to the current student.

```text
Books
  │
  ├── browse
  └── search prefix
          │
          ▼
      select B_id
          │
          ▼
    load book details
          │
          ▼
 choose valid return date
          │
          ▼
Booklist(Id, B_id, dates)
```

The current UI rejects a return date in the past and the cleaned development schema prevents one student from adding the same book twice at the same time with a unique `(Id, B_id)` constraint.

A bigger library system would need copy-level inventory, availability, reservations, return state, overdue policy, and transactional stock changes. This feature is deliberately much smaller.

## 12. past papers

Past papers use the same broad idea as books: a small catalog plus a student-specific list.

That duplication makes sense given how the project was built screen-by-screen, but it suggests a more generic resource model if the product were ever expanded.

```text
ResourceCatalog
 ├── books
 └── papers

StudentResource
 ├── student
 ├── resource
 ├── addedAt
 └── dueAt
```

I would not retrofit that abstraction into the historical project just for aesthetics; the current separate tables make the original intent easier to follow.

## 13. attendance

Attendance is represented by two separate tables, `Attendence1` and `Attendence2`.

The feature selects one of those datasets and loads records for the signed-in student in descending date order.

This works, but the schema is encoding a category by duplicating a table. A current design would likely use one table:

```text
Attendance
 ├── StudentId
 ├── SessionType / CourseId
 ├── Date
 └── Status
```

That makes querying, indexing, constraints and future attendance categories much easier.

## 14. support reports

Despite the filename, `Calander1` is a support/problem-report screen.

The current handler validates that summary and details are present and inserts:

```text
summary
full details
current timestamp
signed-in student id
```

into `Report_problem`.

This is one of several reminders that file/class names drifted while the coursework scope expanded. Renaming everything now would generate noisy WinForms designer changes for little practical benefit, so the docs explain the mismatch instead.

## 15. academy news

News is stored in the same SQL database and loaded into the dashboard.

The cleanup consolidated repeated reads into one query rather than asking the database separately for each UI slot.

That change is small, but it is a good example of a general rule: retrieve the collection once, then let the UI decide how many items to display.

## 16. schema compatibility versus ideal modeling

`Database.sql` is deliberately cleaner than the original dump but still compatible with the current WinForms code.

That is why tables such as these remain:

```text
Other_Qualifications
Other_Qualifications1
Attendence1
Attendence2
```

An ideal greenfield schema would normalize those concepts, but changing them here would mean rewriting large parts of the old form logic and would make this less useful as a historical project.

The cleanup focuses on reproducibility and obvious safety problems, not architecture cosplay.

## 17. checked-in database binaries

The repository originally tracked both:

```text
ZMC_Academy.mdf
ZMC_Academy_log.ldf
```

along with compiled `bin/` / `obj/` output and Visual Studio `.vs/` state.

Those are machine/build artifacts, not source. They are now ignored and removed from the current branch. The SQL script is the version-controlled representation of the development database.

Historical commits still contain the old files. Removing them from today's tree does not rewrite Git history.

## 18. historical data/privacy note

The old SQL script contained development records and contact-like values that do not belong in a public portfolio repository.

The current `Database.sql` contains only generic catalog/news seed data and no real student record.

Again, Git history is separate. If complete historical removal is required, that is a deliberate history-rewrite/privacy-cleanup operation rather than a normal commit.

## 19. historical SMTP credential note

An earlier public version of this project contained an SMTP credential in source.

The current application does not embed it and uses environment-driven configuration instead. But once a credential has been public, deleting the current reference is not enough; the old credential should be considered compromised and revoked/rotated at the provider.

This repository does not attempt a force-push history rewrite as part of the ordinary portfolio cleanup.

## 20. build shape

This is an old-style .NET Framework 4.7.2 project.

The project file predates SDK-style automatic source discovery. To avoid turning the `.csproj` into a large noisy diff just to add the cleanup helpers, `Directory.Build.targets` links these files into compilation:

```text
Infrastructure/Database.cs
Security/PasswordHasher.cs
Services/EmailService.cs
```

That keeps the historical project structure intact while still letting the current source build with the security helpers.

## 21. why there are no unit tests pretending to cover the app

Most behavior is embedded directly in WinForms event handlers and SQL access. There is no natural seam for useful isolated tests without first extracting application/domain services.

Adding a handful of superficial tests around forms would create a badge without meaningfully protecting the highest-risk behavior.

The current CI therefore focuses on the thing that can be checked reliably for this historical codebase: the solution must restore/compile from a clean checkout and the repository must not reintroduce generated database/build artifacts.

## 22. a modern rebuild

If I rebuilt the same product now, the first step would not be changing button colors or replacing WinForms. It would be moving application rules out of the UI.

```text
presentation
 WinForms / WPF / web / mobile
          │
          ▼
application layer
 ├── AuthService
 ├── RegistrationService
 ├── LibraryService
 ├── AttendanceService
 ├── ResourcesService
 └── SupportService
          │
          ▼
domain models
          │
          ▼
repositories
          │
          ├── SQL database
          └── mail / identity provider
```

Registration would be one draft/state machine rather than nine classes sharing static values. Password reset and verification would use expiring server-side tokens. Payments would be provider-hosted. Attendance and qualifications would be normalized. Database evolution would use migrations. Queries and transactions would live behind repositories rather than inside click handlers.

## 23. what I would keep

The app has one quality worth preserving: the features are concrete.

It is easy to trace a student action from the UI to the stored row:

```text
search book → select → choose date → add to list
view attendance → choose dataset → load student history
report problem → validate → save report
sign up → verify email → commit account
```

For an early project, that directness is useful. The modern version should improve boundaries without losing the ability to understand what the product is actually doing.

## 24. takeaway

ZMC Academy is mostly a lesson in what happens when a desktop coursework project keeps acquiring features.

The original form-centric approach made it fast to add screens. As the product expanded, authentication, database configuration, SQL safety, transactions, shared session state, email, and sensitive data all became cross-cutting concerns.

The cleanup does not erase that history. It makes the current public branch safer and reproducible, then documents the architecture honestly enough that the rough edges become part of the engineering story rather than something hidden behind a new README.

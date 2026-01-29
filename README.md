# BrianBird_CS295N_Labs_W26
this is an Example app for CS295N, the ASP.NET MVC course, at Lane Community College

## Purpose of the app

This app will solve the problem of matching up students who are ready for a code review with students who will do a code review for them.

These are the constraints:

- Students who give and get code reviews must be in different assignment version groups.
- Students who ask for a code review should have their app mostly done and it should build without errors and run without crashing on the main pages or features.
- Students who give code reviews should be students who have asked for one (meaning their code is ready.)

requirements for the app:

- Students can register to use the app. This info will be stored about each student:
  - Name
  - GitHub id
  - email
  - Institution
  - Lab partner
- Instructors will add assignments with this information:
  - Course
  - Section (CRN)
  - Institution
  - Assignment name
  - Assignment  number
- Students can upload links to their repos and ask for a review
  - Students can enter a date by which they will be ready to upload their link (if it's past the deadline).
  - The app will match students who are ready giving priority to lab partners

## TODO

- Consider adding a service (registered with DI) to load the readiness checklist JSON instead of reading it in the controller, to avoid file I/O on every request and keep controllers thinner.

### ✅ **Macro-Level Roadmap (Prioritized Breakdown)**

---

#### **1. ARD (Architecture Requirements Document)**

* Includes:

  * High-level system overview
  * Use case coverage
  * Technology constraints (C#, React, RDBMS now, NoSQL-ready)
  * Initial microservice layout
* Output: Early consensus on system boundaries and expectations

---

#### **2. ERD (Relational DB Design)**

* MySQL or PostgreSQL ERD
* Normalize while keeping in mind migration potential to NoSQL
* Ensure:

  * Proper abstraction (e.g., `Question`, `Option`, `Vote`)
  * Clear ownership (poll owner, user auth)
  * No tight coupling to relational-only assumptions in services

---

#### **3. Roadmap Document (Detailed)**

* Breaks macro items into:

  * Milestones
  * Sprints/Weeks
  * Team assignments
  * Deliverables per stage

---

#### **4. Prototype Component Skeletons**

* React components (UI skeletons)
* C# services: Interfaces, base classes, empty controllers
* Example:

  * `SurveyService`, `VoteService`, `UserService` scaffolds
  * React pages like `CreatePoll`, `VotePage`, `ResultsPage`

---

#### **5. Unit Tests for Classes**

* Set up testing frameworks:

  * **C#**: xUnit or NUnit
  * **React**: Jest + React Testing Library
* Test:

  * Business logic (e.g., vote counting, expiration checks)
  * Pure functions, utilities, validations

---

#### **6. Implement Prototype**

* Core flow:

  * Create poll → Vote → View result
* Use in-memory data or SQLite initially

---

#### **7. Components & Integration Testing**

* UI integration tests
* Backend component tests (service-to-service)
* API mocks or stubs for early UI integration

---

#### **8. Redo Documents**

* Revise ARD, ERD, SRS based on prototype insights
* Add gaps/clarity discovered during real usage

---

#### **9. Test Plan Document**

* Manual test cases
* Automation goals
* Load/performance plans

---

#### **10. API Document**

* OpenAPI spec (Swagger)
* Includes:

  * Routes
  * Request/response schema
  * Auth requirements
  * Error codes

---

#### **11. Security Plan Document**

* Auth strategy (JWT, OAuth?)
* Role-based access
* Input validation, rate limits
* Future: encryption, audit logs, etc.

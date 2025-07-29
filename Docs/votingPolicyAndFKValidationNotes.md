## 📜 Named vs. Anonymous Voting Policy

### **1. Overview**

This document defines the behavioral contract of `voting_mode` in the `VoteBatch` entity for handling **named** and **anonymous** voting submissions.

---

### **2. Voting Modes**

#### 🔷 `named`

* Votes are **stored with association** to the user.
* Allows:

  * Editing existing votes
  * Deleting or resubmitting
  * Saving partial progress (incremental voting)

#### 🔷 `anonymous`

* Votes are **submitted in full and detached from the user**
* The only link between user and submission is via `vote_batch.user_id`, used for deduplication
* Does **not allow**:

  * Editing, deleting, or reviewing votes after submission
  * Re-submitting in any other mode

---

### **3. UX/Frontend Responsibilities**

* Must **warn users** before submitting an anonymous vote:

  > “Anonymous votes are final. You won’t be able to change your answers later.”
* Disable voting mode toggle after first submission
* Prevent anonymous submission until all required answers are complete

---

### **4. Backend Responsibilities**

* Validate that anonymous `vote_batches` are:

  * Only stored if `is_submitted = true`
  * Final and immutable
* Enforce:

  * One `anonymous` batch per user per survey
  * No update/delete permissions on anonymous vote data

---

### **5. Exceptions**

* Admins cannot access user-vote linkage for anonymous votes
* Deletion of surveys must cascade anonymized data (see validation notes)

---

## 🔐 External Key Handling Strategy (Without Foreign Keys)

In microservice environments, we don’t use DB-enforced foreign keys for external entities. But we still **need to validate**, **sync**, and **protect integrity** through logic.

---

### 🔧 External Fields:

| Field                | Source Service |
| -------------------- | -------------- |
| `user_id`            | User Service   |
| `survey_id`          | Survey Service |
| `question_id`        | Survey Service |
| `selected_option_id` | Survey Service |

---

### 🔍 Validation Strategy

| Action                 | When to Perform                 | How                                           |
| ---------------------- | ------------------------------- | --------------------------------------------- |
| **Validate existence** | At vote submission              | API call to Survey Service or cache           |
| **Validate ownership** | At batch creation (user/survey) | Verify user has access to survey              |
| **Check options**      | When submitting votes           | Validate `option_id` belongs to `question_id` |
| **Enforce vote rules** | During vote batch insert        | App logic: one vote per question              |

---

### 🧹 Deletion Handling

| Scenario                   | Action in Voting Service                                               |
| -------------------------- | ---------------------------------------------------------------------- |
| Survey deleted             | Soft-delete or purge matching `VoteBatch` + `Vote` records (via event) |
| Question or Option deleted | Optionally archive `Vote` data if already submitted                    |
| User deleted               | Retain batch records for audit (optional anonymization)                |

Use **event-based cleanup** (e.g., Kafka/event bus):

* When Survey Service deletes a survey → Voting Service listens → cleans up
* Optional: background job for orphan cleanup

---

### 🧠 Summary

| Principle            | Strategy                            |
| -------------------- | ----------------------------------- |
| Service independence | No foreign keys                     |
| Data integrity       | Validate through API/cache          |
| Soft coupling        | Use logical references only         |
| Lifecycle management | Clean via events or background jobs |

Excellent — this is a **crucial architecture step**, and we’ll handle it exhaustively.

---

## 1. **System Capabilities**

Our app involves **users voting on surveys** and **viewing results**. Each service manages its own domain:

| Service            | Responsibilities                         |
| ------------------ | ---------------------------------------- |
| **UsersService**   | Register, authenticate, manage user data |
| **SurveysService** | Create/manage surveys                    |
| **VotesService**   | Submit and track votes                   |
| **ResultsService** | Compute & provide aggregated results     |

---

## 2. **All Possible Incoming Requests from the Frontend**

We’ll list the **user-facing requests** across all app screens and features.

### 🔐 UsersService

| Action        | HTTP Verb | Endpoint          | Communication | Auth Required? |
| ------------- | --------- | ----------------- | ------------- | -------------- |
| Register      | POST      | `/users/register` | REST          | ❌              |
| Login         | POST      | `/users/login`    | REST          | ❌              |
| Get profile   | GET       | `/users/me`       | REST          | ✅              |
| Refresh token | POST      | `/users/refresh`  | REST          | ✅              |
| Logout        | POST      | `/users/logout`   | REST          | ✅              |

---

### 📋 SurveysService

| Action          | HTTP Verb | Endpoint        | Communication | Auth Required? |
| --------------- | --------- | --------------- | ------------- | -------------- |
| Create survey   | POST      | `/surveys`      | REST          | ✅              |
| Get all surveys | GET       | `/surveys`      | REST          | ✅              |
| Get one survey  | GET       | `/surveys/{id}` | REST          | ✅              |
| Edit survey     | PUT       | `/surveys/{id}` | REST          | ✅              |
| Delete survey   | DELETE    | `/surveys/{id}` | REST          | ✅              |

---

### ✅ VotesService

| Action                 | HTTP Verb | Endpoint            | Communication  | Auth Required? |
| ---------------------- | --------- | ------------------- | -------------- | -------------- |
| Submit vote            | POST      | `/votes`            | REST + SignalR | ✅              |
| Get my vote (optional) | GET       | `/votes/{surveyId}` | REST           | ✅              |

---

### 📊 ResultsService

| Action                 | HTTP Verb | Endpoint                   | Communication | Auth Required? |
| ---------------------- | --------- | -------------------------- | ------------- | -------------- |
| Get results for survey | GET       | `/results/{surveyId}`      | REST          | ✅              |
| Get live updates       | WebSocket | `/results/live/{surveyId}` | **SignalR**   | ✅              |

---

## Route Map via API Gateway

The gateway routes all traffic from the frontend to services. Here’s the **internal routing** breakdown:

### ➡️ Gateway to Services Routing Table

| Public Endpoint            | Internal Service | Internal Route   | Protocol  |
| -------------------------- | ---------------- | ---------------- | --------- |
| `/users/*`                 | UsersService     | `/api/users/*`   | REST      |
| `/surveys/*`               | SurveysService   | `/api/surveys/*` | REST      |
| `/votes/*`                 | VotesService     | `/api/votes/*`   | REST      |
| `/results/*`               | ResultsService   | `/api/results/*` | REST      |
| `/results/live/{surveyId}` | ResultsService   | SignalR Hub      | WebSocket |

> For **flexibility**, services implement an interface for **receiving communication**. Gateway **calls REST endpoints** for now. Later we can **inject SQS message handler** via DI.

---

## 🛠️ Optional Enhancements

### 📬 Internal Event-Based Messaging (SQS or Kafka)

Used for eventual consistency, background work, or decoupling:

| Event           | Producer       | Consumer                |
| --------------- | -------------- | ----------------------- |
| Vote submitted  | VotesService   | ResultsService          |
| Survey created  | SurveysService | ResultsService          |
| User registered | UsersService   | AuditService (optional) |

This can replace direct REST calls between services if you need **loose coupling**.

## We now have:

* 🔄 **Frontend-to-Gateway** request map
* 🧭 **Gateway-to-Services** routing table
* 💬 Plan for **push notifications** via SignalR
* 📨 Future-proof plan for **message broker support**

Great question. Let’s explore **possible cross-service transactions** or **composite operations** your app might need.

---

## 🧩 What is a Composite Transaction?

A **composite transaction** spans **multiple services** and requires:

* Orchestration of multiple operations
* Possibly consistency across services (eventually or immediately)
* Often requires rollback/compensation on failure

---

## 🧠 Candidate Composite Transactions in Your System

Below is an exhaustive list based on your current service breakdown.

---

### 🧑‍🤝‍🧑 1. **User Registration with Setup**

#### Description:

Register user → optionally create user profile → audit/log registration

#### Involves:

* `UsersService` (creates account)
* *(Optional)* `AuditService` (logs action via event)

#### Type:

* Mostly single service with optional event

---

### 📋 2. **Create Survey & Prepare Result Aggregation**

#### Description:

* A user creates a survey
* The system preps analytics (e.g. initialize result buckets)

#### Involves:

* `SurveysService` (creates survey)
* `ResultsService` (sets up result tracking structure)

#### Type:

* **Synchronous REST** from Gateway
* or **event-driven**: `SurveysService` emits "SurveyCreated" → `ResultsService` consumes

---

### ✅ 3. **Vote + Trigger Result Update**

#### Description:

* User submits a vote
* Update vote tally
* Push updated result to frontend via SignalR

#### Involves:

* `VotesService` (stores vote)
* `ResultsService` (updates tally)
* `API Gateway` or `ResultsService` (pushes via SignalR)

#### Type:

* Ideal as **event-based**: `VotesService` emits `VoteSubmitted` → others react
* Still possible via synchronous REST + push

---

### 📊 4. **Live Results Subscription**

#### Description:

* Client subscribes to live updates for a survey
* SignalR connects user to `ResultsService`
* Backend streams live updates

#### Involves:

* `API Gateway` (SignalR endpoint)
* `ResultsService` (hub + broadcast mechanism)
* Possibly a shared `ResultsCache` (like Redis)

#### Type:

* **Real-time**, **stateful**, **WebSocket**

---

### 🚫 5. **Delete User (and Their Content)**

#### Description:

* Delete user
* Delete their surveys, votes, profile

#### Involves:

* `UsersService` (deletes user)
* `SurveysService`, `VotesService` (clean related data)

#### Type:

* Composite deletion
* Either **synchronous chained** or **event-based** (`UserDeleted` event)
* **Compensation logic** needed if part fails

---

### 🧼 6. **Admin Cleanup (Survey + Results + Votes)**

#### Description:

* Admin removes a survey
* Deletes all related results and votes

#### Involves:

* `SurveysService` (deletes survey)
* `VotesService`, `ResultsService` (delete related data)

#### Type:

* Composite
* Ideal for **event-driven** cleanup

---

## ⚖️ Transaction Types

| Type              | Example                         | Method                                   | Consistency |
| ----------------- | ------------------------------- | ---------------------------------------- | ----------- |
| **Orchestrated**  | User deletion                   | API Gateway coordinates via REST         | Immediate   |
| **Choreographed** | Vote submitted → result updated | Services emit/consume events (SQS/Kafka) | Eventual    |
| **Atomic**        | User login                      | Single service                           | Strong      |

---

## 🧪 Summary of Composite Scenarios

| Scenario         | Services Involved               | Recommended Flow           |
| ---------------- | ------------------------------- | -------------------------- |
| Register + Setup | UsersService + AuditService     | REST + Event               |
| Create Survey    | SurveysService + ResultsService | REST or Event              |
| Submit Vote      | VotesService + ResultsService   | Event                      |
| Delete User      | UsersService + all others       | Orchestrated REST or Event |
| Delete Survey    | SurveysService + others         | Event                      |
| Live Updates     | ResultsService + Gateway        | SignalR                    |


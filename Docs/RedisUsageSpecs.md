## 🔹 Data Structures Needed in Redis

1. **Subscribers Map (question → userTokens)**

   * `subs:{questionId}` → Redis **Set** of user tokens.
   * Example:

     ```redis
     SADD subs:123 tokenA tokenB tokenC
     ```

2. **Updates Map (question → latest payload)**

   * `update:payload:{questionId}` → Redis **Hash/String/JSON** with the most recent payload (overwrites).
   * Example:

     ```redis
     SET update:payload:123 "{...latest QuestionResult...}"
     ```

3. **Queue of Updated QuestionIds**

   * `updates:queue` → Redis **List** (acts like a FIFO).
   * Contains only **unique IDs**, so the same question won’t be enqueued twice before it is processed.

4. **Set of Enqueued QuestionIds**

   * `updates:inqueue` → Redis **Set**.
   * Used to check if a question is already pending in the queue (avoids duplicates).
   * When dequeued → remove it from this set.

---

## 🔹 Flow

### Adding a Vote Update

1. Add/update the payload:
   `SET update:payload:{qid} payload`
2. If there are subscribers:

   * Check `SISMEMBER updates:inqueue {qid}`.
   * If **not present**:

     * Add `qid` to `updates:queue` (`RPUSH`)
     * Add `qid` to `updates:inqueue` (`SADD`)

### Polling Updates

1. `LPOP updates:queue` → dequeue one `qid`.
2. `SREM updates:inqueue {qid}` → mark it as available for future requeue.
3. Fetch latest payload: `GET update:payload:{qid}`.
4. Get subscribers: `SMEMBERS subs:{qid}`.
5. Send to all subscribers.
6. Clear payload: `DEL update:payload:{qid}` (optional).
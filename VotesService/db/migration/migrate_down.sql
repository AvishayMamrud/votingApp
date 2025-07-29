-- voting_db/migrate_down.sql
DROP INDEX IF EXISTS idx_vote_batches_user_survey;
DROP TABLE IF EXISTS votes;
DROP TABLE IF EXISTS vote_batches;
DROP TYPE IF EXISTS voting_mode_enum;

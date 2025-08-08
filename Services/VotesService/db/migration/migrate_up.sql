-- voting_db/migrate_up.sql
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TYPE voting_mode_enum AS ENUM ('anonymous', 'named');
CREATE TYPE question_type AS ENUM ('single', 'range', 'text');

-- Create VoteBatch table
CREATE TABLE vote_batch (
    id UUID PRIMARY KEY,
    survey_id UUID NOT NULL,
    user_id UUID NOT NULL,
    voting_mode voting_mode_enum NOT NULL,
    is_submitted BOOLEAN NOT NULL DEFAULT FALSE,
    submitted_at TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Create Vote table
CREATE TABLE vote (
    id UUID PRIMARY KEY,
    user_id UUID, -- nullable for anonymous
    survey_id UUID NOT NULL,
    question_id UUID NOT NULL,
    question_type question_type NOT NULL,
    selected_option_id UUID, -- for single
    range_value INTEGER,     -- for range
    open_text TEXT           -- for open
);

CREATE INDEX idx_vote_batches_user_survey ON vote_batch(user_id, survey_id);

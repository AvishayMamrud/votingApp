-- voting_db/migrate_up.sql
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TYPE voting_mode_enum AS ENUM ('anonymous', 'named');

CREATE TABLE vote_batches (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    survey_id UUID NOT NULL,         -- external reference
    user_id UUID NOT NULL,           -- external reference
    voting_mode voting_mode_enum NOT NULL,
    is_submitted BOOLEAN DEFAULT FALSE,
    submitted_at TIMESTAMP WITH TIME ZONE
);

CREATE TABLE votes (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    vote_batch_id UUID NOT NULL,
    question_id UUID NOT NULL,       -- external
    selected_option_id UUID NOT NULL, -- external
    created_at TIMESTAMP WITH TIME ZONE DEFAULT now()
);

CREATE INDEX idx_vote_batches_user_survey ON vote_batches(user_id, survey_id);

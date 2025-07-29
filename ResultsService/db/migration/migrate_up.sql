-- Enable UUIDs
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Aggregated results per option per question
CREATE TABLE survey_results (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    survey_id UUID NOT NULL,             -- from surveys service
    question_id UUID NOT NULL,
    option_id UUID NOT NULL,
    total_votes INT DEFAULT 0,
    last_updated TIMESTAMP WITH TIME ZONE DEFAULT now()
);

-- Optional demographic breakdown (if available in future)
CREATE TABLE survey_results_demographics (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    survey_result_id UUID NOT NULL REFERENCES survey_results(id) ON DELETE CASCADE,
    demographic_key TEXT NOT NULL,       -- e.g., "age_group"
    demographic_value TEXT NOT NULL,     -- e.g., "18-25"
    vote_count INT DEFAULT 0
);

CREATE INDEX idx_results_lookup ON survey_results(survey_id, question_id, option_id);
CREATE INDEX idx_results_demo_lookup ON survey_results_demographics(survey_result_id);

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- ENUM for question_type
CREATE TYPE question_type_enum AS ENUM ('single', 'range', 'text');

-- Central per-question result summary
CREATE TABLE question_result (
  id UUID PRIMARY KEY,
  survey_id UUID NOT NULL,
  survey_title VARCHAR NOT NULL,
  question_id UUID NOT NULL,
  question_text TEXT NOT NULL,
  question_type question_type_enum NOT NULL,
  total_answers INT NOT NULL,
  last_updated TIMESTAMP NOT NULL
);

-- Per-option aggregation for single choice
CREATE TABLE single_choice_result (
  id UUID PRIMARY KEY,
  question_result_id UUID NOT NULL,
  option_id UUID NOT NULL,
  option_text TEXT NOT NULL,
  vote_count INT NOT NULL DEFAULT 0,

  CONSTRAINT fk_scr_question_result
    FOREIGN KEY (question_result_id)
    REFERENCES question_result(id)
    ON DELETE CASCADE
);

-- Aggregated stats for range questions
CREATE TABLE range_question_result (
  id UUID PRIMARY KEY,
  question_result_id UUID NOT NULL,
  avg_value DOUBLE PRECISION NOT NULL,
  std_deviation DOUBLE PRECISION NOT NULL,

  CONSTRAINT fk_rqr_question_result
    FOREIGN KEY (question_result_id)
    REFERENCES question_result(id)
    ON DELETE CASCADE
);

CREATE INDEX idx_question_result ON question_result(survey_id, question_id);
CREATE INDEX idx_single_choice_result ON single_choice_result(question_result_id, option_id);
CREATE INDEX idx_range_question_result ON range_question_result(question_result_id);
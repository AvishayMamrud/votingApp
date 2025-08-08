-- 1. Create ENUM type for question_type
CREATE TYPE question_type_enum AS ENUM ('single_choice', 'range_number', 'open_text');

-- 2. Create Survey table
CREATE TABLE survey (
    id UUID PRIMARY KEY,
    title VARCHAR NOT NULL,
    description TEXT,
    owner_id UUID NOT NULL, -- External (not enforced)
    expires_at TIMESTAMP,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- 3. Create Question table
CREATE TABLE question (
    id UUID PRIMARY KEY,
    survey_id UUID NOT NULL,
    question_text TEXT NOT NULL,
    question_type question_type_enum NOT NULL,
    min_value INT,
    max_value INT,
    sort_order INT NOT NULL
);

-- 4. Create Option table
CREATE TABLE option (
    id UUID PRIMARY KEY,
    question_id UUID NOT NULL,
    option_text VARCHAR NOT NULL,
    sort_order INT NOT NULL
);

-- 5. Create SurveyTag table
CREATE TABLE survey_tag (
    survey_id UUID NOT NULL,
    tag VARCHAR NOT NULL,
    PRIMARY KEY (survey_id, tag)
);

-- 6. Indexes
CREATE INDEX idx_question_survey_id ON question(survey_id);
CREATE INDEX idx_option_question_id ON option(question_id);
CREATE INDEX idx_survey_tag_tag ON survey_tag(tag);

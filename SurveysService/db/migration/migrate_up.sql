-- surveys_db/migrate_up.sql
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TYPE question_type_enum AS ENUM ('single_choice', 'multiple_choice', 'text');

CREATE TABLE surveys (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    owner_id UUID NOT NULL,  -- external reference to User Service
    title TEXT NOT NULL,
    description TEXT,
    is_public BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT now(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT now()
);

CREATE TABLE questions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    survey_id UUID NOT NULL,
    question_text TEXT NOT NULL,
    question_type question_type_enum NOT NULL,
    position INT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT now()
);

CREATE TABLE options (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    question_id UUID NOT NULL,
    option_text TEXT NOT NULL,
    position INT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT now()
);

CREATE TABLE survey_access_policies (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    survey_id UUID NOT NULL,
    policy_type TEXT NOT NULL CHECK (policy_type IN ('user', 'group', 'role')),
    reference_id UUID NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT now()
);

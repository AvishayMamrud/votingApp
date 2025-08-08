-- users_db/migrate_up.sql
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE TYPE role_permission_type AS ENUM ('USE', 'CHANGE', 'OWN');
CREATE TYPE survey_permission_type AS ENUM ('VIEW', 'VOTE');

-- Create User table
CREATE TABLE users (
    id UUID PRIMARY KEY,
    username TEXT NOT NULL UNIQUE,
    email TEXT NOT NULL UNIQUE,
    hashed_password TEXT NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Create Role table
CREATE TABLE roles (
    id UUID PRIMARY KEY,
    name VARCHAR NOT NULL,
    description TEXT
);

-- Create RoleUserPermission table (join table between user and role with permission)
CREATE TABLE role_user_permission (
    role_id UUID NOT NULL,
    user_id UUID NOT NULL,
    permission role_permission_type NOT NULL,
    PRIMARY KEY (role_id, user_id)
);

-- Create RoleSurveyPermission table (role access to surveys)
CREATE TABLE role_survey_permission (
    role_id UUID NOT NULL,
    survey_id UUID NOT NULL,
    permission survey_permission_type NOT NULL,
    PRIMARY KEY (role_id, survey_id)
);

-- Indexes for lookup
CREATE INDEX idx_role_user_permission_user_id ON role_user_permission(user_id);
CREATE INDEX idx_role_survey_permission_survey_id ON role_survey_permission(survey_id);

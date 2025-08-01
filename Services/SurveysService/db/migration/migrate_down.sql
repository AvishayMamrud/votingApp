-- surveys_db/migrate_down.sql
DROP TABLE IF EXISTS survey_access_policies;
DROP TABLE IF EXISTS options;
DROP TABLE IF EXISTS questions;
DROP TABLE IF EXISTS surveys;
DROP TYPE IF EXISTS question_type_enum;

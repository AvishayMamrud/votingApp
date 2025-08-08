-- Drop indexes explicitly
DROP INDEX IF EXISTS idx_survey_tag_tag;
DROP INDEX IF EXISTS idx_option_question_id;
DROP INDEX IF EXISTS idx_question_survey_id;

-- Drop tables
DROP TABLE IF EXISTS survey_tag;
DROP TABLE IF EXISTS option;
DROP TABLE IF EXISTS question;
DROP TABLE IF EXISTS survey;

-- Drop ENUM type
DROP TYPE IF EXISTS question_type_enum;

DROP INDEX IF EXISTS idx_question_result;
DROP INDEX IF EXISTS idx_single_choice_result;
DROP INDEX IF EXISTS idx_range_question_result;

DROP TABLE IF EXISTS range_question_result;
DROP TABLE IF EXISTS single_choice_result;
DROP TABLE IF EXISTS question_result;

-- Drop ENUM type
DROP TYPE IF EXISTS question_type_enum;
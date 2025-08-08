-- users_db/migrate_down.sql
DROP INDEX IF EXISTS idx_role_user_permission_user_id;
DROP INDEX IF EXISTS idx_role_survey_permission_survey_id;

DROP TABLE IF EXISTS role_user_permission;
DROP TABLE IF EXISTS role_survey_permission;
DROP TABLE IF EXISTS roles;
DROP TABLE IF EXISTS users;

DROP TYPE IF EXISTS role_permission_type;
DROP TYPE IF EXISTS survey_permission_type;
DROP EXTENSION IF EXISTS "uuid-ossp";
GRANT SELECT, INSERT, UPDATE, DELETE ON `dmarc_entity` TO '{env}-dmarc-ent' IDENTIFIED BY '{password}';
GRANT SELECT, INSERT, UPDATE ON `dmarc_entity_history` TO '{env}-dmarc-ent' IDENTIFIED BY '{password}';

GRANT SELECT ON `dmarc_entity_history` TO '{env}-dmarc-api' IDENTIFIED BY '{password}';
GRANT SELECT ON `dmarc_entity` TO '{env}-dmarc-api' IDENTIFIED BY '{password}';

GRANT SELECT, INSERT, UPDATE, DELETE ON `dmarc_scheduled_records` TO '{env}-dmarc-sch' IDENTIFIED BY '{password}';

GRANT SELECT ON `dmarc_entity` TO '{env}_reports' IDENTIFIED BY '{password}';
GRANT SELECT ON `dmarc_entity_history` TO '{env}_reports' IDENTIFIED BY '{password}';
GRANT SELECT INTO S3 ON *.* TO '{env}_reports' IDENTIFIED BY '{password}';

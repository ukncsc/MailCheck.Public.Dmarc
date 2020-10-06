GRANT SELECT ON `dmarc_entity` TO '{env}_reports' IDENTIFIED BY '{password}';
GRANT SELECT ON `dmarc_entity_history` TO '{env}_reports' IDENTIFIED BY '{password}';
GRANT SELECT INTO S3 ON *.* TO '{env}_reports' IDENTIFIED BY '{password}';
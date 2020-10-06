GRANT SELECT, INSERT, UPDATE ON `dmarc_entity_history` TO '{env}-dmarc-ent' IDENTIFIED BY '{password}';
GRANT SELECT ON `dmarc_entity_history` TO '{env}-dmarc-api' IDENTIFIED BY '{password}';
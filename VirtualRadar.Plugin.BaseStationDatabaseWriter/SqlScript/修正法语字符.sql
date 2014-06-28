BEGIN TRANSACTION;
--UPDATE Country SET Name = 'Cote d''Ivoire' WHERE Name LIKE 'Côte d''Ivoire';
--UPDATE Country SET Name = 'Peru' WHERE Name LIKE 'Perú';
--UPDATE Country SET Name = 'Sao Tome Principe' WHERE Name LIKE 'São Tomé and Principe';

UPDATE Country SET Name = 'Côte d''Ivoire' WHERE Name LIKE 'Cote d''Ivoire';
UPDATE Country SET Name = 'Perú' WHERE Name LIKE 'Peru';
UPDATE Country SET Name = 'São Tomé and Principe' WHERE Name LIKE 'Sao Tome Principe';

COMMIT TRANSACTION;
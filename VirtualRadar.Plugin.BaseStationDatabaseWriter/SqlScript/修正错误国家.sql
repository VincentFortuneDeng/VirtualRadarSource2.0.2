BEGIN TRANSACTION;
--SELECT ModeSCountry, count(ModeSCountry) FROM AircraftReg GROUP BY ModeSCountry ORDER BY ModeSCountry;
--SELECT ModeSCountry, count(ModeSCountry) FROM AircraftReg WHERE ModeSCountry NOT IN(SELECT Name FROM Country) GROUP BY ModeSCountry ORDER BY ModeSCountry ;

UPDATE AircraftReg SET ModeSCountry = 'Brazil' WHERE ModeSCountry LIKE 'BRAZIL';

UPDATE AircraftReg SET ModeSCountry = 'Bosnia and Herzegovina' WHERE ModeSCountry LIKE 'Bosnia';

UPDATE AircraftReg SET ModeSCountry = 'Hong Kong' WHERE ModeSCountry LIKE 'China Hong Kong';

UPDATE AircraftReg SET ModeSCountry = 'Côte d''Ivoire' WHERE ModeSCountry LIKE 'Cote d''Ivoire';

UPDATE AircraftReg SET ModeSCountry = 'Isle of Man' WHERE ModeSCountry LIKE 'Isle of man';

UPDATE AircraftReg SET ModeSCountry = 'Italy' WHERE ModeSCountry LIKE 'ItalY';

UPDATE AircraftReg SET ModeSCountry = 'North Korea' WHERE ModeSCountry LIKE 'Korea North';

UPDATE AircraftReg SET ModeSCountry = 'South Korea' WHERE ModeSCountry LIKE 'Korea South';

UPDATE AircraftReg SET ModeSCountry = 'South Korea' WHERE ModeSCountry LIKE 'Korea south';

UPDATE AircraftReg SET ModeSCountry = 'South Korea' WHERE ModeSCountry LIKE 'Korea_South';

UPDATE AircraftReg SET ModeSCountry = 'Unknown or unassigned country' WHERE ModeSCountry LIKE 'Not Allocated' OR ModeSCountry LIKE '' ;

UPDATE AircraftReg SET ModeSCountry = 'Perú' WHERE ModeSCountry LIKE 'Peru';

UPDATE AircraftReg SET ModeSCountry = 'São Tomé and Principe' WHERE ModeSCountry LIKE 'Sao Tome Principe';

UPDATE AircraftReg SET ModeSCountry = 'Unitd States' WHERE ModeSCountry LIKE 'UnitdStates';

UPDATE AircraftReg SET ModeSCountry = 'United Arab Emirates' WHERE ModeSCountry LIKE 'United Arab emirates';

UPDATE AircraftReg SET ModeSCountry = 'United States' WHERE ModeSCountry LIKE 'United States M';

UPDATE AircraftReg SET ModeSCountry = 'United States' WHERE ModeSCountry LIKE 'United states';

UPDATE AircraftReg SET ModeSCountry = 'United States' WHERE ModeSCountry LIKE 'Unitd States';

UPDATE AircraftReg SET ModeSCountry = 'Viet Nam' WHERE ModeSCountry LIKE 'Viet-nam';

UPDATE AircraftReg SET ModeSCountry = 'Viet Nam' WHERE ModeSCountry LIKE 'Vietnam';

UPDATE AircraftReg SET ModeSCountry = 'Canada' WHERE ModeSCountry LIKE 'canada';

UPDATE AircraftReg SET ModeSCountry = 'India' WHERE ModeSCountry LIKE 'india';

UPDATE AircraftReg SET ModeSCountry = 'Congo (Brazzaville)' WHERE ModeSCountry LIKE 'Congo-ROC';

COMMIT TRANSACTION;
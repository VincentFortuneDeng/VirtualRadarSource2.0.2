BEGIN TRANSACTION;
SELECT * FROM AircraftReg WHERE ModeSCountry LIKE '%MIL%';
UPDATE AircraftReg SET ModeSCountry = substr(ModeSCountry,1,length(ModeScountry)-4) WHERE ModeSCountry LIKE '%MIL%';
SELECT * FROM AircraftReg WHERE ModeSCountry LIKE '%MIL%';
COMMIT TRANSACTION;


                  Begin Transaction;
Create  TABLE MAIN.[AircraftReg](
[AircraftID] integer PRIMARY KEY
,[FirstCreated] datetime NOT NULL
,[LastModified] datetime NOT NULL
,[ModeS] varchar(6) UNIQUE NOT NULL
,[ModeSCountry] varchar(24)
,[Country] varchar(24)
,[Registration] varchar(20)
,[CurrentRegDate] varchar(10)
,[PreviousID] varchar(10)
,[FirstRegDate] varchar(10)
,[Status] varchar(10)
,[DeRegDate] varchar(10)
,[Manufacturer] varchar(60)
,[ICAOTypeCode] varchar(10)
,[Type] varchar(40)
,[SerialNo] varchar(30)
,[PopularName] varchar(20)
,[GenericName] varchar(20)
,[AircraftClass] varchar(20)
,[Engines] varchar(40)
,[OwnershipStatus] varchar(10)
,[RegisteredOwners] varchar(100)
,[MTOW] varchar(10)
,[TotalHours] varchar(20)
,[YearBuilt] varchar(4)
,[CofACategory] varchar(30)
,[CofAExpiry] varchar(10)
,[UserNotes] varchar(300)
,[Interested] boolean NOT NULL DEFAULT 0
,[UserTag] varchar(5)
,[InfoURL] varchar(150)
,[PictureURL1] varchar(150)
,[PictureURL2] varchar(150)
,[PictureURL3] varchar(150)
,[UserBool1] boolean NOT NULL DEFAULT 0
,[UserBool2] boolean NOT NULL DEFAULT 0
,[UserBool3] boolean NOT NULL DEFAULT 0
,[UserBool4] boolean NOT NULL DEFAULT 0
,[UserBool5] boolean NOT NULL DEFAULT 0
,[UserString1] varchar(20)
,[UserString2] varchar(20)
,[UserString3] varchar(20)
,[UserString4] varchar(20)
,[UserString5] varchar(20)
,[UserInt1] integer DEFAULT 0
,[UserInt2] integer DEFAULT 0
,[UserInt3] integer DEFAULT 0
,[UserInt4] integer DEFAULT 0
,[UserInt5] integer DEFAULT 0
,[OperatorFlagCode] varchar(20)
   
) ;

CREATE INDEX AircraftRegAircraftClass ON AircraftReg(AircraftClass);
CREATE INDEX AircraftRegCountry ON AircraftReg(Country);
CREATE INDEX AircraftRegGenericName ON AircraftReg(GenericName);
CREATE INDEX AircraftRegICAOTypeCode ON AircraftReg(ICAOTypeCode);
CREATE INDEX AircraftRegInterested ON AircraftReg(Interested);
CREATE INDEX AircraftRegManufacturer ON AircraftReg(Manufacturer);
CREATE INDEX AircraftRegModeS ON AircraftReg(ModeS);
CREATE INDEX AircraftRegModeSCountry ON AircraftReg(ModeSCountry);
CREATE INDEX AircraftRegPopularName ON AircraftReg(PopularName);
CREATE INDEX AircraftRegRegisteredOwners ON AircraftReg(RegisteredOwners);
CREATE INDEX AircraftRegRegistration ON AircraftReg(Registration);
CREATE INDEX AircraftRegSerialNo ON AircraftReg(SerialNo);
CREATE INDEX AircraftRegType ON AircraftReg(Type);
CREATE INDEX AircraftRegUserTag ON AircraftReg(UserTag);
CREATE INDEX AircraftRegYearBuilt ON AircraftReg(YearBuilt);

Commit Transaction;
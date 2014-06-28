BEGIN TRANSACTION;

--SELECT * FROM AircraftReg WHERE ICAOTypeCode LIKE OperatorFlagCode AND OperatorFlagCode <>'' COLLATE NOCASE;--查询航空公司代码等于机型
--UPDATE AircraftReg SET OperatorFlagCode= '' WHERE OperatorFlagCode <>'' AND Registration LIKE OperatorFlagCode COLLATE NOCASE;--去掉注册码为航空公司代码情况
--SELECT ModeS,Registration,/*CofAExpiry,*/PreviousID,SerialNo,ModeSCountry,ICAOTypeCode,Type,RegisteredOwners,CofACategory,OperatorFlagCode FROM AircraftReg WHERE Registration= OperatorFlagCode AND OperatorFlagCode <>'' COLLATE NOCASE;--查询注册号等于航空公司代码情况
--SELECT * FROM AircraftReg WHERE OperatorFlagCode <>'' AND Registration LIKE OperatorFlagCode COLLATE NOCASE;--查询注册号等于航空公司代码情况
--UPDATE AircraftReg SET OperatorFlagCode= '' WHERE OperatorFlagCode <>'' AND ICAOTypeCode LIKE OperatorFlagCode COLLATE NOCASE;--去掉飞机型号为航空公司代码情况

--UPDATE AircraftReg SET OperatorFlagCode= upper(substr(OperatorFlagCode,1,3)) WHERE substr(OperatorFlagCode,1,3) IN (SELECT Icao FROM Operator WHERE Icao<>'') AND length(OperatorFlagCode)>3 COLLATE NOCASE;--前三位航空公司代码存在查出插入
--UPDATE AircraftReg SET OperatorFlagCode=upper(OperatorFlagCode) WHERE length(OperatorFlagCode)=3;--三字码更新为大写

SELECT ModeS,Registration,/*CofAExpiry,*/PreviousID,SerialNo,ModeSCountry,ICAOTypeCode,Type,RegisteredOwners,CofACategory,OperatorFlagCode FROM AircraftReg WHERE length(OperatorFlagCode)>3;--三字码以上查询

--SELECT ModeS,Registration,/*CofAExpiry,*/PreviousID,SerialNo,ModeSCountry,ICAOTypeCode,Type,RegisteredOwners,CofACategory,OperatorFlagCode FROM AircraftReg WHERE RegisteredOwners IN (SELECT Name FROM Operator WHERE Name <>'') AND length(OperatorFlagCode)>3;--三字码以上且名称存在航空公司表中数据

--UPDATE AircraftReg SET OperatorFlagCode = (SELECT Icao FROM Operator WHERE RegisteredOwners LIKE Operator.Name COLLATE NOCASE ) WHERE RegisteredOwners IN (SELECT Name FROM Operator WHERE Name <>'') AND length(OperatorFlagCode)>3;--查询航空公司库中名称相同代码的公司名称
--SELECT ModeS,Registration,/*CofAExpiry,*/PreviousID,SerialNo,ModeSCountry,ICAOTypeCode,Type,RegisteredOwners,CofACategory,OperatorFlagCode  FROM AircraftReg WHERE OperatorFlagCode NOT IN (SELECT Icao FROM Operator WHERE Name <>'') AND length(OperatorFlagCode)=3;--查询在航空公司表中不存在的三位码数据

COMMIT TRANSACTION;

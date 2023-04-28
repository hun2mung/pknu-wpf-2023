CREATE TABLE `dustsensor` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Dev_id` varchar(20) COLLATE utf8mb3_bin DEFAULT NULL,
  `Name` varchar(20) COLLATE utf8mb3_bin DEFAULT NULL,
  `Loc` varchar(100) COLLATE utf8mb3_bin DEFAULT NULL,
  `Coordx` double DEFAULT NULL,
  `Coordy` double DEFAULT NULL,
  `Ison` bit(1) DEFAULT NULL,
  `Pm10_after` int DEFAULT NULL,
  `Pm25_after` int DEFAULT NULL,
  `State` int DEFAULT NULL,
  `Timestamp` datetime DEFAULT NULL,
  `Company_id` varchar(50) COLLATE utf8mb3_bin DEFAULT NULL,
  `Company_name` varchar(50) COLLATE utf8mb3_bin DEFAULT NULL,
  PRIMARY KEY (`Id`)
)
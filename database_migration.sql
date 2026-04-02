
SET FOREIGN_KEY_CHECKS = 0;


CREATE TABLE IF NOT EXISTS `Authorization` (
    `AuthorizationID` INT NOT NULL AUTO_INCREMENT,
    `Substitute`      TINYINT(1) NOT NULL DEFAULT 0,
    `Employee`        TINYINT(1) NOT NULL DEFAULT 0,
    `Scheduler`       TINYINT(1) NOT NULL DEFAULT 0,
    `Admin`           TINYINT(1) NOT NULL DEFAULT 0,
    PRIMARY KEY (`AuthorizationID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


CREATE TABLE IF NOT EXISTS `Location` (
    `LocationID` INT NOT NULL AUTO_INCREMENT,
    `Name`       VARCHAR(50) NOT NULL,
    `Address`    VARCHAR(50) NOT NULL,
    `ZipCode`    INT NOT NULL,
    PRIMARY KEY (`LocationID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


CREATE TABLE IF NOT EXISTS `Role` (
    `RoleID`             INT NOT NULL AUTO_INCREMENT,
    `Name`               VARCHAR(50) NOT NULL,
    `ResponsibilityArea` VARCHAR(50) NOT NULL,
    PRIMARY KEY (`RoleID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


CREATE TABLE IF NOT EXISTS `Employee` (
    `EmployeeID`      INT NOT NULL AUTO_INCREMENT,
    `FirstName`       VARCHAR(50) NOT NULL,
    `LastName`        VARCHAR(50) NOT NULL,
    `Email`           VARCHAR(80) NOT NULL,
    `PhoneNumber`     INT NOT NULL,
    `ShiftType`       VARCHAR(20) NOT NULL,   -- 'Day' | 'Midday' | 'Night'
    `PinCode`         INT NOT NULL,
    `LocationID`      INT NOT NULL,
    `AuthorizationID` INT NOT NULL,
    PRIMARY KEY (`EmployeeID`),
    INDEX `IX_Employee_LocationID`      (`LocationID`),
    INDEX `IX_Employee_AuthorizationID` (`AuthorizationID`),
    CONSTRAINT `FK_Employee_Location`      FOREIGN KEY (`LocationID`)      REFERENCES `Location`      (`LocationID`) ON DELETE CASCADE,
    CONSTRAINT `FK_Employee_Authorization` FOREIGN KEY (`AuthorizationID`) REFERENCES `Authorization` (`AuthorizationID`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


CREATE TABLE IF NOT EXISTS `Resident` (
    `ResidentID`  INT NOT NULL AUTO_INCREMENT,
    `FirstName`   VARCHAR(50)  NOT NULL,
    `LastName`    VARCHAR(50)  NOT NULL,
    `Room`        VARCHAR(20)  NOT NULL,
    `RiskLevel`   VARCHAR(20)  NOT NULL,   -- 'Green' | 'Yellow' | 'Red'
    `ShoppingDay` DATETIME(6)  NOT NULL,
    `Payment`     VARCHAR(200) NOT NULL,
    `LocationID`  INT NOT NULL,
    PRIMARY KEY (`ResidentID`),
    INDEX `IX_Resident_LocationID` (`LocationID`),
    CONSTRAINT `FK_Resident_Location` FOREIGN KEY (`LocationID`) REFERENCES `Location` (`LocationID`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


CREATE TABLE IF NOT EXISTS `EmployeeRole` (
    `EmployeesEmployeeID` INT NOT NULL,
    `RolesRoleID`         INT NOT NULL,
    PRIMARY KEY (`EmployeesEmployeeID`, `RolesRoleID`),
    INDEX `IX_EmployeeRole_RolesRoleID` (`RolesRoleID`),
    CONSTRAINT `FK_EmployeeRole_Employee` FOREIGN KEY (`EmployeesEmployeeID`) REFERENCES `Employee` (`EmployeeID`) ON DELETE CASCADE,
    CONSTRAINT `FK_EmployeeRole_Role`     FOREIGN KEY (`RolesRoleID`)         REFERENCES `Role`     (`RoleID`)     ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


CREATE TABLE IF NOT EXISTS `Medicin` (
    `MedicinID`  INT NOT NULL AUTO_INCREMENT,
    `Time`       DATETIME(6) NOT NULL,
    `ResidentID` INT NOT NULL,
    PRIMARY KEY (`MedicinID`),
    INDEX `IX_Medicin_ResidentID` (`ResidentID`),
    CONSTRAINT `FK_Medicin_Resident` FOREIGN KEY (`ResidentID`) REFERENCES `Resident` (`ResidentID`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


CREATE TABLE IF NOT EXISTS `PNMedicin` (
    `PNMedicinID` INT NOT NULL AUTO_INCREMENT,
    `Type`        VARCHAR(50) NOT NULL,
    `Time`        DATETIME(6) NOT NULL,
    `ResidentID`  INT NOT NULL,
    PRIMARY KEY (`PNMedicinID`),
    INDEX `IX_PNMedicin_ResidentID` (`ResidentID`),
    CONSTRAINT `FK_PNMedicin_Resident` FOREIGN KEY (`ResidentID`) REFERENCES `Resident` (`ResidentID`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


CREATE TABLE IF NOT EXISTS `Status` (
    `StatusID`    INT NOT NULL AUTO_INCREMENT,
    `Description` LONGTEXT NOT NULL,
    `Time`        DATETIME(6) NOT NULL,
    `ResidentID`  INT NOT NULL,
    PRIMARY KEY (`StatusID`),
    INDEX `IX_Status_ResidentID` (`ResidentID`),
    CONSTRAINT `FK_Status_Resident` FOREIGN KEY (`ResidentID`) REFERENCES `Resident` (`ResidentID`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId`    VARCHAR(150) NOT NULL,
    `ProductVersion` VARCHAR(32)  NOT NULL,
    PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260402154431_InitialCreate', '9.0.0');

SET FOREIGN_KEY_CHECKS = 1;

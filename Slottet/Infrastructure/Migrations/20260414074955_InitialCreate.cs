using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE TABLE IF NOT EXISTS `Authorization` (`AuthorizationID` int NOT NULL AUTO_INCREMENT, `Substitute` tinyint(1) NOT NULL DEFAULT 0, `Employee` tinyint(1) NOT NULL DEFAULT 0, `Scheduler` tinyint(1) NOT NULL DEFAULT 0, `Admin` tinyint(1) NOT NULL DEFAULT 0, CONSTRAINT `PK_Authorization` PRIMARY KEY (`AuthorizationID`)) CHARACTER SET=utf8mb4;");
            migrationBuilder.Sql("ALTER TABLE `Authorization` ADD COLUMN IF NOT EXISTS `Substitute` tinyint(1) NOT NULL DEFAULT 0, ADD COLUMN IF NOT EXISTS `Employee` tinyint(1) NOT NULL DEFAULT 0, ADD COLUMN IF NOT EXISTS `Scheduler` tinyint(1) NOT NULL DEFAULT 0, ADD COLUMN IF NOT EXISTS `Admin` tinyint(1) NOT NULL DEFAULT 0;");
            migrationBuilder.Sql("CREATE TABLE IF NOT EXISTS `Location` (`LocationID` int NOT NULL AUTO_INCREMENT, `Name` varchar(50) NOT NULL, `Address` varchar(50) NOT NULL, `ZipCode` int NOT NULL, CONSTRAINT `PK_Location` PRIMARY KEY (`LocationID`)) CHARACTER SET=utf8mb4;");
            migrationBuilder.Sql("CREATE TABLE IF NOT EXISTS `Role` (`RoleID` int NOT NULL AUTO_INCREMENT, `Name` varchar(50) NOT NULL, `ResponsibilityArea` varchar(50) NOT NULL, CONSTRAINT `PK_Role` PRIMARY KEY (`RoleID`)) CHARACTER SET=utf8mb4;");
            migrationBuilder.Sql("CREATE TABLE IF NOT EXISTS `Resident` (`ResidentID` int NOT NULL AUTO_INCREMENT, `FirstName` varchar(50) NOT NULL, `LastName` varchar(50) NOT NULL, `Room` varchar(20) NOT NULL, `RiskLevel` varchar(20) NOT NULL, `ShoppingDay` datetime(6) NOT NULL, `Payment` varchar(200) NOT NULL, `LocationID` int NOT NULL, CONSTRAINT `PK_Resident` PRIMARY KEY (`ResidentID`), CONSTRAINT `FK_Resident_Location_LocationID` FOREIGN KEY (`LocationID`) REFERENCES `Location` (`LocationID`) ON DELETE CASCADE) CHARACTER SET=utf8mb4;");
            migrationBuilder.Sql("CREATE TABLE IF NOT EXISTS `Employee` (`EmployeeID` int NOT NULL AUTO_INCREMENT, `FirstName` varchar(50) NOT NULL, `LastName` varchar(50) NOT NULL, `Email` varchar(80) NOT NULL, `PhoneNumber` int NOT NULL, `ShiftType` varchar(20) NOT NULL, `PinCode` int NOT NULL, `LocationID` int NOT NULL, `AuthorizationID` int NOT NULL, CONSTRAINT `PK_Employee` PRIMARY KEY (`EmployeeID`), CONSTRAINT `FK_Employee_Authorization_AuthorizationID` FOREIGN KEY (`AuthorizationID`) REFERENCES `Authorization` (`AuthorizationID`) ON DELETE CASCADE, CONSTRAINT `FK_Employee_Location_LocationID` FOREIGN KEY (`LocationID`) REFERENCES `Location` (`LocationID`) ON DELETE CASCADE) CHARACTER SET=utf8mb4;");
            migrationBuilder.Sql("CREATE TABLE IF NOT EXISTS `EmployeeRole` (`EmployeesEmployeeID` int NOT NULL, `RolesRoleID` int NOT NULL, CONSTRAINT `PK_EmployeeRole` PRIMARY KEY (`EmployeesEmployeeID`, `RolesRoleID`), CONSTRAINT `FK_EmployeeRole_Employee_EmployeesEmployeeID` FOREIGN KEY (`EmployeesEmployeeID`) REFERENCES `Employee` (`EmployeeID`) ON DELETE CASCADE, CONSTRAINT `FK_EmployeeRole_Role_RolesRoleID` FOREIGN KEY (`RolesRoleID`) REFERENCES `Role` (`RoleID`) ON DELETE CASCADE) CHARACTER SET=utf8mb4;");
            migrationBuilder.Sql("CREATE TABLE IF NOT EXISTS `Medicin` (`MedicinID` int NOT NULL AUTO_INCREMENT, `Time` datetime(6) NOT NULL, `ResidentID` int NOT NULL, CONSTRAINT `PK_Medicin` PRIMARY KEY (`MedicinID`), CONSTRAINT `FK_Medicin_Resident_ResidentID` FOREIGN KEY (`ResidentID`) REFERENCES `Resident` (`ResidentID`) ON DELETE CASCADE) CHARACTER SET=utf8mb4;");
            migrationBuilder.Sql("CREATE TABLE IF NOT EXISTS `PNMedicin` (`PNMedicinID` int NOT NULL AUTO_INCREMENT, `Type` varchar(50) NOT NULL, `Time` datetime(6) NOT NULL, `ResidentID` int NOT NULL, CONSTRAINT `PK_PNMedicin` PRIMARY KEY (`PNMedicinID`), CONSTRAINT `FK_PNMedicin_Resident_ResidentID` FOREIGN KEY (`ResidentID`) REFERENCES `Resident` (`ResidentID`) ON DELETE CASCADE) CHARACTER SET=utf8mb4;");
            migrationBuilder.Sql("CREATE TABLE IF NOT EXISTS `Status` (`StatusID` int NOT NULL AUTO_INCREMENT, `Description` longtext NOT NULL, `Time` datetime(6) NOT NULL, `ResidentID` int NOT NULL, CONSTRAINT `PK_Status` PRIMARY KEY (`StatusID`), CONSTRAINT `FK_Status_Resident_ResidentID` FOREIGN KEY (`ResidentID`) REFERENCES `Resident` (`ResidentID`) ON DELETE CASCADE) CHARACTER SET=utf8mb4;");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS `IX_Employee_AuthorizationID` ON `Employee` (`AuthorizationID`);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS `IX_Employee_LocationID` ON `Employee` (`LocationID`);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS `IX_EmployeeRole_RolesRoleID` ON `EmployeeRole` (`RolesRoleID`);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS `IX_Medicin_ResidentID` ON `Medicin` (`ResidentID`);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS `IX_PNMedicin_ResidentID` ON `PNMedicin` (`ResidentID`);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS `IX_Resident_LocationID` ON `Resident` (`LocationID`);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS `IX_Status_ResidentID` ON `Status` (`ResidentID`);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeRole");

            migrationBuilder.DropTable(
                name: "Medicin");

            migrationBuilder.DropTable(
                name: "PNMedicin");

            migrationBuilder.DropTable(
                name: "Status");

            migrationBuilder.DropTable(
                name: "Employee");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.DropTable(
                name: "Resident");

            migrationBuilder.DropTable(
                name: "Authorization");

            migrationBuilder.DropTable(
                name: "Location");
        }
    }
}

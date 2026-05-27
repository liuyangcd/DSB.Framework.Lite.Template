CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260527090310_update001') THEN

    ALTER DATABASE CHARACTER SET utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260527090310_update001') THEN

    CREATE TABLE `SystemPermissions` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `ParentId` char(36) COLLATE ascii_general_ci NULL,
        `Code` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Name` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Type` int NOT NULL,
        `Url` varchar(200) CHARACTER SET utf8mb4 NULL,
        `Icon` varchar(200) CHARACTER SET utf8mb4 NULL,
        `Status` tinyint unsigned NOT NULL,
        `Remark` varchar(2000) CHARACTER SET utf8mb4 NULL,
        `Sort` int NOT NULL,
        `EntityStatus` tinyint(1) NOT NULL,
        `CreateDateAt` datetime(6) NOT NULL,
        `UpdateDateAt` datetime(6) NULL,
        CONSTRAINT `PK_SystemPermissions` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260527090310_update001') THEN

    CREATE TABLE `SystemRolePermissions` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `RoleId` char(36) COLLATE ascii_general_ci NOT NULL,
        `PermissionId` char(36) COLLATE ascii_general_ci NOT NULL,
        `EntityStatus` tinyint(1) NOT NULL,
        `CreateDateAt` datetime(6) NOT NULL,
        `UpdateDateAt` datetime(6) NULL,
        CONSTRAINT `PK_SystemRolePermissions` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260527090310_update001') THEN

    CREATE TABLE `SystemRoles` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Code` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Name` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Remark` varchar(2000) CHARACTER SET utf8mb4 NULL,
        `Status` tinyint unsigned NOT NULL,
        `CreatedUserId` char(36) COLLATE ascii_general_ci NULL,
        `Sort` int NOT NULL,
        `EntityStatus` tinyint(1) NOT NULL,
        `CreateDateAt` datetime(6) NOT NULL,
        `UpdateDateAt` datetime(6) NULL,
        CONSTRAINT `PK_SystemRoles` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260527090310_update001') THEN

    CREATE TABLE `SystemUploadFiles` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Type` int NOT NULL,
        `SavePath` varchar(1000) CHARACTER SET utf8mb4 NOT NULL,
        `SaveName` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `OriginName` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
        `Extension` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `FileSize` bigint NOT NULL,
        `Url` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
        `EntityStatus` tinyint(1) NOT NULL,
        `CreateDateAt` datetime(6) NOT NULL,
        `UpdateDateAt` datetime(6) NULL,
        CONSTRAINT `PK_SystemUploadFiles` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260527090310_update001') THEN

    CREATE TABLE `SystemUserRoles` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `UserId` char(36) COLLATE ascii_general_ci NOT NULL,
        `RoleId` char(36) COLLATE ascii_general_ci NOT NULL,
        `EntityStatus` tinyint(1) NOT NULL,
        `CreateDateAt` datetime(6) NOT NULL,
        `UpdateDateAt` datetime(6) NULL,
        CONSTRAINT `PK_SystemUserRoles` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260527090310_update001') THEN

    CREATE TABLE `SystemUsers` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Name` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `Account` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `Password` varchar(50) CHARACTER SET utf8mb4 NULL,
        `Salt` varchar(50) CHARACTER SET utf8mb4 NULL,
        `Birth` datetime(6) NULL,
        `Email` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Phone` varchar(16) CHARACTER SET utf8mb4 NULL,
        `Avatar` varchar(200) CHARACTER SET utf8mb4 NULL,
        `Status` tinyint unsigned NOT NULL,
        `CreatedUserId` char(36) COLLATE ascii_general_ci NULL,
        `Remark` varchar(2000) CHARACTER SET utf8mb4 NULL,
        `EntityStatus` tinyint(1) NOT NULL,
        `CreateDateAt` datetime(6) NOT NULL,
        `UpdateDateAt` datetime(6) NULL,
        CONSTRAINT `PK_SystemUsers` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260527090310_update001') THEN

    CREATE UNIQUE INDEX `IX_SystemPermissions_Code` ON `SystemPermissions` (`Code`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260527090310_update001') THEN

    CREATE INDEX `IX_SystemPermissions_Sort_CreateDateAt` ON `SystemPermissions` (`Sort`, `CreateDateAt` DESC);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260527090310_update001') THEN

    CREATE INDEX `IX_SystemRolePermissions_PermissionId` ON `SystemRolePermissions` (`PermissionId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260527090310_update001') THEN

    CREATE UNIQUE INDEX `IX_SystemRolePermissions_RoleId_PermissionId` ON `SystemRolePermissions` (`RoleId`, `PermissionId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260527090310_update001') THEN

    CREATE UNIQUE INDEX `IX_SystemRoles_Code` ON `SystemRoles` (`Code`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260527090310_update001') THEN

    CREATE INDEX `IX_SystemRoles_Sort_CreateDateAt` ON `SystemRoles` (`Sort`, `CreateDateAt` DESC);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260527090310_update001') THEN

    CREATE INDEX `IX_SystemUserRoles_RoleId` ON `SystemUserRoles` (`RoleId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260527090310_update001') THEN

    CREATE UNIQUE INDEX `IX_SystemUserRoles_UserId_RoleId` ON `SystemUserRoles` (`UserId`, `RoleId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260527090310_update001') THEN

    CREATE UNIQUE INDEX `IX_SystemUsers_Account` ON `SystemUsers` (`Account`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260527090310_update001') THEN

    CREATE INDEX `IX_SystemUsers_CreateDateAt` ON `SystemUsers` (`CreateDateAt` DESC);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260527090310_update001') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260527090310_update001', '8.0.26');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;


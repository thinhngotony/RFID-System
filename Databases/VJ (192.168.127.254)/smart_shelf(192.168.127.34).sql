/*
 Navicat Premium Data Transfer

 Source Server         : Shelf DNP
 Source Server Type    : MariaDB
 Source Server Version : 100704 (10.7.4-MariaDB)
 Source Host           : 192.168.127.34:3306
 Source Schema         : smart_shelf

 Target Server Type    : MariaDB
 Target Server Version : 100704 (10.7.4-MariaDB)
 File Encoding         : 65001

 Date: 26/04/2023 10:55:42
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for ccode_info
-- ----------------------------
DROP TABLE IF EXISTS `ccode_info`;
CREATE TABLE `ccode_info`  (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `type` int(11) NULL DEFAULT NULL,
  `digit` varchar(2) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL DEFAULT NULL,
  `description` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 89 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_unicode_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for drfid_cm_video
-- ----------------------------
DROP TABLE IF EXISTS `drfid_cm_video`;
CREATE TABLE `drfid_cm_video`  (
  `dcv_id` int(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `dcv_jan_cd` varchar(13) CHARACTER SET utf8mb3 COLLATE utf8mb3_unicode_ci NOT NULL,
  `dcv_video_url` text CHARACTER SET utf8mb3 COLLATE utf8mb3_unicode_ci NOT NULL,
  PRIMARY KEY (`dcv_id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 4 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_unicode_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for drfid_log_move
-- ----------------------------
DROP TABLE IF EXISTS `drfid_log_move`;
CREATE TABLE `drfid_log_move`  (
  `dlm_id` mediumint(9) NOT NULL AUTO_INCREMENT,
  `dlm_date` date NULL DEFAULT '0000-00-00',
  `dlm_rfid_cd` varchar(45) CHARACTER SET utf8mb3 COLLATE utf8mb3_unicode_ci NULL DEFAULT NULL,
  `dlm_cnt` int(11) NULL DEFAULT NULL,
  `dlm_outdate` timestamp NULL DEFAULT NULL,
  `dlm_indate` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`dlm_id`) USING BTREE,
  INDEX `dlm_rfid_cd_index`(`dlm_rfid_cd`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 210110 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_unicode_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for drfid_product_pos
-- ----------------------------
DROP TABLE IF EXISTS `drfid_product_pos`;
CREATE TABLE `drfid_product_pos`  (
  `dpp_id` mediumint(9) NOT NULL AUTO_INCREMENT,
  `dpp_shelf_pos` int(7) NULL DEFAULT NULL,
  `dpp_shelf_col_pos` int(7) NULL DEFAULT NULL,
  `dpp_jan_cd` varchar(13) CHARACTER SET utf8mb3 COLLATE utf8mb3_unicode_ci NULL DEFAULT NULL,
  `dpp_rfid_cd` varchar(45) CHARACTER SET utf8mb3 COLLATE utf8mb3_unicode_ci NULL DEFAULT NULL,
  `dpp_isbn` varchar(13) CHARACTER SET utf8mb3 COLLATE utf8mb3_unicode_ci NULL DEFAULT NULL,
  `dpp_product_name` varchar(200) CHARACTER SET utf8mb3 COLLATE utf8mb3_unicode_ci NULL DEFAULT NULL,
  `dpp_scaner_name` varchar(45) CHARACTER SET utf8mb3 COLLATE utf8mb3_unicode_ci NULL DEFAULT NULL,
  `dpp_shelf_name` varchar(45) CHARACTER SET utf8mb3 COLLATE utf8mb3_unicode_ci NULL DEFAULT NULL,
  `dpp_image_url` longtext CHARACTER SET utf8mb3 COLLATE utf8mb3_unicode_ci NULL DEFAULT NULL,
  PRIMARY KEY (`dpp_id`) USING BTREE,
  INDEX `dpp_rfid_cd_index`(`dpp_rfid_cd`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1609 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_unicode_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for drfid_raw_data
-- ----------------------------
DROP TABLE IF EXISTS `drfid_raw_data`;
CREATE TABLE `drfid_raw_data`  (
  `drd_id` int(11) NOT NULL AUTO_INCREMENT,
  `drd_rfid_cd` varchar(45) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `drd_anten_no` varchar(45) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `drd_rssi` varchar(45) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `drd_shelf_no` varchar(45) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `m_10` varchar(45) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `n_10` varchar(45) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  PRIMARY KEY (`drd_id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 530 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for drfid_special_tag
-- ----------------------------
DROP TABLE IF EXISTS `drfid_special_tag`;
CREATE TABLE `drfid_special_tag`  (
  `dst_rfid_cd` varchar(45) CHARACTER SET utf8mb3 COLLATE utf8mb3_unicode_ci NOT NULL,
  `dst_type` varchar(10) CHARACTER SET utf8mb3 COLLATE utf8mb3_unicode_ci NOT NULL,
  `dst_display_title` varchar(100) CHARACTER SET utf8mb3 COLLATE utf8mb3_unicode_ci NULL DEFAULT NULL,
  `dst_sheft_type` varchar(45) CHARACTER SET utf8mb3 COLLATE utf8mb3_unicode_ci NULL DEFAULT NULL,
  `dst_shelf_name` varchar(45) CHARACTER SET utf8mb3 COLLATE utf8mb3_unicode_ci NULL DEFAULT NULL,
  PRIMARY KEY (`dst_rfid_cd`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_unicode_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for mst_antena
-- ----------------------------
DROP TABLE IF EXISTS `mst_antena`;
CREATE TABLE `mst_antena`  (
  `shelf_no` varchar(50) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `antena_no` varchar(50) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `direction` int(11) NULL DEFAULT NULL,
  `row` int(11) NULL DEFAULT NULL,
  `col` int(11) NULL DEFAULT NULL,
  `col_size` int(11) NULL DEFAULT NULL,
  `scan_col_start` int(11) NULL DEFAULT NULL,
  `scan_col_end` int(11) NULL DEFAULT NULL,
  PRIMARY KEY (`shelf_no`, `antena_no`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for shelf_calc_location
-- ----------------------------
DROP TABLE IF EXISTS `shelf_calc_location`;
CREATE TABLE `shelf_calc_location`  (
  `shelf_no` varchar(50) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `EPC` varchar(50) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `col` int(11) NULL DEFAULT NULL,
  `row` int(11) NULL DEFAULT NULL,
  `jancode` varchar(45) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `product_name` varchar(45) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `link_image` varchar(45) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  PRIMARY KEY (`shelf_no`, `EPC`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

SET FOREIGN_KEY_CHECKS = 1;

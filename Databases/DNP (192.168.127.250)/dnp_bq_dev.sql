/*
 Navicat Premium Data Transfer

 Source Server         : Server 250
 Source Server Type    : MariaDB
 Source Server Version : 100611 (10.6.11-MariaDB-0ubuntu0.22.04.1)
 Source Host           : 192.168.127.250:3306
 Source Schema         : dnp_bq_dev

 Target Server Type    : MariaDB
 Target Server Version : 100611 (10.6.11-MariaDB-0ubuntu0.22.04.1)
 File Encoding         : 65001

 Date: 26/04/2023 10:53:38
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for rfid_top_best
-- ----------------------------
DROP TABLE IF EXISTS `rfid_top_best`;
CREATE TABLE `rfid_top_best`  (
  `rtb_create_data` datetime NULL DEFAULT NULL COMMENT '作成日; yyyymmddhhmmss',
  `rtb_update_date` datetime NULL DEFAULT NULL COMMENT '更新日; yyyymmddhhmmss',
  `rtb_rank` int(3) NULL DEFAULT NULL COMMENT '順位',
  `rtb_jan` varchar(13) CHARACTER SET utf8mb3 COLLATE utf8mb3_unicode_ci NULL DEFAULT NULL COMMENT 'JAN',
  `rtb_media_cd` varchar(6) CHARACTER SET utf8mb3 COLLATE utf8mb3_unicode_ci NULL DEFAULT NULL COMMENT 'media_cd',
  `rtb_category_name_1` varchar(200) CHARACTER SET utf8mb3 COLLATE utf8mb3_unicode_ci NULL DEFAULT NULL,
  `rtb_category_name_2` varchar(200) CHARACTER SET utf8mb3 COLLATE utf8mb3_unicode_ci NULL DEFAULT NULL,
  `rtb_category_name_3` varchar(200) CHARACTER SET utf8mb3 COLLATE utf8mb3_unicode_ci NULL DEFAULT NULL,
  `rtb_isbn` varchar(13) CHARACTER SET utf8mb3 COLLATE utf8mb3_unicode_ci NULL DEFAULT NULL COMMENT 'isbn',
  `rtb_goods_name` varchar(50) CHARACTER SET utf8mb3 COLLATE utf8mb3_unicode_ci NULL DEFAULT NULL COMMENT '書名',
  `rtb_publisher` varchar(50) CHARACTER SET utf8mb3 COLLATE utf8mb3_unicode_ci NULL DEFAULT NULL COMMENT '出版社',
  `rtb_author` varchar(50) CHARACTER SET utf8mb3 COLLATE utf8mb3_unicode_ci NULL DEFAULT NULL COMMENT '著者',
  `rtb_price` int(6) NULL DEFAULT NULL COMMENT '価格',
  `rtb_sales_count` int(10) NULL DEFAULT NULL COMMENT '売上数',
  `rtb_filter_num` int(2) NULL DEFAULT NULL
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_unicode_ci ROW_FORMAT = Compact;

SET FOREIGN_KEY_CHECKS = 1;

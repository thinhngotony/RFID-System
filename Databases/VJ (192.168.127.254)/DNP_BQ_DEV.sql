/*
 Navicat Premium Data Transfer

 Source Server         : Server 254
 Source Server Type    : MariaDB
 Source Server Version : 100148 (10.1.48-MariaDB-0ubuntu0.18.04.1)
 Source Host           : 192.168.127.254:3306
 Source Schema         : DNP_BQ_DEV

 Target Server Type    : MariaDB
 Target Server Version : 100148 (10.1.48-MariaDB-0ubuntu0.18.04.1)
 File Encoding         : 65001

 Date: 26/04/2023 10:54:55
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
  `rtb_jan` varchar(13) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL COMMENT 'JAN',
  `rtb_media_cd` varchar(6) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL COMMENT 'media_cd',
  `rtb_isbn` varchar(13) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL COMMENT 'isbn',
  `rtb_goods_name` varchar(200) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL COMMENT '書名',
  `rtb_publisher` varchar(50) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL COMMENT '出版社',
  `rtb_author` varchar(50) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL COMMENT '著者',
  `rtb_price` int(6) NULL DEFAULT NULL COMMENT '価格',
  `rtb_sales_count` int(10) NULL DEFAULT NULL COMMENT '売上数',
  `rtb_filter_num` int(2) NULL DEFAULT NULL
) ENGINE = InnoDB CHARACTER SET = utf8 COLLATE = utf8_unicode_ci ROW_FORMAT = Compact;

SET FOREIGN_KEY_CHECKS = 1;

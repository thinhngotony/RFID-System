/*
 Navicat Premium Data Transfer

 Source Server         : Server 254
 Source Server Type    : MariaDB
 Source Server Version : 100148 (10.1.48-MariaDB-0ubuntu0.18.04.1)
 Source Host           : 192.168.127.254:3306
 Source Schema         : RFID

 Target Server Type    : MariaDB
 Target Server Version : 100148 (10.1.48-MariaDB-0ubuntu0.18.04.1)
 File Encoding         : 65001

 Date: 26/04/2023 10:54:44
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for bq_shop_goods_master
-- ----------------------------
DROP TABLE IF EXISTS `bq_shop_goods_master`;
CREATE TABLE `bq_shop_goods_master`  (
  `bqsg_bq_create_date` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_bq_update_date` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_shop_goodsid` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_servername` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_dbname` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_franchise_id` int(11) NULL DEFAULT NULL,
  `bqsg_shop_cd` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_goods_id` int(11) NULL DEFAULT NULL,
  `bqsg_goods_type` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_jan_cd` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_jan_cd2` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_create_date` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_update_date` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_koyu_goods_cd` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_goods_name` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_goods_ryaku` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_goods_kana` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_goods_kana2` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_tax_type` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_price` float NULL DEFAULT NULL,
  `bqsg_price_type` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_cost_price` float NULL DEFAULT NULL,
  `bqsg_price_taxadd` float NULL DEFAULT NULL,
  `bqsg_price_taxoff` float NULL DEFAULT NULL,
  `bqsg_new_reference_price` float NULL DEFAULT NULL,
  `bqsg_new_reference_price_type` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_new_reference_cost_price` float NULL DEFAULT NULL,
  `bqsg_used_reference_price` float NULL DEFAULT NULL,
  `bqsg_used_reference_price_type` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_used_reference_purchase_price` float NULL DEFAULT NULL,
  `bqsg_used_reference_purchase_price_type` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_used_reference_cost_price` float NULL DEFAULT NULL,
  `bqsg_sales_date` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_text_sales_date` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_stock_control_type` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_stock_control_goods_id` int(11) NULL DEFAULT NULL,
  `bqsg_set_type` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_media_cd` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_genre_cd` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_category_top` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_category_main` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_category_sub` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_goods_cd3` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_product_stop_flag` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_product_stop_date` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_purchase_stop_flag` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_purchase_stop_date` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_resales_day` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_aban_day` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_hachu_stop_flag` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_gross_flag` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_delete_flag` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_auto_hachu_flag` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_tana_media_update_flag` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_sales_update_flag` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_SYOZAICODE2` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_isbn` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_goods_sub_name` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_goods_sub_kana` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_writer_name` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_writer_kana` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_series_name` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_series_kana` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_c_code` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_torihiki_cd` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_volume_name` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_volume` int(11) NULL DEFAULT NULL,
  `bqsg_hangata` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_hannumber` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_publisher_cd` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_publisher_name` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_publisher_kana` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_shinkan_type` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_magazinecode_and_gatugo` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_pagenumber_name` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_pagenumber` int(11) NULL DEFAULT NULL,
  `bqsg_tate` int(11) NULL DEFAULT NULL,
  `bqsg_yoko` int(11) NULL DEFAULT NULL,
  `bqsg_atusa` int(11) NULL DEFAULT NULL,
  `bqsg_series_no_name` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_series_no` int(11) NULL DEFAULT NULL,
  `bqsg_publish_type_name` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_publisher_district` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_comment` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_comment_kana` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_contents_introduction` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_kaikiri_type` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_teiban_type` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_joubi_type` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_hitubi_type` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_kihontosyo_type` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_magazinecode_and_gatugo2` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_kihon_magazinecode` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_standard_number` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_maker_name` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_lifecycle_type` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_artist_cd` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_artist_name` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_artist_kana` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_label_cd` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_rank` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_cd_genre_cd` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_cd_hinshu_cd` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_distribution_type` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_important_type` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_base_type` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_regular_type` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_b_genre` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_m_genre` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_s_genre` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_shop_goods_price` float NULL DEFAULT NULL,
  `bqsg_shop_goods_price_intax` float NULL DEFAULT NULL,
  `bqsg_s_maker_cd` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_s_maker_name` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `bqsg_s_artist_name` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  INDEX `bqgm_jan_cd_index`(`bqsg_jan_cd`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8 COLLATE = utf8_unicode_ci ROW_FORMAT = Compact;

-- ----------------------------
-- Table structure for digital_display_config
-- ----------------------------
DROP TABLE IF EXISTS `digital_display_config`;
CREATE TABLE `digital_display_config`  (
  `step` int(11) NOT NULL,
  `type` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL,
  `duration` int(11) NULL DEFAULT NULL,
  `param1` text CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL,
  PRIMARY KEY (`step`) USING BTREE,
  UNIQUE INDEX `step_UNIQUE`(`step`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8 COLLATE = utf8_unicode_ci ROW_FORMAT = Compact;

-- ----------------------------
-- Table structure for drfid_rfgoods_master
-- ----------------------------
DROP TABLE IF EXISTS `drfid_rfgoods_master`;
CREATE TABLE `drfid_rfgoods_master`  (
  `drgm_create` varchar(8) CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL COMMENT '作成日; yyyymmdd',
  `drgm_pos_shop_cd` varchar(10) CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL COMMENT '取引先コード, 店舗コード(納品データ)',
  `drgm_com_shop_cd` varchar(10) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL COMMENT '店舗コード, 店舗コード(共通管理)',
  `rf_goods_type` varchar(1) CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL COMMENT '商品種別; \'1\'…書籍　\'2\'…雑誌　\'3\'…ＭＭ',
  `rf_goods_cd_type` varchar(1) CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL COMMENT '商品コード区分; \'０\'…その他　\'１\'…ＩＳＢＮコード　\'２\'…ＩＳＢＮ-ＪＡＮコード　\'５\'…雑誌コード \'６\'…定期刊行物コード（雑誌ＪＡＮ）\'７’…雑貨－ＪＡＮ(13桁)　\'８’…規格番号(13桁)\'９’…雑貨－ＪＡＮ(8桁)',
  `drgm_rfid_cd` varchar(30) CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL COMMENT 'RFIDコード',
  `drgm_jan` varchar(13) CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL COMMENT 'JANコード',
  `drgm_jan2` varchar(13) CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL COMMENT 'JAN2段コード; 以下確認',
  `drgm_goods_name` varchar(26) CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL COMMENT 'タイトル名',
  `drgm_goods_name_kana` varchar(20) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL COMMENT 'タイトルカナ名',
  `drgm_artist` varchar(26) CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL COMMENT 'アーティスト',
  `drgm_artist_kana` varchar(20) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL COMMENT 'アーティストカナ名',
  `drgm_maker_cd` varchar(10) CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL COMMENT '出版社記号',
  `drgm_maker_name` varchar(26) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL COMMENT '出版社名称',
  `drgm_genre_cd` varchar(2) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL COMMENT '部門コード; \'DNP部門コード',
  `drgm_maker_name_kana` varchar(20) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL COMMENT '出版社名称カナ',
  `drgm_c_code` varchar(4) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL COMMENT 'Cコード; \'ない場合はゼロ',
  `drgm_selling_date` varchar(8) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL COMMENT '発売日付; YYYYMMDD',
  `drgm_price_tax_off` int(7) NOT NULL COMMENT '本体価格',
  `drgm_cost_rate` float NULL DEFAULT NULL COMMENT '正味率; 整数部3桁　小数点以下1桁',
  `drgm_cost_price` int(6) NULL DEFAULT NULL COMMENT '原単価; ',
  `drgm_media_cd` varchar(6) CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL COMMENT 'mediaコード',
  PRIMARY KEY (`drgm_rfid_cd`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8 COLLATE = utf8_unicode_ci ROW_FORMAT = Compact;

-- ----------------------------
-- Table structure for drfid_taglog
-- ----------------------------
DROP TABLE IF EXISTS `drfid_taglog`;
CREATE TABLE `drfid_taglog`  (
  `dt_create_date` timestamp(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3) ON UPDATE CURRENT_TIMESTAMP COMMENT '作成日; システム付与　yyyymmddhhmmss',
  `dt_com_shop_cd` varchar(45) CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL COMMENT '店舗コード',
  `dt_rfid_cd` varchar(45) CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL,
  `dt_mode` varchar(45) CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL COMMENT '処理; 00=売上 01=仕入 02=返品 03=棚卸 04= 移動入庫 05=移動出庫',
  `sync_flag` varchar(1) CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL
) ENGINE = InnoDB CHARACTER SET = utf8 COLLATE = utf8_unicode_ci ROW_FORMAT = Compact;

-- ----------------------------
-- Table structure for rfid_app_setting
-- ----------------------------
DROP TABLE IF EXISTS `rfid_app_setting`;
CREATE TABLE `rfid_app_setting`  (
  `use_gate_checkpoint` int(1) NULL DEFAULT NULL,
  `url_gate_checkpoint` varchar(200) CHARACTER SET latin1 COLLATE latin1_swedish_ci NULL DEFAULT NULL,
  `username_gate` varchar(45) CHARACTER SET latin1 COLLATE latin1_swedish_ci NULL DEFAULT NULL,
  `password_gate` varchar(45) CHARACTER SET latin1 COLLATE latin1_swedish_ci NULL DEFAULT NULL
) ENGINE = InnoDB CHARACTER SET = latin1 COLLATE = latin1_swedish_ci ROW_FORMAT = Compact;

-- ----------------------------
-- Table structure for rfif_goods_master_top_sales
-- ----------------------------
DROP TABLE IF EXISTS `rfif_goods_master_top_sales`;
CREATE TABLE `rfif_goods_master_top_sales`  (
  `rgmts_create_date` varchar(14) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `rgmts_goods_type` varchar(2) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `rgmts_jan_cd` varchar(45) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `rgmts_goods_name` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `rgmts_goods_name_kana` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `rgmts_artist_name` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `rgmts_artist_kana` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `rgmts_publisher_cd` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `rgmts_publisher_name` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `rgmts_genre_cd` varchar(10) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `rgmts_publisher_name_kana` varchar(100) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `rgmts_c_code` varchar(10) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `rgmts_selling_date` varchar(8) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `rgmts_cost_rate` float NULL DEFAULT NULL,
  `rgmts_cost_price` float NULL DEFAULT NULL,
  `rgmts_media_cd` varchar(10) CHARACTER SET utf8 COLLATE utf8_unicode_ci NULL DEFAULT NULL,
  `rgmts_price` float NULL DEFAULT NULL,
  INDEX `rgmts_jan_cd`(`rgmts_jan_cd`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8 COLLATE = utf8_unicode_ci ROW_FORMAT = Compact;

SET FOREIGN_KEY_CHECKS = 1;
